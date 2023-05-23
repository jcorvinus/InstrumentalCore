using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        [Range(0, 45)]
        [SerializeField] float palmComfyUpOffset = 21;
        Vector3 leftPalmComfyUp = Vector3.up;
        Vector3 rightPalmComfyUp = Vector3.up;

        [SerializeField] HandAvatar handAvatar = HandAvatar.Glove;
        [SerializeField] KeyCode avatarSwitchKey = KeyCode.F1;

        public Transform Head { get { return head; } }

        public Vector3 LeftShoulder { get { return leftShoulder; } }
        public Vector3 RightShoulder { get { return rightShoulder; } }

        public Vector3 LeftPalmComfyUp { get { return leftPalmComfyUp; } }

        public Vector3 RightPalmComfyUp { get { return rightPalmComfyUp; } }

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
        }

        void UpdateTorso()
		{
            Quaternion flattenedNeckRotation = Quaternion.Euler(0, head.rotation.eulerAngles.y, 0);

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
        }

        // Update is called once per frame
        void Update()
        {
            // track virtual torso
            UpdateTorso();


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