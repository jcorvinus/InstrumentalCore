using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Interaction.Input;
using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Interaction
{
    public enum HandAvatar
    {
        Glove = 0,
        Capsule = 1
    }

    public class InstrumentalBody : MonoBehaviour
    {
        private static InstrumentalBody instance;
        public static InstrumentalBody Instance { get { return instance; } }

        private InstrumentalHand leftHand, rightHand;
        private Transform head;

        Quatn torsoRotation;
        const float headRotationThreshold = 24.45f;
        const float neckHeadOffset = 0.11f;
        bool isTorsoSmoothing = false;
        float torsoSmoothTime = 0;
        const float torsoSmoothDuration = 1f;
        Vect3 neckPosition;

        private Vect3 leftShoulder;
        private Vect3 rightShoulder;
        private Vect3 forwardDirection;
        private Quatn noRollHeadRotation;

#region Hand Rays
		private OneEuroFilter<Vect3> leftAimPosFilter;
        private OneEuroFilter<Vect3> leftRayOriginFilter;

        private OneEuroFilter<Vect3> rightAimPosFilter;
        private OneEuroFilter<Vect3> rightRayOriginFilter;

        const float filterBeta = 100;
        const float filterMinCutoff = 5;
        const float filterFreq = 30;

		Vect3 wristOffset = new Vect3(0.0425f, 0.0f, 0.0f);
		Vect3 knuckleOffset = new Vect3 { x = 0.0f, y = 0.02f, z = 0.05f };
        bool doKnuckleOffsetDebug = false;
        private GameObject leftKnuckleOffsetVis;
        private GameObject rightKnuckleOffsetVis;

        private Vect3 leftAimPosition;
        private Vect3 rightAimPosition;
        private RayIC leftHandRay;
        private RayIC rightHandRay;

        bool doWristOffsetDebug = false;
        private GameObject leftWristOffsetVis;
        private GameObject rightWristOffsetVis;
#endregion

		[Range(0, 45)]
        [SerializeField] float palmComfyUpOffset = 21;
        const float palmDiagonalOffset = 45f;
		Vect3 leftPalmComfyUp = Vect3.up;
		Vect3 rightPalmComfyUp = Vect3.up;
		Vect3 leftPalmDiagonal = Vect3.up;
		Vect3 rightPalmDiagonal = Vect3.up;

        [SerializeField] HandAvatar handAvatar = HandAvatar.Glove;
        [SerializeField] KeyCode avatarSwitchKey = KeyCode.F1;

        public Transform Head { get { return head; } }

        public Vect3 LeftShoulder { get { return leftShoulder; } }
        public Vect3 RightShoulder { get { return rightShoulder; } }

        public Vect3 LeftPalmComfyUp { get { return leftPalmComfyUp; } }

        public Vect3 RightPalmComfyUp { get { return rightPalmComfyUp; } }

        public Vect3 LeftPalmDiagonal { get { return leftPalmDiagonal; } }
        public Vect3 RightPalmDiagonal { get { return rightPalmDiagonal; } }

        public Vect3 ForwardDirection { get { return forwardDirection; } }

        public Vect3 LeftAimPosition { get { return leftAimPosition; } }
        public Vect3 RightAimPosition { get { return rightAimPosition; } }
        public RayIC LeftHandRay { get { return leftHandRay; } }
        public RayIC RightHandRay { get { return rightHandRay; } }

        public HandAvatar Avatar { get { return handAvatar; } }

		private void Awake()
		{
            instance = this;

            for(int i=0; i < transform.childCount; i++)
			{
                InstrumentalHand handCandidate = transform.GetChild(i).GetComponent<InstrumentalHand>();

                if(handCandidate)
				{
                    if (handCandidate.Hand == Handedness.Left) leftHand = handCandidate;
                    else rightHand = handCandidate;
				}

                if (transform.GetChild(i).name == "Head") head = transform.GetChild(i);
			}
		}

		// Start is called before the first frame update
		void Start()
        {
            torsoRotation = Quatn.Euler(0, head.rotation.eulerAngles.y, 0);
            neckPosition = (Vect3)head.position + (Vect3.down * neckHeadOffset);

            leftAimPosFilter = new OneEuroFilter<Vect3>(filterFreq, filterMinCutoff, filterBeta);
            leftRayOriginFilter = new OneEuroFilter<Vect3>(filterFreq, filterMinCutoff, filterBeta);

            rightAimPosFilter = new OneEuroFilter<Vect3>(filterFreq, filterMinCutoff, filterBeta);
            rightRayOriginFilter = new OneEuroFilter<Vect3>(filterFreq, filterMinCutoff, filterBeta);

            if(doWristOffsetDebug)
			{
                leftWristOffsetVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                BoxCollider leftWristVisCollider = leftWristOffsetVis.GetComponent<BoxCollider>();
                Destroy(leftWristVisCollider);
                leftWristOffsetVis.SetActive(false);
                leftWristOffsetVis.transform.localScale = Vector3.one * 0.01f;

                rightWristOffsetVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                BoxCollider rightWristVisCollider = rightWristOffsetVis.GetComponent<BoxCollider>();
                Destroy(rightWristVisCollider);
                rightWristOffsetVis.SetActive(false);
                rightWristOffsetVis.transform.localScale = Vector3.one * 0.01f;
            }

            if(doKnuckleOffsetDebug)
			{
                leftKnuckleOffsetVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider leftKnuckleCollider = leftKnuckleOffsetVis.GetComponent<SphereCollider>();
                Destroy(leftKnuckleCollider);
                leftKnuckleOffsetVis.SetActive(false);
                leftKnuckleOffsetVis.transform.localScale = Vector3.one * 0.01f;

                rightKnuckleOffsetVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider rightKnuckleCollider = rightKnuckleOffsetVis.GetComponent<SphereCollider>();
                Destroy(rightKnuckleCollider);
                rightKnuckleOffsetVis.SetActive(false);
                rightKnuckleOffsetVis.transform.localScale = Vector3.one * 0.01f;
			}
        }

        void UpdateTorso()
		{
            Quatn flattenedNeckRotation = Quatn.Euler(0, head.rotation.eulerAngles.y, 0);
            forwardDirection = flattenedNeckRotation * Vect3.forward;

            if (!isTorsoSmoothing)
            {
                // check to see if we should start smoothing to the current rotation
                float angle = Quatn.Angle(flattenedNeckRotation, torsoRotation);

                if (angle > headRotationThreshold)
				{
                    // start smoothing
                    torsoSmoothTime = 0;
                    isTorsoSmoothing = true;
				}
            }
            else
			{
                torsoSmoothTime += Core.Time.deltaTime;
                float tValue = Mathf.InverseLerp(0, torsoSmoothDuration, torsoSmoothTime);

                torsoRotation = Quatn.Slerp(torsoRotation, flattenedNeckRotation, tValue);

                if(torsoSmoothTime >= torsoSmoothDuration)
				{
                    isTorsoSmoothing = false;
				}
			}

			Vect3 headLocalNeckRef = (Vect3)head.transform.TransformPoint(Vector3.down * neckHeadOffset);
			Vect3 headWorldDownRef = (Vect3)(head.position + (Vector3.down * neckHeadOffset));
            neckPosition = (headLocalNeckRef + headWorldDownRef) * 0.5f; // this can SO be improved

            leftShoulder = neckPosition + ((torsoRotation * Vect3.left) * neckHeadOffset);
            rightShoulder = neckPosition + ((torsoRotation * Vect3.right) * neckHeadOffset);

            // get our comfy palm up directions
            leftPalmComfyUp = Quatn.AngleAxis(-palmComfyUpOffset, Vect3.forward) * Vect3.up;
            rightPalmComfyUp = Quatn.AngleAxis(palmComfyUpOffset, Vect3.forward) * Vect3.up;

            leftPalmComfyUp = flattenedNeckRotation * leftPalmComfyUp;
            rightPalmComfyUp = flattenedNeckRotation * rightPalmComfyUp;

            // get our palm diagonal directions
            leftPalmDiagonal = Quatn.AngleAxis(-palmDiagonalOffset, Vect3.forward) * Vect3.up;
            rightPalmDiagonal = Quatn.AngleAxis(palmDiagonalOffset, Vect3.forward) * Vect3.up;

            leftPalmDiagonal = flattenedNeckRotation * leftPalmDiagonal;
            rightPalmDiagonal = flattenedNeckRotation * rightPalmComfyUp;
        }

        void UpdateRaycastPoses()
		{
            if(leftHand.IsTracking)
			{
                // left wrist offset
                HandData leftHandData = leftHand.GetHandData();
                PoseIC leftWrist = leftHandData.WristPose;

				Vect3 leftWristOffset = wristOffset;
                leftWristOffset = leftWrist.rotation * leftWristOffset;
                leftWristOffset += leftWrist.position;

				Vect3 leftOrigin = Vect3.Lerp(leftShoulder, leftWristOffset, 0.532f);
                leftOrigin = leftRayOriginFilter.Filter(leftOrigin, Core.Time.time);

				Vect3 leftKnucklePos = leftHandData.IndexJoints[(int)JointType.Proximal].Pose.position +
                    (leftHand.GetAnchorPose(AnchorPoint.Palm).rotation * knuckleOffset);
                leftKnucklePos = leftAimPosFilter.Filter(leftKnucklePos, Core.Time.time);
                leftAimPosition = leftKnucklePos;

				Vect3 leftDirection = (leftKnucklePos - leftOrigin).normalized;
                leftHandRay.direction = leftDirection;
                leftHandRay.position = leftOrigin;

                if(doKnuckleOffsetDebug)
				{
                    leftKnuckleOffsetVis.transform.position = (Vector3)leftKnucklePos;
                    leftKnuckleOffsetVis.SetActive(true);
				}

                if (leftWristOffsetVis) leftWristOffsetVis.transform.position = (Vector3)leftWristOffset;
            }
            else
			{
                if (leftKnuckleOffsetVis) leftKnuckleOffsetVis.SetActive(false);
			}

            if (leftWristOffsetVis) leftWristOffsetVis.SetActive(leftHand.IsTracking);

            if (rightHand.IsTracking)
			{
                HandData rightHandData = rightHand.GetHandData();
                PoseIC rightWrist = rightHandData.WristPose;

				Vect3 rightWristOffset = -wristOffset;
                rightWristOffset = rightWrist.rotation * rightWristOffset;
                rightWristOffset += rightWrist.position;

				Vect3 rightOrigin = Vect3.Lerp(rightShoulder, rightWristOffset, 0.532f);
                rightOrigin = rightRayOriginFilter.Filter(rightOrigin, Core.Time.time);

				Vect3 rightKnucklePos = rightHandData.IndexJoints[(int)JointType.Proximal].Pose.position + 
                    (rightHand.GetAnchorPose(AnchorPoint.Palm).rotation * knuckleOffset);
                rightAimPosFilter.Filter(rightKnucklePos, Core.Time.time);
                rightAimPosition = rightKnucklePos;

				Vect3 rightDirection = (rightKnucklePos - rightOrigin).normalized;
                rightHandRay.direction = rightDirection;
                rightHandRay.position = rightOrigin;

                if(doKnuckleOffsetDebug)
				{
                    rightKnuckleOffsetVis.transform.position = (Vector3)rightKnucklePos;
                    rightKnuckleOffsetVis.SetActive(true);
				}

                if (rightWristOffsetVis) rightWristOffsetVis.transform.position = (Vector3)rightWristOffset;
			}
            else
			{
                if (rightKnuckleOffsetVis) rightKnuckleOffsetVis.SetActive(false);
			}

            if (rightWristOffsetVis) rightWristOffsetVis.SetActive(rightHand.IsTracking);
		}

        // Update is called once per frame
        void Update()
        {
            // track virtual torso
            UpdateTorso();
            UpdateRaycastPoses();

            // allow avatar switching
            if(UnityEngine.Input.GetKeyUp(avatarSwitchKey))
			{
                if (handAvatar == HandAvatar.Glove) handAvatar = HandAvatar.Capsule;
                else handAvatar = HandAvatar.Glove;
            }
        }

		private void OnDrawGizmos()
		{
#if UNITY
			if (head)
            {
                // draw our yaw line
                Vector3 neckDebugPoint = (Vector3)neckPosition;
                Quaternion flattenedNeckRotation = Quaternion.Euler(0, head.rotation.eulerAngles.y, 0);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(neckDebugPoint,
                    neckDebugPoint + (flattenedNeckRotation * Vector3.forward) * 0.22f);

                Vector3 thresholdMin = ((Quaternion)torsoRotation * Vector3.forward);
                thresholdMin = Quaternion.AngleAxis(-headRotationThreshold, Vector3.up) * thresholdMin;
                Vector3 thresholdMax = ((Quaternion)torsoRotation * Vector3.forward);
                thresholdMax = Quaternion.AngleAxis(headRotationThreshold, Vector3.up) * thresholdMax;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(neckDebugPoint,
                    neckDebugPoint + (thresholdMin * 0.22f));
                Gizmos.DrawLine(neckDebugPoint,
                    neckDebugPoint + (thresholdMax * 0.22f));

                Gizmos.DrawWireSphere((Vector3)leftShoulder, 0.01f);
                Gizmos.DrawWireSphere((Vector3)rightShoulder, 0.01f);

                Vector3 leftUpStartPos = head.position + (Vector3.down * 0.3f);
                leftUpStartPos += ((Quaternion)torsoRotation * Vector3.left) * 0.2f;
                Vector3 leftUpEndPos = leftUpStartPos + ((Vector3)leftPalmComfyUp * 0.1f);

                Vector3 rightUpStartPos = head.position + (Vector3.down * 0.3f);
                rightUpStartPos += ((Quaternion)torsoRotation * Vector3.right) * 0.2f;
                Vector3 rightUpEndPos = rightUpStartPos + ((Vector3)rightPalmComfyUp * 0.1f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(leftUpStartPos, leftUpEndPos);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rightUpStartPos, rightUpEndPos);
            }
#endif
		}
	}
}