using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapNormalTest : MonoBehaviour
{
    [SerializeField] Transform normalRef;
	[SerializeField] bool drawGizmos = false;

	public enum Axis : byte
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		All = 7
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	Axis GetSnapAxisForNormal(Vector3 objectNormal, out bool positive)
	{
		positive = false;

		Vector3 localNormal = transform.InverseTransformDirection(objectNormal);

		float maxComponent = Mathf.Max(Mathf.Abs(localNormal.x), Mathf.Abs(localNormal.y), Mathf.Abs(localNormal.z));

		bool xMax = maxComponent == Mathf.Abs(localNormal.x);
		bool yMax = maxComponent == Mathf.Abs(localNormal.y);
		bool zMax = maxComponent == Mathf.Abs(localNormal.z);
		
		if(xMax)
		{
			positive = (Mathf.Sign(localNormal.x) > 0) ? true : false;
			return Axis.X;
		}
		else if (yMax)
		{
			positive = (Mathf.Sign(localNormal.y) > 0) ? true : false;
			return Axis.Y;
		}
		else
		{
			positive = (Mathf.Sign(localNormal.z) > 0) ? true : false;
			return Axis.Z;
		}
	}

	Axis GetForwardAndRightAxisForNormal(Axis normalAxis, bool normalPositive,
	out bool forwardPositive, out Axis rightAxis, out bool rightPositive)
	{
		switch (normalAxis)
		{
			case Axis.X:
				rightAxis = Axis.Y;
				rightPositive = normalPositive;
				forwardPositive = normalPositive;
				return Axis.Z;

			case Axis.Y:
				rightAxis = Axis.X;
				rightPositive = normalPositive;
				forwardPositive = normalPositive;
				return Axis.Z;

			case Axis.Z:
				forwardPositive = normalPositive;
				rightPositive = normalPositive;
				rightAxis = Axis.Y;
				return Axis.X;

			default:
				forwardPositive = true;
				rightAxis = Axis.X;
				rightPositive = true;
				return Axis.Z;
		}
	}

	Vector3 GetVectorForAxis(Axis axis, bool positive)
	{
		switch (axis)
		{
			case Axis.None:
				return Vector3.zero;
			case Axis.X:
				return Vector3.right * ((positive) ? 1 : -1);
			case Axis.Y:
				return Vector3.up * ((positive) ? 1 : -1);
			case Axis.Z:
				return Vector3.forward * ((positive) ? 1 : -1);
			case Axis.All:
				return Vector3.one;
			default:
				return Vector3.zero;
		}
	}

	private void OnDrawGizmos()
	{
		if (drawGizmos)
		{
			bool normalPositive;
			Axis objectNormalLocalAxis = GetSnapAxisForNormal(normalRef.up, out normalPositive);

			Vector3 objectNormalLocal = GetVectorForAxis(objectNormalLocalAxis, normalPositive);

			bool objectForwardPositive, objectRightPositive;
			Axis objectForwardAxis, ObjectRightAxis;

			objectForwardAxis = GetForwardAndRightAxisForNormal(objectNormalLocalAxis,
				normalPositive, out objectForwardPositive, out ObjectRightAxis, out objectRightPositive);

			Vector3 objectForward, objectRight;
			objectForward = GetVectorForAxis(objectForwardAxis, objectForwardPositive);
			objectRight = GetVectorForAxis(ObjectRightAxis, objectRightPositive);

			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(transform.position, transform.TransformDirection(objectNormalLocal));

			Gizmos.color = Color.blue;
			Gizmos.DrawRay(transform.position, transform.TransformDirection(objectForward));

			Gizmos.color = Color.red;
			Gizmos.DrawRay(transform.position, transform.TransformDirection(objectRight));
		}
	}
}
