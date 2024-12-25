using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class SnapHandOpener : MonoBehaviour
    {
        Snap snap;
        [SerializeField] Transform otherHand;

		#region Pre-deployment variables
		[SerializeField] Transform projectedTarget;

        [Range(0, -1)]
        [SerializeField] float dotActivation = -0.5f;

        [Range(0,1)]
        [SerializeField] float ringRadius = 0.1f;

        float dot;

        const float facingAnimDuration = 0.15f;
        float facingTime = 0;

        [SerializeField] float maxDistScale=0.125f;
        float minScale = 0.25f;

        [SerializeField] LineRenderer targetProjectionRenderer;
        Vector3[] targetProjectionRendererPoints;
		#endregion

		private void Awake()
		{
            snap = GetComponentInParent<Snap>();
		}

		// Start is called before the first frame update
		void Start()
        {
            projectedTarget.gameObject.SetActive(false);

            targetProjectionRendererPoints = new Vector3[targetProjectionRenderer.positionCount];
        }

        void DoUndeployedLogic()
		{
            dot = Vector3.Dot(otherHand.forward, projectedTarget.forward);

            bool isFacing = dot < dotActivation;

            // do projection
            if (isFacing)
            {
                Plane plane = new Plane(transform.forward, transform.position);

                float center = 0;
                Ray ray = new Ray(otherHand.position, otherHand.forward);
                plane.Raycast(ray, out center);

                Vector3 targetPosition = otherHand.position + (otherHand.forward * center);

                projectedTarget.transform.position = targetPosition;

                float dist = Vector3.Distance(targetPosition, transform.position);

                float distTValue = 1 - Mathf.InverseLerp(ringRadius, maxDistScale, dist);
                projectedTarget.transform.localScale = Vector3.Lerp(Vector3.one * minScale,
                    Vector3.one, distTValue);

                facingTime += Core.Time.deltaTime;
                float facingTValue = Mathf.InverseLerp(0, facingAnimDuration, facingTime);

                // send our points to the line renderer
                for (int i = 0; i < targetProjectionRendererPoints.Length; i++)
                {
                    float tValue = ((float)i / (float)targetProjectionRendererPoints.Length);
                    tValue *= facingTValue;

                    targetProjectionRendererPoints[i] =
                        Vector3.Lerp(otherHand.position, projectedTarget.position, tValue);
                    targetProjectionRenderer.SetPositions(targetProjectionRendererPoints);
                    targetProjectionRenderer.enabled = true;
                }

                if(dist < ringRadius)
				{
                    StartDeployment();
                    return;
				}
            }
            else
            {
                targetProjectionRenderer.enabled = false;
                facingTime = 0;
            }

            projectedTarget.gameObject.SetActive(isFacing);
        }

        void StartDeployment()
		{
            snap.StartDeployment();
        }

        void DoDeployedLogic()
		{
            projectedTarget.gameObject.SetActive(false);
            targetProjectionRenderer.enabled = false;            
		}

        // Update is called once per frame
        void Update()
        {
            if (!snap.IsDeployed)
            {
                DoUndeployedLogic();
            }
            else
			{
                DoDeployedLogic();
			}
        }

		private void OnDrawGizmos()
		{
            DebugExtension.DrawCircle(transform.position, Vector3.forward, maxDistScale);
            DebugExtension.DrawCircle(transform.position, Vector3.forward, Color.blue, ringRadius);
        }
	}
}