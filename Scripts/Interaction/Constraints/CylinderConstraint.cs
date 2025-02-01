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
    public class CylinderConstraint : GraspConstraint
    {
        [SerializeField] float radius=0.1f;
        [SerializeField] float height = 0.1f;
		[SerializeField] Transform reference;

		public override PoseIC DoConstraint(PoseIC targetPose)
		{
			Vect3 position = (Vect3)reference.InverseTransformPoint((Vector3)targetPose.position);

			Vect3 radial = new Vect3(position.x, 0, position.z);
			float radialMag = radial.magnitude;

			if (radialMag > radius)
			{
				radial /= radialMag;
				radial *= radius;
			}

			position = new Vect3(radial.x, Mathf.Min(position.y, height), radial.z);

			return new PoseIC((Vect3)reference.TransformPoint((Vector3)position), targetPose.rotation);
		}

		private void OnDrawGizmos()
		{
#if UNITY
			// draw our cylinder
			if(reference)
			{
				DebugExtension.DrawCylinder(reference.position, reference.position +
					reference.up * height, radius);
			}
#endif
		}
	}
}