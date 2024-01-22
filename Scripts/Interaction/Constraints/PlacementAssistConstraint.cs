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
		const float snapDuration = 0.25f;
		float snapTimer = 0;

		// surface mode vars
		const float surfaceSnapDistance = 0.04f;
		[Range(0, 0.01f)]
		[SerializeField] float surfaceAdjustAmt = 0.005f;
		float surfaceSnapDetectRadius = 0.1f;

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

		[SerializeField] bool previewPlacement; // if false, and a preview is present, the preview will go to the
					// 'base' position and the item will go to the 'placement' position
		[SerializeField] GameObject placementPreview;
		[SerializeField] bool clearConstraint;
		[SerializeField] bool setConstraint;

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

			graspItem.OnUngrasped += GraspItem_OnUngrasped;
        }

		private void GraspItem_OnUngrasped(InteractiveItem sender)
		{
			isSnapping = false;
			snapTimer = 0;
		}

		void DoDebugCommands()
		{
			if(clearConstraint)
			{
				clearConstraint = false;
				graspItem.SetConstraint(null);
			}

			if(setConstraint)
			{
				setConstraint = false;
				graspItem.SetConstraint(this);
			}
		}

		// Update is called once per frame
		void Update()
        {
			DoDebugCommands();
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

				// raycast for our surface normal
				RaycastHit objectToSurfaceHit;
				Vector3 objectToSurfaceDirection = (candidateSnapInfo.SurfacePoint - candidateSnapInfo.ObjectPoint);
				float objectToSurfaceDistance = objectToSurfaceDirection.magnitude;

				if(objectToSurfaceDistance <= Mathf.Epsilon)
				{
					// our length is zero, because the object is already sitting on the surface,
					// likely because gravity is on and the object has settled.
					Debug.Log("object to surface direction changed because of zero length distance");
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

				candidateSnapInfo.SurfacePoint =
					candidateSnapInfo.SurfacePoint + (candidateSnapInfo.SurfaceNormal * surfaceAdjustAmt);
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
								closestDistance = surfaceSnapDistance;
							}
						}
					}

					if (candidateFound)
					{
						// check to see if our snap distance passes the check
						if (closestDistance < surfaceSnapDistance)
						{
							isSnapping = true;
							Debug.Log(string.Format("Starting snap on object: {0}", closestSnap.SurfaceCollider.name));
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

		Pose GetSurfaceSnapPose(Pose inputPose, bool updateSnap=true)
		{
			Pose snappedPose = inputPose;

			// get the most recent 2 points
			float distance = float.PositiveInfinity;
			Vector3 centerOfMass = ItemCenterOfMass();
			SurfaceSnapInfo updatedSnapInfo = (updateSnap) ? GetSnapForCollider(centerOfMass,
				surfaceSnap.SurfaceCollider, out distance) :
				surfaceSnap;

			// I think you forgot to update surface snap
			// just sayin
			surfaceSnap = updatedSnapInfo;

			// get the offset between the object and surface, apply to object
			Vector3 offset = updatedSnapInfo.SurfacePoint - updatedSnapInfo.ObjectPoint;
			snappedPose.position = snappedPose.position + offset;

			return snappedPose;
		}

		Pose DoSurfacePose(Pose targetPose)
		{
			CheckSurfaceSnap();

			float snapTValue = Mathf.InverseLerp(0, snapDuration, snapTimer);

			// conditional below is for guarding against no recent snaps
			Pose snapPose = (surfaceSnap.SurfaceCollider) ? GetSurfaceSnapPose(targetPose, true) : targetPose;

			Pose lerpPose = new Pose(Vector3.Lerp(targetPose.position, snapPose.position, snapTValue),
				Quaternion.Slerp(targetPose.rotation, snapPose.rotation, snapTValue));

			debugSphereB.transform.position = surfaceSnap.ObjectPoint;
			debugSphereB.SetActive(true);

			debugSphereA.transform.position = surfaceSnap.SurfacePoint;
			debugSphereA.SetActive(true);

			if (!previewPlacement)
			{
				placementPreview.transform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
				return lerpPose;
			}
			else
			{
				placementPreview.transform.SetPositionAndRotation(lerpPose.position, lerpPose.rotation);
				return targetPose;
			}
		}

		Pose DoGridPose(Pose targetPose)
		{
			return targetPose;
		}

        public override Pose DoConstraint(Pose targetPose)
        {
			if(isSnapping)
			{
				snapTimer += Time.deltaTime;
				snapTimer = Mathf.Clamp(snapTimer, 0, snapDuration);
			}
			else
			{
				snapTimer -= Time.deltaTime;
				snapTimer = Mathf.Clamp(snapTimer, 0, snapDuration);
			}

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