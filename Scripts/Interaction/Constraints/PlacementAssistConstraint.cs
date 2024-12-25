using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Constraints
{
	public class PlacementAssistConstraint : GraspConstraint
	{
		public enum AssistMode
		{
			None = 0,
			Surface = 1,
			Grid = 2
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

		#region Surface Snap Vars
		const float surfaceSnapDistance = 0.04f;
		[Range(0, 0.1f)]
		[SerializeField] float surfaceAdjustAmt = 0.00493f;
		float surfaceSnapDetectRadius = 0.1f;
		float snapBreakStrainAmount = 0.125f;
		[SerializeField] bool angleAroundSurfaceSnap = true;
		[SerializeField] float aroundNormalAngleSnap = 10;

		public struct SurfaceSnapInfo
		{
			public Vector3 SurfacePoint;
			public Vector3 SurfaceNormal;
			public Vector3 ObjectPoint;
			public Vector3 ObjectNormal;
			public float Distance;
			public Collider SurfaceCollider;
		}

		SurfaceSnapInfo surfaceSnap;

		public struct SurfaceAngleSnapDirections
		{
			public Vector3 Forward;
			public Vector3 Back;
			public Vector3 Right;
			public Vector3 Left;
		}
		#endregion

		// grid mode vars
		public struct GridSnapInfo2D
		{
			public Vector3 SurfacePoint;
			public Vector3 SurfaceNormal;
			public Vector3 ObjectPoint;
			public Vector3 ObjectNormal;
			public float Distance;
			public SnapGrid Grid;
		}

		SnapGrid currentGrid;

		// debug vars
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
			MeshRenderer debugSphereARenderer = debugSphereA.GetComponent<MeshRenderer>();
			debugSphereARenderer.material.color = Color.red;

			debugSphereB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			SphereCollider debugSphereBCollider = debugSphereB.GetComponent<SphereCollider>();
			Destroy(debugSphereBCollider);
			debugSphereB.transform.localScale = Vector3.one * 0.02f;
			debugSphereB.name = "DebugB";
			MeshRenderer debugSphereBRenderer = debugSphereB.GetComponent<MeshRenderer>();
			debugSphereBRenderer.material.color = Color.blue;

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

		}

		// Update is called once per frame
		void Update()
        {
			DoDebugCommands();
        }

		#region Surface Snap Methods
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
							SurfaceSnapInfo candidateSnapInfo = GetSnapForCollider(graspItem.RigidBody.position, candidateCollider);
							float surfaceSnapDistance = candidateSnapInfo.Distance;

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
		SurfaceSnapInfo GetSnapForCollider(Vector3 inputPosition, Collider surfaceCollider)
		{
			SurfaceSnapInfo snapInfo = new SurfaceSnapInfo();
			snapInfo.SurfaceCollider = surfaceCollider;

			Vector3 surfaceClosest = surfaceCollider.ClosestPoint(inputPosition);
			snapInfo.SurfacePoint = surfaceClosest;

			Vector3 objectClosest = Vector3.zero;

			// use raycast
			Vector3 surfToObject = (graspItem.RigidBody.worldCenterOfMass - surfaceClosest).normalized;
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(surfaceClosest, surfToObject), out hitInfo))
			{
				objectClosest = hitInfo.point;
				snapInfo.ObjectNormal = hitInfo.normal;
			}
			else
			{
				// this is weird, what do we do here?
				Debug.Log("Hit weird raycast failure in GetSnapForCollider");
				Debug.Break();
			}

			snapInfo.ObjectPoint = objectClosest;

			// get our surface normal
			Vector3 objectToSurfaceDirection = (surfaceClosest - objectClosest);
			float objectToSurfaceDistance = objectToSurfaceDirection.magnitude; // you've screwed this up
			objectToSurfaceDirection /= objectToSurfaceDistance;

			if (objectToSurfaceDistance <= Mathf.Epsilon)
			{
				// our length is zero, because the object is already sitting on the surface,
				// likely because gravity is on and the object has settled.
				Debug.Log("object to surface direction changed because of zero length distance");
				objectToSurfaceDirection = (snapInfo.SurfacePoint - graspItem.RigidBody.worldCenterOfMass).normalized;
			}
			else
			{
				objectToSurfaceDirection /= objectToSurfaceDistance;
			}
			snapInfo.Distance = objectToSurfaceDistance;

			RaycastHit surfNormalHit;
			if(snapInfo.SurfaceCollider.Raycast(new Ray(graspItem.RigidBody.worldCenterOfMass, objectToSurfaceDirection),
				out surfNormalHit, surfaceSnapDetectRadius))
			{
				snapInfo.SurfaceNormal = surfNormalHit.normal;
			}

			// push our point away from the surface a small amount so that it doesn't freak out
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

		SurfaceAngleSnapDirections GetAngleSnapDirectionsFromSurface(Vector3 normal, Transform surface)
		{
			Vector3 normalLocal = surface.transform.InverseTransformDirection(normal);

			bool normalLocalPositive;
			Axis normalAxisLocal = GetSnapAxisForNormal(normalLocal, out normalLocalPositive);

			Axis forwardAxisLocal, rightAxisLocal;
			bool forwardAxisLocalPositive, rightAxisLocalPositive;
			forwardAxisLocal = GetForwardAndRightAxisForNormal(normalAxisLocal, normalLocalPositive, out forwardAxisLocalPositive,
				out rightAxisLocal, out rightAxisLocalPositive);

			Vector3 forward, back, right, left;
			forward = GetVectorForAxis(forwardAxisLocal, forwardAxisLocalPositive);
			back = GetVectorForAxis(forwardAxisLocal, !forwardAxisLocalPositive);
			right = GetVectorForAxis(rightAxisLocal, rightAxisLocalPositive);
			left = GetVectorForAxis(rightAxisLocal, !rightAxisLocalPositive);

			return new SurfaceAngleSnapDirections()
			{
				Forward = surface.transform.TransformDirection(forward),
				Back = surface.transform.TransformDirection(back),
				Right = surface.transform.TransformDirection(right),
				Left = surface.transform.TransformDirection(left)
			};
		}

		Vector3 SnapInputVector(Vector3 inputVector, SurfaceAngleSnapDirections directions,
			float threshold)
		{
			float forwardDot, backDot, rightDot, leftDot;

			forwardDot = Vector3.Dot(inputVector, directions.Forward);
			backDot = Vector3.Dot(inputVector, directions.Back);
			rightDot = Vector3.Dot(inputVector, directions.Right);
			leftDot = Vector3.Dot(inputVector, directions.Left);

			float maxDot = Mathf.Max(forwardDot, leftDot, rightDot, backDot);
			Vector3 matchDirection = Vector3.zero;

			if (maxDot == forwardDot) matchDirection = directions.Forward;
			else if (maxDot == backDot) matchDirection = directions.Back;
			else if (maxDot == rightDot) matchDirection = directions.Right;
			else matchDirection = directions.Left;

			Debug.Assert(matchDirection != Vector3.zero, "match direction was zero");

			float angle = Vector3.Angle(inputVector, matchDirection);

			return (angle < threshold) ? matchDirection : inputVector;
		}

		Pose GetSurfaceSnapPose(Pose inputPose)
		{
			Pose snappedPose = inputPose;

			// get the most recent 2 points
			surfaceSnap = GetSnapForCollider(inputPose.position, surfaceSnap.SurfaceCollider);

			Vector3 poseOffset = (graspItem.RigidBody.position - surfaceSnap.ObjectPoint);
			float distance = poseOffset.magnitude;

			// Start building our rotation
			// get our surface normal in object local space, then find our local forward and right vectors
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

			// this approach lets us define a regular old rotation,
			// by using our surface normal as up, a vector from the object as a 'forward',
			// and project that forward onto the plane of the surface normal to achieve a perfect alignment
			// to the surface
			Vector3 upVector = surfaceSnap.SurfaceNormal;
			Vector3 forwardVector = inputPose.rotation * (forwardVectorLocal);
			forwardVector = Vector3.ProjectOnPlane(forwardVector, surfaceSnap.SurfaceNormal);

			// to do angle snap, we can use basis vectors from the surface collider to provide snap angles
			// for the forward vector
			if(angleAroundSurfaceSnap)
			{
				SurfaceAngleSnapDirections directions = GetAngleSnapDirectionsFromSurface(upVector, surfaceSnap.SurfaceCollider.transform);
				forwardVector = SnapInputVector(forwardVector, directions, aroundNormalAngleSnap);
			}

			Quaternion surfaceRotation = Quaternion.LookRotation(forwardVector, upVector);
			Quaternion rotation = surfaceRotation * Quaternion.Inverse(localRebasedRotation); // this operation lets us 'rebase' the simple surface rotation
																							  // into our object's local coordinates, the object to align
																							  // to the surface regardless of the object's entry orientation.

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
		#endregion

		void CheckGridSnap()
		{
			if (SnapGrid.SnapGridCount() > 0)
			{
				Vector3 centerOfMass = graspItem.RigidBody.worldCenterOfMass;

				if (!isSnapping)
				{
					// should we start snapping?
					// check to see if any of our grid snaps are within the distance threshold

					surfaceSnapDetectRadius = surfaceSnapDistance * 2f; // why are we modifying surface snap detect radius?
					/*int hits = Physics.OverlapSphereNonAlloc(centerOfMass,
						graspItem.ItemRadius + surfaceSnapDetectRadius, colliderCheckBuffer);*/

					float closestDistance = float.PositiveInfinity;
					int closestIndex = -1;

					for(int gridIndx=0; gridIndx < SnapGrid.SnapGridCount(); gridIndx++)
					{
						SnapGrid grid = SnapGrid.GetGridForIndex(gridIndx);

						// get closest point on grid
						Vector3 pointInGridLocal = grid.transform.InverseTransformPoint(centerOfMass);

						// are we on the correct side of the grid?
						bool correctSide = pointInGridLocal.y > 0;
						bool isInBounds = grid.IsInBounds(new Vector3(pointInGridLocal.x, 0, pointInGridLocal.z));

						if(correctSide && isInBounds)
						{
							if(grid.Type == SnapGrid.GridType.TwoDimensional) // no clue what to do with 3d grids yet
							{
								float distance = pointInGridLocal.y;

								if(distance < closestDistance && 
									distance < 
									(surfaceSnapDetectRadius + graspItem.ItemRadius)) // if greater than the surface snap detect radius, don't bother
								{
									closestDistance = distance;
									closestIndex = gridIndx;
								}
							}
						}
					}

					// we've got a grid. Let's see if we should snap to it
					if(closestIndex >= 0)
					{
						if (!snapHasReset)
						{
							if (closestDistance > (surfaceSnapDistance + 0.05f))
							{
								snapHasReset = true;
							}
						}

						SnapGrid grid = SnapGrid.GetGridForIndex(closestIndex);
						GridSnapInfo2D snapInfo = GetGridSnap(centerOfMass, grid);

						if(snapInfo.Distance < surfaceSnapDistance && snapHasReset)
						{
							isSnapping = true;
							snapHasReset = false;
							Debug.Log("Grid snap start on grid: " + grid.name);
							currentGrid = grid;
						}
					}
				}
				else
				{
					// should we stop snapping?
					if (graspItem.MaxStrain > snapBreakStrainAmount)
					{
						isSnapping = false;
						currentGrid = null;
					}
				}
			}
			else
			{
				isSnapping = false;
			}
		}

		GridSnapInfo2D GetGridSnap(Vector3 inputPosition, SnapGrid grid)
		{
			GridSnapInfo2D snapInfo = new GridSnapInfo2D();

			Vector3 projectedPointOnPlane = grid.GetSnappedPosition(inputPosition);
			snapInfo.SurfacePoint = projectedPointOnPlane;

			Vector3 objectClosest = Vector3.zero;
			Vector3 surfToObject = (graspItem.RigidBody.worldCenterOfMass - projectedPointOnPlane).normalized;
			RaycastHit hitInfo;

			if (Physics.Raycast(new Ray(projectedPointOnPlane, surfToObject), out hitInfo))
			{
				objectClosest = hitInfo.point;
				snapInfo.ObjectNormal = hitInfo.normal;
			}
			else
			{
				// this is weird, what do we do here?
				Debug.Log("Hit weird raycast failure in GetSnapForCollider");
				//Debug.Break();
			}

			snapInfo.ObjectPoint = objectClosest;

			snapInfo.Distance = Vector3.Distance(projectedPointOnPlane, objectClosest);

			// get our surface normal
			snapInfo.SurfaceNormal = grid.transform.up;

			// push our point away from the surface a small amount so that it doesn't freak out
			snapInfo.SurfacePoint += (surfaceSnap.SurfaceNormal * surfaceAdjustAmt);

			snapInfo.Grid = grid;

			return snapInfo;
		}

		Pose GetGridSnapPose(Pose inputPose)
		{
			Pose snappedPose = inputPose;

			GridSnapInfo2D gridSnap = GetGridSnap(snappedPose.position, currentGrid);

			SetDebugPointsToGridSnap(gridSnap);

			Vector3 poseOffset = (graspItem.RigidBody.position - gridSnap.ObjectPoint);
			float distance = poseOffset.magnitude;

			// Start building our rotation
			// get our surface normal in object local space, then find our local forward and right vectors
			bool objectNormalPositive = false;
			Vector3 worldSpaceObjectNormalInverse = gridSnap.ObjectNormal * -1;
			Axis objectNormalLocal = GetSnapAxisForNormal(worldSpaceObjectNormalInverse, out objectNormalPositive);

			bool objectForwardPositive, objectRightPositive;
			Axis objectForwardAxis, objectRightAxis;

			objectForwardAxis = GetForwardAndRightAxisForNormal(objectNormalLocal, objectNormalPositive,
				out objectForwardPositive, out objectRightAxis, out objectRightPositive);

			Vector3 upVectorLocal = GetVectorForAxis(objectNormalLocal, objectNormalPositive),
				forwardVectorLocal = GetVectorForAxis(objectForwardAxis, objectForwardPositive);
			Quaternion localRebasedRotation = Quaternion.LookRotation(forwardVectorLocal, upVectorLocal);

			// this approach lets us define a regular old rotation,
			// by using our surface normal as up, a vector from the object as a 'forward',
			// and project that forward onto the plane of the surface normal to achieve a perfect alignment
			// to the surface
			Vector3 upVector = gridSnap.SurfaceNormal;
			Vector3 forwardVector = inputPose.rotation * (forwardVectorLocal);
			forwardVector = Vector3.ProjectOnPlane(forwardVector, gridSnap.SurfaceNormal);

			// to do angle snap, we can use basis vectors from the surface collider to provide snap angles
			// for the forward vector
			if (true) // formerly angleAroundSurfaceSnap
			{
				SurfaceAngleSnapDirections directions = GetAngleSnapDirectionsFromSurface(upVector, gridSnap.Grid.transform);
				forwardVector = SnapInputVector(forwardVector, directions, 44); // not quite 45 even though it should be because 45 exactly prevents hitting diagonals
			}

			Quaternion surfaceRotation = Quaternion.LookRotation(forwardVector, upVector);
			Quaternion rotation = surfaceRotation * Quaternion.Inverse(localRebasedRotation); // this operation lets us 'rebase' the simple surface rotation
																							  // into our object's local coordinates, the object to align
																							  // to the surface regardless of the object's entry orientation.

			Vector3 position = gridSnap.SurfacePoint + (gridSnap.SurfaceNormal * distance);
			snappedPose.position = position;
			snappedPose.rotation = rotation;

			return snappedPose;
		}

		void SetDebugPointsToGridSnap(GridSnapInfo2D snapInfo)
		{
			debugSphereA.transform.position = snapInfo.ObjectPoint;
			debugSphereB.transform.position = snapInfo.SurfacePoint;

			debugSphereA.SetActive(true);
			debugSphereB.SetActive(true);
		}

		Pose DoGridPose(Pose targetPose)
		{
			CheckGridSnap();

			float snapTValue = Mathf.InverseLerp(0, snapDuration, snapTimer);

			// conditional below is for guarding against no recent snaps
			// also important to note that this will not allow for switching surfaces at snap time
			// with the distances involved in snapping/unsnapping, this could make it hard to handle
			// scenarios with multiple surfaces in close proximity
			Pose snapPose = (currentGrid) ? GetGridSnapPose(targetPose) : targetPose;

			Pose lerpPose = new Pose(Vector3.Lerp(targetPose.position, snapPose.position, snapTValue),
				Quaternion.Slerp(targetPose.rotation, snapPose.rotation, snapTValue));

			return lerpPose;
		}

        public override Pose DoConstraint(Pose targetPose)
        {
			if(isSnapping)
			{
				snapTimer += Core.Time.fixedDeltaTime;
				snapTimer = Mathf.Clamp(snapTimer, 0, snapDuration);
			}
			else
			{
				snapTimer -= Core.Time.fixedDeltaTime;
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

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(surfaceSnap.ObjectPoint, surfaceSnap.ObjectPoint + (surfaceSnap.ObjectNormal * 0.01f));

			Gizmos.color = Color.green;
			Gizmos.DrawLine(surfaceSnap.SurfacePoint, surfaceSnap.SurfacePoint + (surfaceSnap.SurfaceNormal * 0.01f));
		}
	}
}