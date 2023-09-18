using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;
using Instrumental.Interaction.Constraints;
using Instrumental.Interaction.Solvers;

namespace Instrumental.Interaction
{
    public struct BoundsVertices
    {
        public Vector3 v1, v2, v3, v4, v5, v6, v7, v8;
        public Vector3 GetVertex(int index)
        {
            switch (index)
            {
                case (0):
                    return v1;
                case (1):
                    return v2;
                case (2):
                    return v3;
                case (3):
                    return v4;
                case (4):
                    return v5;
                case (5):
                    return v6;
                case (6):
                    return v7;
                case (7):
                    return v8;
                default:
                    return Vector3.zero;
            }
        }
    }

    /// <summary>
    /// Determines which posing system a graspable item will use
    /// It might be a good idea to eventually turn this into an
    /// abstract class system, so we can code arbitrary ones
    /// </summary>
	public enum GraspPoseType
	{ 
        None=0,
        StrictPoseMatch=1,
        OffsetPoseMatch=2
        //WeightDrag // used for heavy objects that have a sense of heft
        //SnapPoint // used for objects with 'handle' like affordances
    }

	/// <summary>
	/// Add this script to an item to make it interactive.
	/// Todo: we will need to add support for having more than one
	/// hand interact with a graspable at once (hovering, contacting, and grasping)
	/// </summary>
	public class InteractiveItem : MonoBehaviour
    {
        // These influence the state of all grasp-related functions,
        // so calculate them once per update, and use them in various
        // states
        public class GraspDataVars
        {
            public InstrumentalHand Hand;
            public bool IsGrasping;
            public Pose ThumbTip;
            public bool ThumbOverlap;
            public bool ThumbStartedGrasping; // true if this finger has contributed to the current grasp action
            public float ThumbCurlOnGrasp;
            public float ThumbCurlCurrent; // refactor for these is to calculate fixed timestep velocities
            public float ThumbCurlPrevious; // directly in instrumental hand

            public Pose IndexTip;
            public bool IndexOverlap;
            public float IndexCurlOnGrasp;
            public bool IndexStartedGrasping; // true if this finger has contributed to the current grasp action, even after grasp start
            public float IndexCurlCurrent;
            public float IndexCurlPrevious;

            public Pose MiddleTip;
            public bool MiddleOverlap;
            public float MiddleCurlOnGrasp;
            public bool MiddleStartedGrasping; // true if this finger has contributed to the current grasp action, even after grasp start
            public float MiddleCurlCurrent;
            public float MiddleCurlPrevious;

            public Pose RingTip;
            public bool RingOverlap;
            public float RingCurlOnGrasp;
            public bool RingStartedGrasping; // true if this finger has contributed to the current grasp action, even after grasp start
            public float RingCurlCurrent;
            public float RingCurlPrevious;

            public Pose PinkyTip;
            public bool PinkyOverlap;
            public float PinkyCurlOnGrasp;
            public bool PinkyStartedGrasp; // true if this finger has contributed to the current grasp action, even after grasp start
            public float PinkyCurlCurrent;
            public float PinkyCurlPrevious;

            public Vector3 GraspCenter;
            public Vector3 GraspPositionOffset;
            public Quaternion GraspRotationOffset;
            public Vector3 GraspStartPosition;
            public Vector3[] GraspStartConstellation;
            public float RegraspTimer; // turn this into an ungrasp filter instead of a regrasp filter

            public Collider[] ThumbColliderResults;
            public Collider[] IndexColliderResults;
            public Collider[] MiddleColliderResults;
            public Collider[] RingColliderResults;
            public Collider[] PinkyColliderResults;

            public bool GetOverlap(int fingerIndex)
			{
				switch (fingerIndex)
				{
                    case (0):
                        return ThumbOverlap;
                    case (1):
                        return IndexOverlap;
                    case (2):
                        return MiddleOverlap;
                    case (3):
                        return RingOverlap;
                    case (4):
                        return PinkyOverlap;
					default:
                        return false;
				}
			}
		}



		// events
		public delegate void GraspEventHandler(InteractiveItem sender);
        public event GraspEventHandler OnGrasped;
        public event GraspEventHandler OnUngrasped;

        float itemRadius;
        Bounds itemBounds;
        Collider[] itemColliders;
        Rigidbody rigidBody;
        Vector3 previousCenterOfMass;
        GraspConstraint constraint;

		#region Hover Vars
		float leftHoverDist =float.PositiveInfinity;
        float rightHoverDist=float.PositiveInfinity;

        float hoverTValue;
        [Range(0.05f, 0.3f)]
        float hoverDistance = 0.125f;
		#endregion

		#region Grasp Vars
		bool allowTwoHandedGrasp = true; // make this customizable later
        const bool useKabschSolve = false;
        bool isGrasped;
        bool graspStartedThisFrame = false;
        bool gravityStateOnGrasp;

        List<GraspDataVars> graspableHands;
        List<Vector3> graspStartPoints;
        List<Vector4> graspCurrentPoints;
        List<Vector3> offsetStartPoints; // these are used so we can subtract the body position from
                                    // grasp start points before solving
        AudioSource graspSource;
        KabschSolver poseSolver;

        const float ungraspDistance = 0.003636f;

        // negative values are near-grasp,
        // positive values are grasp.
        // actual distance, not normalized. Use this if you need to make sure your signifier scales to a specific value
        float currentGraspDistance;

        const float regraspDuration = 0.125f;

        [SerializeField] GraspPoseType poseType = GraspPoseType.OffsetPoseMatch;

