using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Constraints
{
    public class PlacementAssistConstraint : GraspConstraint
	{
		public enum AssistMode
		{ 
            None=0,
            Surface=1,
            Grid=2
        }

        [SerializeField] AssistMode mode = AssistMode.Surface;
		const int colliderCheckCount = 5;
		Collider[] colliderCheckBuffer;

		// all snap vars
		bool isSnapping = false;

		// surface mode vars
		const float surfaceSnapDistance = 0.1f;
		float surfaceSnapDetectRadius = 0.2f;
		Vector3 surfaceSnapNormal;
		Collider surfaceSnapCollider;

		// grid mode vars

		protected override void Awake()
		{
			base.Awake();
			colliderCheckBuffer = new Collider[colliderCheckCount];
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

		Pose DoSurfacePose(Pose targetPose)
		{
			if(!isSnapping)
			{
				// should we start snapping
				// first we need our reference point - where's our nearest surface?
				surfaceSnapDetectRadius = surfaceSnapDistance * 2f;
				int hits = Physics.OverlapSphereNonAlloc(graspItem.RigidBody.centerOfMass,
					graspItem.ItemRadius + surfaceSnapDetectRadius, colliderCheckBuffer);

				if(hits > 0)
				{
					Vector3 closestSurfacePoint = Vector3.zero;
					Vector3 closestObjectPoint = Vector3.zero;
					Vector3 closestSurfaceNormal = Vector3.zero;
					Collider closestCollider = null;
					float closestDistance = float.PositiveInfinity;
					bool candidateFound = false;

					// check our colliders for tag
					for(int i=0; i < hits; i++)
					{
						if(colliderCheckBuffer[i].tag.Contains("SNP")) // this is one we want.
																	   // How do we handle multiple valid candidates?
						{
							Collider candidateCollider = colliderCheckBuffer[i];
							Vector3 candidateSurfaceClosest = candidateCollider.ClosestPoint(graspItem.RigidBody.centerOfMass);
							Vector3 candidateObjectClosest = Vector3.zero;
							Vector3 raycastDirection = (graspItem.RigidBody.centerOfMass - candidateSurfaceClosest).normalized;
							RaycastHit hitInfo;
							if(Physics.Raycast(new Ray(candidateSurfaceClosest, raycastDirection), out hitInfo))
							{
								//candidateFound = true;
								candidateObjectClosest = hitInfo.point;


								float distance = Vector3.Distance(candidateSurfaceClosest, candidateObjectClosest);
								if(distance < closestDistance)
								{
									candidateFound = true;
									candidateSurfaceClosest = hitInfo.point;
									closestSurfaceNormal = hitInfo.normal * -1;
									closestDistance = distance;
									closestCollider = candidateCollider;
								}
							}
						}
					}

					if(candidateFound)
					{
						// check to see if our snap distance passes the check
						if(closestDistance < surfaceSnapDistance)
						{
							isSnapping = true;
							surfaceSnapCollider = closestCollider;
							surfaceSnapNormal = closestSurfaceNormal;
						}
					}
				}
			}
			else
			{
				// should we stop snapping
			}

			Pose snappedPose = targetPose;

			return (isSnapping) ? snappedPose : targetPose;
		}

		Pose DoGridPose(Pose targetPose)
		{
			return targetPose;
		}

        public override Pose DoConstraint(Pose targetPose)
        {
			switch (mode)
			{
				case AssistMode.None:
					return targetPose;

				case AssistMode.Surface:
					return DoSurfacePose(targetPose);

				case AssistMode.Grid:
					return DoGridPose(targetPose);

				default:
					return targetPose;
			}
		}
    }
}