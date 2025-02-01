using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Interaction.Triggers
{
	public enum ExtensionState
	{
		None = 0,
		Extended = 1,
		Unextended = 2
	}

	public class FingerExtensionTrigger : PoseTrigger
    {
		[SerializeField] Handedness handedness;
		InstrumentalHand hand;

		[SerializeField] ExtensionState thumbExtension;
		[SerializeField] ExtensionState indexExtension;
		[SerializeField] ExtensionState middleExtension;
		[SerializeField] ExtensionState ringExtension;
		[SerializeField] ExtensionState pinkyExtension;

		bool thumbPasses = false;
		bool indexPasses = false;
		bool middlePasses = false;
		bool ringPasses = false;
		bool pinkyPasses = false;

		const float dotCutoff=0.45f;
		const float thumbDotCutoff=0.31f;
			
		const float lineDist=0.05f;

		void GetHand()
		{
			if (handedness == Handedness.Left) hand = InstrumentalHand.LeftHand;
			else if (handedness == Handedness.Right) hand = InstrumentalHand.RightHand;
		}

		// Start is called before the first frame update
		void Start()
        {
			GetHand();
        }

        // Update is called once per frame
        void Update()
        {
			if (hand)
			{
				thumbPasses = ValuePasses(hand.ThumbIsExtended, thumbExtension);
				indexPasses = ValuePasses(hand.IndexIsExtended, indexExtension);
				middlePasses = ValuePasses(hand.MiddleIsExtended, middleExtension);
				ringPasses = ValuePasses(hand.RingIsExtended, ringExtension);
				pinkyPasses = ValuePasses(hand.PinkyIsExtended, pinkyExtension);

				if (thumbPasses && indexPasses && middlePasses && ringPasses && pinkyPasses)
				{
					Activate();
				}
				else Deactivate();
			}
			else GetHand();
        }

		bool ValuePasses(bool input, ExtensionState extensionState)
		{
			bool passes = false;
			if (extensionState == ExtensionState.Extended)
			{
				passes = input;
			}
			else if (extensionState == ExtensionState.Unextended)
			{
				passes = !input;
			}
			else passes = true;

			return passes;
		}

		void DrawFingerState(bool passes, AnchorPoint anchorPoint)
		{
			Vector3 position = (Vector3)hand.GetAnchorPose(anchorPoint).position;
			Quaternion rotation = (Quaternion)hand.GetAnchorPose(anchorPoint).rotation;
			Vector3 forward = rotation * Vector3.forward;

			Gizmos.color = (passes) ? Color.green : Color.red;
			Gizmos.DrawLine(position, position + (forward * lineDist));
		}

		private void OnDrawGizmos()
		{
			if(hand)
			{
				PoseIC palmPose = hand.GetAnchorPose(AnchorPoint.Palm);

				Vect3 palmDirection = palmPose.rotation * Vect3.up;
				Vect3 palmThumbRef = palmPose.rotation * Vect3.right;
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine((Vector3)(palmPose.position), 
					(Vector3)(palmPose.position + (palmDirection * lineDist)));
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine((Vector3)palmPose.position, 
					(Vector3)(palmPose.position + palmThumbRef));
				DrawFingerState(thumbPasses, AnchorPoint.ThumbTip);
				DrawFingerState(indexPasses, AnchorPoint.IndexTip);
				DrawFingerState(middlePasses, AnchorPoint.MiddleTip);
				DrawFingerState(ringPasses, AnchorPoint.RingTip);
				DrawFingerState(pinkyPasses, AnchorPoint.PinkyTip);
			}
		}
	}
}