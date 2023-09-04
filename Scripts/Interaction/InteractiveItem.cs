using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;
using Instrumental.Interaction.Constraints;

namespace Instrumental.Interaction
{
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
        public struct GraspDataVars
        {
            public bool IsValid;
            //public Vector3 IndexTip;
            //public Vector3 MiddleTip;
            //public Vector3 ThumbTip;
            public Vector3 ItemCenter;
            public Vector3 GraspCenter;

            public Vector3 IndexDirection;
            public float IndexDistance;
            public float IndexPinchDistance; // this is the index-thumbtip distance

            public Vector3 MiddleDirection;
            public float MiddleDistance;
            public float MiddlePinchDistance; // this is the middle-thumbtip distance

            public Vector3 ThumbDirection;
            public float ThumbDistance;
        }

        // events
        public delegate void GraspEventHandler(InteractiveItem sender, InstrumentalHand hand);
        public event GraspEventHandler OnGrasped;
        public event GraspEventHandler OnUngrasped;

        SphereCollider itemCollider;
        Collider[] itemColliders;
        Rigidbody rigidBody;
        Vector3 previousCenterOfMass;
        GraspConstraint constraint;

        bool isGrasped;
        bool graspStartedThisFrame = false;
        float leftHoverDist=float.PositiveInfinity;
        float rightHoverDist=float.PositiveInfinity;

        float hoverTValue;
        Handedness graspingHand; // we'll want to change this if we
                                 // ever decide to add two handed grasping.
        bool gravityStateOnGrasp;

        GraspDataVars currentGraspData;
        AudioSource graspSource;

        const float ungraspDistance = 0.003636f;

        [Range(0.05f, 0.3f)]
        float hoverDistance = 0.125f;

        // negative values are near-grasp,
        // positive values are grasp.
        // actual distance, not normalized. Use this if you need to make sure your signifier scales to a specific value
        float currentGraspDistance;

        [SerializeField] GraspPoseType poseType = GraspPoseType.OffsetPoseMatch;
        // pose offset values
        // these are the local-to-hand offsets of the item when the grasp begins
        Vector3 graspPositionOffset;
        Quaternion graspRotationOffset;

