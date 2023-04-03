using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Constraints
{
    public class CylinderConstraint : GraspConstraint
    {
        [SerializeField] float radius=0.1f;
        [SerializeField] float height = 0.1f;
		[SerializeField] Transform reference;

		public override Pose DoConstraint(Pose targetPose)
		{
			Vector3 position = reference.InverseTransformPoint(targetPose.position);

			Vector3 radial = new Vector3(position.x, 0, position.z);
			float radialMag = radial.magnitude;

			if (radialMag > radius)
			{
				radial /= radialMag;
				radial *= radius;
			}

			position = new Vector3(radial.x, Mathf.Min(position.y, height), radial.z);

			return new Pose(reference.TransformPoint(position), targetPose.rotation);
		}

		private void OnDrawGizmos()
		{
			// draw our cylinder
			if(reference)
			{
				DebugExtension.DrawCylinder(reference.position, reference.position +
					reference.up * height, radius);
			}
		}
	}
}