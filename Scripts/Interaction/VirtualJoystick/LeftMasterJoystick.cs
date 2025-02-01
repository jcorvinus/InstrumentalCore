using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;
using Instrumental.Core.Math;
using Instrumental.Overlay;
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

        public Vector2 Value { get { return joystick.Value; } }
        public bool InputActive { get { return joystick.InputActive; } }

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
                distance = Vect3.Distance(hand.GraspPinch.PinchCenter,
                    (Vect3)joystick.transform.position);

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
                PoseIC anchorPose = hand.GetAnchorPose(AnchorPoint.Palm);
                ringActivator.transform.position = (Vector3)anchorPose.position;
                Vect3 forward, up;
                forward = anchorPose.rotation * Vect3.up;
                up = anchorPose.rotation * Vect3.forward;

                ringActivator.transform.rotation = (Quaternion)Quatn.LookRotation(forward, up);

                ringActivator.enabled = logicTrigger.IsActive && 
                    !joystick.gameObject.activeInHierarchy;


                DoDistanceJoystickFeedback();
            }
        }

		private void OnDrawGizmos()
		{
#if UNITY
			Gizmos.DrawWireSphere(joystick.transform.position,
                GetOuterRadius());
#endif
		}
	}
}