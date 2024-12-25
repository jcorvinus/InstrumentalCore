using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace Instrumental.Interaction.Input
{
    public class HandDataContainer : MonoBehaviour
    {
        public HandData Data;

		[SerializeField] bool isLeft;
        [SerializeField] SteamVR_Action_Skeleton skeleton;
        [SerializeField] bool convertTestData;
		[SerializeField] bool getStaticTestData;
		[SerializeField] EVRSkeletalReferencePose testDataPose = EVRSkeletalReferencePose.BindPose;
		[SerializeField] bool printTestDataStruct;
		[SerializeField] bool doUpdate;
		[SerializeField] bool drawBones;
		[SerializeField] bool drawBoneBasis;
		[SerializeField] bool drawPalmPose;

		[Range(0, 0.1f)]
		[SerializeField] float jointRadius = 0.08f;

		Vector3 previousTrackedPosition;
		Quaternion previousTrackedRotation;
		VelocityEstimation velocityEstimation;

		// steamVR palm offsets
		const float palmForwardOffset = 0.0153f;
		const float palmUpOffset = 0.06f;
		const float palmRightOffset = 0.0074f;

		Vector3 skeletonPosition = Vector3.zero;
		Quaternion skeletonRotation= Quaternion.identity;

		private void Awake()
		{
			SteamVR.Initialize();
			skeleton.SetRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
			skeleton.SetSkeletalTransformSpace(EVRSkeletalTransformSpace.Model);

			velocityEstimation = GetComponent<VelocityEstimation>();
		}

		// Start is called before the first frame update
		void Start()
        {
			InitializeData();
		}

        // Update is called once per frame
        void Update()
        {
			if (doUpdate)
			{
				skeletonPosition = skeleton.GetLocalPosition();
				skeletonRotation = skeleton.GetLocalRotation();

				if (transform.parent)
				{
					skeletonPosition = transform.parent.TransformPoint(skeletonPosition);
					skeletonRotation = transform.parent.rotation * skeletonRotation;
				}

				ConvertData(skeleton.bonePositions, skeleton.boneRotations, skeleton.poseIsValid);

				// send tracked pose updates to the velocity estimator
				if (Data.IsTracking)
				{
					if (!velocityEstimation.IsEstimating) velocityEstimation.StartEstimation();
					else
					{
						velocityEstimation.SubmitSample(Data.PalmPose.position, previousTrackedPosition,
							Data.PalmPose.rotation, previousTrackedRotation);
					}

					Data.Velocity = velocityEstimation.Velocity;
					Data.AngularVelocity = velocityEstimation.AngularVelocity;
				}
				else
				{
					if (velocityEstimation.IsEstimating) velocityEstimation.StopEstimation();
					Data.Velocity = Vector3.zero;
					Data.AngularVelocity = Vector3.zero;
				}

				previousTrackedPosition = Data.PalmPose.position;
				previousTrackedRotation = Data.PalmPose.rotation;
			}
		}

		private void InitializeData()
		{
			Data = new HandData()
			{
				Handedness = isLeft ? Handedness.Left : Handedness.Right,
				ThumbJoints = new Joint[4],
				IndexJoints = new Joint[4],
				MiddleJoints = new Joint[4],
				RingJoints = new Joint[4],
				PinkyJoints = new Joint[4],
				HasForearm = false,
				ForearmJoint = new Joint(),
				WristPose = new Pose(Vector3.zero, Quaternion.identity),
				PalmPose = new Pose(Vector3.zero, Quaternion.identity)
			};

			// wrist joint 
			Data.WristPose = new Pose(Vector3.zero, Quaternion.identity);

			#region Thumb Joints
			Data.ThumbJoints[0] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Metacarpal
			};

			Data.ThumbJoints[1] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Proximal
			};

			Data.ThumbJoints[2] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Medial
			};

			Data.ThumbJoints[3] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Distal
			};
			#endregion

			#region Index Joints
			Data.IndexJoints[0] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Metacarpal
			};

			Data.IndexJoints[1] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Proximal
			};

			Data.IndexJoints[2] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Medial
			};

			Data.MiddleJoints[3] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Distal
			};
			#endregion

			#region MiddleJoints
			Data.MiddleJoints[0] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Metacarpal
			};
			Data.MiddleJoints[1] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Proximal
			};
			Data.MiddleJoints[2] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Medial
			};
			Data.MiddleJoints[3] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Distal
			};
			#endregion

			#region Ring Joints
			Data.RingJoints[0] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Metacarpal
			};
			Data.RingJoints[1] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Proximal
			};
			Data.RingJoints[2] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Medial
			};
			Data.RingJoints[3] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Distal
			};
			#endregion

			#region Pinky Joints
			Data.PinkyJoints[0] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Metacarpal
			};

			Data.PinkyJoints[1] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Proximal
			};

			Data.PinkyJoints[2] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Medial
			};

			Data.PinkyJoints[3] = new Joint()
			{
				Pose = new Pose(Vector3.zero, Quaternion.identity),
				Radius = jointRadius,
				Type = JointType.Distal
			};
			#endregion

			skeletonPosition = Vector3.zero;
			skeletonRotation = Quaternion.identity;
		}

		/// <summary>
		/// Converts SteamVR tracking data to our internal data format.
		/// <param name="referenceData"></param>
		void ConvertData(Vector3[] bonePositions, Quaternion[] boneRotations, bool isTracked)
		{
			Data.IsTracking = isTracked;
			if (isTracked)
			{
				Data.TrackedForTime += Core.Time.deltaTime;
			}
			else Data.TrackedForTime = 0;

			Matrix4x4 skeletonMatrix = Matrix4x4.identity;
			Quaternion rootOffset = Quaternion.AngleAxis(180, Vector3.forward);
			Quaternion rootOffset2 = Quaternion.AngleAxis(180, Vector3.right);
			skeletonMatrix = Matrix4x4.TRS(skeletonPosition, skeletonRotation.normalized *
				(rootOffset * rootOffset2), Vector3.one);

			for (int i=0; i < bonePositions.Length; i++)
			{
                SteamVR_Skeleton_JointIndexEnum jointIndexEnum = (SteamVR_Skeleton_JointIndexEnum)i;

				Vector3 bonePosition = bonePositions[i];
				Quaternion boneRotation = boneRotations[i];

				Vector3 up, forward; // for us, up is... well up, and forward goes distal
				up = boneRotation * Vector3.up;
				forward = boneRotation * Vector3.forward;

				if(!isLeft)
				{
					up = boneRotation * Vector3.up;
					forward = boneRotation * Vector3.right;
				}
				else
				{
					up = boneRotation * Vector3.up * -1;
					forward = boneRotation * Vector3.right * -1;
				}

				Quaternion rotation = Quaternion.LookRotation(forward, up);
				Matrix4x4 boneMatrix = Matrix4x4.TRS(bonePosition, rotation, Vector3.one);
				Matrix4x4 combined = skeletonMatrix * boneMatrix;

				// note that on the right hand from steamVR, x is forward, and y is up
				// on the left hand, this is flipped.
				switch (jointIndexEnum)
				{
					case SteamVR_Skeleton_JointIndexEnum.root:
						break;
					case SteamVR_Skeleton_JointIndexEnum.wrist:
						if(!isLeft)
						{
							up = boneRotation * Vector3.right;
							forward = boneRotation * Vector3.forward;
						}
						else
						{
							up = boneRotation * -Vector3.right;
							forward = boneRotation * Vector3.forward;
						}

						rotation = Quaternion.LookRotation(forward, up);
						boneMatrix = Matrix4x4.TRS(bonePosition, rotation, Vector3.one);
						combined = skeletonMatrix * boneMatrix;

						Data.WristPose.position = combined.GetPosition();
						Data.WristPose.rotation = combined.GetRotation();

						Quaternion combinedRotation = combined.GetRotation();

						float palmDirOffset = (isLeft) ? palmRightOffset : -palmRightOffset;
						Vector3 palmOffset = (Vector3.right * palmDirOffset) +
							(Vector3.up * -palmForwardOffset) + (Vector3.forward * palmUpOffset);
						palmOffset = combinedRotation * palmOffset;
						Data.PalmPose = new Pose(combined.GetPosition() + palmOffset,
							Quaternion.LookRotation(combinedRotation * Vector3.up * -1,
							combinedRotation * Vector3.forward));
						break;

					case SteamVR_Skeleton_JointIndexEnum.thumbProximal: // also thumb metacarpal
						Data.ThumbJoints[0].Pose.position = combined.GetPosition(); //bonePosition;
						Data.ThumbJoints[0].Pose.rotation = combined.GetRotation(); //rotation;

						Data.ThumbJoints[1].Pose.position = combined.GetPosition(); //bonePosition;
						Data.ThumbJoints[1].Pose.rotation = combined.GetRotation(); // rotation;
						break;
					case SteamVR_Skeleton_JointIndexEnum.thumbMiddle:
						Data.ThumbJoints[2].Pose.position = combined.GetPosition(); //bonePosition;
						Data.ThumbJoints[2].Pose.rotation = combined.GetRotation();//rotation;
						break;

					case SteamVR_Skeleton_JointIndexEnum.thumbDistal:
						Data.ThumbJoints[3].Pose.position = combined.GetPosition();
						Data.ThumbJoints[3].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.thumbTip:
						Data.ThumbTip = combined.GetPosition();
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexMetacarpal:
						Data.IndexJoints[0].Pose.position = combined.GetPosition();
						Data.IndexJoints[0].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexProximal:
						Data.IndexJoints[1].Pose.position = combined.GetPosition();
						Data.IndexJoints[1].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexMiddle:
						Data.IndexJoints[2].Pose.position = combined.GetPosition();
						Data.IndexJoints[2].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexDistal:
						Data.IndexJoints[3].Pose.position = combined.GetPosition();
						Data.IndexJoints[3].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexTip:
						Data.IndexTip = combined.GetPosition();
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleMetacarpal:
						Data.MiddleJoints[0].Pose.position = combined.GetPosition();
						Data.MiddleJoints[0].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleProximal:
						Data.MiddleJoints[1].Pose.position = combined.GetPosition();
						Data.MiddleJoints[1].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleMiddle:
						Data.MiddleJoints[2].Pose.position = combined.GetPosition();
						Data.MiddleJoints[2].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleDistal:
						Data.MiddleJoints[3].Pose.position = combined.GetPosition();
						Data.MiddleJoints[3].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleTip:
						Data.MiddleTip = combined.GetPosition();
						break;

					case SteamVR_Skeleton_JointIndexEnum.ringMetacarpal:
						Data.RingJoints[0].Pose.position = combined.GetPosition();
						Data.RingJoints[0].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.ringProximal:
						Data.RingJoints[1].Pose.position = combined.GetPosition();
						Data.RingJoints[1].Pose.rotation = combined.GetRotation();
						break;
					case SteamVR_Skeleton_JointIndexEnum.ringMiddle:
						Data.RingJoints[2].Pose.position = combined.GetPosition();
						Data.RingJoints[2].Pose.rotation = combined.GetRotation();
						break;
					case SteamVR_Skeleton_JointIndexEnum.ringDistal:
						Data.RingJoints[3].Pose.position = combined.GetPosition();
						Data.RingJoints[3].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.ringTip:
						Data.RingTip = combined.GetPosition();
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyMetacarpal:
						Data.PinkyJoints[0].Pose.position = combined.GetPosition();
						Data.PinkyJoints[0].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyProximal:
						Data.PinkyJoints[1].Pose.position = combined.GetPosition();
						Data.PinkyJoints[1].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyMiddle:
						Data.PinkyJoints[2].Pose.position = combined.GetPosition();
						Data.PinkyJoints[2].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyDistal:
						Data.PinkyJoints[3].Pose.position = combined.GetPosition();
						Data.PinkyJoints[3].Pose.rotation = combined.GetRotation();
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyTip:
						Data.PinkyTip = combined.GetPosition();
						break;

					case SteamVR_Skeleton_JointIndexEnum.thumbAux: // these have already been filled
						break;
					case SteamVR_Skeleton_JointIndexEnum.indexAux:
						break;
					case SteamVR_Skeleton_JointIndexEnum.middleAux:
						break;
					case SteamVR_Skeleton_JointIndexEnum.ringAux:
						break;
					case SteamVR_Skeleton_JointIndexEnum.pinkyAux:
						break;
					default:
						break;
				}
			}
        }

		private void ConvertTestData()
		{
			// convert data to bone arrays
			SteamVR_Utils.RigidTransform[] referenceData = skeleton.GetReferenceTransforms(EVRSkeletalTransformSpace.Model,
				testDataPose);

			Vector3[] bonePositions = new Vector3[referenceData.Length];
			Quaternion[] boneRotations = new Quaternion[referenceData.Length];

			for(int i=0; i < referenceData.Length; i++)
			{
				bonePositions[i] = referenceData[i].pos;
				boneRotations[i] = referenceData[i].rot;
			}

			InitializeData();
			skeletonPosition = Vector3.zero;
			skeletonRotation = Quaternion.identity;
			ConvertData(bonePositions, boneRotations, false);
		}

		void DrawBasis(Joint joint)
		{
			DrawBasis(joint.Pose);
		}

		void DrawBasis(UnityEngine.Pose pose)
		{
			const float size = 0.01f;

			Gizmos.color = Color.red;
			Gizmos.DrawLine(pose.position,
				pose.position + ((pose.rotation * Vector3.right) * size));
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(pose.position,
				pose.position + ((pose.rotation * Vector3.up) * size));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(pose.position,
				pose.position + ((pose.rotation * Vector3.forward) * size));
		}

        private void OnDrawGizmos()
		{
			if (convertTestData)
			{
				convertTestData = false;
				ConvertTestData();
			}

			if(getStaticTestData)
			{
				getStaticTestData = false;
				Data = HandData.GetTestData(isLeft);
			}

			if(printTestDataStruct)
			{
				printTestDataStruct = false;
				Debug.Log(Data.PrintInitCode());
			}

			if(drawPalmPose)
			{
				DrawBasis(Data.PalmPose);
			}

			if (drawBones)
			{
				if (drawBoneBasis)
				{
					DrawBasis(new Pose(skeletonPosition, skeletonRotation));
				}

				// draw the hand
				for (int fingerIndx = 0; fingerIndx < 5; fingerIndx++)
				{
					Finger finger = (Finger)fingerIndx;

					Joint[] joints = Data.GetJointForFinger(finger);

					if (joints.Length == 0) continue;

					// draw our joints
					for (int jointIndx = 0; jointIndx < 5; jointIndx++)
					{
						Vector3 start, end;
						if (jointIndx == 0)
						{
							start = Data.WristPose.position;
							end = joints[jointIndx].Pose.position;
							if (drawBoneBasis) DrawBasis(Data.WristPose);
						}
						else if (jointIndx == 4)
						{
							start = joints[jointIndx - 1].Pose.position;
							if (drawBoneBasis) DrawBasis(joints[jointIndx - 1]);
							end = Data.GetFingertip(finger);
						}
						else
						{
							start = joints[jointIndx - 1].Pose.position;
							end = joints[jointIndx].Pose.position;
							if (drawBoneBasis) DrawBasis(joints[jointIndx - 1]);
						}

						Gizmos.color = Color.white;
						Gizmos.DrawLine(start, end);
					}
				}
			}
		}
	}
}