using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToFromRotationTest : MonoBehaviour
{
    [SerializeField] BoxCollider surface;
    Pose defaultPose;

    bool isRotating = false;

    float rotationTimer = 0;
    const float rotationDuration = 0.25f;

    Quaternion offset;

    [SerializeField] bool startRotating = false;
    [SerializeField] bool stopRotating = false;

    [SerializeField] bool doOffset2=false;

    // Start is called before the first frame update
    void Start()
    {
        defaultPose = new Pose(transform.position, transform.rotation);
    }

    Quaternion FromToRotation(Vector3 from, Vector3 to, float multiplier)
    {
        return Quaternion.AngleAxis(AccurateAngleBetween(to, from) * multiplier, Vector3.Cross(from, to));
    }

    float AccurateAngleBetween(Vector3 from, Vector3 to)
    {
        return RadiansToDegrees(Mathf.Atan2(Vector3.Magnitude(Vector3.Cross(from, to)), Vector3.Dot(from, to)));
    }

     float RadiansToDegrees(float radians)
    {
        return radians * Mathf.Rad2Deg;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isRotating)
		{
            if(startRotating)
			{
                Vector3 upNormal = transform.up;

                // calculate our offset
                offset = FromToRotation(transform.up,
                    surface.transform.up, 1);

                if(doOffset2)
				{
                    Quaternion rotated = defaultPose.rotation * offset;
                    upNormal = rotated * upNormal;

                    Quaternion offset2 = FromToRotation(upNormal, surface.transform.up, 1);

                    Debug.Log(string.Format("offset2 angle: {0}", Quaternion.Angle(offset, offset2)));

                    offset = offset * offset2;
				}

				isRotating = true;
                rotationTimer = 0;

                startRotating = false;
            }
		}
		else
		{
            rotationTimer += Instrumental.Core.Time.deltaTime;
            rotationTimer = Mathf.Clamp(rotationTimer, 0, rotationDuration);
            float rotationTValue = Mathf.InverseLerp(0, rotationDuration, rotationTimer);

            Quaternion rotation = offset * defaultPose.rotation;

            transform.rotation = Quaternion.Slerp(defaultPose.rotation, rotation, rotationTValue);

            if(stopRotating)
			{
                isRotating = false;
                transform.rotation = defaultPose.rotation;
                rotationTimer = 0;

                stopRotating = false;
			}
        }
    }
}
