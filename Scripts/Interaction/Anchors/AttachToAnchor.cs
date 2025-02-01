using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;

namespace Instrumental.Interaction
{
    public class AttachToAnchor : MonoBehaviour
    {
        [SerializeField] Handedness handToAttach;
        [SerializeField] AnchorPoint pointToAttach;

		private void Update()
		{
			DoAttach();
		}

		private void LateUpdate()
		{
			DoAttach();
		}

		void DoAttach()
		{
			if (handToAttach != Handedness.None)
			{
				InstrumentalHand hand = (handToAttach == Handedness.Left) ?
					InstrumentalHand.LeftHand : InstrumentalHand.RightHand;

				PoseIC attachPose = hand.GetAnchorPose(pointToAttach);
				transform.SetPositionAndRotation((Vector3)attachPose.position, (Quaternion)attachPose.rotation);
			}
		}
	}
}