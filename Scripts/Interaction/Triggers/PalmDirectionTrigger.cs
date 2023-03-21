using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental.Interaction.Triggers
{
    public class PalmDirectionTrigger : PoseTrigger
	{
		public enum DirectionToCheck
        {
            UserUp,
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

        Vector3 comparisonDirection;
        Vector3 palmDirection;

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

        Vector3 GetDirectionToCheck()
		{
			switch (directionToCheck)
			{
				case DirectionToCheck.UserUp:
                    return Vector3.up; // todo: replace this with a smoothly rotating tracked plane
				case DirectionToCheck.HeadForward:
                    return head.forward;
                case DirectionToCheck.DirectionToHead:
                    Vector3 palmPosition = hand.GetAnchorPose(AnchorPoint.Palm).position;
                    Vector3 directionToHead = (head.position - palmPosition).normalized;
                    return directionToHead;
				default:
                    return Vector3.up;
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
                palmDirection = hand.GetAnchorPose(AnchorPoint.Palm).rotation * Vector3.forward;
                bool isCorrectSide = (Vector3.Dot(palmDirection, comparisonDirection) > 0); // vector3.angle doesn't 
                                                                                          // have a notion of sidedness and I'm not screwing around with vector3.signed angle rn
                measuredAngle = Vector3.Angle(palmDirection, comparisonDirection);

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

        void DrawCone(Vector3 source, float length, float coneAngle, Vector3 normal)
        {
            Vector3 center = source + (normal * length);
            // so we want to draw a single circle at a specific distance.
            float radius = Mathf.Tan(coneAngle * Mathf.Deg2Rad) * length;

            DebugExtension.DrawCircle(center, normal, Gizmos.color, radius);

            // then draw our connecting lines
            float iter = 360 * 0.25f;
            for (int i = 0; i < 4; i++)
            {
                Vector3 startPoint = ((Vector3.forward) * radius);
                Vector3 destination = Quaternion.AngleAxis(i * iter, normal) *
                    startPoint;
                Gizmos.DrawLine(source, source + (normal * length) + destination);
            }
        }

        [Range(0,1)]
        public float DrawLength = 0.2f;

        private void OnDrawGizmos()
		{
		    if(hand)
			{
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.blue;
                Vector3 rayOrigin = hand.GetAnchorPose(AnchorPoint.Palm).position;
                Gizmos.DrawRay(rayOrigin,
                    palmDirection);

                DrawCone(rayOrigin, DrawLength, entryAngle,
                    palmDirection);

                Gizmos.color = (IsActive) ? Color.green : Color.red;
                DrawCone(rayOrigin, DrawLength, exitAngle,
                    palmDirection);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(rayOrigin, comparisonDirection);
			}
		}
	}
}