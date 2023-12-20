using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction.Input;

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

        Quaternion torsoRotation;
        const float headRotationThreshold = 24.45f;
        const float neckHeadOffset = 0.11f;
        bool isTorsoSmoothing = false;
        float torsoSmoothTime = 0;
        const float torsoSmoothDuration = 1f;
        Vector3 neckPosition;

        private Vector3 leftShoulder;
        private Vector3 rightShoulder;
        private Vector3 forwardDirection;
        private Quaternion noRollHeadRotation;

		#region Hand Rays
		private OneEuroFilter<Vector3> leftAimPosFilter;
        private OneEuroFilter<Vector3> leftRayOriginFilter;

        private OneEuroFilter<Vector3> rightAimPosFilter;
        private OneEuroFilter<Vector3> rightRayOriginFilter;

        const float filterBeta = 100;
        const float filterMinCutoff = 5;
        const float filterFreq = 30;

        Vector3 wristOffset = new Vector3(0.0425f, 0.0f, 0.0f);
        Vector3 knuckleOffset = new Vector3 { x = 0.0f, y = 0.02f, z = 0.05f };
        bool doKnuckleOffsetDebug = false;
        private GameObject leftKnuckleOffsetVis;
        private GameObject rightKnuckleOffsetVis;

        private Vector3 leftAimPosition;
        private Vector3 rightAimPosition;
        private Ray leftHandRay;
        private Ray rightHandRay;

        bool doWristOffsetDebug = false;
        private GameObject leftWristOffsetVis;
        private GameObject rightWristOffsetVis;
		#endregion

		[Range(0, 45)]
        [SerializeField] float palmComfyUpOffset = 21;
        const float palmDiagonalOffset = 45f;
        Vector3 leftPalmComfyUp = Vector3.up;
        Vector3 rightPalmComfyUp = Vector3.up;
        Vector3 leftPalmDiagonal = Vector3.up;
        Vector3 rightPalmDiagonal = Vector3.up;

        [SerializeField] HandAvatar handAvatar = HandAvatar.Glove;
        [SerializeField] KeyCode avatarSwitchKey = KeyCode.F1;

        public Transform Head { get { return head; } }

        public Vector3 LeftShoulder { get { return leftShoulder; } }
        public Vector3 RightShoulder { get { return rightShoulder; } }

        public Vector3 LeftPalmComfyUp { get { return leftPalmComfyUp; } }

        public Vector3 RightPalmComfyUp { get { return rightPalmComfyUp; } }

        public Vector3 LeftPalmDiagonal { get { return leftPalmDiagonal; } }
        public Vector3 RightPalmDiagonal { get { return rightPalmDiagonal; } }

        public Vector3 ForwardDirection { get { return forwardDirection; } }

        public Vector3 LeftAimPosition { get { return leftAimPosition; } }
        public Vector3 RightAimPosition { get { return rightAimPosition; } }
        public Ray LeftHandRay { get { return leftHandRay; } }
        public Ray RightHandRay { get { return rightHandRay; } }

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
            torsoRotation = Quaternion.Euler(0, head.rotation.eulerAngles.y, 0);
            neckPosition = head.position + (Vector3.down * neckHeadOffset);

            leftAimPosFilter = new OneEuroFilter<Vector3>(filterFreq, filterMinCutoff, filterBeta);
            leftRayOriginFilter = new OneEuroFilter<Vector3>(filterFreq, filterMinCutoff, filterBeta);

            rightAimPosFilter = new OneEuroFilter<Vector3>(filterFreq, filterMinCutoff, filterBeta);
            rightRayOriginFilter = new OneEuroFilter<Vector3>(filterFreq, filterMinCutoff, filterBeta);

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
            Quaternion flattenedNeckRotation = Quaternion.Euler(0, head.rotation.eulerAngles.y, 0);
            forwardDirection = flattenedNeckRotation * Vector3.forward;

            if (!isTorsoSmoothing)
            {
                // check to see if we should start smoothing to the current rotation
                float angle = Quaternion.Angle(flattenedNeckRotation, torsoRotation);

                if (angle > headRotationThreshold)
				{
                    // start smoothing
                    torsoSmoothTime = 0;
                    isTorsoSmoothing = true;
				}
            }
            else
			{
                torsoSmoothTime += Time.deltaTime;
                float tValue = Mathf.InverseLerp(0, torsoSmoothDuration, torsoSmoothTime);

                torsoRotation = Quaternion.Slerp(torsoRotation, flattenedNeckRotation, tValue);

                if(torsoSmoothTime >= torsoSmoothDuration)
				{
                    isTorsoSmoothing = false;
				}
			}

            Vector3 headLocalNeckRef = head.transform.TransformPoint(Vector3.down * neckHeadOffset);
            Vector3 headWorldDownRef = head.position + (Vector3.down * neckHeadOffset);
            neckPosition = (headLocalNeckRef + headWorldDownRef) * 0.5f; // this can SO be improved

            leftShoulder = neckPosition + ((torsoRotation * Vector3.left) * neckHeadOffset);
            rightShoulder = neckPosition + ((torsoRotation * Vector3.right) * neckHeadOffset);

            // get our comfy palm up directions
            leftPalmComfyUp = Quaternion.AngleAxis(-palmComfyUpOffset, Vector3.forward) * Vector3.up;
            rightPalmComfyUp = Quaternion.AngleAxis(palmComfyUpOffset, Vector3.forward) * Vector3.up;

            leftPalmComfyUp = flattenedNeckRotation * leftPalmComfyUp;
            rightPalmComfyUp = flattenedNeckRotation * rightPalmComfyUp;

            // get our palm diagonal directions
            leftPalmDiagonal = Quaternion.AngleAxis(-palmDiagonalOffset, Vector3.forward) * Vector3.up;
            rightPalmDiagonal = Quaternion.AngleAxis(palmDiagonalOffset, Vector3.forward) * Vector3.up;

            leftPalmDiagonal = flattenedNeckRotation * leftPalmDiagonal;
            rightPalmDiagonal = flattenedNeckRotation * rightPalmComfyUp;
        }

        void UpdateRaycastPoses()
		{
            if(leftHand.IsTracking)
			{
                // left wrist offset
                HandData leftHandData = leftHand.GetHandData();
                Pose leftWrist = leftHandData.WristPose;

                Vector3 leftWristOffset = wristOffset;
                leftWristOffset = leftWrist.rotation * leftWristOffset;
                leftWristOffset += leftWrist.position;

                Vector3 leftOrigin = Vector3.Lerp(leftShoulder, leftWristOffset, 0.532f);
                leftOrigin = leftRayOriginFilter.Filter(leftOrigin, Time.time);

                Vector3 leftKnucklePos = leftHandData.IndexJoints[(int)JointType.Proximal].Pose.position +
                    (leftHand.GetAnchorPose(AnchorPoint.Palm).rotation * knuckleOffset);
                leftKnucklePos = leftAimPosFilter.Filter(leftKnucklePos, Time.time);
                leftAimPosition = leftKnucklePos;

                Vector3 leftDirection = (leftKnucklePos - leftOrigin).normalized;
                leftHandRay.direction = leftDirection;
                leftHandRay.origin = leftOrigin;

                if(doKnuckleOffsetDebug)
				{
                    leftKnuckleOffsetVis.transform.position = leftKnucklePos;
                    leftKnuckleOffsetVis.SetActive(true);
				}

                if (leftWristOffsetVis) leftWristOffsetVis.transform.position = leftWristOffset;
            }
            else
			{
                if (leftKnuckleOffsetVis) leftKnuckleOffsetVis.SetActive(false);
			}

            if (leftWristOffsetVis) leftWristOffsetVis.SetActive(leftHand.IsTracking);

            if (rightHand.IsTracking)
			{
                HandData rightHandData = rightHand.GetHandData();
                Pose rightWrist = rightHandData.WristPose;

                Vector3 rightWristOffset = -wristOffset;
                rightWristOffset = rightWrist.rotation * rightWristOffset;
                rightWristOffset += rightWrist.position;

                Vector3 rightOrigin = Vector3.Lerp(rightShoulder, rightWristOffset, 0.532f);
                rightOrigin = rightRayOriginFilter.Filter(rightOrigin, Time.time);

                Vector3 rightKnucklePos = rightHandData.IndexJoints[(int)JointType.Proximal].Pose.position + 
                    (rightHand.GetAnchorPose(AnchorPoint.Palm).rotation * knuckleOffset);
                rightAimPosFilter.Filter(rightKnucklePos, Time.time);
                rightAimPosition = rightKnucklePos;

                Vector3 rightDirection = (rightKnucklePos - rightOrigin).normalized;
                rightHandRay.direction = rightDirection;
                rightHandRay.origin = rightOrigin;

                if(doKnuckleOffsetDebug)
				{
                    rightKnuckleOffsetVis.transform.position = rightKnucklePos;
                    rightKnuckleOffsetVis.SetActive(true);
				}

                if (rightWristOffsetVis) rightWristOffsetVis.transform.position = rightWristOffset;
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
            if (head)
            {
                // draw our yaw line
                Vector3 neckDebugPoint = neckPosition;
                Quaternion flattenedNeckRotation = Quaternion.Euler(0, head.rotation.eulerAngles.y, 0);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(neckDebugPoint,
                    neckDebugPoint + (flattenedNeckRotation * Vector3.forward) * 0.22f);

                Vector3 thresholdMin = (torsoRotation * Vector3.forward);
                thresholdMin = Quaternion.AngleAxis(-headRotationThreshold, Vector3.up) * thresholdMin;
                Vector3 thresholdMax = (torsoRotation * Vector3.forward);
                thresholdMax = Quaternion.AngleAxis(headRotationThreshold, Vector3.up) * thresholdMax;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(neckDebugPoint,
                    neckDebugPoint + (thresholdMin * 0.22f));
                Gizmos.DrawLine(neckDebugPoint,
                    neckDebugPoint + (thresholdMax * 0.22f));

                Gizmos.DrawWireSphere(leftShoulder, 0.01f);
                Gizmos.DrawWireSphere(rightShoulder, 0.01f);

                Vector3 leftUpStartPos = head.position + (Vector3.down * 0.3f);
                leftUpStartPos += (torsoRotation * Vector3.left) * 0.2f;
                Vector3 leftUpEndPos = leftUpStartPos + (leftPalmComfyUp * 0.1f);

                Vector3 rightUpStartPos = head.position + (Vector3.down * 0.3f);
                rightUpStartPos += (torsoRotation * Vector3.right) * 0.2f;
                Vector3 rightUpEndPos = rightUpStartPos + (rightPalmComfyUp * 0.1f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(leftUpStartPos, leftUpEndPos);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rightUpStartPos, rightUpEndPos);
            }
        }
	}
}