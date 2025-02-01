using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using Instrumental.Interaction.Input;
using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Interaction
{
	[System.Serializable]
    public enum Handedness { None=0, Left = 1, Right = 2 }
	[System.Serializable]
    public enum AnchorPoint 
    { 
        None=0,
        Palm=1,
		ThumbTip=2,
        IndexTip=3,
        MiddleTip=4,
		RingTip=5,
		PinkyTip=6
	}

	public enum Finger
	{ 
		Thumb=0,
		Index=1,
		Middle=2,
		Ring=3,
		Pinky=4
	}

	public struct PinchInfo
	{
		public Vect3 PinchCenter;
		public float PinchDistance;
		public float PinchAmount;
	}

	public class InstrumentalHand : MonoBehaviour
	{
		InstrumentalBody body;

		[SerializeField] GameObject[] handAvatars;
		[SerializeField] HandDataContainer dataHand;
		[SerializeField] private Handedness hand;
		public Handedness Hand { get { return hand; } }

		const float basisDrawDist = 0.02f;

		#region finger extension and curls
		PoseIC palmPose;
		PoseIC thumbPose;
		PoseIC indexPose;
		PoseIC middlePose;
		PoseIC ringPose;
		PoseIC pinkyPose;

		bool thumbIsExtended = false;
		bool indexIsExtended = false;
		bool middleIsExtended = false;
		bool ringIsExtended = false;
		bool pinkyIsExtended = false;

		float thumbCurl = 0;
		float indexCurl = 0;
		float middleCurl = 0;
		float ringCurl = 0;
		float pinkyCurl = 0;
		#endregion

		#region Pinch stuff
		PinchInfo[] pinches;

		[Range(0,0.1f)]
		[SerializeField] float pinchMaxDistance=0.09f;
		[Range(0,0.1f)]
		[SerializeField] float pinchMinDistance = 0.01f;
		#endregion

		PinchInfo graspPinch;

		public PinchInfo GraspPinch { get { return graspPinch; } }

		#region Finger Extension and curl accessors
		public bool ThumbIsExtended { get { return thumbIsExtended; } }
		public bool IndexIsExtended { get { return indexIsExtended; } }
		public bool MiddleIsExtended { get { return middleIsExtended; } }
		public bool RingIsExtended { get { return ringIsExtended; } }
		public bool PinkyIsExtended { get { return pinkyIsExtended; } }

		public float ThumbCurl { get { return thumbCurl; } }
		public float IndexCurl { get { return indexCurl; } }
		public float MiddleCurl { get { return middleCurl; } }

		public float RingCurl { get { return ringCurl; } }

		public float PinkyCurl { get { return pinkyCurl; } }
		#endregion

		#region Pinch Accessors
		public bool HasPinchInfo(Finger finger)
		{
			return (finger != Finger.Thumb);
		}

		public PinchInfo GetPinchInfo(Finger finger)
		{
			if(finger != Finger.Thumb)
			{
				return pinches[(int)finger];
			}
			else
			{
				Debug.LogError("No pinch for thumb");
				return new PinchInfo();
			}
		}

		/// <summary>
		/// The distance used for a 'fully open' pinch gesture, useful for
		/// pinch based visualizations. Does not tell you that the pinch is active
		/// </summary>
		public float PinchMaxDistance { get { return pinchMaxDistance; } }
		public float PinchMinDistance { get { return pinchMinDistance; } }
		#endregion

		public HandData GetHandData()
		{
			return dataHand.Data;
		}

		const float curlCutoff = 0.3f;
		const float thumbCurlCutoff = 0.71f;

		public bool IsTracking { get { return dataHand.Data.IsTracking; } }

		private static InstrumentalHand leftHand;
		private static InstrumentalHand rightHand;

		public static InstrumentalHand LeftHand { get { return leftHand; } }
		public static InstrumentalHand RightHand { get { return rightHand; } }
		public InstrumentalBody Body { get { return body; } }

		public Vect3 Velocity { get { return dataHand.Data.Velocity; } }
		public Vect3 AngularVelocity { get { return dataHand.Data.AngularVelocity; } }

		private void Awake()
		{
			body = GetComponentInParent<InstrumentalBody>();
			if (hand == Handedness.Left) leftHand = this;
			else if (hand == Handedness.Right) rightHand = this;

			pinches = new PinchInfo[5];
		}

		// Start is called before the first frame update
		void Start()
        {
			UpdateHandAvatars();
        }

		void UpdateHandAvatars()
		{
			for (int i = 0; i < handAvatars.Length; i++)
			{
				handAvatars[i].gameObject.SetActive(i == (int)body.Avatar);
			}
		}

		void GetAnchorPoses()
		{
			palmPose = GetAnchorPose(AnchorPoint.Palm);
			thumbPose = GetAnchorPose(AnchorPoint.ThumbTip);
			indexPose = GetAnchorPose(AnchorPoint.IndexTip);
			middlePose = GetAnchorPose(AnchorPoint.MiddleTip);
			ringPose = GetAnchorPose(AnchorPoint.RingTip);
			pinkyPose = GetAnchorPose(AnchorPoint.PinkyTip);
		}

		public PoseIC GetAnchorPose(AnchorPoint anchorPoint)
		{
			switch (anchorPoint)
			{
				case AnchorPoint.None:
					return PoseIC.identity;

				case AnchorPoint.Palm:
					return dataHand.Data.PalmPose;

				case AnchorPoint.IndexTip:
					return new PoseIC(dataHand.Data.IndexTip,
						dataHand.Data.IndexJoints[3].Pose.rotation);

				case AnchorPoint.MiddleTip:
					return new PoseIC(dataHand.Data.MiddleTip,
						dataHand.Data.MiddleJoints[3].Pose.rotation);

				case AnchorPoint.ThumbTip:
					return new PoseIC(dataHand.Data.ThumbTip,
						dataHand.Data.ThumbJoints[3].Pose.rotation);

				case AnchorPoint.RingTip:
					return new PoseIC(dataHand.Data.RingTip,
						dataHand.Data.RingJoints[3].Pose.rotation);

				case AnchorPoint.PinkyTip:
					return new PoseIC(dataHand.Data.PinkyTip,
						dataHand.Data.PinkyJoints[3].Pose.rotation);

				default:
					return new PoseIC(Vect3.zero, Quatn.identity);
			}
		}

		float GetFingerAngle(Vect3 baseDirection,
			Vect3 forward, Vect3 axis)
		{
			float signedAngle = Vect3.SignedAngle(baseDirection, forward, axis);

			if (signedAngle < 0 && signedAngle > -60) signedAngle = 0;
			else if (signedAngle < 0 /*&& signedAngle > -130*/)
			{
				float absExtra = Mathf.Abs(signedAngle);
				signedAngle = 180 + absExtra;
			}

			return Mathf.Clamp(signedAngle, 0, 340);
		}

		float GetFingerCurl(float angle)
		{
			return angle / 340;
		}

		float GetThumbCurl(float angle)
		{
			if (angle > 3)
				angle = 360.0f - angle;

			return angle / 90.0f;
		}

		void CalculateExtension()
		{
			Vect3 palmDirection = palmPose.rotation * Vect3.up;
			Vect3 palmThumbRef = palmPose.rotation * Vect3.right;

			Vect3 thumbForward = thumbPose.rotation * Vect3.forward;
			Vect3 indexForward = indexPose.rotation * Vect3.forward;
			Vect3 middleForward = middlePose.rotation * Vect3.forward;
			Vect3 ringForward = ringPose.rotation * Vect3.forward;
			Vect3 pinkyForward = pinkyPose.rotation * Vect3.forward;

			PoseIC thumbMedialPose = dataHand.Data.ThumbJoints[dataHand.Data.ThumbJoints.Length - 2].Pose;
			Quatn thumbInverse = thumbPose.rotation * Quatn.Inverse(thumbMedialPose.rotation);
			Vect3 thumbEuler = thumbInverse.eulerAngles;

			thumbCurl = Vect3.Dot(thumbForward, palmThumbRef);
			if (hand == Handedness.Right) thumbCurl *= -1;
			thumbCurl = 1 - Mathf.InverseLerp(-0.8f, 0.8f, thumbCurl);

			//GetThumbCurl(thumbEuler.x); //GetFingerCurl(GetFingerAngle(palmThumbRef, thumbForward, palmDirection)); // old method
			//Debug.Log("Thumb " + ((hand == Handedness.Left) ? "L" : "R") + " curl: " + thumbCurl /*thumbEuler.x*/);

			indexCurl = GetFingerCurl(GetFingerAngle(palmDirection, indexForward, palmThumbRef));
			middleCurl = GetFingerCurl(GetFingerAngle(palmDirection, middleForward, palmThumbRef));
			ringCurl = GetFingerCurl(GetFingerAngle(palmDirection, ringForward, palmThumbRef));
			pinkyCurl = GetFingerCurl(GetFingerAngle(palmDirection, pinkyForward, palmThumbRef));

			indexIsExtended = (indexCurl < curlCutoff);
			middleIsExtended = (middleCurl < curlCutoff);
			ringIsExtended = (ringCurl < curlCutoff);
			pinkyIsExtended = (pinkyCurl < curlCutoff);
			thumbIsExtended = (thumbCurl < thumbCurlCutoff); // it does look like the thumb is not extended when it's lower than 0.4 to 0.2 or so
		}

        // Update is called once per frame
        void Update()
        {
			UpdateHandAvatars();
			GetAnchorPoses();
			CalculateExtension();
			ProcessPinches();
		}

		void ProcessPinches()
		{
			pinches[(int)Finger.Index] = ProcessPinch(Finger.Index);
			pinches[(int)Finger.Middle] = ProcessPinch(Finger.Middle);
			pinches[(int)Finger.Ring] = ProcessPinch(Finger.Ring);
			pinches[(int)Finger.Pinky] = ProcessPinch(Finger.Pinky);

			graspPinch = ProcessGraspPinch();
		}

		PinchInfo ProcessPinch(Finger finger)
		{
			Vect3 thumbTip, fingerTip = Vect3.zero;
			Vect3 pinchCenter = Vect3.zero;
			float pinchDistance = 0, pinchAmount = 0;
			thumbTip = thumbPose.position;

			switch (finger)
			{
				case Finger.Thumb:
					break;
				case Finger.Index:
					fingerTip = indexPose.position;
					break;
				case Finger.Middle:
					fingerTip = middlePose.position;
					break;
				case Finger.Ring:
					fingerTip = ringPose.position;
					break;
				case Finger.Pinky:
					fingerTip = pinkyPose.position;
					break;
				default:
					break;
			}

			pinchCenter = (fingerTip + thumbTip) * 0.5f;

			Vect3 offset = (fingerTip - thumbTip);
			pinchDistance = offset.magnitude;
			pinchAmount = 1 - Mathf.InverseLerp(pinchMinDistance, pinchMaxDistance, pinchDistance); // using mathf for now because unity's is clamped

			return new PinchInfo()
			{
				PinchAmount = pinchAmount,
				PinchDistance = pinchDistance,
				PinchCenter = pinchCenter
			};
		}

		PinchInfo ProcessGraspPinch()
		{
			PinchInfo indexPinch, middlePinch;
			indexPinch = GetPinchInfo(Finger.Index);
			middlePinch = GetPinchInfo(Finger.Middle);

			float indexAmount = indexPinch.PinchAmount, middleAmount = middlePinch.PinchAmount;
			float blendValue = 1 - Mathf.Abs(indexAmount - middleAmount);
			if(blendValue < 0.5f)
			{
				blendValue *= 2;
			}
			else
			{
				blendValue = 1 - (blendValue - 0.5f) * 2;
			}

			return new PinchInfo()
			{
				PinchAmount = Mathf.Max(indexPinch.PinchAmount, middlePinch.PinchAmount),
				PinchCenter = Vect3.Lerp(indexPinch.PinchCenter, middlePinch.PinchCenter, blendValue),
				PinchDistance = Mathf.Min(indexPinch.PinchDistance, middlePinch.PinchDistance)
			};
		}


		void DrawBasis(PoseIC pose)
		{
			Vect3 up, forward, right;
			up = pose.rotation * Vect3.up;
			forward = pose.rotation * Vect3.forward;
			right = pose.rotation * Vect3.right;

			Gizmos.color = Color.yellow;
			Vector3 position = (Vector3)pose.position;
			Gizmos.DrawLine(position, (Vector3)(pose.position + 
				(up * basisDrawDist)));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(position, (Vector3)(pose.position + 
				(forward * basisDrawDist)));
			Gizmos.color = Color.red;
			Gizmos.DrawLine(position, (Vector3)(pose.position + 
				(right * basisDrawDist)));
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				// draw the palm
				DrawBasis(GetAnchorPose(AnchorPoint.Palm));

				// draw the index finger
				DrawBasis(GetAnchorPose(AnchorPoint.IndexTip));

				// draw middle finger
				DrawBasis(GetAnchorPose(AnchorPoint.MiddleTip));

				// draw the thumb tip
				DrawBasis(GetAnchorPose(AnchorPoint.ThumbTip));
			}
		}
	}
}