        [Range(1, 100)]
        [SerializeField] float velocityPower = 9.3f;
        const float maxMovementSpeed = 6f;
        AnimationCurve distanceMotionCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0),
            new Keyframe(0.02f, 0.3f, 0, 0));
        [SerializeField] bool applyThrowBoost;

        [Range(5, 45f)]
        [SerializeField] float ungraspCurlVelocity = 25f;
        #endregion

        #region Debug Vars
        [Range(0.01f, 0.07f)]
        [SerializeField] float tipRadius=0.01f;
        MeshRenderer[] tipGrabSpheres;
        MeshRenderer[] constellationStartSpheres;
        MeshRenderer[] constellationRuntimeSpheres;

        [SerializeField] bool showConstellationGraspPoints = false;
        #endregion

        private Vector3 respawnPosition;
        private Quaternion respawnRotation;

		public bool IsGrasped { get { return isGrasped; } }
        public bool IsHovering { get { return (leftHoverDist < hoverDistance) || (rightHoverDist < hoverDistance); } }
        public float HoverTValue { get { return hoverTValue; } }
        public Rigidbody RigidBody { get { return rigidBody; } }

        public float CurrentGraspDistance { get { return currentGraspDistance; } }
        public float UngraspDistance { get { return ungraspDistance; } }

        public List<GraspDataVars> GraspableHands { get { return graspableHands; } }

		private void Awake()
		{
            rigidBody = GetComponent<Rigidbody>();

            if (!rigidBody)
            {
                AddRigidBody();
            }

            graspSource = GetComponent<AudioSource>();
            if (!graspSource) AddAudioSource();

            RefreshColliders();

            // init hands
            graspableHands = new List<GraspDataVars>();
            graspStartPoints = new List<Vector3>(10);
            graspCurrentPoints = new List<Vector4>(10);
            offsetStartPoints = new List<Vector3>(10);

            poseSolver = new KabschSolver();
		}

        // Start is called before the first frame update
        void Start()
        {
            graspSource.clip = GlobalSpace.Instance.UICommon.GrabClip;

            GraspDataVars leftHandGraspData = new GraspDataVars()
            {
                GraspCenter = Vector3.zero,
                Hand = InstrumentalHand.LeftHand,
                IsGrasping = false,
                IndexTip = new Pose(),
                MiddleTip = new Pose(),
                PinkyTip = new Pose(),
                RingTip = new Pose(),
                ThumbTip = new Pose(),
                ThumbColliderResults = new Collider[5],
                IndexColliderResults = new Collider[5],
                MiddleColliderResults = new Collider[5],
                RingColliderResults = new Collider[5],
                PinkyColliderResults = new Collider[5]
            };
            GraspDataVars rightHandGraspData = new GraspDataVars()
            {
                GraspCenter = Vector3.zero,
                Hand = InstrumentalHand.RightHand,
                IsGrasping = false,
                IndexTip = new Pose(),
                MiddleTip = new Pose(),
                PinkyTip = new Pose(),
                RingTip = new Pose(),
                ThumbTip = new Pose(),
                ThumbColliderResults = new Collider[5],
                IndexColliderResults = new Collider[5],
                MiddleColliderResults = new Collider[5],
                RingColliderResults = new Collider[5],
                PinkyColliderResults = new Collider[5]
            };
            graspableHands.Add(leftHandGraspData);
            graspableHands.Add(rightHandGraspData);

            tipGrabSpheres = new MeshRenderer[10];
            for (int i = 0; i < tipGrabSpheres.Length; i++)
            {
                GameObject tipObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider tipCollider = tipObject.GetComponent<SphereCollider>();
                tipCollider.isTrigger = true;
                tipCollider.enabled = false;
                MeshRenderer tipRenderer = tipObject.GetComponent<MeshRenderer>();
                tipRenderer.transform.localScale = Vector3.one * tipRadius;
                tipGrabSpheres[i] = tipRenderer;
            }

            constellationStartSpheres = new MeshRenderer[10];
            for(int i=0; i < constellationStartSpheres.Length; i++)
			{
                GameObject jointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider jointCollider = jointSphere.GetComponent<SphereCollider>();
                jointCollider.enabled = false;
                jointCollider.isTrigger = true;
                jointSphere.transform.localScale = Vector3.one * tipRadius;
                MeshRenderer jointRenderer = jointSphere.GetComponent<MeshRenderer>();
                constellationStartSpheres[i] = jointRenderer;
                jointRenderer.material.color = Color.cyan;
                jointRenderer.gameObject.SetActive(false);
			}

            constellationRuntimeSpheres = new MeshRenderer[10];
            for (int i = 0; i < constellationRuntimeSpheres.Length; i++)
            {
                GameObject jointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider jointCollider = jointSphere.GetComponent<SphereCollider>();
                jointCollider.enabled = false;
                jointCollider.isTrigger = true;
                jointSphere.transform.localScale = Vector3.one * tipRadius;
                MeshRenderer jointRenderer = jointSphere.GetComponent<MeshRenderer>();
                constellationRuntimeSpheres[i] = jointRenderer;
                jointRenderer.material.color = Color.yellow;
                jointRenderer.gameObject.SetActive(false);
            }

            SetRespawnLocation();
        }

        public void SetRespawnLocation(Vector3 position, Quaternion rotation)
		{
            respawnPosition = position;
            respawnRotation = rotation;
		}

        public void SetRespawnLocation()
		{
            Vector3 position = rigidBody.position;
            Quaternion rotation = rigidBody.rotation;

            SetRespawnLocation(position, rotation);
        }

        public void Respawn()
		{
            rigidBody.position = respawnPosition;
            rigidBody.rotation = respawnRotation;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
		}

        public void SetConstraint(GraspConstraint constraint)
		{
            this.constraint = constraint;
		}

        void AddRigidBody()
		{
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;
        }

        void AddAudioSource()
		{
            graspSource = gameObject.AddComponent<AudioSource>();
            graspSource.playOnAwake = false;
        }

		#region Collider Handling
		public void RefreshColliders()
		{
            // todo: it may be wise to automatically put interactive items on their own layer
            // so that tests to find them can be less computationally expensive in complicated physics environments.

            // todo: if we ever want to support nested interactive items,
            // change this to a traversal walk that collects as it goes but stops
            // when it finds a child interactiveItem.
            // child interactive items will require design thought, so make sure to do that
            // before settling on a decision.
            itemColliders = GetComponentsInChildren<Collider>(true);

            // get our bounds and radius here
            CalculateRadius(itemColliders);
        }

        public bool ClosestPointOnItem(Vector3 position, out Vector3 closestPoint,
            out bool isPointInside)
        {
            float closestDistance = float.PositiveInfinity;
            closestPoint = position;

            if (!rigidBody || itemColliders == null ||
                itemColliders.Length == 0)
            {
                isPointInside = false;
                return false;
            }
            else
            {
                bool foundValidCollider = false;

                for (int i = 0; i < itemColliders.Length; i++)
                {
                    Collider testCollider = itemColliders[i];
                    Vector3 closestPointOnCollider = testCollider.ClosestPoint(closestPoint);

                    isPointInside = (closestPointOnCollider == position);

                    if (isPointInside)
                    {
                        return true;
                    }

                    float squareDistance = (position - closestPointOnCollider).sqrMagnitude;

                    if (closestDistance > squareDistance)
                    {
                        closestPoint = closestPointOnCollider;
                    }

                    foundValidCollider = true;
                }

                isPointInside = false;
                return foundValidCollider;
            }
        }

        void StackWalk(Transform currentTransform, ref List<Transform> stack, Rigidbody rootBody)
        {
            if (currentTransform == null)
            {
                Debug.Log("Stackwalk hit scene root, this is bad");
            }
            else if (currentTransform.gameObject.GetInstanceID() != rootBody.gameObject.GetInstanceID())
            {
                stack.Add(currentTransform);
                Debug.Log("Added " + currentTransform.name + " to stack, now has " + stack.Count);
                StackWalk(currentTransform.parent, ref stack, rigidBody);
            }
            else
            {
                Debug.Log("Finished stack at " + currentTransform.name + ", now has " + stack.Count);
            }
        }

        Bounds GetColliderLocalBounds(Collider _collider)
        {
            Vector3 center = Vector3.zero;
            Vector3 size = Vector3.zero;

            if (_collider is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)_collider;
                center = boxCollider.center;
                size = boxCollider.size;
            }
            else if (_collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)_collider;
                center = sphereCollider.center;
                size = Vector3.one * sphereCollider.radius * 2;
            }
            else if (_collider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider = (CapsuleCollider)_collider;
                center = capsuleCollider.center;

                switch (capsuleCollider.direction)
                {
                    case (0): // x
                        size = new Vector3(capsuleCollider.height, capsuleCollider.radius * 2, capsuleCollider.radius * 2);
                        break;
                    case (1): // y
                        size = new Vector3(capsuleCollider.radius * 2, capsuleCollider.height, capsuleCollider.radius * 2);
                        break;
                    case (2): // z
                        size = new Vector3(capsuleCollider.radius * 2, capsuleCollider.radius * 2, capsuleCollider.height);
                        break;

                    default:
                        break;
                }
            }
            else if (_collider is MeshCollider)
            {
                MeshCollider meshCollider = (MeshCollider)_collider;
                center = meshCollider.sharedMesh.bounds.center;
                size = meshCollider.sharedMesh.bounds.size;
            }

            return new Bounds(center, size);
        }

        BoundsVertices GetVerticesForCollider(Collider _collider)
        {
            // we might not need this anymore, look into optimizing it out
            Bounds bounds = GetColliderLocalBounds(_collider);

            // transform our vertices stack walked upwards to the root
            List<Transform> transformStack = new List<Transform>();

            StackWalk(_collider.transform, ref transformStack, rigidBody);

            // in a bounds configuration:
            // v1 is down front left, v2 is down front right, v3 is down back left, v4 is down back right
            Vector3 v1 = bounds.center, v2 = bounds.center, v3 = bounds.center, v4 = bounds.center,
                // v5 is up front left, v6 is up front right, v7 is up back left, v8 is up back right
                v5 = bounds.center, v6 = bounds.center, v7 = bounds.center, v8 = bounds.center;

            if (_collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)_collider;
                v1 = sphereCollider.center + (Vector3.up * sphereCollider.radius);
                v2 = sphereCollider.center + (Vector3.down * sphereCollider.radius);
                v3 = sphereCollider.center + (Vector3.forward * sphereCollider.radius);
                v4 = sphereCollider.center + (Vector3.back * sphereCollider.radius);
                v5 = sphereCollider.center + (Vector3.left * sphereCollider.radius);
                v6 = sphereCollider.center + (Vector3.right * sphereCollider.radius);
                v7 = sphereCollider.center;
                v8 = sphereCollider.center;
            }
            else if (_collider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider = (CapsuleCollider)_collider;

                Vector3 direction = Vector3.zero;
                Vector3 radiusDirA = Vector3.zero, radiusDirB = Vector3.zero;
                switch (capsuleCollider.direction)
                {
                    case (0):
                        direction = Vector3.right;
                        radiusDirA = Vector3.forward;
                        radiusDirB = Vector3.up;
                        break;

                    case (1):
                        direction = Vector3.up;
                        radiusDirA = Vector3.right;
                        radiusDirB = Vector3.forward;
                        break;

                    case (2):
                        direction = Vector3.right;
                        radiusDirA = Vector3.forward;
                        radiusDirB = Vector3.up;
                        break;

                    default:
                        break;
                }

                v1 = capsuleCollider.center + (direction * capsuleCollider.height * 0.5f);
                v2 = capsuleCollider.center - (direction * capsuleCollider.height * 0.5f);
                v3 = capsuleCollider.center + (radiusDirA * capsuleCollider.radius);
                v4 = capsuleCollider.center - (radiusDirA * capsuleCollider.radius);
                v5 = capsuleCollider.center + (radiusDirB * capsuleCollider.radius);
                v6 = capsuleCollider.center - (radiusDirB * capsuleCollider.radius);
                v7 = capsuleCollider.center;
                v8 = capsuleCollider.center;
            }
            else
            {
                // if mesh collider
                // todo: in the future what we can do is calculate this by making a cage around the mesh, then
                // fitting those points to the closest point on mesh.
                // we can then store this for a given collider so that we never have to re-calculate this 
                // fitted sparse collection of points for a given mesh collider.
                v1 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v2 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);
                v3 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v4 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);

                v5 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v6 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);
                v7 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v8 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);
            }

            for (int i = 0; i < transformStack.Count; i++)
            {
                Transform currentTransform = transformStack[i];
                Matrix4x4 trs = Matrix4x4.TRS(currentTransform.localPosition, currentTransform.localRotation, currentTransform.localScale);
                v1 = trs.MultiplyPoint3x4(v1);
                v2 = trs.MultiplyPoint3x4(v2);
                v3 = trs.MultiplyPoint3x4(v3);
                v4 = trs.MultiplyPoint3x4(v4);
                v5 = trs.MultiplyPoint3x4(v5);
                v6 = trs.MultiplyPoint3x4(v6);
                v7 = trs.MultiplyPoint3x4(v7);
                v8 = trs.MultiplyPoint3x4(v8);
            }

            return new BoundsVertices()
            {
                v1 = v1,
                v2 = v2,
                v3 = v3,
                v4 = v4,
                v5 = v5,
                v6 = v6,
                v7 = v7,
                v8 = v8
            };
        }

        void CalculateRadius(Collider[] colliders)
        {
            Vector3 furthestPoint = rigidBody.centerOfMass;
            float furthestSqrDist = 0;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider currentCollider = colliders[i];
                BoundsVertices boundsVertices = GetVerticesForCollider(currentCollider);

                for (int v = 0; v < 8; v++)
                {
                    Vector3 vertex = boundsVertices.GetVertex(v);
                    Vector3 offset = vertex - rigidBody.centerOfMass;
                    float sqrMag = offset.sqrMagnitude;

                    if (sqrMag > furthestSqrDist)
                    {
                        furthestPoint = vertex;
                        furthestSqrDist = sqrMag;
                    }
                }
            }

            itemRadius = (furthestSqrDist > 0) ? Mathf.Sqrt(furthestSqrDist) : 0;
        }

        bool HasItemCollider(int hits, Collider[] testColliders)
        {
            for (int i = 0; i < hits; i++)
            {
                for (int c = 0; c < itemColliders.Length; c++)
                {
                    if (testColliders[i].GetInstanceID() == itemColliders[c].GetInstanceID())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

		#region Grasping
		void CalculateGraspVars(ref GraspDataVars handDataVars)		
        {
            InstrumentalHand hand = handDataVars.Hand;

            if (hand.IsTracking)
            {
                handDataVars.IndexTip = hand.GetAnchorPose(AnchorPoint.IndexTip);
                handDataVars.MiddleTip = hand.GetAnchorPose(AnchorPoint.MiddleTip);
                handDataVars.ThumbTip = hand.GetAnchorPose(AnchorPoint.ThumbTip);
                handDataVars.RingTip = hand.GetAnchorPose(AnchorPoint.RingTip);
                handDataVars.PinkyTip = hand.GetAnchorPose(AnchorPoint.PinkyTip);

                bool indexPinchIsCloser = (hand.GetPinchInfo(Finger.Index).PinchAmount > hand.GetPinchInfo(Finger.Middle).PinchAmount);
                handDataVars.GraspCenter =
                    indexPinchIsCloser ? hand.GetPinchInfo(Finger.Index).PinchCenter :
                    hand.GetPinchInfo(Finger.Middle).PinchCenter; // replace this with a blended version
                                                                  // once you figure out how to make the blend work properly.
                int numThumbHits = Physics.OverlapSphereNonAlloc(handDataVars.ThumbTip.position, tipRadius, handDataVars.ThumbColliderResults);
                handDataVars.ThumbOverlap = HasItemCollider(numThumbHits, handDataVars.ThumbColliderResults);
                handDataVars.ThumbCurlPrevious = handDataVars.ThumbCurlCurrent;
                handDataVars.ThumbCurlCurrent = hand.ThumbCurl;

                int numIndexHits = Physics.OverlapSphereNonAlloc(handDataVars.IndexTip.position, tipRadius, handDataVars.IndexColliderResults);
                handDataVars.IndexOverlap = HasItemCollider(numIndexHits, handDataVars.IndexColliderResults);
                handDataVars.IndexCurlPrevious = handDataVars.IndexCurlCurrent;
                handDataVars.IndexCurlCurrent = hand.IndexCurl;

                int numMiddleHits = Physics.OverlapSphereNonAlloc(handDataVars.MiddleTip.position, tipRadius, handDataVars.MiddleColliderResults);
                handDataVars.MiddleOverlap = HasItemCollider(numMiddleHits, handDataVars.MiddleColliderResults);
                handDataVars.MiddleCurlPrevious = handDataVars.MiddleCurlCurrent;
                handDataVars.MiddleCurlCurrent = hand.MiddleCurl;

                int numRingHits = Physics.OverlapSphereNonAlloc(handDataVars.RingTip.position, tipRadius, handDataVars.RingColliderResults);
                handDataVars.RingOverlap = HasItemCollider(numRingHits, handDataVars.RingColliderResults);
                handDataVars.RingCurlPrevious = handDataVars.RingCurlCurrent;
                handDataVars.RingCurlCurrent = hand.RingCurl;

                int numPinkyHits = Physics.OverlapSphereNonAlloc(handDataVars.PinkyTip.position, tipRadius, handDataVars.PinkyColliderResults);
                handDataVars.PinkyOverlap = HasItemCollider(numPinkyHits, handDataVars.PinkyColliderResults);
                handDataVars.PinkyCurlPrevious = handDataVars.PinkyCurlCurrent;
                handDataVars.PinkyCurlCurrent = hand.PinkyCurl;
            }
		}

		bool CheckHandGrasp(GraspDataVars graspVars)
		{
            InstrumentalHand hand = graspVars.Hand;
			if (hand == null) return false;
			if (!hand.IsTracking) return false; // may want to replace untracked ungrasp
                                                // with untracked suspend at some point in the future. Possibly
                                                // with predicted motion

            return CheckPinchGrip(graspVars);
		}

        bool CheckPinchGrip(GraspDataVars graspVars)
		{
            bool thumbTestPasses = graspVars.ThumbOverlap;
            bool indexTestPasses = graspVars.IndexOverlap;
            bool middleTestPasses = graspVars.MiddleOverlap;
            bool ringTestPasses = graspVars.RingOverlap;
            bool pinkyTestPasses = graspVars.PinkyOverlap; // 'pinch grip' and 'palm grip' bools?

            return thumbTestPasses && (indexTestPasses || middleTestPasses || ringTestPasses || pinkyTestPasses);
        }

        bool CheckHandUngrasp(GraspDataVars graspVars)
        {
            InstrumentalHand hand = graspVars.Hand;
            if (hand == null) return true;
            if (!hand.IsTracking) return false; // we can suspend like this, kinda.

            float thumbCurlVel = graspVars.ThumbCurlCurrent - graspVars.ThumbCurlPrevious;
            float indexCurlVel = graspVars.IndexCurlCurrent - graspVars.IndexCurlPrevious;
            float middleCurlVel = graspVars.MiddleCurlCurrent - graspVars.MiddleCurlCurrent;
            float ringCurlVel = graspVars.RingCurlCurrent - graspVars.RingCurlPrevious;
            float pinkyCurlVel = graspVars.PinkyCurlCurrent - graspVars.PinkyCurlPrevious;

            bool thumbReleaseVelocity = (thumbCurlVel < -ungraspCurlVelocity); // factor in grasp started?
            bool indexReleaseVelocity = indexCurlVel < -ungraspCurlVelocity;
            bool middleReleaseVelocity = middleCurlVel < -ungraspCurlVelocity;
            bool ringReleaseVelocity = ringCurlVel < -ungraspCurlVelocity;
            bool pinkyReleaseVelocity = pinkyCurlVel < -ungraspCurlVelocity;

            bool thumbReleaseCurl = (graspVars.ThumbCurlCurrent < graspVars.ThumbCurlOnGrasp);
            bool indexReleaseCurl = (graspVars.IndexCurlCurrent < graspVars.IndexCurlOnGrasp);
            bool middleReleaseCurl = (graspVars.MiddleCurlCurrent < graspVars.MiddleCurlOnGrasp);
            bool ringReleaseCurl = (graspVars.RingCurlCurrent < graspVars.RingCurlOnGrasp);
            bool pinkyReleaseCurl = (graspVars.PinkyCurlCurrent < graspVars.PinkyCurlOnGrasp);

            bool release = false;

            bool thumbRelease = thumbReleaseVelocity || thumbReleaseCurl;
            bool indexRelease = indexReleaseVelocity || indexReleaseCurl;
            bool middleRelease = middleReleaseVelocity || middleReleaseCurl;
            bool ringRelease = ringReleaseVelocity || ringReleaseCurl;
            bool pinkyRelease = pinkyReleaseVelocity || pinkyReleaseCurl;

            int numberOfFingersReleased = 0;
            int numberOfFingersStarted = 0;

            if (graspVars.IndexStartedGrasping && indexRelease) numberOfFingersReleased++;
            if (graspVars.MiddleStartedGrasping && middleRelease) numberOfFingersReleased++;
            if (graspVars.RingStartedGrasping && ringRelease) numberOfFingersReleased++;
            if (graspVars.PinkyStartedGrasp && pinkyRelease) numberOfFingersReleased++;

            if (graspVars.IndexStartedGrasping) numberOfFingersStarted++;
            if (graspVars.MiddleStartedGrasping) numberOfFingersStarted++;
            if (graspVars.RingStartedGrasping) numberOfFingersStarted++;
            if (graspVars.PinkyStartedGrasp) numberOfFingersStarted++;

            release = thumbRelease || numberOfFingersReleased == numberOfFingersStarted;

            return release;
        }

        float ungraspRadius { get { return (itemRadius) + ungraspDistance; } }


        void PerHandUngrasp(ref GraspDataVars graspData)
		{
            InstrumentalHand hand = graspData.Hand;
            graspData.IsGrasping = false;
            graspData.ThumbStartedGrasping = false;
            graspData.IndexStartedGrasping = false;
            graspData.MiddleStartedGrasping = false;
            graspData.RingStartedGrasping = false;
            graspData.PinkyStartedGrasp = false;
            graspData.ThumbCurlOnGrasp = 0;
            graspData.IndexCurlOnGrasp = 0;
            graspData.MiddleCurlOnGrasp = 0;
            graspData.RingCurlOnGrasp = 0;
            graspData.PinkyCurlOnGrasp = 0;

            int currentGraspCount = NumberOfGraspingHands();
            graspData.RegraspTimer = regraspDuration;

            if (currentGraspCount == 0) Ungrasp(hand.Velocity, hand.AngularVelocity);
        }

        void Ungrasp(Vector3 velocity, Vector3 angularVelocity)
		{
            isGrasped = false;

            rigidBody.useGravity = gravityStateOnGrasp;

            if (applyThrowBoost)
            {
                rigidBody.velocity = velocity * velocityPower;
                rigidBody.angularVelocity = angularVelocity * velocityPower;
            }

            if(OnUngrasped != null)
			{
                OnUngrasped(this);
			}
		}

        void PerHandGrasp(ref GraspDataVars graspData)
		{
            int currentGraspCount = NumberOfGraspingHands();

            GetGraspStartingOffset(ref graspData);
            graspData.IsGrasping = true;

            // todo: come up with a way to check these during grasp time and not just
            // on start. Remember to keep them one way tho - don't re-check on a finger
            // that has already hit 'started grasping == true'
            // store our first checks for iscontributing and start curl values
            graspData.ThumbStartedGrasping = graspData.ThumbOverlap;
            if(graspData.ThumbStartedGrasping) graspData.ThumbCurlOnGrasp = graspData.Hand.ThumbCurl;

            graspData.IndexStartedGrasping = graspData.IndexOverlap;
            if(graspData.IndexStartedGrasping) graspData.IndexCurlOnGrasp = graspData.Hand.IndexCurl;

            graspData.MiddleStartedGrasping = graspData.MiddleOverlap;
            if(graspData.MiddleStartedGrasping) graspData.MiddleCurlOnGrasp = graspData.Hand.MiddleCurl;

            graspData.RingStartedGrasping = graspData.RingOverlap;
            if(graspData.RingStartedGrasping) graspData.RingCurlOnGrasp = graspData.Hand.RingCurl;

            graspData.PinkyStartedGrasp = graspData.PinkyOverlap;
            if(graspData.PinkyStartedGrasp) graspData.PinkyCurlOnGrasp = graspData.Hand.PinkyCurl;

            InstrumentalHand hand = graspData.Hand;
            Input.HandData handData = hand.GetHandData();
            Pose palmPose = hand.GetAnchorPose(AnchorPoint.Palm);

            Vector3[] handConstellation = new Vector3[5]
            {
                handData.IndexJoints[1].Pose.position, // index knuckle
                handData.ThumbJoints[0].Pose.position, // wrist thumb joint
                handData.PinkyJoints[0].Pose.position, // pinky wrist joint,
                handData.PinkyJoints[1].Pose.position,
                palmPose.position + ((palmPose.rotation * Vector3.forward) * 0.03f)// palm normal offset
            };

            if(graspData.GraspStartConstellation == null || graspData.GraspStartConstellation.Length == 0)
			{
                graspData.GraspStartConstellation = handConstellation;
			}
            else
			{
                for(int i=0; i < handConstellation.Length; i++)
				{
                    graspData.GraspStartConstellation[i] = handConstellation[i];
				}
			}

            int startOffset = -1;

            for(int i=0; i < graspableHands.Count; i++)
			{
                if(graspableHands[i].Hand.GetInstanceID() == hand.GetInstanceID())
				{
                    startOffset = i * 4;
                    break;
				}
			}

            for(int i=0; i < handConstellation.Length; i++)
            { 
                constellationStartSpheres[i + startOffset].transform.position = handConstellation[i];
                constellationStartSpheres[i + startOffset].gameObject.SetActive(showConstellationGraspPoints);

                constellationRuntimeSpheres[i + startOffset].transform.position = handConstellation[i];
                constellationRuntimeSpheres[i + startOffset].gameObject.SetActive(showConstellationGraspPoints);
            }

            if (currentGraspCount == 0) StartGrasp();
        }

        void StartGrasp()
		{
            isGrasped = true;
            graspSource.Play();
            gravityStateOnGrasp = rigidBody.useGravity;
            rigidBody.useGravity = false;
            previousCenterOfMass = rigidBody.centerOfMass;

            graspStartedThisFrame = true;

            if (OnGrasped != null)
			{
                OnGrasped(this);
			}
		}

        void GetGraspStartingOffset(ref GraspDataVars graspData)
		{
            // todo: when we add more specific grasp poses,
            // create a code flow branch here to allow for that.
            Vector3 graspPositionOffset = transform.InverseTransformPoint(graspData.GraspCenter);
            Quaternion handRotation = graspData.Hand.GetAnchorPose(AnchorPoint.Palm).rotation;
            Vector3 handRotationLocalUp = handRotation * Vector3.up;
            Vector3 handRotationLocalForward = handRotation * Vector3.forward;
            handRotationLocalUp = transform.InverseTransformDirection(handRotationLocalUp);
            handRotationLocalForward = transform.InverseTransformDirection(handRotationLocalForward);

            if(poseType == GraspPoseType.StrictPoseMatch)
			{
                graspPositionOffset = Vector3.zero;
            }

            Quaternion handRotationLocal = Quaternion.LookRotation(handRotationLocalForward, handRotationLocalUp);
            Quaternion graspRotationOffset = Quaternion.Inverse(handRotationLocal);

            graspData.GraspPositionOffset = graspPositionOffset;
            graspData.GraspRotationOffset = graspRotationOffset;

            // storing the original grasp position for our solver
            graspData.GraspStartPosition = graspData.GraspCenter;
        }

        Vector3 CalculateSingleShotVelocity(Vector3 position, Vector3 previousPosition,
            float deltaTime)
        {
            float velocityFactor = 1.0f / deltaTime;
            return velocityFactor * (position - previousPosition);
        }

        Vector3 CalculateSingleShotAngularVelocity(Quaternion rotation, Quaternion previousRotation,
            float deltaTime)
		{
            Quaternion deltaRotation = rotation * Quaternion.Inverse(previousRotation);

            Vector3 deltaAxis;
            float deltaAngle;

            deltaRotation.ToAngleAxis(out deltaAngle, out deltaAxis);

            if (float.IsInfinity(deltaAxis.x))
            {
                deltaAxis = Vector3.zero;
                deltaAngle = 0;
            }

            if (deltaAngle > 180)
            {
                deltaAngle -= 360.0f;
            }

            Vector3 angularVelocity = deltaAxis * deltaAngle * Mathf.Deg2Rad / deltaTime;

            return angularVelocity;
        }


        /// <summary>
        /// Look into using average rotation from:
        /// https://forum.unity.com/threads/average-quaternions.86898/
        /// </summary>
        /// <returns></returns>
        Pose GetSolvedPose()
        {
            int graspCount = 0;

            Vector3 averagePosition = Vector3.zero;
            Vector3 forwardSum = Vector3.zero;
            Vector3 upSum = Vector3.zero;

            for (int i = 0; i < graspableHands.Count; i++)
            {
                GraspDataVars currentGraspData = graspableHands[i];

                if (currentGraspData.IsGrasping)
                {
                    InstrumentalHand hand = currentGraspData.Hand;
                    Pose currentGraspPose = new Pose(currentGraspData.GraspCenter,
                        hand.GetAnchorPose(AnchorPoint.Palm).rotation);

                    Vector3 worldSpaceOffsetPose = transform.TransformPoint(currentGraspData.GraspPositionOffset);
                    Vector3 offset = currentGraspPose.position - worldSpaceOffsetPose;
                    Vector3 position = transform.position + offset; // maybe change transform.position to body.position?
                    /*destinationPose = new Pose(position + destinationPose.position,
                        destinationPose.rotation * (currentGraspPose.rotation * currentGraspData.GraspRotationOffset));*/

                    averagePosition += position;

                    Quaternion rotation = (currentGraspPose.rotation * currentGraspData.GraspRotationOffset);

                    forwardSum += rotation * Vector3.forward;
                    upSum += rotation * Vector3.up;

                    graspCount++;
                }
            }

            averagePosition /= graspCount;
            forwardSum /= graspCount;
            upSum /= graspCount;

            Pose destinationPose = new Pose(averagePosition, Quaternion.LookRotation(forwardSum, upSum));

            if(graspCount == 0)
			{
                destinationPose = new Pose(rigidBody.position, rigidBody.rotation); // this should never happen but whatever
			}

            return destinationPose;
		}

        Pose GetKabschSolvedPose()
		{
            Vector3 bodyPosition = rigidBody.position;
            Quaternion bodyRotation = rigidBody.rotation;

            offsetStartPoints.Clear();
            for (int i = 0; i < graspStartPoints.Count; i++)
            {
                offsetStartPoints.Add(graspStartPoints[i] - bodyPosition);
            }

            for (int i = 0; i < graspCurrentPoints.Count; i++)
            {
                Vector3 currentPoint = new Vector3(graspCurrentPoints[i].x,
                    graspCurrentPoints[i].y, graspCurrentPoints[i].z);
                Vector3 adjustedPoint = currentPoint - bodyPosition;
                graspCurrentPoints[i] = new Vector4(currentPoint.x, currentPoint.y, currentPoint.z, 1);
            }

            // I think we're having this shoot us off in the distance because we're not keeping track of our
            // original and current manipulator positions, and not handling the offset between current manipulator
            // points and the body position
            Matrix4x4 solvedMatrix = poseSolver.SolveKabsch(offsetStartPoints, graspCurrentPoints);

            return new Pose(bodyPosition + solvedMatrix.GetVector3(),
                    solvedMatrix.GetRotation() * bodyRotation);
		}

        void DoGraspMovement()
		{
            if (poseType != GraspPoseType.None)
            {
                Pose destinationPose = Pose.identity;

                if(useKabschSolve)
				{
                    destinationPose = GetKabschSolvedPose();
				}
                else
				{
                    destinationPose = GetSolvedPose();
				}

                if (constraint) destinationPose = constraint.DoConstraint(destinationPose);

                // use strict placement if kinematic,
                // use physics movement if non-kinematic
                if (rigidBody.isKinematic)
                {
                    rigidBody.MovePosition(destinationPose.position);
                    rigidBody.MoveRotation(destinationPose.rotation);
                }
                else
				{
                    // calculate our center of mass, target velocity, angular velocity, etc
                    //Vector3 solvedCenterOfMass = destinationPose.rotation * rigidBody.centerOfMass + destinationPose.position;
                    //Vector3 currentCenterOfMass = rigidBody.rotation * rigidBody.centerOfMass + rigidBody.position;

                    Vector3 targetVelocity = CalculateSingleShotVelocity(destinationPose.position, 
                         rigidBody.position, Time.fixedDeltaTime);
                    Vector3 targetAngularVelocity = CalculateSingleShotAngularVelocity(destinationPose.rotation, 
                        rigidBody.rotation, Time.fixedDeltaTime);

                    float targetSpeedSquared = targetVelocity.sqrMagnitude;
                    if(targetSpeedSquared > maxMovementSpeed)
					{
                        float percent = maxMovementSpeed / Mathf.Sqrt(targetSpeedSquared);
                        targetVelocity *= percent;
                        targetAngularVelocity *= percent;
					}

                    float strength = 1;
                    if(!graspStartedThisFrame)
					{
                        float remainingDistance = Vector3.Distance(rigidBody.position, destinationPose.position);
                        strength = distanceMotionCurve.Evaluate(remainingDistance);
					}

                    Vector3 lerpedVelocity = Vector3.Lerp(rigidBody.velocity, targetVelocity, strength);
                    Vector3 lerpedAngularVelocity = Vector3.Lerp(rigidBody.angularVelocity, targetAngularVelocity, strength);

                    rigidBody.velocity = lerpedVelocity;
                    rigidBody.angularVelocity = lerpedAngularVelocity;

                    //previousCenterOfMass = solvedCenterOfMass;
				}
            }
        }

        void UpdateGrasp()
        {
            graspStartPoints.Clear();
            graspCurrentPoints.Clear();

            for(int handIndex=0; handIndex < graspableHands.Count; handIndex++)
			{
                GraspDataVars graspData = graspableHands[handIndex];
                CalculateGraspVars(ref graspData);
                graspableHands[handIndex] = graspData; // this might not be necessary.

                bool isCurrentlyGrasping = graspData.IsGrasping;

                if(isCurrentlyGrasping)
				{
                    bool shouldUngrasp = (CheckHandUngrasp(graspData));
                    if (shouldUngrasp) PerHandUngrasp(ref graspData);
				}
                else
				{
                    graspData.RegraspTimer -= Time.fixedDeltaTime;
                    graspData.RegraspTimer = Mathf.Max(graspData.RegraspTimer, 0);
                    bool shouldGrasp = CheckHandGrasp(graspData) && graspData.RegraspTimer == 0;
                    if (shouldGrasp) PerHandGrasp(ref graspData);
				}

                if(graspData.IsGrasping) // don't reuse currentlygrasping because shouldgrasp may have made grasping true
                    // without updating iscurrentlygrasping
				{
                    // add our grasp points to the solver's point list
                    graspStartPoints.Add(graspData.GraspStartPosition);
                    graspCurrentPoints.Add(new Vector4(graspData.GraspCenter.x, graspData.GraspCenter.y, graspData.GraspCenter.z, 1));
				}
            }
        }

        private void CalculateGraspDistance()
        {
            float indexDistance = float.PositiveInfinity;
            float middleDistance = float.PositiveInfinity;

            //indexDistance = currentGraspData.IndexDistance; //- itemCollider.radius; 
            //middleDistance = currentGraspData.MiddleDistance; //- itemCollider.radius; 

            //indexDistance = currentGraspData.IndexPinchDistance * 0.5f;
            //middleDistance = currentGraspData.MiddlePinchDistance * 0.5f;

            currentGraspDistance = 0;//Mathf.Min(indexDistance, middleDistance);
        }

        private int NumberOfGraspingHands()
		{
            int graspCount = 0;
            for(int i=0; i < graspableHands.Count; i++)
			{
                if (graspableHands[i].IsGrasping) graspCount++;
			}
            return graspCount;
		}
        #endregion

        private void FixedUpdate()
		{
            UpdateGrasp();

            // move according to grasp position
            if (isGrasped)
            {
                DoGraspMovement();
			}

            graspStartedThisFrame = false;
		}

		void StartHover(InstrumentalHand hand)
		{
            /*isHovering = true;
            graspingHand = hand.Hand;*/
        }

        void StopHover(InstrumentalHand hand)
		{
            /*isHovering = false;
            graspingHand = Handedness.None;*/
		}

        float HoverDist(InstrumentalHand hand)
		{
            Vector3 hoverPoint = hand.GraspPinch.PinchCenter;

            // check to see if we should distance hover
            Vector3 hoverClosestPoint = hoverPoint;
            bool isInside = false;

            if(ClosestPointOnItem(hoverClosestPoint, out hoverClosestPoint, out isInside))
			{
                float hoverDist = (hoverClosestPoint - hoverPoint).magnitude;
                return (isInside) ? 0 : hoverDist;
            }
            else
			{
                return float.PositiveInfinity;
			}
        }

        void UpdateHover()
		{
            bool previousLeftHover = leftHoverDist < hoverDistance;
            bool previousRightHover = rightHoverDist < hoverDistance;

            leftHoverDist = InstrumentalHand.LeftHand.IsTracking ?
                HoverDist(InstrumentalHand.LeftHand) : float.PositiveInfinity;

            rightHoverDist = InstrumentalHand.RightHand.IsTracking ?
                HoverDist(InstrumentalHand.RightHand) : float.PositiveInfinity;

			bool leftHover = (leftHoverDist < hoverDistance);
			bool rightHover = (rightHoverDist < hoverDistance);

            if(leftHover != previousLeftHover)
			{
                if (leftHover) StartHover(InstrumentalHand.LeftHand);
                else StopHover(InstrumentalHand.LeftHand);
			}

            if(rightHover != previousRightHover)
			{
                if (rightHover) StartHover(InstrumentalHand.RightHand);
                else StopHover(InstrumentalHand.RightHand);
			}

            float minHoverDist = Mathf.Min(leftHoverDist, rightHoverDist);
            hoverTValue = 1 - Mathf.InverseLerp(0, hoverDistance, minHoverDist);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateHover();

            if (graspableHands.Count > 0)
            {
                GraspDataVars hand0 = graspableHands[0];
                for (int i = 0; i < 5; i++)
                {
                    Pose pose = hand0.Hand.GetAnchorPose((AnchorPoint)i + 2);
                    tipGrabSpheres[i].transform.position = pose.position;
                    tipGrabSpheres[i].transform.localScale = Vector3.one * tipRadius;

                    tipGrabSpheres[i].gameObject.SetActive(hand0.Hand.IsTracking);

                    // material color depending on sphere overlap
                    bool tipOverlap = hand0.GetOverlap(i);
                    tipGrabSpheres[i].material.color = (tipOverlap) ? Color.green : Color.white;
                }
            }

            if (graspableHands.Count > 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    GraspDataVars hand1 = graspableHands[1];
                    int offset = 5;
                    Pose pose = hand1.Hand.GetAnchorPose((AnchorPoint)i + 2);
                    tipGrabSpheres[i + offset].transform.position = pose.position;
                    tipGrabSpheres[i + offset].transform.localScale = Vector3.one * tipRadius;

                    tipGrabSpheres[i + offset].gameObject.SetActive(hand1.Hand.IsTracking);

                    // material color depending on sphere overlap
                    bool tipOverlap = hand1.GetOverlap(i);
                    tipGrabSpheres[i + offset].material.color = (tipOverlap) ? Color.green : Color.white;
                }
            }
		}
		private void OnDrawGizmos()
		{
            if(rigidBody)
			{
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, ungraspRadius); // this is an old visualization from when we were only doing sphere colliders

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.TransformPoint(rigidBody.centerOfMass),
                    itemRadius);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.TransformPoint(rigidBody.centerOfMass),
                    itemRadius + hoverDistance);
			}
        }
	}
}