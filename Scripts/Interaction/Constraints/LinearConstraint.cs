using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Constraints
{
    public class LinearConstraint : GraspConstraint
    {
        Vector3 pointA;
        Vector3 pointB;

        public void SetPoints(Vector3 pointA, Vector3 pointB)
		{
            this.pointA = pointA;
            this.pointB = pointB;
		}

		public override Pose DoConstraint(Pose targetPose)
		{
            // Get the direction vector of the line segment
            Vector3 lineDirection = (pointB - pointA);
            float lineLength = lineDirection.magnitude;
            lineDirection /= lineLength;

            // Get the vector from point A to the reference point
            Vector3 referenceOffset = targetPose.position - pointA;

            // Project the reference offset vector onto the line direction vector
            float projectedDistance = Vector3.Dot(referenceOffset, lineDirection);
            Vector3 projectedOffset = lineDirection * projectedDistance;

            // Clamp the projected offset vector to the length of the line segment
            float clampedDistance = Mathf.Clamp(projectedDistance, 0f, lineLength);
            Vector3 clampedOffset = lineDirection * clampedDistance;

            // Calculate the final constrained position by adding the clamped offset to point A
            Vector3 constrainedPosition = pointA + clampedOffset;

            return new Pose(constrainedPosition, targetPose.rotation);
        }
    }
}