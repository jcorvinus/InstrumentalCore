using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;

namespace Instrumental.Interaction.Constraints
{
    public class PanelHandleConstraint : GraspConstraint
    {
		PanelHandle handle;

		protected override void Awake()
		{
			base.Awake();
			handle = GetComponent<PanelHandle>();
		}

		public override Pose DoConstraint(Pose targetPose)
		{
			Vector3 constrainedLocalPosition = transform.parent.InverseTransformPoint(targetPose.position);
            constrainedLocalPosition.z = 0;

            // enforce any handle-specific constraints
            switch (handle.Type)
            {
                case PanelHandle.HandleType.UpperRail:
                    constrainedLocalPosition.x = 0;
                    constrainedLocalPosition.y = Mathf.Max(0.001f, constrainedLocalPosition.y); // don't let it go negative or get too small
                    break;
                case PanelHandle.HandleType.LowerRail:
                    constrainedLocalPosition.x = 0;
                    constrainedLocalPosition.y = Mathf.Min(-0.001f, constrainedLocalPosition.y);// don't let it go negative or get too small
                    break;
                case PanelHandle.HandleType.LeftRail:
                    constrainedLocalPosition.x = Mathf.Min(-0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = 0;
                    break;
                case PanelHandle.HandleType.RightRail:
                    constrainedLocalPosition.x = Mathf.Max(0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = 0;
                    break;
                case PanelHandle.HandleType.UpperLeftCorner:
                    constrainedLocalPosition.y = Mathf.Max(0.001f, constrainedLocalPosition.y); // don't let it go negative or get too small
                    constrainedLocalPosition.x = Mathf.Min(-0.001f, constrainedLocalPosition.x);
                    break;
                case PanelHandle.HandleType.LowerLeftCorner:
                    constrainedLocalPosition.x = Mathf.Min(-0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = Mathf.Min(-0.001f, constrainedLocalPosition.y);// don't let it go negative or get too small
                    break;

                case PanelHandle.HandleType.UpperRightCorner:
                    constrainedLocalPosition.x = Mathf.Max(0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = Mathf.Max(0.001f, constrainedLocalPosition.y); // don't let it go negative or get too small

                    break;
                case PanelHandle.HandleType.LowerRightCorner:
                    constrainedLocalPosition.y = Mathf.Min(-0.001f, constrainedLocalPosition.y);// don't let it go negative or get too small
                    constrainedLocalPosition.x = Mathf.Max(0.001f, constrainedLocalPosition.x);
                    break;

                default:
                    break;
            }

            Vector3 worldConstrainedPosition = transform.parent.TransformPoint(constrainedLocalPosition);

            return new Pose(worldConstrainedPosition, transform.parent.rotation);
		}
	}
}