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

		public bool IsTracking;
		public float TrackedForTime;

		public Vector3 Velocity;
		public Vector3 AngularVelocity;

		// hey remember, the test data's palm information has not been updated just so you know
		public static HandData GetTestData(bool isLeft)
		{
			if (isLeft)
			{
				return new HandData()
				{
					Handedness = Handedness.Left,
					ThumbJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.0158929f, 0.03720177f, 0.1296259f), new Quaternion(0.1024715f, 0.9808373f, 0.1491119f, -0.07227269f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.0158929f, 0.03720177f, 0.1296259f), new Quaternion(0.1024715f, 0.9808373f, 0.1491119f, -0.07227269f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01139905f, 0.04961932f, 0.09143889f), new Quaternion(0.01370804f, 0.991088f, 0.1022565f, -0.08426324f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.006059121f, 0.05628523f, 0.06006386f), new Quaternion(0.03301888f, 0.9415163f, 0.3197442f, -0.1010963f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					ThumbTip = new Vector3(0.000902975f, 0.07483073f, 0.03645181f),
					IndexJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.02901937f, 0.04476842f, 0.1355028f), new Quaternion(0.690018f, 0.6562133f, -0.1621175f, 0.2587995f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.04255126f, 0.002261709f, 0.07599282f), new Quaternion(0.6986173f, 0.6336015f, -0.2478836f, 0.2214423f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.0396634f, -0.02512968f, 0.04176969f), new Quaternion(0.6584769f, 0.6761305f, -0.2268985f, 0.2403594f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.04041553f, -0.04301763f, 0.01934458f), new Quaternion(0.6486021f, 0.6746782f, -0.2331988f, 0.2640889f)),
							Radius = 0f,
							Type = JointType.Metacarpal
						}
					},
					IndexTip = new Vector3(0.04164416f, -0.05801683f, 0.002188532f),
					MiddleJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03416368f, 0.03017576f, 0.1479382f), new Quaternion(0.5705046f, 0.7381278f, -0.2350368f, 0.2728544f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.04315938f, -0.01743053f, 0.09608619f), new Quaternion(0.6830919f, 0.5903111f, -0.2810158f, 0.3254972f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.04317523f, -0.05090264f, 0.0689208f), new Quaternion(0.693973f, 0.5891962f, -0.312798f, 0.2709368f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03935372f, -0.07567403f, 0.04704835f), new Quaternion(0.6673292f, 0.5984473f, -0.291596f, 0.3339227f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					MiddleTip = new Vector3(0.03962541f, -0.09625024f, 0.03133338f),
					RingJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03507797f, 0.02042013f, 0.1576069f), new Quaternion(0.5727981f, 0.7043664f, -0.2818755f, 0.3103492f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.04095789f, -0.02992787f, 0.1154904f), new Quaternion(0.6024527f, 0.6119634f, -0.3703441f, 0.3541141f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.04043607f, -0.06573886f, 0.0961637f), new Quaternion(0.6142027f, 0.5991966f, -0.3877887f, 0.3366577f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03834013f, -0.09098659f, 0.0825789f), new Quaternion(0.6227691f, 0.5782678f, -0.4192132f, 0.319414f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					RingTip = new Vector3(0.03491442f, -0.1107853f, 0.07260934f),
					PinkyJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.0345086f, 0.01220977f, 0.167464f), new Quaternion(0.6205293f, 0.6076928f, -0.2976757f, 0.3962854f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03872009f, -0.04178508f, 0.1353904f), new Quaternion(0.6356088f, 0.4884204f, -0.3959487f, 0.4479638f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03673316f, -0.07068235f, 0.1267747f), new Quaternion(0.6787023f, 0.4446436f, -0.4466219f, 0.3770732f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.031806f, -0.08721431f, 0.1210154f), new Quaternion(0.6258499f, 0.4546704f, -0.4466999f, 0.4494952f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					PinkyTip = new Vector3(0.02909627f, -0.1046707f, 0.1174689f),
					ForearmJoint = new Joint()
					{
						Pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f)),
						Radius = 0f,
						Type = JointType.Metacarpal
					},
					WristPose = new Pose(new Vector3(0.03403769f, 0.03650266f, 0.1647216f), new Quaternion(0.5951514f, 0.7063201f, -0.3071976f, 0.2292084f)),
					PalmPose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f))
				};
			}
			else
			{
				return new HandData()
				{
					Handedness = Handedness.Right,
					ThumbJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0158929f, 0.03720177f, 0.1296259f), new Quaternion(-0.1024715f, 0.9808373f, 0.149112f, 0.07227268f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0158929f, 0.03720177f, 0.1296259f), new Quaternion(-0.1024715f, 0.9808373f, 0.149112f, 0.07227268f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01139906f, 0.04961932f, 0.09143889f), new Quaternion(-0.01370804f, 0.991088f, 0.1022566f, 0.08426323f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.006059127f, 0.05628524f, 0.06006386f), new Quaternion(-0.03301888f, 0.9415163f, 0.3197443f, 0.1010964f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					ThumbTip = new Vector3(-0.000902975f, 0.07483073f, 0.03645182f),
					IndexJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.02901937f, 0.04476842f, 0.1355028f), new Quaternion(0.6900179f, -0.6562134f, 0.1621175f, 0.2587994f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.04255127f, 0.002261713f, 0.07599282f), new Quaternion(0.6986173f, -0.6336015f, 0.2478837f, 0.2214422f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0396634f, -0.02512967f, 0.04176969f), new Quaternion(-0.658477f, 0.6761306f, -0.2268986f, -0.2403593f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.04041553f, -0.04301761f, 0.01934458f), new Quaternion(-0.6486021f, 0.6746782f, -0.2331989f, -0.2640888f)),
							Radius = 0f,
							Type = JointType.Metacarpal
						}
					},
					IndexTip = new Vector3(-0.04164416f, -0.05801681f, 0.00218853f),
					MiddleJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03416368f, 0.03017576f, 0.1479382f), new Quaternion(-0.5705047f, 0.7381278f, -0.2350368f, -0.2728544f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.04315939f, -0.01743052f, 0.09608618f), new Quaternion(0.6830919f, -0.5903111f, 0.2810158f, 0.3254972f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.04317524f, -0.05090264f, 0.06892078f), new Quaternion(0.6939729f, -0.5891963f, 0.312798f, 0.2709368f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03935373f, -0.07567403f, 0.04704833f), new Quaternion(0.6673291f, -0.5984474f, 0.291596f, 0.3339227f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					MiddleTip = new Vector3(-0.03962542f, -0.09625024f, 0.03133336f),
					RingJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03507797f, 0.02042013f, 0.1576069f), new Quaternion(-0.5727981f, 0.7043664f, -0.2818755f, -0.3103492f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.04095789f, -0.02992786f, 0.1154904f), new Quaternion(-0.6024528f, 0.6119634f, -0.3703442f, -0.3541141f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.04043606f, -0.06573886f, 0.0961637f), new Quaternion(0.6142027f, -0.5991967f, 0.3877887f, 0.3366576f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03834012f, -0.09098659f, 0.0825789f), new Quaternion(0.6227692f, -0.5782679f, 0.4192131f, 0.3194141f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					RingTip = new Vector3(-0.03491441f, -0.1107853f, 0.07260934f),
					PinkyJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0345086f, 0.01220977f, 0.167464f), new Quaternion(0.6205293f, -0.6076928f, 0.2976756f, 0.3962854f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03872009f, -0.04178508f, 0.1353904f), new Quaternion(0.6356087f, -0.4884205f, 0.3959487f, 0.4479638f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03673317f, -0.07068235f, 0.1267747f), new Quaternion(0.6787022f, -0.4446437f, 0.4466219f, 0.3770732f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03180601f, -0.08721431f, 0.1210154f), new Quaternion(0.6258498f, -0.4546705f, 0.4466999f, 0.4494952f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					PinkyTip = new Vector3(-0.02909628f, -0.1046707f, 0.1174689f),
					ForearmJoint = new Joint()
					{
						Pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f)),
						Radius = 0f,
						Type = JointType.Metacarpal
					},
					WristPose = new Pose(new Vector3(-0.03403769f, 0.03650266f, 0.1647216f), new Quaternion(-0.5951514f, 0.7063201f, -0.3071976f, -0.2292084f)),
					PalmPose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f))
				};
			}
		}

		public string GetJointTypeSuffixForType(JointType type)
		{
			switch (type)
			{
				case JointType.Unknown:
					return "";
				case JointType.Metacarpal:
					return "Metacarpal";
				case JointType.Proximal:
					return "Proximal";
				case JointType.Medial:
					return "Medial";
				case JointType.Distal:
					return "Distal";
				case JointType.Forearm:
					return "Forearm";
				default:
					return "";
			}
		}

		public string PrintInitCode()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

			stringBuilder.AppendLine("return new HandData()");
			stringBuilder.AppendLine("{");
			stringBuilder.AppendLine(string.Format("	Handedness = Handedness.{0},", (Handedness == Handedness.Left) ? "Left" : "Right"));
			stringBuilder.AppendLine("	ThumbJoints = new Joint[]");
			stringBuilder.AppendLine("	{");
			for (int i = 0; i < ThumbJoints.Length; i++)
			{
				stringBuilder.AppendLine("		new Joint()");
				stringBuilder.AppendLine("		{");
				stringBuilder.AppendLine(string.Format("			Pose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f)),",
					ThumbJoints[i].Pose.position.x, ThumbJoints[i].Pose.position.y, ThumbJoints[i].Pose.position.z,
					ThumbJoints[i].Pose.rotation.x, ThumbJoints[i].Pose.rotation.y, ThumbJoints[i].Pose.rotation.z, ThumbJoints[i].Pose.rotation.w));
				stringBuilder.AppendLine(string.Format("			Radius = {0}f,", ThumbJoints[i].Radius));
				stringBuilder.AppendLine(string.Format("			Type = JointType.{0}", GetJointTypeSuffixForType(ThumbJoints[i].Type)));
				stringBuilder.AppendLine("		}" + ((i != ThumbJoints.Length - 1) ? "," : ""));
			}
			stringBuilder.AppendLine("	},");
			stringBuilder.AppendLine(string.Format("	ThumbTip = new Vector3({0}f, {1}f, {2}f),",
				ThumbTip.x, ThumbTip.y, ThumbTip.z));

			stringBuilder.AppendLine("	IndexJoints = new Joint[]");
			stringBuilder.AppendLine("	{");
			for (int i = 0; i < IndexJoints.Length; i++)
			{
				stringBuilder.AppendLine("		new Joint()");
				stringBuilder.AppendLine("		{");
				stringBuilder.AppendLine(string.Format("			Pose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f)),",
					IndexJoints[i].Pose.position.x, IndexJoints[i].Pose.position.y, IndexJoints[i].Pose.position.z,
					IndexJoints[i].Pose.rotation.x, IndexJoints[i].Pose.rotation.y, IndexJoints[i].Pose.rotation.z, IndexJoints[i].Pose.rotation.w));
				stringBuilder.AppendLine(string.Format("			Radius = {0}f,", IndexJoints[i].Radius));
				stringBuilder.AppendLine(string.Format("			Type = JointType.{0}", GetJointTypeSuffixForType(IndexJoints[i].Type)));
				stringBuilder.AppendLine("		}" + ((i != IndexJoints.Length - 1) ? "," : ""));
			}
			stringBuilder.AppendLine("	},");
			stringBuilder.AppendLine(string.Format("	IndexTip = new Vector3({0}f, {1}f, {2}f),",
				IndexTip.x, IndexTip.y, IndexTip.z));

			stringBuilder.AppendLine("	MiddleJoints = new Joint[]");
			stringBuilder.AppendLine("	{");
			for (int i = 0; i < MiddleJoints.Length; i++)
			{
				stringBuilder.AppendLine("		new Joint()");
				stringBuilder.AppendLine("		{");
				stringBuilder.AppendLine(string.Format("			Pose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f)),",
					MiddleJoints[i].Pose.position.x, MiddleJoints[i].Pose.position.y, MiddleJoints[i].Pose.position.z,
					MiddleJoints[i].Pose.rotation.x, MiddleJoints[i].Pose.rotation.y, MiddleJoints[i].Pose.rotation.z, MiddleJoints[i].Pose.rotation.w));
				stringBuilder.AppendLine(string.Format("			Radius = {0}f,", MiddleJoints[i].Radius));
				stringBuilder.AppendLine(string.Format("			Type = JointType.{0}", GetJointTypeSuffixForType(MiddleJoints[i].Type)));
				stringBuilder.AppendLine("		}" + ((i != MiddleJoints.Length - 1) ? "," : ""));
			}
			stringBuilder.AppendLine("	},");
			stringBuilder.AppendLine(string.Format("	MiddleTip = new Vector3({0}f, {1}f, {2}f),",
				MiddleTip.x, MiddleTip.y, MiddleTip.z));

			stringBuilder.AppendLine("	RingJoints = new Joint[]");
			stringBuilder.AppendLine("	{");
			for (int i = 0; i < RingJoints.Length; i++)
			{
				stringBuilder.AppendLine("		new Joint()");
				stringBuilder.AppendLine("		{");
				stringBuilder.AppendLine(string.Format("			Pose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f)),",
					RingJoints[i].Pose.position.x, RingJoints[i].Pose.position.y, RingJoints[i].Pose.position.z,
					RingJoints[i].Pose.rotation.x, RingJoints[i].Pose.rotation.y, RingJoints[i].Pose.rotation.z, RingJoints[i].Pose.rotation.w));
				stringBuilder.AppendLine(string.Format("			Radius = {0}f,", RingJoints[i].Radius));
				stringBuilder.AppendLine(string.Format("			Type = JointType.{0}", GetJointTypeSuffixForType(RingJoints[i].Type)));
				stringBuilder.AppendLine("		}" + ((i != RingJoints.Length - 1) ? "," : ""));
			}
			stringBuilder.AppendLine("	},");
			stringBuilder.AppendLine(string.Format("	RingTip = new Vector3({0}f, {1}f, {2}f),",
				RingTip.x, RingTip.y, RingTip.z));

			stringBuilder.AppendLine("	PinkyJoints = new Joint[]");
			stringBuilder.AppendLine("	{");
			for (int i = 0; i < PinkyJoints.Length; i++)
			{
				stringBuilder.AppendLine("		new Joint()");
				stringBuilder.AppendLine("		{");
				stringBuilder.AppendLine(string.Format("			Pose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f)),",
					PinkyJoints[i].Pose.position.x, PinkyJoints[i].Pose.position.y, PinkyJoints[i].Pose.position.z,
					PinkyJoints[i].Pose.rotation.x, PinkyJoints[i].Pose.rotation.y, PinkyJoints[i].Pose.rotation.z, PinkyJoints[i].Pose.rotation.w));
				stringBuilder.AppendLine(string.Format("			Radius = {0}f,", PinkyJoints[i].Radius));
				stringBuilder.AppendLine(string.Format("			Type = JointType.{0}", GetJointTypeSuffixForType(PinkyJoints[i].Type)));
				stringBuilder.AppendLine("		}" + ((i != PinkyJoints.Length - 1) ? "," : ""));
			}
			stringBuilder.AppendLine("	},");
			stringBuilder.AppendLine(string.Format("	PinkyTip = new Vector3({0}f, {1}f, {2}f),",
				PinkyTip.x, PinkyTip.y, PinkyTip.z));

			stringBuilder.AppendLine("	ForearmJoint = new Joint()");
			stringBuilder.AppendLine("	{");
			stringBuilder.AppendLine(string.Format("		Pose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f)),",
				ForearmJoint.Pose.position.x, ForearmJoint.Pose.position.y, ForearmJoint.Pose.position.z,
				ForearmJoint.Pose.rotation.x, ForearmJoint.Pose.rotation.y, ForearmJoint.Pose.rotation.z, ForearmJoint.Pose.rotation.w));
			stringBuilder.AppendLine(string.Format("		Radius = {0}f,", ForearmJoint.Radius));
			stringBuilder.AppendLine(string.Format("		Type = JointType.{0}", GetJointTypeSuffixForType(ForearmJoint.Type)));
			stringBuilder.AppendLine("	},");
			stringBuilder.AppendLine(string.Format("	WristPose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f)),",
				WristPose.position.x, WristPose.position.y, WristPose.position.z,
				WristPose.rotation.x, WristPose.rotation.y, WristPose.rotation.z, WristPose.rotation.w));
			stringBuilder.AppendLine(string.Format("	PalmPose = new Pose(new Vector3({0}f, {1}f, {2}f), new Quaternion({3}f, {4}f, {5}f, {6}f))",
				PalmPose.position.x, PalmPose.position.y, PalmPose.position.z,
				PalmPose.rotation.x, PalmPose.rotation.y, PalmPose.rotation.z, PalmPose.rotation.w));
			stringBuilder.AppendLine("};");
			return stringBuilder.ToString();
		}
	}
}