using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction.Input;
using Instrumental.Interaction;

namespace Instrumental.Avatar
{
    public class SkinnedHand : MonoBehaviour
    {
        [SerializeField] HandDataContainer dataContainer;

        [SerializeField] Vector3 fingerDirection;
        [SerializeField] Vector3 palmDirection;

        [SerializeField] Vector3 wristForwardDirection = Vector3.forward;
        [SerializeField] Vector3 wristUpDirection = Vector3.right;

        [SerializeField] Transform[] fingers;
        Transform[,] joints;
        [SerializeField] Transform wrist;
        Vector3 wristDefaultScale;

        Vector3 pinkyProximalInWristSpace;
        Vector3 dataPinkyInDataWristSpace;

        // need to figure out our curves (If any)
        [Range(0, 1)]
        [SerializeField] float wristPinkyMiddleScaleFactor = 0.614f;
        float[] modelFingerLengths;
        float modelWristToMiddleProximalDistance;
        float modelWristToPinkyProximalDistance;

        float dataMiddleFingerLength;
        float dataWristToMiddleProximalDistance;
        const float lengthAcquireDuration=0.11f;

        SkinnedMeshRenderer skinnedMeshRenderer;
        [SerializeField] bool drawFingerJointBasis = false;
        [SerializeField] bool drawWristJointBasis = true;

		private void Awake()
		{
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            joints = new Transform[5, 4];
            for(int i=0; i < fingers.Length; i++)
			{
                Transform metaCarpal, proximal, medial, distal;

                if (i == 0)
				{
                    proximal = fingers[0];
                    medial = proximal.GetChild(0);
                    distal = medial.GetChild(0);

                    joints[i, 0] = proximal;
                    joints[i, 1] = medial;
                    joints[i, 2] = distal;
                }
                else
				{
                    metaCarpal = fingers[i];
                    proximal = metaCarpal.GetChild(0);
                    medial = proximal.GetChild(0);
                    distal = medial.GetChild(0);

                    joints[i, 0] = metaCarpal;
                    joints[i, 1] = proximal;
                    joints[i, 2] = medial;
                    joints[i, 3] = distal;
                }
			}

            wristDefaultScale = wrist.transform.localScale;
		}

		// Start is called before the first frame update
		void Start()
        {
            GetInitialMeasurements();
        }

        void GetInitialMeasurements()
		{
            modelFingerLengths = new float[fingers.Length];
            for(int i=0; i < fingers.Length; i++)
			{
                Transform metaCarpal, proximal, medial, distal, tip=null;

                if(i == 0) // do thumb
				{
                    proximal = fingers[0];
                    medial = proximal.GetChild(0);
                    distal = medial.GetChild(0);
                    if(distal.childCount == 1)
					{
                        tip = distal.GetChild(0);
					}

                    float length1 = Vector3.Distance(proximal.position, medial.position);
                    float length2 = Vector3.Distance(medial.position, distal.position);
                    float length3 = ((tip != null) ? Vector3.Distance(distal.position, tip.position) : length2); // re-use medial length if no tip available

                    modelFingerLengths[i] = length1 + length2 + length3;
				}
                else // do the others
				{
                    metaCarpal = fingers[i];
                    proximal = metaCarpal.GetChild(0);
                    medial = proximal.GetChild(0);
                    distal = medial.GetChild(0);

                    if (distal.childCount == 1)
                    {
                        tip = distal.GetChild(0);
                    }

                    float length0 = Vector3.Distance(metaCarpal.position, proximal.position);
                    float length1 = Vector3.Distance(proximal.position, medial.position);
                    float length2 = Vector3.Distance(medial.position, distal.position);
                    float length3 = ((tip != null) ? Vector3.Distance(distal.position, tip.position) : length2); // re-use medial length if no tip available

                    modelFingerLengths[i] = length0 + length1 + length2 + length3;
                }
			}

            modelWristToMiddleProximalDistance = Vector3.Distance(wrist.position, fingers[2].GetChild(0).position);
            pinkyProximalInWristSpace = wrist.InverseTransformPoint(fingers[(int)Finger.Pinky].GetChild(0).position);
            Vector3 scalar = new Vector3(((Mathf.Abs(wristForwardDirection.x) > 0) ? 0 : 1),
                ((Mathf.Abs(wristForwardDirection.y) > 0) ? 0 : 1),
                ((Mathf.Abs(wristForwardDirection.z) > 0) ? 0 : 1));
            pinkyProximalInWristSpace.Scale(scalar);

            modelWristToPinkyProximalDistance = Vector3.Distance(wrist.transform.TransformPoint(pinkyProximalInWristSpace), fingers[(int)Finger.Pinky].GetChild(0).position);
        }

