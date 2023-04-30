using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Interaction.Input;

namespace Instrumental.Avatar
{
    public class CapsuleHand : MonoBehaviour
    {
        public struct BoneInfo
		{
            public GameObject StartEpiphysis;
            public GameObject Diaphysis;
            public GameObject EndEpiphysis;
		}

        [SerializeField] HandDataContainer dataContainer;

        BoneInfo[,] bones;

        [Range(0, 0.1f)]
        [SerializeField] float radius = 0.001f;

		private void Awake()
		{
            bones = new BoneInfo[5, 4];

            for(int fingerIndx=0; fingerIndx < 5; fingerIndx++)
			{
                BoneInfo[] bonesForFinger = GenerateBones((Finger)fingerIndx);

                for(int boneIndx=0; boneIndx < bonesForFinger.Length; boneIndx++)
				{
                    bones[fingerIndx, boneIndx] = bonesForFinger[boneIndx];
				}
			}
		}

        BoneInfo[] GenerateBones(Finger finger)
		{
            bool isThumb = finger == Finger.Thumb;
            int boneCount = (isThumb) ? 3 : 4;
            BoneInfo[] bones = new BoneInfo[boneCount];

            for(int i=0; i < boneCount; i++)
			{
                GameObject startEpiphysis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                startEpiphysis.transform.parent = transform;
                startEpiphysis.transform.localScale = Vector3.one * radius;
                SphereCollider startCollider = startEpiphysis.GetComponent<SphereCollider>();
                Destroy(startCollider);

                // bone forward axis is up for this gameobject
                GameObject diaphysis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                diaphysis.transform.parent = transform;
                diaphysis.transform.localScale = new Vector3(radius, radius * 2, radius);
                CapsuleCollider diaphysisCollider = diaphysis.GetComponent<CapsuleCollider>();
                Destroy(diaphysisCollider);

                GameObject endEpiphysis = null;
                if(i == boneCount - 1)
				{
                    endEpiphysis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    endEpiphysis.transform.parent = transform;
                    endEpiphysis.transform.localScale = Vector3.one * radius;
                    SphereCollider endCollider = endEpiphysis.GetComponent<SphereCollider>();
                    Destroy(endCollider);
				}

                BoneInfo bone = new BoneInfo()
                {
                    StartEpiphysis = startEpiphysis,
                    Diaphysis = diaphysis,
                    EndEpiphysis = endEpiphysis
                };

                bones[i] = bone;
			}

			return bones;
		}

		private void OnDisable()
		{
            SetBoneEnable(false);
        }

		// Start is called before the first frame update
		void Start()
        {

        }

        void SetBoneEnable(BoneInfo bone, bool state)
		{
            if (bone.StartEpiphysis) bone.StartEpiphysis.SetActive(state);
            if (bone.Diaphysis) bone.Diaphysis.SetActive(state);
            if (bone.EndEpiphysis) bone.EndEpiphysis.SetActive(state);
		}

        void SetBoneEnable(bool state)
		{
            // call SetBoneEnable here on the proper list of bones
            for(int i=0; i < 5; i++)
			{
                bool isthumb = i == (int)(Finger.Thumb);
                int boneCount = isthumb ? 3 : 4;
                for(int j=0; j < boneCount; j++)
				{
                    SetBoneEnable(bones[i, j], state);
				}
			}
		}

        // Update is called once per frame
        void Update()
        {
            if(true)
			{
                for(int fingerIndx=0; fingerIndx < 5; fingerIndx++)
				{
                    Finger finger = (Finger)fingerIndx;

                    Instrumental.Interaction.Input.Joint[] boneJoints = dataContainer.Data.GetJointForFinger(finger);

                    bool isThumb = finger == Finger.Thumb;
                    int boneCount = isThumb ? 3 : 4;

                    for(int boneIndx=0; boneIndx < boneCount; boneIndx++)
					{
                        int offset = isThumb ? 1 : 0;
                        bool isLast = boneIndx == boneCount - 1;

                        Vector3 startPosition, endPosition, center;

                        Instrumental.Interaction.Input.Joint currentJoint, nextJoint;
                        currentJoint = boneJoints[boneIndx + offset];
                        startPosition = currentJoint.Pose.position;

                        if (isLast)
						{
                            endPosition = dataContainer.Data.GetFingertip(finger);
						}
                        else 
                        {
                            nextJoint = boneJoints[boneIndx + offset + 1];
                            endPosition = nextJoint.Pose.position;
                        }

                        center = (startPosition + endPosition) * 0.5f;
                        float length = Vector3.Distance(startPosition, endPosition);

                        BoneInfo bone = bones[fingerIndx, boneIndx];

                        bone.StartEpiphysis.transform.position = currentJoint.Pose.position;
                        bone.Diaphysis.transform.position = center;
                        Quaternion rotation = currentJoint.Pose.rotation;
                        Quaternion basisRotation = Quaternion.AngleAxis(90, Vector3.right);
                        bone.Diaphysis.transform.rotation = rotation * basisRotation; 
                        bone.Diaphysis.transform.localScale = new Vector3(radius, length * 0.5f, radius);
                        bone.StartEpiphysis.transform.localScale = Vector3.one * radius;

                        if(isLast)
						{
                            bone.EndEpiphysis.transform.localScale = Vector3.one * radius;
                            bone.EndEpiphysis.transform.position = endPosition;
                        }
                    }
				}

                SetBoneEnable(true);
            }
            else
			{
                SetBoneEnable(false);
			}
        }
    }
}