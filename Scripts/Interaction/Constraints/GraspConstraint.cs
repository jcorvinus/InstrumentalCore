using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Constraints
{
    public abstract class GraspConstraint : MonoBehaviour
    {
		protected InteractiveItem graspItem;

		protected virtual void Awake()
		{
			graspItem = GetComponent<InteractiveItem>();
			graspItem.SetConstraint(this);
		}

		public abstract Pose DoConstraint(Pose targetPose);
    }
}