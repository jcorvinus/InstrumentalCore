using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalToFromTest : MonoBehaviour
{
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] Collider surfaceCollider;
    Pose defaultPose;

    bool isRotating = false;

    float rotationTimer = 0;
    const float rotationDuration = 0.25f;

    Vector3 objectPoint;
    Vector3 surfacePoint;
    Vector3 surfaceNormal;
    Vector3 objectNormal;

    Vector3 objectPointInverse;
    Vector3 objectNormalInverse;

    [SerializeField] bool setDefaultPose=false;
    [SerializeField] bool startRotating = false;
    [SerializeField] bool stopRotating = false;

	// Start is called before the first frame update
	void Start()
    {
        defaultPose = new Pose(boxCollider.transform.position,
            boxCollider.transform.rotation);
    }

    void GetPointsAndSurfaces()
	{
        // get our normals
        surfacePoint = surfaceCollider.ClosestPoint(boxCollider.transform.position);
        objectPoint = boxCollider.ClosestPoint(surfacePoint);

        Vector3 objectToSurfaceDirection = (surfacePoint - objectPoint).normalized;
        Vector3 surfaceToObjectDirection = (objectPoint - surfacePoint).normalized;

        RaycastHit objectToSurfaceHit;
        surfaceCollider.Raycast(new Ray(objectPoint, objectToSurfaceDirection), out objectToSurfaceHit, 20f);
        surfaceNormal = objectToSurfaceHit.normal;

        RaycastHit surfaceToObjectHit;
        boxCollider.Raycast(new Ray(surfacePoint, surfaceNormal), out surfaceToObjectHit, 20f);
        objectNormal = surfaceToObjectHit.normal;
        objectPoint = surfaceToObjectHit.point;

        objectPointInverse = boxCollider.transform.InverseTransformPoint(objectPoint);
        objectNormalInverse = boxCollider.transform.InverseTransformDirection(objectNormal);
    }

    // Update is called once per frame
    void Update()
    {
        if (setDefaultPose)
		{
            setDefaultPose = false;
            defaultPose = new Pose(boxCollider.transform.position,
                boxCollider.transform.rotation);
        }
        if(!isRotating)
		{
            GetPointsAndSurfaces();

            if (startRotating)
            {
                isRotating = true;
                startRotating = false;
            }
        }
        else
		{
            GetPointsAndSurfaces();

            rotationTimer += Time.deltaTime;
            rotationTimer = Mathf.Clamp(rotationTimer, 0, rotationDuration);
            float rotationTValue = Mathf.InverseLerp(0, rotationDuration, rotationTimer);

            Quaternion offsetRotation = Quaternion.FromToRotation(objectNormal * -1, surfaceNormal);
            Quaternion rotation = boxCollider.transform.rotation * offsetRotation;
            boxCollider.transform.rotation = Quaternion.Slerp(boxCollider.transform.rotation, rotation, rotationTValue);

            if(stopRotating)
			{
                stopRotating = false;
                isRotating = false;
                rotationTimer = 0;
                boxCollider.transform.rotation = defaultPose.rotation;
			}
		}
    }

	private void OnDrawGizmos()
	{
        if (!isRotating)
        {
            Gizmos.DrawLine(objectPoint, objectPoint + (objectNormal * 0.02f));
        }
        else
		{
            Vector3 objectPointWorld = boxCollider.transform.TransformPoint(objectPointInverse);
            Vector3 objectNormalWorld = boxCollider.transform.TransformDirection(objectNormalInverse);
            Gizmos.DrawLine(objectPointWorld,
                objectPointWorld + (objectNormalWorld * 0.02f));
        }
        Gizmos.DrawLine(surfacePoint, surfacePoint + (surfaceNormal * 0.02f));
	}
}
