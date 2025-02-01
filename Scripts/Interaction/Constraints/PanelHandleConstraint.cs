using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;
using Instrumental.Core.Math;
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

		public override PoseIC DoConstraint(PoseIC targetPose)
		{
			Vect3 constrainedLocalPosition = (Vect3)transform.parent.InverseTransformPoint(
				(Vector3)targetPose.position);
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

			Vect3 worldConstrainedPosition = 
				(Vect3)transform.parent.TransformPoint((Vector3)constrainedLocalPosition);

            return new PoseIC(worldConstrainedPosition, (Quatn)transform.parent.rotation);
		}
	}
}