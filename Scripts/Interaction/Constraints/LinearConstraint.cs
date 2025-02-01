using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Interaction.Constraints
{
    public class LinearConstraint : GraspConstraint
    {
        Vect3 pointA;
		Vect3 pointB;

        public void SetPoints(Vect3 pointA, Vect3 pointB)
		{
            this.pointA = pointA;
            this.pointB = pointB;
		}

		public override PoseIC DoConstraint(PoseIC targetPose)
		{
			// Get the direction vector of the line segment
			Vect3 lineDirection = (pointB - pointA);
            float lineLength = lineDirection.magnitude;
            lineDirection /= lineLength;

			// Get the vector from point A to the reference point
			Vect3 referenceOffset = targetPose.position - pointA;

            // Project the reference offset vector onto the line direction vector
            float projectedDistance = Vect3.Dot(referenceOffset, lineDirection);
			Vect3 projectedOffset = lineDirection * projectedDistance;

            // Clamp the projected offset vector to the length of the line segment
            float clampedDistance = Mathf.Clamp(projectedDistance, 0f, lineLength);
			Vect3 clampedOffset = lineDirection * clampedDistance;

			// Calculate the final constrained position by adding the clamped offset to point A
			Vect3 constrainedPosition = pointA + clampedOffset;

            return new PoseIC(constrainedPosition, targetPose.rotation);
        }
    }
}