        [Range(1, 100)]
        [SerializeField] float velocityPower = 9.3f;
        const float maxMovementSpeed = 6f;
        AnimationCurve distanceMotionCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0),
            new Keyframe(0.02f, 0.3f, 0, 0));
        [SerializeField] bool applyThrowBoost;

        public bool IsGrasped { get { return isGrasped; } }
        public bool IsHovering { get { return (leftHoverDist < hoverDistance) || (rightHoverDist < hoverDistance); } }
        public float HoverTValue { get { return hoverTValue; } }
        public Rigidbody RigidBody { get { return rigidBody; } }

        public float CurrentGraspDistance { get { return currentGraspDistance; } }
        public float UngraspDistance { get { return ungraspDistance; } }

		private void Awake()
		{
            itemCollider = GetComponent<SphereCollider>();
            rigidBody = GetComponent<Rigidbody>();

            if (!rigidBody)
            {
                AddRigidBody();
            }

            graspSource = GetComponent<AudioSource>();
            if (!graspSource) AddAudioSource();

            RefreshColliders();
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

        public void RefreshColliders()
		{
            // todo: if we ever want to support nested interactive items,
            // change this to a traversal walk that collects as it goes but stops
            // when it finds a child interactiveItem.
            // child interactive items will require design thought, so make sure to do that
            // before settling on a decision.
            itemColliders = GetComponentsInChildren<Collider>(true);
        }

		// Start is called before the first frame update
		void Start()
		{
            graspSource.clip = GlobalSpace.Instance.UICommon.GrabClip;
        }

        GraspDataVars CalulcateGraspVars(InstrumentalHand hand)
		{
            bool isValid = hand;

            Pose indexTip, middleTip, thumbTip;
            if (isValid)
            {
                indexTip = hand.GetAnchorPose(AnchorPoint.IndexTip);
                middleTip = hand.GetAnchorPose(AnchorPoint.MiddleTip);
                thumbTip = hand.GetAnchorPose(AnchorPoint.ThumbTip);
            }
            else
			{
                indexTip = new Pose();
                middleTip = new Pose();
                thumbTip = new Pose();
			}

            Vector3 center = transform.TransformPoint(itemCollider.center);
            Vector3 indexDirection = (indexTip.position - center);
            float indexDistance = indexDirection.magnitude;
            indexDirection /= indexDistance;

            Vector3 middleDirection = (middleTip.position - center);
            float middleDistance = middleDirection.magnitude;
            middleDirection /= middleDistance;

            Vector3 thumbDirection = (thumbTip.position - center);
            float thumbDistance = thumbDirection.magnitude;
            thumbDirection /= thumbDistance;

            float indexPinchDistance = 0;
            float middlePinchDistance = 0;

            Vector3 graspCenter = center;

            if (isValid)
            {
                PinchInfo indexPinch, middlePinch;
                indexPinch = hand.GetPinchInfo(Finger.Index);
                middlePinch = hand.GetPinchInfo(Finger.Middle);

                indexPinchDistance = indexPinch.PinchDistance;
                middlePinchDistance = middlePinch.PinchDistance;

                bool indexPinchIsCloser = (hand.GetPinchInfo(Finger.Index).PinchAmount > hand.GetPinchInfo(Finger.Middle).PinchAmount);
                graspCenter = 
                    indexPinchIsCloser ? hand.GetPinchInfo(Finger.Index).PinchCenter :
                    hand.GetPinchInfo(Finger.Middle).PinchCenter; // replace this with a blended version
                        // once you figure out how to make the blend work properly.
            }

            return new GraspDataVars()
            {
                IsValid = isValid,
                ItemCenter = center,

                IndexDistance = indexDistance,
                IndexPinchDistance = indexPinchDistance,
                MiddleDistance = middleDistance,
                MiddlePinchDistance = middlePinchDistance,
                ThumbDistance = thumbDistance,
                GraspCenter = graspCenter
                //IndexDirection = indexDirection, // get rid of these 
                //IndexTip = indexTip.position, // if they wind up not being necessary.
                //MiddleDirection = middleDireciton,
                //MiddleTip = middleTip.position,
                //ThumbTip = thumbTip.position,
                //ThumbDirection = thumbDirection
            };
		}

		bool CheckHandGrasp(InstrumentalHand hand, GraspDataVars graspVars)
		{
			if (hand == null) return false;
			if (!hand.IsTracking) return false; // may want to replace untracked ungrasp
												// with untracked suspend at some point in the future. Possibly
												// with predicted motion

			bool thumbDistancePasses = graspVars.ThumbDistance < itemCollider.radius;
			bool indexDistancePasses = graspVars.IndexDistance < itemCollider.radius;
			bool middleDistancePasses = graspVars.MiddleDistance < itemCollider.radius;

			return thumbDistancePasses && (indexDistancePasses || middleDistancePasses);
		}

        bool CheckHandUngrasp(InstrumentalHand hand, GraspDataVars graspVars)
		{
            if (hand == null) return true;
            if (!hand.IsTracking) return false; // we can suspend like this, kinda.

            float minPinchDistance = Mathf.Min(graspVars.IndexPinchDistance,
                graspVars.MiddlePinchDistance);

            return (minPinchDistance > ungraspRadius);
		}

        float ungraspRadius { get { return (itemCollider.radius * 2) + ungraspDistance; } }


        void Ungrasp(InstrumentalHand hand)
		{
            isGrasped = false;
            rigidBody.useGravity = gravityStateOnGrasp;

            if (applyThrowBoost)
            {
                rigidBody.velocity = hand.Velocity * velocityPower;
                rigidBody.angularVelocity = hand.AngularVelocity * velocityPower;
            }

            if(OnUngrasped != null)
			{
                OnUngrasped(this, hand);
			}
		}

        void Grasp(InstrumentalHand hand)
		{
            isGrasped = true;
            graspSource.Play();
            gravityStateOnGrasp = rigidBody.useGravity;
            rigidBody.useGravity = false;
            previousCenterOfMass = rigidBody.centerOfMass;

            GetGraspStartingOffset(hand);

            graspStartedThisFrame = true;

            if (OnGrasped != null)
			{
                OnGrasped(this, hand);
			}
		}

        void GetGraspStartingOffset(InstrumentalHand hand)
		{
            GraspDataVars graspData = CalulcateGraspVars(hand);

            // todo: when we add more specific grasp poses,
            // create a code flow branch here to allow for that.
            graspPositionOffset = transform.InverseTransformPoint(graspData.GraspCenter);
            Quaternion handRotation = hand.GetAnchorPose(AnchorPoint.Palm).rotation;
            Vector3 handRotationLocalUp = handRotation * Vector3.up;
            Vector3 handRotationLocalForward = handRotation * Vector3.forward;
            handRotationLocalUp = transform.InverseTransformDirection(handRotationLocalUp);
            handRotationLocalForward = transform.InverseTransformDirection(handRotationLocalForward);

            if(poseType == GraspPoseType.StrictPoseMatch)
			{
                graspPositionOffset = Vector3.zero;
            }

            Quaternion handRotationLocal = Quaternion.LookRotation(handRotationLocalForward, handRotationLocalUp);
            graspRotationOffset = Quaternion.Inverse(handRotationLocal);
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

        void DoGraspMovement()
		{
            // this is dumb - I should probably just include the InstrumentalHand right in the
            // grasping data vars
            InstrumentalHand hand = null;
            if (graspingHand == Handedness.Left) hand = InstrumentalHand.LeftHand;
            else if (graspingHand == Handedness.Right) hand = InstrumentalHand.RightHand;
            else hand = null;

            if (hand != null && poseType != GraspPoseType.None)
            {
                Pose currentGraspPose = new Pose(currentGraspData.GraspCenter,
                    hand.GetAnchorPose(AnchorPoint.Palm).rotation);

                Vector3 worldSpaceOffsetPose = transform.TransformPoint(graspPositionOffset);
                Vector3 offset = currentGraspPose.position - worldSpaceOffsetPose;
                Pose destinationPose = new Pose(transform.position + offset,
                    currentGraspPose.rotation * graspRotationOffset);

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
                    // calculate our center of mass, target vleocity, angular velocity, etc
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

		private void FixedUpdate()
		{
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

        void DoHover()
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
            DoHover();

            // old update kinda sorta used to go here
		}

        void OldUpdate()
		{
            //         InstrumentalHand hand = null;
            //         if (graspingHand == Handedness.Left) hand = InstrumentalHand.LeftHand;
            //         else if (graspingHand == Handedness.Right) hand = InstrumentalHand.RightHand;
            //         else hand = null;

            //         currentGraspData = CalulcateGraspVars(hand);

            //         // check to see if we are in a state already
            //         // and if so, should we exit
            //         if (isGrasped)
            //{
            //             // check to see if we should ungrasp
            //             bool stillGrasping = !CheckHandUngrasp(hand, currentGraspData);
            //             if (!stillGrasping) Ungrasp(hand);
            //}
            //         else if (isHovering)
            //{
            //             // check to see if we should grasp
            //             bool shouldGrasp = CheckHandGrasp(hand, currentGraspData);

            //             if (shouldGrasp)
            //             {
            //                 Grasp(hand);
            //             }
            //             else
            //             {
            //                 float distance = HoverDist(hand);
            //                 hoverTValue = 1 - Mathf.InverseLerp(0, hoverDistance, distance);

            //		// if hover distance is lower than radius, then hover clamp hover distance to 0
            //		// aside from that, inverse lerp
            //		if (distance > hoverDistance)
            //		{
            //			StopHover(hand);
            //		}
            //	}
            //         }
            //         else // we should look at either hand to figure out if hovering should start.
            //{
            //             float leftHoverDist = InstrumentalHand.LeftHand.IsTracking ?
            //                 HoverDist(InstrumentalHand.LeftHand) : float.PositiveInfinity;
            //             float rightHoverDist = InstrumentalHand.RightHand.IsTracking ?
            //                 HoverDist(InstrumentalHand.RightHand) : float.PositiveInfinity;

            //             bool leftHoverClose = (leftHoverDist < hoverDistance);
            //             bool rightHoverClose = (rightHoverDist < hoverDistance);

            //             if (leftHoverClose && rightHoverClose)
            //             {
            //                 InstrumentalHand hoverHand = (leftHoverDist < rightHoverDist) ?
            //                     InstrumentalHand.LeftHand : InstrumentalHand.RightHand;

            //                 StartHover(hoverHand);
            //             }
            //             else if (leftHoverClose) StartHover(InstrumentalHand.LeftHand);
            //             else if (rightHoverClose) StartHover(InstrumentalHand.RightHand);
            //         }

            //         CalculateGraspDistance();
        }

        private void CalculateGraspDistance()
		{
            float indexDistance = float.PositiveInfinity;
            float middleDistance = float.PositiveInfinity;

            indexDistance = currentGraspData.IndexDistance; //- itemCollider.radius; 
            middleDistance = currentGraspData.MiddleDistance; //- itemCollider.radius; 

			//indexDistance = currentGraspData.IndexPinchDistance * 0.5f;
			//middleDistance = currentGraspData.MiddlePinchDistance * 0.5f;

			currentGraspDistance = Mathf.Min(indexDistance, middleDistance);
        }

		private void OnDrawGizmos()
		{
            Gizmos.DrawWireSphere(transform.position, hoverDistance);
            if (itemCollider)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, ungraspRadius);
            }
        }
	}
}