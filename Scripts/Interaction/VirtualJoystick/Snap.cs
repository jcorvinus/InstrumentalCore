using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Overlay;
using Instrumental.Interaction;
using Instrumental.Interaction.Constraints;
using Instrumental.Interaction.VirtualJoystick;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class Snap : MonoBehaviour
    {
        [SerializeField] Joystick joystick;
        LeftMasterJoystick joystickMaster;
        InputDataSources inputDataSources;

        [SerializeField] LineRenderer outboundRenderer;
        bool isDeployed = false;
        float deployedTime = 0;
        const float deployedTimeDuration = 0.15f;
        Vector3 deployementSourcePosition;
        bool freshDeployment = true;
        Vector3 velocity;
        Vector3 direction;

        Vector3[] linePoints;

        [SerializeField] InteractiveItem handle;
        SphereCollider handleCollider;
        [Range(0, 1)]
        [SerializeField] float graspDistance = 0.21f;
        LinearConstraint linearConstraint;
        [Range(0,1)]
        [SerializeField] float shoulderHeadRightBlend = 0.797f;

        // snap amount signifiers
        [SerializeField] Transform signifierContainer;
        [SerializeField] ConeSignifier leftCone;
        [SerializeField] ConeSignifier rightCone;

        [Range(0,0.1f)]
        [SerializeField] float coneVisDistance = 0.042f;
        [Range(0, 0.1f)]
        [SerializeField] float coneUpOffset = 0.0376f;
        const float coneSnapDist = 0.023f;
        private bool isSnapLeft = false;
        private bool isSnapRight = false;
        int snapLeftIndex;
        int snapRightIndex;

        AudioSource snapSource;
        [SerializeField] AudioClip[] snapClips;

        public bool IsDeployed { get { return isDeployed; } }

        public bool IsSnapActive { get { return isSnapLeft || isSnapRight; } }

        public Vector2 Value 
        { 
            get 
            {
                if (isSnapLeft) return Vector2.left;
                else if (isSnapRight) return Vector2.right;
                else return Vector2.zero;
            }
        }

		private void Awake()
		{
            joystickMaster = joystick.GetComponentInParent<LeftMasterJoystick>();
            linePoints = new Vector3[2];
            linearConstraint = handle.GetComponent<LinearConstraint>();
            handleCollider = handle.GetComponent<SphereCollider>();
            snapSource = GetComponent<AudioSource>();

            inputDataSources = GetComponent<InputDataSources>();

            if(inputDataSources)
			{
                snapLeftIndex = inputDataSources.GetIndexForDataSource("SnapLeft");
                snapRightIndex = inputDataSources.GetIndexForDataSource("SnapRight");
			}
        }

		// Start is called before the first frame update
		void Start()
        {
            StopDeployment();
            joystick.OnMoved += () =>
            {
                StopDeployment();
                StartDeployment();
            };
        }

        public void StartDeployment()
        {
            isDeployed = true;
            deployedTime = 0;
            deployementSourcePosition = transform.position;
            freshDeployment = true;
            handle.transform.localScale = Vector3.one * Mathf.Epsilon;
            handle.gameObject.SetActive(true);
            direction = GetDirection();
        }

        public void StopDeployment()
		{
            isDeployed = false;
            deployedTime = 0;
            outboundRenderer.enabled = false;
            handle.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (isDeployed)
            {
                // check to see if we should un-deploy
                if (joystick) // snap hand opener has its own un-deploy capability (or will eventually)
                {
                    if(!joystick.gameObject.activeInHierarchy)
					{
                        StopDeployment();
                        return;
					}
                }

                // if no, we should also check for un-usable states,
                // such as being un-grasped already, and the shrinking state of the joystick

                deployedTime += Time.deltaTime;

                if(handle.IsGrasped)
				{
                    deployedTime = 0;
                    freshDeployment = false;
                    deployementSourcePosition = handle.transform.position;
				}

                float tValue = Mathf.InverseLerp(0, deployedTimeDuration, deployedTime);
                Vector3 targetPosition = GetTargetPosition(tValue);
                Vector3 startPosition = GetStartPosition();

                if (!handle.IsGrasped) // don't move if grasped
                {
                    if (freshDeployment)
                    {
                        handle.transform.localScale = Vector3.one * tValue;
                        handle.transform.position = targetPosition;
                    }
                    else
                    {
                        // smoothdamp back to position
                        handle.transform.position = Vector3.Lerp(deployementSourcePosition, targetPosition,
                            Mathf.InverseLerp(0, deployedTimeDuration, deployedTime));
                    }

                    signifierContainer.gameObject.SetActive(false);
                    isSnapLeft = false;
                    isSnapRight = false;
                }
                else
				{
                    // handle our cone signifiers and 
                    // actual processing of snap turning
                    signifierContainer.gameObject.SetActive(true);

                    // we can solve the 'where do the cones go?' problem by moving them upwards
                    // a lot simpler than the other ideas I'd had in mind.
                    Vector3 coneRef = GetTargetPosition(1) + (direction * handleCollider.radius);

                    // place our left and right cones
                    Vector3 leftConeLinePosition = coneRef + (direction * -coneVisDistance);
                    leftCone.transform.position = leftConeLinePosition + (Vector3.up * coneUpOffset);
                    leftCone.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);

                    Vector3 rightConeLinePosition = coneRef + (direction * coneVisDistance);
                    rightCone.transform.position = coneRef + (Vector3.up * coneUpOffset);
                    rightCone.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

                    // get our distance. Do it relative to the start point since we know that can
                    // be done in one distance check instead of two and doesn't have the double-bounds problem
                    // of my old check. We can also use offset from grab dist to get the baseline.
                    float dist = Vector3.Distance(handle.transform.position, startPosition);
                    float zeroed = dist - graspDistance;

                    float leftScale = (zeroed > 0) ? 0 : Mathf.InverseLerp(0, coneSnapDist, Mathf.Abs(zeroed));
                    float rightScale = (zeroed < 0) ? 0 : Mathf.InverseLerp(0, coneSnapDist, Mathf.Abs(zeroed));
                    leftCone.Scale = leftScale;
                    rightCone.Scale = rightScale;

                    bool previousSnapLeft, previousSnapRight;
                    previousSnapLeft = isSnapLeft;
                    previousSnapRight = isSnapRight;

                    isSnapLeft = leftScale > 0.99f;
                    isSnapRight = rightScale > 0.99f;

                    if(previousSnapLeft != isSnapLeft || previousSnapRight != isSnapRight)
					{
                        PlaySnapSound();
					}
                }

                // handle line renderer stuff
                linePoints[0] = startPosition;
                linePoints[1] = targetPosition;
                outboundRenderer.SetPositions(linePoints);
                outboundRenderer.enabled = true;
                linearConstraint.SetPoints(linePoints[0], GetMaxConstraintPos());
            }
            else
			{
                if (joystick) // snap hand opener has its own deploy capability
                {
                    if(joystick.gameObject.activeInHierarchy)
					{
                        StartDeployment();
                        return;
					}
                }

                signifierContainer.gameObject.SetActive(false);
                isSnapLeft = false;
                isSnapRight = false;
            }

            if(inputDataSources)
			{
                inputDataSources.SetData(snapLeftIndex, isSnapLeft);
                inputDataSources.SetData(snapRightIndex, isSnapRight);
			}
        }

        void PlaySnapSound()
		{
            int snapSoudIndex = Random.Range(0, snapClips.Length);

            snapSource.PlayOneShot(snapClips[snapSoudIndex]);
		}

        Vector3 GetMaxConstraintPos()
		{
            return GetStartPosition() + (direction);
		}

        Vector3 GetTargetPosition(float tValue)
		{
            return GetStartPosition() +
                (direction * (graspDistance * tValue));
        }

        Vector3 GetDirection()
		{
            if (Application.isPlaying)
            {
                if (joystick)
                {
                    // get our joystick relative position
                    Vector3 shoulderPoint = InstrumentalBody.Instance.RightShoulder;
                    Vector3 shoulderDirection = (shoulderPoint - joystick.transform.position);
                    shoulderDirection = Vector3.Scale(shoulderDirection, new Vector3(1, 0, 1)).normalized;

                    Vector3 headRight = Vector3.Scale(InstrumentalBody.Instance.Head.right, new Vector3(1, 0, 1)).normalized;

                    return Vector3.Slerp(shoulderDirection, headRight, shoulderHeadRightBlend).normalized;
                }
                else return transform.forward;
            }
            else
			{
                return transform.forward;
			}
		}

        Vector3 GetStartPosition()
		{
            if(Application.isPlaying)
			{
                if (joystick)
                {
                    // get our joystick relative position
                    return joystick.transform.position + (direction * joystickMaster.GetInnerRadius());
                }
                else return transform.position;
			}
            else return transform.position;
		}
	}
}