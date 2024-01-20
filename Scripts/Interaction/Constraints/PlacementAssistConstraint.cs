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

		public struct SurfaceSnapInfo
		{
			public Vector3 SurfacePoint;
			public Vector3 SurfaceNormal;
			public Vector3 ObjectPoint;
			public Vector3 ObjectNormal;
			public Collider SurfaceCollider;
		}

		SurfaceSnapInfo surfaceSnap;

		// grid mode vars

		// debug vis
		GameObject debugSphereA;
		GameObject debugSphereB;

		protected override void Awake()
		{
			base.Awake();
			colliderCheckBuffer = new Collider[colliderCheckCount];

			debugSphereA = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			SphereCollider debugSphereACollider = debugSphereA.GetComponent<SphereCollider>();
			Destroy(debugSphereACollider);
			debugSphereA.transform.localScale = Vector3.one * 0.02f;
			debugSphereA.name = "DebugA";

			debugSphereB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			SphereCollider debugSphereBCollider = debugSphereB.GetComponent<SphereCollider>();
			Destroy(debugSphereBCollider);
			debugSphereB.transform.localScale = Vector3.one * 0.02f;
			debugSphereB.name = "DebugB";

			debugSphereA.SetActive(false);
			debugSphereB.SetActive(false);
		}

		// Start is called before the first frame update
		void Start()
        {
			graspItem.SetConstraint(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

		Vector3 ItemCenterOfMass()
		{
			Matrix4x4 comTransform = Matrix4x4.TRS(graspItem.transform.position,
				graspItem.transform.rotation, Vector3.one);
			return comTransform.MultiplyPoint3x4(graspItem.RigidBody.centerOfMass);
		}

		SurfaceSnapInfo GetSnapForCollider(Vector3 centerOfMass, Collider candidateCollider, out float distance)
		{
			distance = float.PositiveInfinity;
			SurfaceSnapInfo candidateSnapInfo = new SurfaceSnapInfo();

			// center of mass usage here is wrong
			// It's in local coordinates iirc, but we're expecting worldspace

			Vector3 candidateSurfaceClosest = candidateCollider.ClosestPoint(centerOfMass);
			Vector3 candidateObjectClosest = Vector3.zero;
			Vector3 raycastDirection = (centerOfMass - candidateSurfaceClosest).normalized;

			RaycastHit hitInfo;

			if (Physics.Raycast(new Ray(candidateSurfaceClosest, raycastDirection), out hitInfo))
			{
				candidateObjectClosest = hitInfo.point;

				distance = Vector3.Distance(candidateSurfaceClosest, candidateObjectClosest);
				candidateSnapInfo.ObjectPoint = candidateObjectClosest;
				candidateSnapInfo.ObjectNormal = hitInfo.normal;
				candidateSnapInfo.SurfaceCollider = candidateCollider;
				candidateSnapInfo.SurfacePoint = candidateCollider.ClosestPoint(candidateSnapInfo.ObjectPoint);

				debugSphereB.transform.position = candidateSnapInfo.ObjectPoint;
				debugSphereB.SetActive(true);

				debugSphereA.transform.position = candidateSnapInfo.SurfacePoint;
				debugSphereA.SetActive(true);

				// raycast for our surface normal
				RaycastHit objectToSurfaceHit;
				Vector3 objectToSurfaceDirection = (candidateSnapInfo.SurfacePoint - candidateSnapInfo.ObjectPoint);
				float objectToSurfaceDistance = objectToSurfaceDirection.magnitude;

				if(objectToSurfaceDistance <= Mathf.Epsilon)
				{
					// our length is zero, because the object is already sitting on the surface,
					// likely because gravity is on and the object has settled.
					objectToSurfaceDirection = (candidateSnapInfo.SurfacePoint - centerOfMass).normalized;
				}
				else
				{
					objectToSurfaceDirection /= objectToSurfaceDistance;
				}

				Ray objectToSurfaceRay = new Ray(candidateSnapInfo.ObjectPoint, 
					objectToSurfaceDirection);
				Debug.DrawRay(candidateSnapInfo.ObjectPoint, objectToSurfaceDirection);
				candidateCollider.Raycast(objectToSurfaceRay, out objectToSurfaceHit, surfaceSnapDetectRadius);
				candidateSnapInfo.SurfaceNormal = objectToSurfaceHit.normal;
			}

			return candidateSnapInfo;
		}

		void CheckSurfaceSnap()
		{
			Vector3 centerOfMass = ItemCenterOfMass();

			if (!isSnapping)
			{
				// should we start snapping
				// first we need our reference point - where's our nearest surface?
				surfaceSnapDetectRadius = surfaceSnapDistance * 2f;
				int hits = Physics.OverlapSphereNonAlloc(centerOfMass,
					graspItem.ItemRadius + surfaceSnapDetectRadius, colliderCheckBuffer);

				if (hits > 0)
				{
					SurfaceSnapInfo closestSnap = new SurfaceSnapInfo();

					float closestDistance = float.PositiveInfinity;
					bool candidateFound = false;

					// check our colliders for tag
					for (int i = 0; i < hits; i++)
					{
						if (colliderCheckBuffer[i].tag.Contains("SNP")) // this is one we want.
																		// How do we handle multiple valid candidates?
						{
							Collider candidateCollider = colliderCheckBuffer[i];
							float surfaceSnapDistance = float.PositiveInfinity;
							SurfaceSnapInfo candidateSnapInfo = GetSnapForCollider(centerOfMass, candidateCollider, out surfaceSnapDistance);

							if(surfaceSnapDistance < closestDistance)
							{
								candidateFound = true;
								closestSnap = candidateSnapInfo;
							}
						}
					}

					if (candidateFound)
					{
						// check to see if our snap distance passes the check
						if (closestDistance < surfaceSnapDistance)
						{
							isSnapping = true;
							surfaceSnap = closestSnap;
						}
					}
				}
			}
			else
			{
				// todo: should we stop snapping
			}
		}

		Pose GetSurfaceSnapPose(Pose inputPose)
		{
			Pose snappedPose = inputPose;

			// get the most recent 2 points
			float distance = float.PositiveInfinity;
			Vector3 centerOfMass = ItemCenterOfMass();
			SurfaceSnapInfo updatedSnapInfo = GetSnapForCollider(centerOfMass,
				surfaceSnap.SurfaceCollider, out distance);


			return snappedPose;
		}

		Pose DoSurfacePose(Pose targetPose)
		{
			CheckSurfaceSnap();

			return (isSnapping) ? GetSurfaceSnapPose(targetPose) : targetPose;
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