        // Update is called once per frame
        void Update()
        {
            if (dataContainer.Data.IsTracking)
            {
                skinnedMeshRenderer.enabled = true;
                // do retargeting
                Quaternion wristRotationOffset = Quaternion.LookRotation(wristForwardDirection, wristUpDirection);

                wrist.SetPositionAndRotation(dataContainer.Data.WristPose.position,
                    dataContainer.Data.WristPose.rotation * Quaternion.Inverse(wristRotationOffset));

                // set up the fingers
                for (int i = 0; i < fingers.Length; i++)
                {
                    // this might result in allocs, look into removing it
                    Interaction.Input.Joint[] fingerJoints = dataContainer.Data.GetJointForFinger((Finger)i);

                    if (i == 0) // thumbs!
                    {
                        for (int jointIndx = 0; jointIndx < 3; jointIndx++)
                        {
                            Transform joint = joints[i, jointIndx];
                            Quaternion jointRotationOffset = Quaternion.LookRotation(fingerDirection, -palmDirection);

                            Interaction.Input.Joint fingerJoint = fingerJoints[jointIndx + 1];
                            joint.SetPositionAndRotation(fingerJoint.Pose.position,
                                fingerJoint.Pose.rotation * Quaternion.Inverse(jointRotationOffset));
                        }
                    }
                    else
                    {
                        for (int jointIndx = 0; jointIndx < 4; jointIndx++)
                        {
                            Transform joint = joints[i, jointIndx];
                            Vector3 jointBasisForward, jointBasisUp, jointBasisRight;
                            GetBasis(joint, out jointBasisRight, out jointBasisForward, out jointBasisUp);
                            Quaternion jointRotationOffset = Quaternion.LookRotation(fingerDirection, -palmDirection);

                            Interaction.Input.Joint fingerJoint = fingerJoints[jointIndx];
                            joint.SetPositionAndRotation(fingerJoint.Pose.position,
                                fingerJoint.Pose.rotation * Quaternion.Inverse(jointRotationOffset));
                        }
                    }
                }

                if(dataContainer.Data.TrackedForTime < lengthAcquireDuration) // fix this when our istracking value gets fixed
				{
                    dataMiddleFingerLength = 0;

                    for(int boneIndx=0; boneIndx < 4; boneIndx++)
				    {
                        Instrumental.Interaction.Input.Joint currentJoint = dataContainer.Data.MiddleJoints[boneIndx];

                        // instead of next joint, for the last one we should do the tip
                        Vector3 endPoint;
                        if (boneIndx == 3)
                        {
                            endPoint = dataContainer.Data.MiddleTip;
                        }
                        else
                        {
                            Instrumental.Interaction.Input.Joint nextJoint = dataContainer.Data.MiddleJoints[boneIndx + 1];
                            endPoint = nextJoint.Pose.position;
                        }

                        dataMiddleFingerLength += Vector3.Distance(currentJoint.Pose.position, endPoint);
                    }

                    dataWristToMiddleProximalDistance = Vector3.Distance(dataContainer.Data.WristPose.position,
                        dataContainer.Data.MiddleJoints[1].Pose.position);

                    Pose pinkyDataPose = dataContainer.Data.PinkyJoints[1].Pose;
                    Pose wristDataPose = dataContainer.Data.WristPose;

                    dataPinkyInDataWristSpace = pinkyDataPose.position - wristDataPose.position;
                    dataPinkyInDataWristSpace = Quaternion.Inverse(wristDataPose.rotation) * dataPinkyInDataWristSpace;
                    dataPinkyInDataWristSpace.Scale(new Vector3(1, 0, 0));
                    dataPinkyInDataWristSpace = wristDataPose.rotation * dataPinkyInDataWristSpace;
                    dataPinkyInDataWristSpace += wristDataPose.position;

                    float dataWristPinkyLength = Vector3.Distance(dataContainer.Data.PinkyJoints[0].Pose.position, dataPinkyInDataWristSpace);

                    float fullDataLength = dataMiddleFingerLength + dataWristToMiddleProximalDistance;
                    float fullModelLength = modelFingerLengths[(int)Finger.Middle] + modelWristToMiddleProximalDistance;

                    float fingerRatio = dataMiddleFingerLength / modelFingerLengths[(int)Finger.Middle];
                    float wristToMiddleRatio = dataWristToMiddleProximalDistance / modelWristToMiddleProximalDistance; // magic number lol
                    float wristToPinkyRatio = dataWristPinkyLength / modelWristToPinkyProximalDistance;
                    float fullRatio = fullDataLength / fullModelLength;
                    float wristRatio = Mathf.Lerp(wristToPinkyRatio, wristToMiddleRatio, wristPinkyMiddleScaleFactor);

                    Debug.Log(string.Format("finger Ratio: {0}    wrist ratio: {1} ", fingerRatio, wristRatio));
                    Vector3 wristScale = new Vector3(fingerRatio, fingerRatio, fingerRatio);

                    if (Mathf.Abs(wristForwardDirection.x) != 0)
                    {
                        wristScale.x = wristRatio;
                    }
                    else if (Mathf.Abs(wristForwardDirection.y) != 0)
                    {
                        wristScale.y = wristRatio;
                    }
                    else wristScale.z = wristRatio;

                    wrist.transform.localScale = Vector3.Scale(wristScale, wristDefaultScale);
                }
            }
            else
			{
                skinnedMeshRenderer.enabled = false;
			}
        }

