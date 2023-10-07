using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Space
{

    public class SpaceTest : MonoBehaviour
	{
		public enum TestMode
		{ 
            None,
            Point,
            Rotation,
            Direction
        }

		public enum TestSpace
		{
            None, 
            Local,
            World
		}

        [SerializeField] TransformSpace space;
        [SerializeField] TestSpace testSpace;
        [SerializeField] TestMode mode;

        [SerializeField] Transform localRefPoint; // we use this for testing rectilinear-to-curved space transform
        [SerializeField] Transform worldRefPoint; // we use this world point for testing the curved-to-rectilinear-space transform

        int numberOfPoints = 4;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void DrawTestModePoint()
		{
            Vector3[] localPoints = new Vector3[4];

            localPoints[0] = Vector3.right;
            localPoints[1] = Vector3.Lerp(Vector3.right, Vector3.left, 0.25f);
            localPoints[2] = Vector3.Lerp(Vector3.right, Vector3.left, 0.75f);
            localPoints[3] = Vector3.left;

            Vector3[] worldPoints = new Vector3[4];
            worldPoints[0] = space.transform.TransformPoint(localPoints[0]);
            worldPoints[1] = space.transform.TransformPoint(localPoints[1]);
            worldPoints[2] = space.transform.TransformPoint(localPoints[2]);
            worldPoints[3] = space.transform.TransformPoint(localPoints[3]);

            if (testSpace == TestSpace.Local) // taking local points and transforming them into world mode, then drawing
            {
                if (space)
                {
                    worldPoints[0] = space.TransformPoint(localPoints[0]);
                    worldPoints[1] = space.TransformPoint(localPoints[1]);
                    worldPoints[2] = space.TransformPoint(localPoints[2]);
                    worldPoints[3] = space.TransformPoint(localPoints[3]);

                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(localPoints[0], 0.01f);
                    Gizmos.DrawWireSphere(localPoints[1], 0.01f);
                    Gizmos.DrawWireSphere(localPoints[2], 0.01f);
                    Gizmos.DrawWireSphere(localPoints[3], 0.01f);

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(worldPoints[0], 0.01f);
                    Gizmos.DrawWireSphere(worldPoints[1], 0.01f);
                    Gizmos.DrawWireSphere(worldPoints[2], 0.01f);
                    Gizmos.DrawWireSphere(worldPoints[3], 0.01f);
                }
            }
            else if (testSpace == TestSpace.World) // taking world points and transforming them into local space, then drawing
            {
                if (space)
                {
                    worldPoints[0] = space.TransformPoint(localPoints[0]);
                    worldPoints[1] = space.TransformPoint(localPoints[1]);
                    worldPoints[2] = space.TransformPoint(localPoints[2]);
                    worldPoints[3] = space.TransformPoint(localPoints[3]);

                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(localPoints[0], 0.01f);
                    Gizmos.DrawWireSphere(localPoints[1], 0.01f);
                    Gizmos.DrawWireSphere(localPoints[2], 0.01f);
                    Gizmos.DrawWireSphere(localPoints[3], 0.01f);

                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(worldPoints[0], 0.01f);
                    Gizmos.DrawWireSphere(worldPoints[1], 0.01f);
                    Gizmos.DrawWireSphere(worldPoints[2], 0.01f);
                    Gizmos.DrawWireSphere(worldPoints[3], 0.01f);

                    Vector3 worldToLocal = transform.InverseTransformPoint(worldRefPoint.position);
                    worldToLocal = space.InverseTransformPoint(worldToLocal);

                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(worldRefPoint.position, 0.01f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(worldToLocal, 0.01f);
                }
            }
        }

        void DrawTestModeRotation()
		{
            if(testSpace == TestSpace.Local) // take a local rotation and turn it into a warped one
			{
                // I don't know if this is even what I want to do lmao
                Quaternion inverse = Quaternion.Inverse(space.transform.rotation) * worldRefPoint.rotation; //space.transform.rotation * Quaternion.Inverse(worldRefPoint.rotation);
                Vector3 localPosition = space.transform.InverseTransformPoint(worldRefPoint.position);

                //Vector3 forward, up/*, right*/;
                //forward = space.transform.InverseTransformDirection(worldRefPoint.forward);
                //up = space.transform.InverseTransformDirection(worldRefPoint.up);
                ////right = space.transform.InverseTransformDirection(worldRefPoint.right);

                //Quaternion vectorInverse = Quaternion.LookRotation(forward, up);

                //Gizmos.matrix = Matrix4x4.TRS(worldRefPoint.position, space.transform.rotation, Vector3.one);
                //DrawBasis(Vector3.zero, inverse);

                //// direction inverse looks like it's working well. Not sure why the main inverse isn't
                //Gizmos.matrix = Matrix4x4.TRS(space.transform.position, space.transform.rotation, Vector3.one);
                //DrawBasis(Vector3.zero, vectorInverse);

                Quaternion warpedRotation = space.InverseTransformRotation(localPosition, inverse);

                Gizmos.matrix = Matrix4x4.TRS(space.transform.position, space.transform.rotation, Vector3.one);
                DrawBasis(Vector3.zero, warpedRotation);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else if (testSpace == TestSpace.World) // take a warped roation and turn it into a local one
			{
                Quaternion inverseWarped = space.InverseTransformRotation(worldRefPoint.position, worldRefPoint.rotation);
                Vector3 localPosition = space.transform.InverseTransformPoint(worldRefPoint.position);
                    Vector3 position = space.InverseTransformPoint(localPosition);

                Gizmos.matrix = Matrix4x4.TRS(space.transform.position, space.transform.rotation, Vector3.one);
                DrawBasis(position, inverseWarped);

                // draw our other stuff
                Vector3[] localPoints = new Vector3[4];
                localPoints[0] = Vector3.right;
                localPoints[1] = Vector3.Lerp(Vector3.right, Vector3.left, 0.25f);
                localPoints[2] = Vector3.Lerp(Vector3.right, Vector3.left, 0.75f);
                localPoints[3] = Vector3.left;

                Vector3[] worldPoints = new Vector3[4];
                worldPoints[0] = space.TransformPoint(localPoints[0]);
                worldPoints[1] = space.TransformPoint(localPoints[1]);
                worldPoints[2] = space.TransformPoint(localPoints[2]);
                worldPoints[3] = space.TransformPoint(localPoints[3]);

                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawLine(worldPoints[0], worldPoints[1]);
                Gizmos.DrawLine(worldPoints[1], worldPoints[2]);
                Gizmos.DrawLine(worldPoints[2], worldPoints[3]);
            }
		}

        void DrawBasis(Vector3 position, Quaternion rotation)
		{
            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, position + forward * 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, position + right * 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(position, position + up * 0.1f);
		}

		private void OnDrawGizmos()
		{
			switch (mode)
			{
				case TestMode.None:
					break;
				case TestMode.Point:
                    DrawTestModePoint();
					break;
				case TestMode.Rotation:
                    DrawTestModeRotation();
					break;
				case TestMode.Direction:
					break;
				default:
					break;
			}
		}
	}
}