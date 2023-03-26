using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    /// <summary>
    /// Add this script to an item to make it graspable.
    /// Todo: we will need to add support for having more than one
    /// hand interact with a graspable at once (both hovering, contacting, and grasping)
    /// </summary>
    public class GraspableItem : MonoBehaviour
    {
        // These influence the state of all grasp-related functions,
        // so calculate them once per update, and use them in various
        // states
        public struct GraspDataVars
        {
            public bool IsValid;
            public Vector3 IndexTip;
            public Vector3 MiddleTip;
            public Vector3 ThumbTip;
            public Vector3 ItemCenter;
            public Vector3 GraspCenter;

            public Vector3 IndexDirection;
            public float IndexDistance;

            public Vector3 MiddleDirection;
            public float MiddleDistance;


            public Vector3 ThumbDirection;
            public float ThumbDistance;
        }

        // events
        public delegate void GraspEventHandler(GraspableItem sender, InstrumentalHand hand);
        public event GraspEventHandler OnGrasped;
        public event GraspEventHandler OnUngrasped;

        SphereCollider itemCollider;
        Rigidbody rigidBody;
        MeshRenderer meshRenderer; // simple debugging for now

        bool isGrasped;
        bool isHovering;
        float hoverTValue;
        Handedness graspingHand; // we'll want to change this if we
                                 // ever decide to add two handed grasping.

        GraspDataVars currentGraspData;

        [Range(0.05f, 0.3f)]
        [SerializeField]
        float hoverDistance = 0.2f;

        public bool IsGrasped { get { return isGrasped; } }
        public bool IsHovering { get { return isHovering; } }
        public Rigidbody RigidBody { get { return rigidBody; } }

		private void Awake()
		{
            itemCollider = GetComponent<SphereCollider>();
            rigidBody = GetComponent<Rigidbody>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();

            if (!rigidBody)
            {
                AddRigidBody();
            }
		}

        void AddRigidBody()
		{
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;
        }

		// Start is called before the first frame update
		void Start()
		{

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

            Vector3 middleDireciton = (middleTip.position - center);
            float middleDistance = middleDireciton.magnitude;
            middleDireciton /= middleDistance;

            Vector3 thumbDirection = (thumbTip.position - center);
            float thumbDistance = thumbDirection.magnitude;
            thumbDirection /= thumbDistance;

            Vector3 graspCenter = (indexTip.position + 
                middleTip.position + 
                thumbTip.position) * 0.333f;

            return new GraspDataVars()
            {
                IsValid = isValid,
                ItemCenter = center,
                IndexDirection = indexDirection,
                IndexDistance = indexDistance,
                IndexTip = indexTip.position,
                MiddleDirection = middleDireciton,
                MiddleDistance = middleDistance,
                MiddleTip = middleTip.position,
                ThumbTip = thumbTip.position,
                ThumbDirection = thumbDirection,
                ThumbDistance = thumbDistance, 
                GraspCenter = graspCenter
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

        void Ungrasp(InstrumentalHand hand)
		{
            isGrasped = false;

            if(OnUngrasped != null)
			{
                OnUngrasped(this, hand);
			}
		}

        void Grasp(InstrumentalHand hand)
		{
            isGrasped = true;

            if(OnGrasped != null)
			{
                OnGrasped(this, hand);
			}
		}

		private void FixedUpdate()
		{
            // move according to grasp position
            // todo: if we're going to apply constraints, do so here.
            if (isGrasped)
            {
                rigidBody.position = currentGraspData.GraspCenter;
			}
		}

		void StartHover(InstrumentalHand hand)
		{
            isHovering = true;
        }

        void StopHover(InstrumentalHand hand)
		{
            isHovering = false;
		}

        float HoverDistSqr(InstrumentalHand hand)
		{
            Vector3 hoverPoint = hand.GraspPinch.PinchCenter;

            // check to see if we should distance hover
            Vector3 hoverClosestPoint = (hoverPoint -
                currentGraspData.ItemCenter);
            float hoverSqrDistanceToCenter = hoverClosestPoint.sqrMagnitude;
            float hoverDistSqr = hoverSqrDistanceToCenter - (itemCollider.radius * itemCollider.radius);

            return hoverSqrDistanceToCenter;
        }

        /*
         * StartHover(hand);*/


		// Update is called once per frame
		void Update()
        {
            InstrumentalHand hand = null;
            if (graspingHand == Handedness.Left) hand = InstrumentalHand.LeftHand;
            else if (graspingHand == Handedness.Right) hand = InstrumentalHand.RightHand;
            else hand = null;

            currentGraspData = CalulcateGraspVars(hand);

            // check to see if we are in a state already
            // and if so, should we exit
            if (isGrasped)
			{
                // check to see if we should ungrasp
                bool stillGrasping = CheckHandGrasp(hand, currentGraspData);
                if (!stillGrasping) Ungrasp(hand);
			}
            else if (isHovering)
			{
                // check to see if we should grasp
                bool shouldGrasp = CheckHandGrasp(hand, currentGraspData);

                if (shouldGrasp)
                {
                    Grasp(hand);
                }
                else
                {
                    // check to see if we should distance un-hover
                    Vector3 hoverClosestPoint = (currentGraspData.GraspCenter -
                        currentGraspData.ItemCenter);
                    float hoverDistanceToCenter = hoverClosestPoint.magnitude;
                    Vector3 hoverDirection = (hoverClosestPoint / hoverDistanceToCenter);
                    hoverClosestPoint = hoverDirection * itemCollider.radius;

                    float hoverAmount = Mathf.Max(0, hoverDistanceToCenter - itemCollider.radius);
                    hoverTValue = 1 - Mathf.InverseLerp(0, hoverAmount, hoverDistance);
                    Debug.Log("Hover amount: " + hoverAmount);

                    // if hover distance is lower than radius, then hover clamp hover distance to 0
                    // aside from that, inverse lerp
                    if (hoverAmount > hoverDistance)
                    {
                        StopHover(hand);
                    }
                }
            }
            else // we should look at either hand to figure out if hovering should start.
			{
                float leftHoverDistSqr = InstrumentalHand.LeftHand.IsTracking ?
                    HoverDistSqr(InstrumentalHand.LeftHand) : float.PositiveInfinity;
                float rightHoverDistSqr = InstrumentalHand.RightHand.IsTracking ?
                    HoverDistSqr(InstrumentalHand.RightHand) : 0;

                bool leftHoverClose = (leftHoverDistSqr < hoverDistance);
                bool rightHoverClose = (rightHoverDistSqr < hoverDistance);

                if (leftHoverClose && rightHoverClose)
                {
                    InstrumentalHand hoverHand = (leftHoverDistSqr < rightHoverDistSqr) ?
                        InstrumentalHand.LeftHand : InstrumentalHand.RightHand;

                    StartHover(hoverHand);
                }
                else if (leftHoverClose) StartHover(InstrumentalHand.LeftHand);
                else if (rightHoverClose) StartHover(InstrumentalHand.RightHand);
            }

            Color hoverColor = Color.Lerp(Color.white, Color.cyan, hoverTValue);
            Color graspColor = Color.green;
            Color debugColor = Color.white;

            if (isGrasped) debugColor = graspColor;
            else if (isHovering) debugColor = hoverColor;

            meshRenderer.material.color = debugColor;
            
        }

		private void OnDrawGizmos()
		{
            Gizmos.DrawWireSphere(transform.position, hoverDistance);
		}
	}
}