        private void GetBasis(Transform reference, out Vector3 right, out Vector3 forward, out Vector3 up)
        {
            forward = fingerDirection;
            up = palmDirection * -1;
            right = Vector3.Cross(forward, up);

            forward = reference.TransformDirection(forward);
            up = reference.TransformDirection(up);
            right = reference.TransformDirection(right);
        }

        const float basisDrawDist = 0.02f;
        void DrawBasis(Transform bone)
        {
            Vector3 forward = fingerDirection;
            Vector3 up = palmDirection * -1;
            Vector3 right = Vector3.Cross(forward, up);

            Color storedColor = Gizmos.color;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(right) * 0.01f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(up) * 0.01f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(forward) * 0.01f);

            Gizmos.color = storedColor;
        }

        void DrawWristBasis(Transform bone)
        {
            Vector3 forward = wristForwardDirection;
            Vector3 up = wristUpDirection;
            Vector3 right = Vector3.Cross(forward, up);

            Color storedColor = Gizmos.color;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(right) * 0.01f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(up) * 0.01f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(forward) * 0.01f);

            Gizmos.color = storedColor;
        }

        private void OnDrawGizmos()
		{
            if(drawWristJointBasis)
			{
                DrawWristBasis(wrist);
			}

            // draw model pinky in wrist space
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wrist.TransformPoint(pinkyProximalInWristSpace), Vector3.one * 0.02f);

            // draw data pinky in wrist space
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(dataPinkyInDataWristSpace, Vector3.one * 0.01f);

            if (drawFingerJointBasis)
            {
                for (int i = 0; i < fingers.Length; i++)
                {
                    Transform metaCarpal, proximal, medial, distal, tip = null;

                    if (i == 0) // do thumb
                    {
                        proximal = fingers[0];
                        DrawBasis(proximal);

                        medial = proximal.GetChild(0);
                        DrawBasis(medial);

                        distal = medial.GetChild(0);
                        DrawBasis(distal);

                        if (distal.childCount == 1)
                        {
                            tip = distal.GetChild(0);
                            DrawBasis(tip);
                        }
                    }
                    else // do the others
                    {
                        metaCarpal = fingers[i];
                        DrawBasis(metaCarpal);

                        proximal = metaCarpal.GetChild(0);
                        DrawBasis(proximal);

                        medial = proximal.GetChild(0);
                        DrawBasis(medial);

                        distal = medial.GetChild(0);
                        DrawBasis(distal);

                        if (distal.childCount == 1)
                        {
                            tip = distal.GetChild(0);
                            DrawBasis(tip);
                        }
                    }
                }
            }
        }
	}
}