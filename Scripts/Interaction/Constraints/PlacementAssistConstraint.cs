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

		public enum Axis : byte
		{
			None = 0,
			X = 1,
			Y = 2,
			Z = 4,
			All = 7
		}

        [SerializeField] AssistMode mode = AssistMode.Surface;
		const int colliderCheckCount = 5;
		Collider[] colliderCheckBuffer;

		// all snap vars
		bool isSnapping = false;
		const float snapDuration = 0.25f;
		float snapTimer = 0;
		bool snapHasReset = true;

		// surface mode vars
		const float surfaceSnapDistance = 0.04f;
		[Range(0, 0.1f)]
		[SerializeField] float surfaceAdjustAmt = 0.00493f;
		float surfaceSnapDetectRadius = 0.1f;
		float snapBreakStrainAmount = 0.125f;

		[System.Serializable]
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
			snapHasReset = true;
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

		void CheckSurfaceSnap()
		{
			Vector3 centerOfMass = graspItem.RigidBody.worldCenterOfMass;

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
							SurfaceSnapInfo candidateSnapInfo = GetCandidateSnapForCollider(centerOfMass, candidateCollider, out surfaceSnapDistance);

							if (surfaceSnapDistance < closestDistance)
							{
								candidateFound = true;
								closestSnap = candidateSnapInfo;
								closestDistance = surfaceSnapDistance;
							}
						}
					}

					if (candidateFound)
					{
						if(!snapHasReset)
						{
							if(closestDistance > (surfaceSnapDistance + 0.05f))
							{
								snapHasReset = true;
							}
						}

						// check to see if our snap distance passes the check
						if (closestDistance < surfaceSnapDistance && snapHasReset)
						{
							isSnapping = true;
							snapHasReset = false;
							Debug.Log(string.Format("Starting snap on object: {0}", closestSnap.SurfaceCollider.name));
							surfaceSnap = closestSnap;
						}
					}
				}
			}
			else
			{
				// should we stop snapping?
				if(graspItem.MaxStrain > snapBreakStrainAmount)
				{
					isSnapping = false;
					surfaceSnap.SurfaceCollider = null;
				}
			}
		}

		/// <summary>
		/// Get point-on-object and point-on-surface, as well as normals,
		/// for this object, given a center of mass and collider
		/// </summary>
		/// <param name="centerOfMass"></param>
		/// <param name="candidateCollider"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		SurfaceSnapInfo GetCandidateSnapForCollider(Vector3 centerOfMass, Collider surfaceCollider, out float distance)
		{
			distance = float.PositiveInfinity;
			SurfaceSnapInfo candidateSnapInfo = new SurfaceSnapInfo();

			Vector3 candidateSurfaceClosest = surfaceCollider.ClosestPoint(centerOfMass);
			Vector3 candidateObjectClosest = Vector3.zero;
			Vector3 raycastDirection = (centerOfMass - candidateSurfaceClosest).normalized;

			RaycastHit hitInfo;

			if (Physics.Raycast(new Ray(candidateSurfaceClosest, raycastDirection), out hitInfo))
			{
				candidateObjectClosest = hitInfo.point;

				distance = Vector3.Distance(candidateSurfaceClosest, candidateObjectClosest);
				candidateSnapInfo.ObjectPoint = candidateObjectClosest;
				candidateSnapInfo.ObjectNormal = hitInfo.normal;
				candidateSnapInfo.SurfaceCollider = surfaceCollider;

				// this will break if candidateSnapInfo.ObjectPoint is touching or inside of
				// candidate collider. I think this has changed from previous versions of unity
				// it might now just return the unfiltered point, it used to return 0
				candidateSnapInfo.SurfacePoint = surfaceCollider.ClosestPoint(candidateSnapInfo.ObjectPoint);

				// raycast for our surface normal
				RaycastHit objectToSurfaceHit;
				Vector3 objectToSurfaceDirection = (candidateSnapInfo.SurfacePoint - candidateSnapInfo.ObjectPoint);
				float objectToSurfaceDistance = objectToSurfaceDirection.magnitude;

				if (objectToSurfaceDistance <= Mathf.Epsilon)
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
				surfaceCollider.Raycast(objectToSurfaceRay, out objectToSurfaceHit, surfaceSnapDetectRadius);
				candidateSnapInfo.SurfaceNormal = objectToSurfaceHit.normal;

				// push our point away from the surface a small amount so that it doesn't freak out
				candidateSnapInfo.SurfacePoint =
					candidateSnapInfo.SurfacePoint + (candidateSnapInfo.SurfaceNormal * surfaceAdjustAmt);
			}

			return candidateSnapInfo;
		}

		SurfaceSnapInfo GetSnapForCollider(Pose inputPose, Collider surfaceCollider)
		{
			SurfaceSnapInfo snapInfo = new SurfaceSnapInfo();
			snapInfo.SurfaceCollider = surfaceCollider;

			Vector3 surfaceClosest = surfaceCollider.ClosestPoint(inputPose.position);
			snapInfo.SurfacePoint = surfaceClosest;

			Vector3 objectClosest = Vector3.zero;

			// use raycast
			Vector3 surfToObject = (graspItem.RigidBody.worldCenterOfMass - surfaceClosest).normalized;
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(surfaceClosest, surfToObject), out hitInfo, surfaceSnapDetectRadius))
			{
				objectClosest = hitInfo.point;
				snapInfo.ObjectNormal = hitInfo.normal;
			}
			else
			{
				// this is weird, what do we do here?
				Debug.Log("Hit weird raycast failure in GetSnapForCollider");
			}

			snapInfo.ObjectPoint = objectClosest;

			// get our surface normal
			Vector3 objectToSurfaceDirection = (surfaceClosest - objectClosest).normalized;
			float objectToSurfaceDistance = objectToSurfaceDirection.magnitude;
			if (objectToSurfaceDistance <= Mathf.Epsilon)
			{
				// our length is zero, because the object is already sitting on the surface,
				// likely because gravity is on and the object has settled.
				Debug.Log("object to surface direction changed because of zero length distance");
				objectToSurfaceDirection = (snapInfo.SurfacePoint - graspItem.RigidBody.position).normalized;
			}
			else
			{
				objectToSurfaceDirection /= objectToSurfaceDistance;
			}

			RaycastHit surfNormalHit;
			if(snapInfo.SurfaceCollider.Raycast(new Ray(graspItem.RigidBody.position, objectToSurfaceDirection),
				out surfNormalHit, surfaceSnapDetectRadius))
			{
				snapInfo.SurfaceNormal = surfNormalHit.normal;
			}

			snapInfo.SurfacePoint += (surfaceSnap.SurfaceNormal * surfaceAdjustAmt);

			return snapInfo;
		}

		Axis GetSnapAxisForNormal(Vector3 objectNormal, out bool positive)
		{
			positive = false;

			Vector3 localNormal = transform.InverseTransformDirection(objectNormal);

			float maxComponent = Mathf.Max(Mathf.Abs(localNormal.x), Mathf.Abs(localNormal.y), Mathf.Abs(localNormal.z));

			bool xMax = maxComponent == Mathf.Abs(localNormal.x);
			bool yMax = maxComponent == Mathf.Abs(localNormal.y);
			bool zMax = maxComponent == Mathf.Abs(localNormal.z);

			if (xMax)
			{
				positive = (Mathf.Sign(localNormal.x) > 0) ? true : false;
				return Axis.X;
			}
			else if (yMax)
			{
				positive = (Mathf.Sign(localNormal.y) > 0) ? true : false;
				return Axis.Y;
			}
			else
			{
				positive = (Mathf.Sign(localNormal.z) > 0) ? true : false;
				return Axis.Z;
			}
		}

		Axis GetForwardAndRightAxisForNormal(Axis normalAxis, bool normalPositive,
			out bool forwardPositive, out Axis rightAxis, out bool rightPositive)
		{
			switch (normalAxis)
			{
				case Axis.X:
					rightAxis = Axis.Y;
					rightPositive = normalPositive;
					forwardPositive = normalPositive;
					return Axis.Z;
					
				case Axis.Y:
					rightAxis = Axis.X;
					rightPositive = normalPositive;
					forwardPositive = normalPositive;
					return Axis.Z;

				case Axis.Z:
					forwardPositive = normalPositive;
					rightPositive = normalPositive;
					rightAxis = Axis.Y;
					return Axis.X;

				default:
					forwardPositive = true;
					rightAxis = Axis.X;
					rightPositive = true;
					return Axis.Z;
			}
		}

		Vector3 GetVectorForAxis(Axis axis, bool positive)
		{
			switch (axis)
			{
				case Axis.None:
					return Vector3.zero;
				case Axis.X:
					return Vector3.right * ((positive) ? 1 : -1);
				case Axis.Y:
					return Vector3.up * ((positive) ? 1 : -1);
				case Axis.Z:
					return Vector3.forward * ((positive) ? 1 : -1);
				case Axis.All:
					return Vector3.one;
				default:
					return Vector3.zero;
			}
		}

		Quaternion testRotation = Quaternion.identity;

		Pose GetSurfaceSnapPose(Pose inputPose)
		{
			Pose snappedPose = inputPose;

			// get the most recent 2 points
			surfaceSnap = GetSnapForCollider(inputPose,	surfaceSnap.SurfaceCollider);

			Vector3 poseOffset = (graspItem.RigidBody.position - surfaceSnap.ObjectPoint);
			float distance = poseOffset.magnitude;

			// calculate rotation offsets
			bool objectNormalPositive = false;
			Vector3 worldSpaceObjectNormalInverse = surfaceSnap.ObjectNormal * -1;
			Axis objectNormalLocal = GetSnapAxisForNormal(worldSpaceObjectNormalInverse, out objectNormalPositive);

			bool objectForwardPositive, objectRightPositive;
			Axis objectForwardAxis, objectRightAxis;

			objectForwardAxis = GetForwardAndRightAxisForNormal(objectNormalLocal, objectNormalPositive,
				out objectForwardPositive, out objectRightAxis, out objectRightPositive);

			Vector3 upVectorLocal = GetVectorForAxis(objectNormalLocal, objectNormalPositive),
				forwardVectorLocal = GetVectorForAxis(objectForwardAxis, objectForwardPositive);
			Quaternion localRebasedRotation = Quaternion.LookRotation(forwardVectorLocal, upVectorLocal);

			Vector3 upVector = surfaceSnap.SurfaceNormal; //inputPose.rotation * (upVectorLocal);
			Vector3 forwardVector = inputPose.rotation * (forwardVectorLocal);

			forwardVector = Vector3.ProjectOnPlane(forwardVector, surfaceSnap.SurfaceNormal);
			testRotation = Quaternion.LookRotation(forwardVector, upVector); // remove when done testing

			Quaternion surfaceRotation = Quaternion.LookRotation(forwardVector, upVector);

			Quaternion rotationOffset = Quaternion.Inverse(localRebasedRotation);

			Quaternion rotation = surfaceRotation * rotationOffset;

			Vector3 position = surfaceSnap.SurfacePoint + (surfaceSnap.SurfaceNormal * distance);
			snappedPose.position = position;
			snappedPose.rotation = rotation;

			return snappedPose;
		}

		Pose DoSurfacePose(Pose targetPose)
		{
			CheckSurfaceSnap();

			float snapTValue = Mathf.InverseLerp(0, snapDuration, snapTimer);

			// conditional below is for guarding against no recent snaps
			// also important to note that this will not allow for switching surfaces at snap time
			// with the distances involved in snapping/unsnapping, this could make it hard to handle
			// scenarios with multiple surfaces in close proximity
			Pose snapPose = (surfaceSnap.SurfaceCollider) ? GetSurfaceSnapPose(targetPose) : targetPose;

			Pose lerpPose = new Pose(Vector3.Lerp(targetPose.position, snapPose.position, snapTValue),
				Quaternion.Slerp(targetPose.rotation, snapPose.rotation, snapTValue));

			return lerpPose;
		}

		Pose DoGridPose(Pose targetPose)
		{
			return targetPose;
		}

        public override Pose DoConstraint(Pose targetPose)
        {
			if(isSnapping)
			{
				snapTimer += Time.fixedDeltaTime;
				snapTimer = Mathf.Clamp(snapTimer, 0, snapDuration);
			}
			else
			{
				snapTimer -= Time.fixedDeltaTime;
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

		void DrawBasis(Vector3 position, Quaternion rotation)
		{
			Gizmos.color = Color.blue;
			Vector3 forward = (testRotation * Vector3.forward);
			Gizmos.DrawLine(position, position + (forward * 0.1f));
			Vector3 up = (testRotation * Vector3.up);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(position, position + (up * 0.1f));
			Vector3 right = Vector3.Cross(up, forward);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(position, position + (right * 0.1f));
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(surfaceSnap.ObjectPoint, surfaceSnap.ObjectPoint + (surfaceSnap.ObjectNormal * 0.01f));

			Gizmos.color = Color.green;
			Gizmos.DrawLine(surfaceSnap.SurfacePoint, surfaceSnap.SurfacePoint + (surfaceSnap.SurfaceNormal * 0.01f));

			// draw our test rotation
			Vector3 position = transform.position;

			DrawBasis(position, testRotation);
		}
	}
}