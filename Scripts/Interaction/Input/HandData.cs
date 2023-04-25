using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Input
{
	public enum JointType
	{
		Unknown=-1,
		Metacarpal=0,
		Proximal=1,
		Medial=2,
		Distal=3,
		Forearm
	}

	[System.Serializable]
	public struct Joint
	{
		public JointType Type;
		public Pose Pose;
		public float Radius;
	}

	[System.Serializable]
	public struct HandData
	{
		// handedness
		public Handedness Handedness;

		// joint arrays for fingers
		// thumb
		public Joint[] ThumbJoints;
		public Vector3 ThumbTip;

		// index
		public Joint[] IndexJoints;
		public Vector3 IndexTip;

		// middle
		public Joint[] MiddleJoints;
		public Vector3 MiddleTip;

		// ring
		public Joint[] RingJoints;
		public Vector3 RingTip;

		// pinky
		public Joint[] PinkyJoints;
		public Vector3 PinkyTip;

		public Joint[] GetJointForFinger(Finger finger)
		{
			switch (finger)
			{
				case Finger.Thumb:
					return ThumbJoints;

				case Finger.Index:
					return IndexJoints;

				case Finger.Middle:
					return MiddleJoints;

				case Finger.Ring:
					return RingJoints;

				case Finger.Pinky:
					return PinkyJoints;

				default:
					return new Joint[0];
			}
		}

		public Vector3 GetFingertip(Finger finger)
		{
			switch (finger)
			{
				case Finger.Thumb:
					return ThumbTip;
				case Finger.Index:
					return IndexTip;
				case Finger.Middle:
					return MiddleTip;
				case Finger.Ring:
					return RingTip;
				case Finger.Pinky:
					return PinkyTip;
				default:
					return Vector3.zero;
			}
		}

		// arm joint
		public bool HasForearm;
		public Joint ForearmJoint;

		// wrist
		public Pose WristPose;

		// palm
		public Pose PalmPose;
	}
}