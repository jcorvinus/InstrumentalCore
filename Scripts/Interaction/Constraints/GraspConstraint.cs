using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;

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

		public abstract PoseIC DoConstraint(PoseIC targetPose);
    }
}