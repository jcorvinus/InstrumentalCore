using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Constraints
{
    public abstract class GraspConstraint : MonoBehaviour
    {
		GraspableItem graspItem;

		protected void Awake()
		{
			graspItem = GetComponent<GraspableItem>();
			graspItem.SetConstraint(this);
		}

		public abstract Pose DoConstraint(Pose targetPose);
    }
}