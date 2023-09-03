using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Test
{
    public class AngularVelocityComparison : MonoBehaviour
    {
        [SerializeField] Transform rotationA;
        [SerializeField] Transform rotationB;

        [SerializeField] bool doCalculation;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        Vector3 LeapAngularCalculation(Quaternion startRotation, Quaternion destinationRotation)
		{
            float deltaTime = 0.02f;
            Quaternion deltaRotation = destinationRotation * Quaternion.Inverse(startRotation);

            Vector3 deltaAxis;
            float deltaAngle;

            deltaRotation.ToAngleAxis(out deltaAngle, out deltaAxis);

            if(float.IsInfinity(deltaAxis.x))
			{
                deltaAxis = Vector3.zero;
                deltaAngle = 0;
			}

            if(deltaAngle > 180)
			{
                deltaAngle -= 360.0f;
			}

            Vector3 angularVelocity = deltaAxis * deltaAngle * Mathf.Deg2Rad / deltaTime;

            return angularVelocity;
        }

        Vector3 VRTKAngularCalculation(Quaternion startRotation, Quaternion destinationRotation)
        {
            float deltaTime = 0.02f;

            float velocityFactor = 1.0f / deltaTime;

            Quaternion offsetRotation = startRotation * Quaternion.Inverse(destinationRotation);
            float theta = 2.0f * Mathf.Acos(Mathf.Clamp(offsetRotation.w, -1.0f, 1.0f));

            if (theta > Mathf.PI)
            {
                theta -= 2.0f * Mathf.PI;
            }

            Vector3 angularVelocity = new Vector3(offsetRotation.x, offsetRotation.y,
                offsetRotation.z);

            if (angularVelocity.sqrMagnitude > 0.0f)
            {
                angularVelocity = theta * velocityFactor * angularVelocity.normalized;
            }

            return angularVelocity;
        }

        void DoCalculation()
		{
            Vector3 leapAngularVelocity = LeapAngularCalculation(rotationA.rotation, rotationB.rotation);
            Vector3 vrtkAngularVelocity = VRTKAngularCalculation(rotationA.rotation, rotationB.rotation);

            Debug.Log(string.Format("leap angular velocity: {0}   vrtk angular velocity: {1}",
                leapAngularVelocity, vrtkAngularVelocity));
		}

		private void DrawBasis(Transform pose)
		{
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pose.position, pose.forward * 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pose.position, pose.up * 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pose.position, pose.right * 0.1f);
		}

		private void OnDrawGizmos()
		{
            DrawBasis(rotationA);
            DrawBasis(rotationB);

            if(doCalculation)
			{
                doCalculation = false;
                DoCalculation();
			}
		}
	}
}