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
		[SerializeField] bool printTestDataStruct;

		[Range(0, 0.1f)]
		[SerializeField] float jointRadius = 0.08f;

		[SerializeField] bool doSetDetails;

		SteamVR_Action_Source_Map sourceMap;

		private void Awake()
		{
			SteamVR.Initialize();
			skeleton.SetRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
			skeleton.SetSkeletalTransformSpace(EVRSkeletalTransformSpace.Model);
		}

		// Start is called before the first frame update
		void Start()
        {

		}

        // Update is called once per frame
        void Update()
        {
			ConvertData(skeleton.bonePositions, skeleton.boneRotations);
		}

		/// <summary>
		///  ok so I think using this rigid transform array will cause a bit of a problem.
		///  It only works in a special context - that being getting the reference transforms.
		///  I can probably get the data in a different format from SteamVR_ActionSkeleton,
		///  by using the bone positions and bone rotations as they are stored. I don't want to have
		///  to convert these to RigidTransform though, since that would incur overhead.
		///  
		/// It does look like some data filtering is going on in SteamVR_Skeleton_Source though, flipping and rotating
		/// and whatnot, and I'd like to get rid of those.
		/// </summary>
		/// <param name="referenceData"></param>
        void ConvertData(Vector3[] bonePositions, Quaternion[] boneRotations)
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


			// turn the rigid transforms into poses (GetReferenceTransforms already fixes them for us!)
			for (int i=0; i < bonePositions.Length; i++)
			{
                SteamVR_Skeleton_JointIndexEnum jointIndexEnum = (SteamVR_Skeleton_JointIndexEnum)i;
				//SteamVR_Utils.RigidTransform jointTransform = referenceData[(int)jointIndexEnum];

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

				// note that on the right hand from steamVR, x is forward, and y is up
				// on the left hand, this is flipped.
				switch (jointIndexEnum)
				{
					case SteamVR_Skeleton_JointIndexEnum.root:
						break;
					case SteamVR_Skeleton_JointIndexEnum.wrist:
						Data.WristPose = new Pose(bonePosition,
							rotation);

						break;
					//case SteamVR_Skeleton_JointIndexEnum.thumbMetacarpal:
					//	break;
					case SteamVR_Skeleton_JointIndexEnum.thumbProximal: // also thumb metacarpal
						Data.ThumbJoints[0] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Metacarpal
						};

						Data.ThumbJoints[1] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Proximal
						};
						break;
					case SteamVR_Skeleton_JointIndexEnum.thumbMiddle:
						Data.ThumbJoints[2] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Medial
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.thumbDistal:
						Data.ThumbJoints[3] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Distal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.thumbTip:
						Data.ThumbTip = bonePosition;
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexMetacarpal:
						Data.IndexJoints[0] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Metacarpal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexProximal:
						Data.IndexJoints[1] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Proximal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexMiddle:
						Data.IndexJoints[2] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Medial
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexDistal:
						Data.IndexJoints[3] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Distal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.indexTip:
						Data.IndexTip = bonePosition;
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleMetacarpal:
						Data.MiddleJoints[0] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Metacarpal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleProximal:
						Data.MiddleJoints[1] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Proximal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleMiddle:
						Data.MiddleJoints[2] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Medial
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleDistal:
						Data.MiddleJoints[3] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Distal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.middleTip:
						Data.MiddleTip = bonePosition;
						break;

					case SteamVR_Skeleton_JointIndexEnum.ringMetacarpal:
						Data.RingJoints[0] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Metacarpal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.ringProximal:
						Data.RingJoints[1] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Proximal
						};
						break;
					case SteamVR_Skeleton_JointIndexEnum.ringMiddle:
						Data.RingJoints[2] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Medial
						};
						break;
					case SteamVR_Skeleton_JointIndexEnum.ringDistal:
						Data.RingJoints[3] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Distal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.ringTip:
						Data.RingTip = bonePosition;
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyMetacarpal:
						Data.PinkyJoints[0] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Metacarpal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyProximal:
						Data.PinkyJoints[1] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Proximal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyMiddle:
						Data.PinkyJoints[2] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Medial
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyDistal:
						Data.PinkyJoints[3] = new Joint()
						{
							Pose = new Pose(bonePosition, rotation),
							Radius = jointRadius,
							Type = JointType.Distal
						};
						break;

					case SteamVR_Skeleton_JointIndexEnum.pinkyTip:
						Data.PinkyTip = bonePosition;
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
				EVRSkeletalReferencePose.BindPose);

			Vector3[] bonePositions = new Vector3[referenceData.Length];
			Quaternion[] boneRotations = new Quaternion[referenceData.Length];

			for(int i=0; i < referenceData.Length; i++)
			{
				bonePositions[i] = referenceData[i].pos;
				boneRotations[i] = referenceData[i].rot;
			}

			ConvertData(bonePositions, boneRotations);
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

			// draw the hand
			for (int fingerIndx=0; fingerIndx < 5; fingerIndx++)
			{
                Finger finger = (Finger)fingerIndx;

				Joint[] joints = Data.GetJointForFinger(finger);

				if (joints.Length == 0) continue;

                // draw our joints
                for(int jointIndx=0; jointIndx < 5; jointIndx++)
				{
                    Vector3 start, end;
					if(jointIndx == 0)
					{
						start = Data.WristPose.position;
						end = joints[jointIndx].Pose.position;
						DrawBasis(Data.WristPose);
					}
					else if (jointIndx == 4)
					{
						start = joints[jointIndx - 1].Pose.position;
						DrawBasis(joints[jointIndx - 1]);
						end = Data.GetFingertip(finger);
					}
					else
					{
						start = joints[jointIndx - 1].Pose.position;
						end = joints[jointIndx].Pose.position;
						DrawBasis(joints[jointIndx - 1]);
					}

					Gizmos.color = Color.white;
					Gizmos.DrawLine(start, end);
				}
			}
		}
	}
}