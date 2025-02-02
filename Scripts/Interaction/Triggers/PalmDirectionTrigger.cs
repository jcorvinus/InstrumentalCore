using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Interaction;
using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Interaction.Triggers
{
    public class PalmDirectionTrigger : PoseTrigger
	{
		public enum DirectionToCheck
        {
            UserUp,
            UserDiagonal, // 45 degrees towards midline
            HeadForward,
            DirectionToHead
        }

		// want to support checking against the up direction.
		// eventually this should be a stand-in for the hip-up
		// direction, but for now just up is fine.
		[SerializeField] Handedness handedness;
        InstrumentalHand hand;
        Transform head;

        [SerializeField] DirectionToCheck directionToCheck;
        [Range(0, 90)]
        public float entryAngle = 25;

        [Range(0,90)]
        public float exitAngle = 35;

        [Range(0, 80)]
        [SerializeField] float feedbackActivationAngleOuter = 80f;

        float measuredAngle;

        Vect3 comparisonDirection;
		Vect3 palmDirection;

		[SerializeField] bool showDebugGizmos = false;

        // Start is called before the first frame update
        void Start()
        {
            GetHand();
        }

        void GetHand()
		{
            if (handedness == Handedness.Left) hand = InstrumentalHand.LeftHand;
            else if (handedness == Handedness.Right) hand = InstrumentalHand.RightHand;

            if (hand) head = hand.Body.Head;
        }

		Vect3 GetDirectionToCheck()
		{
			switch (directionToCheck)
			{
				case DirectionToCheck.UserUp:
                    return (handedness == Handedness.Left) ? InstrumentalBody.Instance.LeftPalmComfyUp :
                        InstrumentalBody.Instance.RightPalmComfyUp;
                case DirectionToCheck.UserDiagonal:
                    return (handedness == Handedness.Left) ? InstrumentalBody.Instance.LeftPalmDiagonal :
                        InstrumentalBody.Instance.RightPalmDiagonal;
                case DirectionToCheck.HeadForward:
                    return (Vect3)head.forward;
                case DirectionToCheck.DirectionToHead:
					Vect3 palmPosition = hand.GetAnchorPose(AnchorPoint.Palm).position;
					Vect3 directionToHead = ((Vect3)head.position - palmPosition).normalized;
                    return directionToHead;
				default:
                    return Vect3.up;
			}
		}

        // Update is called once per frame
        void Update()
        {
            if(!hand)
			{
                GetHand();
			}
            else
			{
                comparisonDirection = GetDirectionToCheck();
                palmDirection = hand.GetAnchorPose(AnchorPoint.Palm).rotation * Vect3.forward;
                bool isCorrectSide = (Vect3.Dot(palmDirection, comparisonDirection) > 0); // vector3.angle doesn't 
                                                                                          // have a notion of sidedness and I'm not screwing around with vector3.signed angle rn
                measuredAngle = Vect3.Angle(palmDirection, comparisonDirection);

                if (IsActive)
                {
                    // should we disengage?
                    if (!isCorrectSide || measuredAngle > exitAngle)
                    {
                        Deactivate();
                    }
                    else
                    {
                        feedback = Mathf.InverseLerp(0, exitAngle, measuredAngle);
                    }
                }
                else
                {
                    if (isCorrectSide && measuredAngle < entryAngle)
                    {
                        Activate();
                    }
                    else
                    {
                        feedback = 1 - Mathf.InverseLerp(entryAngle, feedbackActivationAngleOuter,
                            measuredAngle);
                    }
                }
            }
        }

		void DrawCone(Vect3 source, float length, float coneAngle, Vect3 normal)
        {
			Vect3 center = source + (normal * length);
            // so we want to draw a single circle at a specific distance.
            float radius = Mathf.Tan(coneAngle * Mathf.Deg2Rad) * length;

            DebugExtension.DrawCircle((Vector3)center, (Vector3)normal, Gizmos.color, radius);

            // then draw our connecting lines
            float iter = 360 * 0.25f;
            for (int i = 0; i < 4; i++)
            {
				Vect3 startPoint = ((Vect3.forward) * radius);
				Vect3 destination = Quatn.AngleAxis(i * iter, normal) *
                    startPoint;
                Gizmos.DrawLine((Vector3)source, (Vector3)(source + (normal * length) + destination));
            }
        }

        [Range(0,1)]
        public float DrawLength = 0.2f;

        private void OnDrawGizmos()
		{
#if UNITY
			if(hand && showDebugGizmos)
			{
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.blue;
				Vect3 rayOrigin = hand.GetAnchorPose(AnchorPoint.Palm).position;
                Gizmos.DrawRay((Vector3)rayOrigin,
                    (Vector3)palmDirection);

                DrawCone(rayOrigin, DrawLength, entryAngle,
                    palmDirection);

                Gizmos.color = (IsActive) ? Color.green : Color.red;
                DrawCone(rayOrigin, DrawLength, exitAngle,
                    palmDirection);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay((Vector3)rayOrigin, (Vector3)comparisonDirection);
			}
#endif
		}
	}
}