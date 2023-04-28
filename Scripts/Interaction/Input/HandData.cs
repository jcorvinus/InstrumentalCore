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
							Pose = new Pose(new Vector3(-0.01775401f, 0.02914611f, -0.02467261f), new Quaternion(0.06672087f, 0.9293011f, 0.3241491f, -0.1639366f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01775401f, 0.02914611f, -0.02467261f), new Quaternion(0.06672087f, 0.9293011f, 0.3241491f, -0.1639366f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0283177f, 0.05437329f, -0.05441574f), new Quaternion(0.0246618f, 0.9807455f, 0.08511616f, -0.1740267f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.03928087f, 0.06008123f, -0.08449184f), new Quaternion(-0.002529389f, 0.9819561f, -0.06622165f, -0.1771176f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					ThumbTip = new Vector3(-0.04986737f, 0.05609199f, -0.1127773f),
					IndexJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.001397458f, 0.02104131f, -0.01416126f), new Quaternion(0.6379428f, 0.7570527f, 0.1409398f, -0.006025487f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01119977f, 0.03735687f, -0.08502162f), new Quaternion(0.6229248f, 0.7689475f, -0.07991651f, -0.1195734f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.001070035f, 0.03848523f, -0.1265175f), new Quaternion(0.5905683f, 0.740277f, -0.2174817f, -0.2364758f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01823283f, 0.03727835f, -0.1489556f), new Quaternion(0.570482f, 0.7217757f, -0.2741258f, -0.2800807f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					IndexTip = new Vector3(-0.03459767f, 0.03554043f, -0.1647668f),
					MiddleJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002337092f, 0.007087619f, -0.01569313f), new Quaternion(0.6739864f, 0.7316698f, 0.08038815f, 0.06276429f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01652884f, 0.009429009f, -0.08510402f), new Quaternion(0.6704485f, 0.7306091f, -0.0895475f, -0.09322274f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.005480433f, 0.009176982f, -0.1267719f), new Quaternion(0.6470159f, 0.7078094f, -0.2058894f, -0.1948995f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0125607f, 0.007871132f, -0.1546903f), new Quaternion(0.6139116f, 0.6707433f, -0.3011712f, -0.287249f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					MiddleTip = new Vector3(-0.03211264f, 0.006542179f, -0.1716127f),
					RingJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.00067321f, -0.006577051f, -0.01572195f), new Quaternion(0.7157619f, 0.6929868f, 0.0149513f, 0.08503354f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.009860715f, -0.01324088f, -0.08071335f), new Quaternion(0.7175256f, 0.6745214f, -0.1673809f, -0.04649288f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.002356464f, -0.01965694f, -0.1186104f), new Quaternion(0.7053196f, 0.6458806f, -0.2543328f, -0.1437961f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01786924f, -0.02323778f, -0.1422354f), new Quaternion(0.6781461f, 0.6062587f, -0.3423913f, -0.235237f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					RingTip = new Vector3(-0.03468313f, -0.02539343f, -0.1569241f),
					PinkyJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.00231837f, -0.0190133f, -0.01458793f), new Quaternion(0.7783645f, 0.6095779f, -0.09139961f, 0.1192044f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.002127017f, -0.03768138f, -0.07460708f), new Quaternion(0.7866181f, 0.6019535f, -0.1365937f, 0.01504038f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.008005896f, -0.04330099f, -0.1033531f), new Quaternion(0.7798259f, 0.5786312f, -0.2241698f, -0.08249448f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01600804f, -0.04565187f, -0.11928f), new Quaternion(0.7693536f, 0.5493547f, -0.2757933f, -0.1739037f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					PinkyTip = new Vector3(-0.0270969f, -0.04629024f, -0.1334672f),
					ForearmJoint = new Joint()
					{
						Pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f)),
						Radius = 0f,
						Type = JointType.Metacarpal
					},
					WristPose = new Pose(new Vector3(0.0001597875f, -3.192438E-05f, 0.0006257091f), new Quaternion(0.7071068f, 0f, 0.7071068f, 8.659561E-17f)),
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
							Pose = new Pose(new Vector3(0.017754f, 0.02914625f, -0.02467269f), new Quaternion(-0.06672087f, 0.9293011f, 0.3241491f, 0.1639366f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.017754f, 0.02914625f, -0.02467269f), new Quaternion(-0.06672087f, 0.9293011f, 0.3241491f, 0.1639366f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.0283177f, 0.05437328f, -0.05441571f), new Quaternion(-0.02466179f, 0.9807455f, 0.08511616f, 0.1740267f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.03928085f, 0.06008116f, -0.0844918f), new Quaternion(0.002529386f, 0.9819561f, -0.06622165f, 0.1771176f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					ThumbTip = new Vector3(0.0498674f, 0.05609209f, -0.1127772f),
					IndexJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.001397436f, 0.02104131f, -0.01416115f), new Quaternion(-0.6379428f, 0.7570527f, 0.1409398f, 0.006025487f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0111998f, 0.0373568f, -0.08502159f), new Quaternion(-0.6229248f, 0.7689475f, -0.07991651f, 0.1195734f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.001070029f, 0.03848532f, -0.1265175f), new Quaternion(-0.5905684f, 0.7402769f, -0.2174817f, 0.2364759f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01823281f, 0.03727832f, -0.1489557f), new Quaternion(-0.5704821f, 0.7217756f, -0.2741259f, 0.2800806f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					IndexTip = new Vector3(0.03459765f, 0.03554045f, -0.1647667f),
					MiddleJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.002337101f, 0.00708762f, -0.01569303f), new Quaternion(-0.6739864f, 0.7316698f, 0.08038815f, -0.06276429f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.01652897f, 0.009429025f, -0.08510397f), new Quaternion(-0.6704485f, 0.7306091f, -0.0895475f, 0.09322272f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.005480452f, 0.009177009f, -0.1267718f), new Quaternion(-0.6470159f, 0.7078094f, -0.2058894f, 0.1948995f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01256069f, 0.007871144f, -0.1546902f), new Quaternion(-0.6139116f, 0.6707432f, -0.3011713f, 0.287249f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					MiddleTip = new Vector3(0.03211262f, 0.00654219f, -0.1716127f),
					RingJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.0006732229f, -0.006577047f, -0.01572198f), new Quaternion(0.7157619f, -0.6929868f, -0.01495129f, 0.08503353f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(-0.009860706f, -0.01324086f, -0.08071329f), new Quaternion(0.7175256f, -0.6745214f, 0.1673809f, -0.04649286f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002356464f, -0.01965693f, -0.1186103f), new Quaternion(0.7053196f, -0.6458806f, 0.2543328f, -0.1437961f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01786923f, -0.02323769f, -0.1422353f), new Quaternion(0.6781462f, -0.6062587f, 0.3423913f, -0.235237f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					RingTip = new Vector3(0.03468312f, -0.02539339f, -0.1569241f),
					PinkyJoints = new Joint[]
					{
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002318364f, -0.01901329f, -0.01458787f), new Quaternion(0.7783645f, -0.6095778f, 0.0913996f, 0.1192044f)),
							Radius = 0.08f,
							Type = JointType.Metacarpal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.002126995f, -0.0376814f, -0.07460703f), new Quaternion(0.7866181f, -0.6019535f, 0.1365937f, 0.01504038f)),
							Radius = 0.08f,
							Type = JointType.Proximal
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.008005876f, -0.0433009f, -0.103353f), new Quaternion(0.7798259f, -0.5786312f, 0.2241698f, -0.08249451f)),
							Radius = 0.08f,
							Type = JointType.Medial
						},
						new Joint()
						{
							Pose = new Pose(new Vector3(0.01600803f, -0.04565193f, -0.1192801f), new Quaternion(0.7693536f, -0.5493547f, 0.2757933f, -0.1739037f)),
							Radius = 0.08f,
							Type = JointType.Distal
						}
					},
					PinkyTip = new Vector3(0.0270969f, -0.04629023f, -0.1334672f),
					ForearmJoint = new Joint()
					{
						Pose = new Pose(new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f)),
						Radius = 0f,
						Type = JointType.Metacarpal
					},
					WristPose = new Pose(new Vector3(-0.0001597873f, -3.19245E-05f, 0.0006257087f), new Quaternion(-8.659561E-17f, -0.7071068f, 0f, 0.7071068f)),
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