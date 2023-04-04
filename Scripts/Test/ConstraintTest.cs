using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ConstraintTest : MonoBehaviour
{
    public Transform constraintReference;
    public Vector3 pointA;
    public Vector3 pointB;

    void Update()
    {
        // Get the direction vector of the line segment
        Vector3 lineDirection = (pointB - pointA).normalized;

        // Get the vector from point A to the reference point
        Vector3 referenceOffset = constraintReference.position - pointA;

        // Project the reference offset vector onto the line direction vector
        float projectedDistance = Vector3.Dot(referenceOffset, lineDirection);
        Vector3 projectedOffset = lineDirection * projectedDistance;

        // Clamp the projected offset vector to the length of the line segment
        float lineLength = Vector3.Distance(pointA, pointB);
        float clampedDistance = Mathf.Clamp(projectedDistance, 0f, lineLength);
        Vector3 clampedOffset = lineDirection * clampedDistance;

        // Calculate the final constrained position by adding the clamped offset to point A
        Vector3 constrainedPosition = pointA + clampedOffset;

        // Set the position of the constraint reference transform
        constraintReference.position = constrainedPosition;
    }

    private void OnDrawGizmos()
	{
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(constraintReference.position, 0.1f);
	}
}
