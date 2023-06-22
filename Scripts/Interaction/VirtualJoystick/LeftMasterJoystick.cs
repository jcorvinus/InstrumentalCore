using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction.Triggers;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class LeftMasterJoystick : MonoBehaviour
    {
        InstrumentalHand hand;
        LogicTrigger logicTrigger;
        RingActivator ringActivator;
        [SerializeField] Joystick joystick;
        [Range(1, 2)]
        [SerializeField] float outerRadiusMultiplier = 1.5f;

		private void Awake()
		{
            logicTrigger = GetComponent<LogicTrigger>();
            ringActivator = GetComponentInChildren<RingActivator>();
            GetHand();
		}

		void GetHand()
		{
            hand = InstrumentalHand.LeftHand;
        }

		// Start is called before the first frame update
		void Start()
        {
            ringActivator.enabled = false;
            GetHand();

            ringActivator.Activated += () =>
            {
                joystick.transform.position = ringActivator.GetChildSpawnPosition();
                joystick.gameObject.SetActive(true);
            };
        }

        void DoDistanceJoystickFeedback()
		{
            float distance = float.PositiveInfinity;

            if(hand.IsTracking)
			{
                distance = Vector3.Distance(hand.GraspPinch.PinchCenter,
                    joystick.transform.position);

                distance -= ringActivator.Radius;
                distance = Mathf.Max(0, distance);
			}

            float outerRadius = GetOuterRadius();

            joystick.SignifierValue = 1 - Mathf.InverseLerp(0, outerRadius, distance);
		}

        public float GetInnerRadius()
		{
            return ringActivator.Radius;
		}

        public float GetOuterRadius()
		{
            return ((ringActivator) ? ringActivator.Radius :
                0.1f) * outerRadiusMultiplier;
        }

        // Update is called once per frame
        void Update()
        {
            if (hand)
            {
                Pose anchorPose = hand.GetAnchorPose(AnchorPoint.Palm);
                ringActivator.transform.position = anchorPose.position;
                Vector3 forward, up;
                forward = anchorPose.rotation * Vector3.up;
                up = anchorPose.rotation * Vector3.forward;

                ringActivator.transform.rotation = Quaternion.LookRotation(forward, up);

                ringActivator.enabled = logicTrigger.IsActive && 
                    !joystick.gameObject.activeInHierarchy;


                DoDistanceJoystickFeedback();
            }
        }

		private void OnDrawGizmos()
		{
            Gizmos.DrawWireSphere(joystick.transform.position,
                GetOuterRadius());
		}
	}
}