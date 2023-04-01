using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Interaction.VirtualJoystick;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class Snap : MonoBehaviour
    {
        [SerializeField] Joystick joystick;
        LeftMasterJoystick joystickMaster;
        Transform headTransform;

        [SerializeField] LineRenderer outboundRenderer;
        bool isDeployed = false;
        float deployedTime = 0;
        const float deployedTimeDuration = 0.15f;
        Vector3 deployementSourcePosition;
        bool freshDeployment = true;
        Vector3 velocity;

        [SerializeField] GraspableItem handle;
        [Range(0, 1)]
        [SerializeField] float graspDistance = 0.21f;
        [Range(0, 0.1f)]
        [SerializeField] float graspRadius = 0.1f;

        public bool IsDeployed { get { return isDeployed; } }

		private void Awake()
		{
            joystickMaster = joystick.GetComponentInParent<LeftMasterJoystick>();
		}

		// Start is called before the first frame update
		void Start()
        {
            headTransform = Interaction.InstrumentalBody.Instance.Head;
            StopDeployment();
        }

        public void StartDeployment()
        {
            isDeployed = true;
            deployedTime = 0;
            deployementSourcePosition = transform.position;
            freshDeployment = true;
            handle.transform.localScale = Vector3.one * Mathf.Epsilon;
            handle.gameObject.SetActive(true);
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
                }
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
            }
        }

        Vector3 GetTargetPosition(float tValue)
		{
            return GetStartPosition() +
                (GetDirection() * (graspDistance * tValue));
        }

        Vector3 GetDirection()
		{
            if (Application.isPlaying)
            {
                if (joystick)
                {
                    // get our joystick relative position
                    Vector3 headRight = headTransform.right;
                    headRight = Vector3.Scale(headRight, new Vector3(1, 0, 1)).normalized;

                    return headRight;
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
                    Vector3 direction = GetDirection();

                    return joystick.transform.position + (direction * joystickMaster.GetInnerRadius());
                }
                else return transform.position;
			}
            else return transform.position;
		}

		private void OnDrawGizmos()
		{
            Vector3 direction = GetDirection();
            Vector3 startPosition = GetStartPosition();
            Vector3 graspPoint = GetTargetPosition(1);
            Gizmos.DrawLine(startPosition, graspPoint);
            Gizmos.DrawWireSphere(graspPoint, graspRadius);
        }
	}
}