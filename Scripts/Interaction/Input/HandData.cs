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
							Pose = new Pose(new Vector3(-0.01775401f, 0.02914611f, -0.02467261f), new Quaternion(0.7730358f, -0.2763868f, 0.5411944f, -0.1820292f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01775401f, 0.02914611f, -0.02467261f), new Quaternion(0.7730358f, -0.2763868f, 0.5411944f, -0.1820292f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0283177f, 0.05437329f, -0.05441574f), new Quaternion(-0.8165473f, 0.07762473f, -0.5704363f, 0.04274769f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03928087f, 0.06008123f, -0.08449184f), new Quaternion(-0.8195889f, -0.04861432f, -0.5691067f, -0.04503723f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					ThumbTip = new Vector3(-0.04986737f, 0.05609199f, -0.1127773f),
					IndexJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.001397458f, 0.02104131f, -0.01416126f), new Quaternion(-0.5395777f, 0.5507531f, -0.5310564f, -0.3514342f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01119977f, 0.03735687f, -0.08502162f), new Quaternion(0.6282791f, -0.3839648f, 0.4591768f, 0.4969838f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.001070035f, 0.03848523f, -0.1265175f), new Quaternion(0.6906685f, -0.2638121f, 0.3562412f, 0.5713776f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01823283f, 0.03727835f, -0.1489556f), new Quaternion(0.7084194f, -0.2095554f, 0.3123255f, 0.597228f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					IndexTip = new Vector3(-0.03459767f, 0.03554043f, -0.1647668f),
					MiddleJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002337092f, 0.007087619f, -0.01569313f), new Quaternion(-0.4729876f, 0.5334234f, -0.5617498f, -0.4197373f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01652884f, 0.009429009f, -0.08510402f), new Quaternion(0.5825371f, -0.410759f, 0.4507002f, 0.5373983f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.005480433f, 0.009176982f, -0.1267719f), new Quaternion(0.6383116f, -0.3119235f, 0.3626821f, 0.6030952f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0125607f, 0.007871132f, -0.1546903f), new Quaternion(0.6774029f, -0.2211408f, 0.2711714f, 0.6470613f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					MiddleTip = new Vector3(-0.03211264f, 0.006542179f, -0.1716127f),
					RingJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.00067321f, -0.006577051f, -0.01572195f), new Quaternion(-0.4298879f, 0.5166922f, -0.5501435f, -0.4955479f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.009860715f, -0.01324088f, -0.08071335f), new Quaternion(0.5098341f, -0.389011f, 0.4440832f, 0.6257234f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.002356464f, -0.01965694f, -0.1186104f), new Quaternion(0.5583858f, -0.3188958f, 0.3550274f, 0.6785767f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01786924f, -0.02323778f, -0.1422354f), new Quaternion(0.5950274f, -0.2374145f, 0.262352f, 0.7216289f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					RingTip = new Vector3(-0.03468313f, -0.02539343f, -0.1569241f),
					PinkyJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.00231837f, -0.0190133f, -0.01458793f), new Quaternion(-0.3467464f, 0.4857576f, -0.5153269f, -0.6150161f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.002127017f, -0.03768138f, -0.07460708f), new Quaternion(0.4150102f, -0.4596366f, 0.4362805f, 0.6528093f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.008005896f, -0.04330099f, -0.1033531f), new Quaternion(0.4674865f, -0.3929082f, 0.3508216f, 0.7099322f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01600804f, -0.04565187f, -0.11928f), new Quaternion(0.5114209f, -0.3489999f, 0.2654839f, 0.7390305f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					PinkyTip = new Vector3(-0.0270969f, -0.04629024f, -0.1334672f),
					ForearmJoint = new Joint()
					{
						Pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f)),
						Radius = 0f,
						Type = JointType.Forearm
					},
					WristPose = new Pose(new Vector3(0.0001597875f, -3.192438E-05f, 0.0006257091f), new Quaternion(-6.123234E-17f, -1f, 6.123234E-17f, 3.749399E-33f)),
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
							Pose = new Pose(new Vector3(0.017754f, 0.02914625f, -0.02467269f), new Quaternion(-0.1820292f, -0.5411944f, -0.2763868f, -0.7730358f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.017754f, 0.02914625f, -0.02467269f), new Quaternion(-0.1820292f, -0.5411944f, -0.2763868f, -0.7730358f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.0283177f, 0.05437328f, -0.05441571f), new Quaternion(0.04274769f, 0.5704363f, 0.07762472f, 0.8165473f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03928085f, 0.06008116f, -0.0844918f), new Quaternion(-0.04503724f, 0.5691067f, -0.04861432f, 0.8195889f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					ThumbTip = new Vector3(0.0498674f, 0.05609209f, -0.1127772f),
					IndexJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.001397436f, 0.02104131f, -0.01416115f), new Quaternion(0.3514342f, -0.5310564f, -0.5507531f, -0.5395777f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0111998f, 0.0373568f, -0.08502159f), new Quaternion(-0.4969838f, 0.4591768f, 0.3839648f, 0.6282791f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.001070029f, 0.03848532f, -0.1265175f), new Quaternion(-0.5713777f, 0.3562411f, 0.2638121f, 0.6906685f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01823281f, 0.03727832f, -0.1489557f), new Quaternion(-0.5972281f, 0.3123255f, 0.2095554f, 0.7084194f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					IndexTip = new Vector3(0.03459765f, 0.03554045f, -0.1647667f),
					MiddleJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.002337101f, 0.00708762f, -0.01569303f), new Quaternion(0.4197373f, -0.5617498f, -0.5334234f, -0.4729876f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01652897f, 0.009429025f, -0.08510397f), new Quaternion(-0.5373983f, 0.4507002f, 0.410759f, 0.5825371f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.005480452f, 0.009177009f, -0.1267718f), new Quaternion(-0.6030952f, 0.3626821f, 0.3119235f, 0.6383116f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01256069f, 0.007871144f, -0.1546902f), new Quaternion(-0.6470613f, 0.2711714f, 0.2211408f, 0.6774029f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					MiddleTip = new Vector3(0.03211262f, 0.00654219f, -0.1716127f),
					RingJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0006732229f, -0.006577047f, -0.01572198f), new Quaternion(0.4955479f, -0.5501435f, -0.5166922f, -0.4298879f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.009860706f, -0.01324086f, -0.08071329f), new Quaternion(-0.6257234f, 0.4440832f, 0.389011f, 0.5098341f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002356464f, -0.01965693f, -0.1186103f), new Quaternion(-0.6785768f, 0.3550274f, 0.3188958f, 0.5583858f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01786923f, -0.02323769f, -0.1422353f), new Quaternion(-0.721629f, 0.262352f, 0.2374145f, 0.5950274f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					RingTip = new Vector3(0.03468312f, -0.02539339f, -0.1569241f),
					PinkyJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002318364f, -0.01901329f, -0.01458787f), new Quaternion(0.6150161f, -0.5153269f, -0.4857576f, -0.3467464f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002126995f, -0.0376814f, -0.07460703f), new Quaternion(-0.6528093f, 0.4362805f, 0.4596366f, 0.4150102f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.008005876f, -0.0433009f, -0.103353f), new Quaternion(-0.7099322f, 0.3508216f, 0.3929082f, 0.4674865f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01600803f, -0.04565193f, -0.1192801f), new Quaternion(-0.7390305f, 0.2654839f, 0.3489998f, 0.5114209f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					PinkyTip = new Vector3(0.0270969f, -0.04629023f, -0.1334672f),
					ForearmJoint = new Joint()
					{
						Pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f)),
						Radius = 0f,
						Type = JointType.Forearm
					},
					WristPose = new Pose(new Vector3(-0.0001597873f, -3.19245E-05f, 0.0006257087f), new Quaternion(-6.123234E-17f, -1f, 6.123234E-17f, 3.749399E-33f)),
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