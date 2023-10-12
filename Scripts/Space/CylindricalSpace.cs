using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Space
{
	// so the trick to understanding what's going on here is that 
	// each transformation from rectilinear space (aka 'local' space) is that the distortion is
	// a function of the x axis. We might eventually introduce a concept of 'anchor',
	// which is a 'zero point' for defining where our distortion space starts.
    public class CylindricalSpace : RadialSpace
    {
		public override Vector3 TransformPoint(Vector3 localPoint)
		{
			Vector3 anchorDelta;
			Vector3 anchorRectPos = this.transform.InverseTransformPoint(Origin.position);
			anchorDelta = localPoint - anchorRectPos;

			float angle = anchorDelta.x / this.radius;
			float height = anchorDelta.y;
			float radius = this.radius + anchorDelta.z; 

			Vector3 position = Vector3.zero;
			position.x = Mathf.Sin(angle) * radius;
			position.y = height;
			position.z = Mathf.Cos(angle) * radius - this.radius;
			return position;
		}

		public override Vector3 InverseTransformPoint(Vector3 worldPoint)
		{
			worldPoint.z += this.radius;

			float angle = Mathf.Atan2(worldPoint.x, worldPoint.z);
			float height = worldPoint.y;
			float radius = new Vector2(worldPoint.x, worldPoint.z).magnitude;

			Vector3 anchorDelta;
			anchorDelta.x = (angle) * this.radius;
			anchorDelta.y = height;
			anchorDelta.z = radius - this.radius;

			Vector3 anchorRectPos = Vector3.zero;
			Vector3 localRectPos = anchorRectPos + anchorDelta;
			return localRectPos;
		}

		public override Quaternion TransformRotation(Vector3 localPosition, Quaternion localRotation)
		{
			Vector3 anchorDelta;
			Vector3 anchorRectPos = this.transform.InverseTransformPoint(Origin.position);
			anchorDelta = localPosition - anchorRectPos;

			float angle = anchorDelta.x / this.radius;

			Quaternion rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);

			return rotation * localRotation;
		}

		public override Quaternion InverseTransformRotation(Vector3 position, Quaternion rotation)
		{
			position.z += this.radius;

			float angle = Mathf.Atan2(position.x, position.z);

			return Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0) * rotation;
		}

		public override Vector3 TransformDirection(Vector3 localPosition, Vector3 localDirection)
		{
			Vector3 anchorDelta = Vector3.zero;
			Vector3 anchorRectPos = this.transform.InverseTransformPoint(Origin.position);

			anchorDelta = localPosition - anchorRectPos;
			float angle = anchorDelta.x / this.radius;

			Quaternion rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
			return rotation * localDirection;
		}

		public override Vector3 InverseTransformDirection(Vector3 position, Vector3 direction)
		{
			position.z += this.radius;

			float angle = Mathf.Atan2(position.x, position.z);

			return Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0) * direction;
		}
	}
}