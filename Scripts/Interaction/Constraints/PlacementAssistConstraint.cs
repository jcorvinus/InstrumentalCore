using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;
using Instrumental.Core.Math;

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
			public Vect3 SurfacePoint;
			public Vect3 SurfaceNormal;
			public Vect3 ObjectPoint;
			public Vect3 ObjectNormal;
			public float Distance;
			public Collider SurfaceCollider;
		}

		SurfaceSnapInfo surfaceSnap;

		public struct SurfaceAngleSnapDirections
		{
			public Vect3 Forward;
			public Vect3 Back;
			public Vect3 Right;
			public Vect3 Left;
		}
		#endregion

		// grid mode vars
		public struct GridSnapInfo2D
		{
			public Vect3 SurfacePoint;
			public Vect3 SurfaceNormal;
			public Vect3 ObjectPoint;
			public Vect3 ObjectNormal;
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
			Vect3 centerOfMass = (Vect3)graspItem.RigidBody.worldCenterOfMass;

			if (!isSnapping)
			{
				// should we start snapping
				// first we need our reference point - where's our nearest surface?
				surfaceSnapDetectRadius = surfaceSnapDistance * 2f;
				int hits = Physics.OverlapSphereNonAlloc((Vector3)centerOfMass,
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
							SurfaceSnapInfo candidateSnapInfo = GetSnapForCollider(
								(Vect3)graspItem.RigidBody.position, candidateCollider);
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
		SurfaceSnapInfo GetSnapForCollider(Vect3 inputPosition, Collider surfaceCollider)
		{
			SurfaceSnapInfo snapInfo = new SurfaceSnapInfo();
			snapInfo.SurfaceCollider = surfaceCollider;

			Vect3 surfaceClosest = (Vect3)surfaceCollider.ClosestPoint((Vector3)inputPosition);
			snapInfo.SurfacePoint = surfaceClosest;

			Vect3 objectClosest = Vect3.zero;

			// use raycast
			Vect3 surfToObject = ((Vect3)graspItem.RigidBody.worldCenterOfMass - surfaceClosest).normalized;
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray((Vector3)surfaceClosest, (Vector3)surfToObject), out hitInfo))
			{
				objectClosest = (Vect3)hitInfo.point;
				snapInfo.ObjectNormal = (Vect3)hitInfo.normal;
			}
			else
			{
				// this is weird, what do we do here?
				Debug.Log("Hit weird raycast failure in GetSnapForCollider");
				Debug.Break();
			}

			snapInfo.ObjectPoint = objectClosest;

			// get our surface normal
			Vect3 objectToSurfaceDirection = (surfaceClosest - objectClosest);
			float objectToSurfaceDistance = objectToSurfaceDirection.magnitude; // you've screwed this up
			objectToSurfaceDirection /= objectToSurfaceDistance;

			if (objectToSurfaceDistance <= Mathf.Epsilon)
			{
				// our length is zero, because the object is already sitting on the surface,
				// likely because gravity is on and the object has settled.
				Debug.Log("object to surface direction changed because of zero length distance");
				objectToSurfaceDirection = (snapInfo.SurfacePoint - (Vect3)graspItem.RigidBody.worldCenterOfMass).normalized;
			}
			else
			{
				objectToSurfaceDirection /= objectToSurfaceDistance;
			}
			snapInfo.Distance = objectToSurfaceDistance;

			RaycastHit surfNormalHit;
			if(snapInfo.SurfaceCollider.Raycast(new Ray(graspItem.RigidBody.worldCenterOfMass, 
				(Vector3)objectToSurfaceDirection),
				out surfNormalHit, surfaceSnapDetectRadius))
			{
				snapInfo.SurfaceNormal = (Vect3)surfNormalHit.normal;
			}

			// push our point away from the surface a small amount so that it doesn't freak out
			snapInfo.SurfacePoint += (surfaceSnap.SurfaceNormal * surfaceAdjustAmt);

			return snapInfo;
		}

		Axis GetSnapAxisForNormal(Vect3 objectNormal, out bool positive)
		{
			positive = false;

			Vect3 localNormal = (Vect3)transform.InverseTransformDirection((Vector3)objectNormal);

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

		Vect3 GetVectorForAxis(Axis axis, bool positive)
		{
			switch (axis)
			{
				case Axis.None:
					return Vect3.zero;
				case Axis.X:
					return Vect3.right * ((positive) ? 1 : -1);
				case Axis.Y:
					return Vect3.up * ((positive) ? 1 : -1);
				case Axis.Z:
					return Vect3.forward * ((positive) ? 1 : -1);
				case Axis.All:
					return Vect3.one;
				default:
					return Vect3.zero;
			}
		}

		SurfaceAngleSnapDirections GetAngleSnapDirectionsFromSurface(Vect3 normal, Transform surface)
		{
			Vect3 normalLocal = (Vect3)surface.transform.InverseTransformDirection((Vector3)normal);

			bool normalLocalPositive;
			Axis normalAxisLocal = GetSnapAxisForNormal(normalLocal, out normalLocalPositive);

			Axis forwardAxisLocal, rightAxisLocal;
			bool forwardAxisLocalPositive, rightAxisLocalPositive;
			forwardAxisLocal = GetForwardAndRightAxisForNormal(normalAxisLocal, normalLocalPositive, out forwardAxisLocalPositive,
				out rightAxisLocal, out rightAxisLocalPositive);

			Vect3 forward, back, right, left;
			forward = GetVectorForAxis(forwardAxisLocal, forwardAxisLocalPositive);
			back = GetVectorForAxis(forwardAxisLocal, !forwardAxisLocalPositive);
			right = GetVectorForAxis(rightAxisLocal, rightAxisLocalPositive);
			left = GetVectorForAxis(rightAxisLocal, !rightAxisLocalPositive);

			return new SurfaceAngleSnapDirections()
			{
				Forward = (Vect3)surface.transform.TransformDirection((Vector3)forward),
				Back = (Vect3)surface.transform.TransformDirection((Vector3)back),
				Right = (Vect3)surface.transform.TransformDirection((Vector3)right),
				Left = (Vect3)surface.transform.TransformDirection((Vector3)left)
			};
		}

		Vect3 SnapInputVector(Vect3 inputVector, SurfaceAngleSnapDirections directions,
			float threshold)
		{
			float forwardDot, backDot, rightDot, leftDot;

			forwardDot = Vect3.Dot(inputVector, directions.Forward);
			backDot = Vect3.Dot(inputVector, directions.Back);
			rightDot = Vect3.Dot(inputVector, directions.Right);
			leftDot = Vect3.Dot(inputVector, directions.Left);

			float maxDot = Mathf.Max(forwardDot, leftDot, rightDot, backDot);
			Vect3 matchDirection = Vect3.zero;

			if (maxDot == forwardDot) matchDirection = directions.Forward;
			else if (maxDot == backDot) matchDirection = directions.Back;
			else if (maxDot == rightDot) matchDirection = directions.Right;
			else matchDirection = directions.Left;

			Debug.Assert(matchDirection.x != 0 ||
				matchDirection.y != 0 ||
				matchDirection.z != 0, "match direction was zero"); // used to be matchDirection != Vector3.zero

			float angle = Vect3.Angle(inputVector, matchDirection);

			return (angle < threshold) ? matchDirection : inputVector;
		}

		PoseIC GetSurfaceSnapPose(PoseIC inputPose)
		{
			PoseIC snappedPose = inputPose;

			// get the most recent 2 points
			surfaceSnap = GetSnapForCollider(inputPose.position, surfaceSnap.SurfaceCollider);

			Vect3 poseOffset = ((Vect3)graspItem.RigidBody.position - surfaceSnap.ObjectPoint);
			float distance = poseOffset.magnitude;

			// Start building our rotation
			// get our surface normal in object local space, then find our local forward and right vectors
			bool objectNormalPositive = false;
			Vect3 worldSpaceObjectNormalInverse = surfaceSnap.ObjectNormal * -1;
			Axis objectNormalLocal = GetSnapAxisForNormal(worldSpaceObjectNormalInverse, out objectNormalPositive);

			bool objectForwardPositive, objectRightPositive;
			Axis objectForwardAxis, objectRightAxis;

			objectForwardAxis = GetForwardAndRightAxisForNormal(objectNormalLocal, objectNormalPositive,
				out objectForwardPositive, out objectRightAxis, out objectRightPositive);

			Vect3 upVectorLocal = GetVectorForAxis(objectNormalLocal, objectNormalPositive),
				forwardVectorLocal = GetVectorForAxis(objectForwardAxis, objectForwardPositive);
			Quatn localRebasedRotation = Quatn.LookRotation(forwardVectorLocal, upVectorLocal);

			// this approach lets us define a regular old rotation,
			// by using our surface normal as up, a vector from the object as a 'forward',
			// and project that forward onto the plane of the surface normal to achieve a perfect alignment
			// to the surface
			Vect3 upVector = surfaceSnap.SurfaceNormal;
			Vect3 forwardVector = inputPose.rotation * (forwardVectorLocal);
			forwardVector = Vect3.ProjectOnPlane(forwardVector, surfaceSnap.SurfaceNormal);

			// to do angle snap, we can use basis vectors from the surface collider to provide snap angles
			// for the forward vector
			if(angleAroundSurfaceSnap)
			{
				SurfaceAngleSnapDirections directions = GetAngleSnapDirectionsFromSurface(upVector, surfaceSnap.SurfaceCollider.transform);
				forwardVector = SnapInputVector(forwardVector, directions, aroundNormalAngleSnap);
			}

			Quatn surfaceRotation = Quatn.LookRotation(forwardVector, upVector);
			Quatn rotation = surfaceRotation * Quatn.Inverse(localRebasedRotation); // this operation lets us 'rebase' the simple surface rotation
																					// into our object's local coordinates, the object to align
																					// to the surface regardless of the object's entry orientation.

			Vect3 position = surfaceSnap.SurfacePoint + (surfaceSnap.SurfaceNormal * distance);
			snappedPose.position = position;
			snappedPose.rotation = rotation;

			return snappedPose;
		}

		PoseIC DoSurfacePose(PoseIC targetPose)
		{
			CheckSurfaceSnap();

			float snapTValue = Mathf.InverseLerp(0, snapDuration, snapTimer);

			// conditional below is for guarding against no recent snaps
			// also important to note that this will not allow for switching surfaces at snap time
			// with the distances involved in snapping/unsnapping, this could make it hard to handle
			// scenarios with multiple surfaces in close proximity
			PoseIC snapPose = (surfaceSnap.SurfaceCollider) ? GetSurfaceSnapPose(targetPose) : targetPose;

			PoseIC lerpPose = new PoseIC(Vect3.Lerp(targetPose.position, snapPose.position, snapTValue),
				Quatn.Slerp(targetPose.rotation, snapPose.rotation, snapTValue));

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
						bool isInBounds = grid.IsInBounds(
							new Vect3(pointInGridLocal.x, 0, pointInGridLocal.z));

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
						GridSnapInfo2D snapInfo = GetGridSnap((Vect3)centerOfMass, grid);

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

		GridSnapInfo2D GetGridSnap(Vect3 inputPosition, SnapGrid grid)
		{
			GridSnapInfo2D snapInfo = new GridSnapInfo2D();

			Vect3 projectedPointOnPlane = grid.GetSnappedPosition(inputPosition);
			snapInfo.SurfacePoint = projectedPointOnPlane;

			Vect3 objectClosest = Vect3.zero;
			Vect3 surfToObject = ((Vect3)graspItem.RigidBody.worldCenterOfMass - projectedPointOnPlane).normalized;
			RaycastHit hitInfo;

			if (Physics.Raycast(new Ray((Vector3)projectedPointOnPlane, (Vector3)surfToObject), out hitInfo))
			{
				objectClosest = (Vect3)hitInfo.point;
				snapInfo.ObjectNormal = (Vect3)hitInfo.normal;
			}
			else
			{
				// this is weird, what do we do here?
				Debug.Log("Hit weird raycast failure in GetSnapForCollider");
				//Debug.Break();
			}

			snapInfo.ObjectPoint = objectClosest;

			snapInfo.Distance = Vect3.Distance(projectedPointOnPlane, objectClosest);

			// get our surface normal
			snapInfo.SurfaceNormal = (Vect3)grid.transform.up;

			// push our point away from the surface a small amount so that it doesn't freak out
			snapInfo.SurfacePoint += (surfaceSnap.SurfaceNormal * surfaceAdjustAmt);

			snapInfo.Grid = grid;

			return snapInfo;
		}

		PoseIC GetGridSnapPose(PoseIC inputPose)
		{
			PoseIC snappedPose = inputPose;

			GridSnapInfo2D gridSnap = GetGridSnap(snappedPose.position, currentGrid);

			SetDebugPointsToGridSnap(gridSnap);

			Vect3 poseOffset = ((Vect3)graspItem.RigidBody.position - gridSnap.ObjectPoint);
			float distance = poseOffset.magnitude;

			// Start building our rotation
			// get our surface normal in object local space, then find our local forward and right vectors
			bool objectNormalPositive = false;
			Vect3 worldSpaceObjectNormalInverse = gridSnap.ObjectNormal * -1;
			Axis objectNormalLocal = GetSnapAxisForNormal(worldSpaceObjectNormalInverse, out objectNormalPositive);

			bool objectForwardPositive, objectRightPositive;
			Axis objectForwardAxis, objectRightAxis;

			objectForwardAxis = GetForwardAndRightAxisForNormal(objectNormalLocal, objectNormalPositive,
				out objectForwardPositive, out objectRightAxis, out objectRightPositive);

			Vect3 upVectorLocal = GetVectorForAxis(objectNormalLocal, objectNormalPositive),
				forwardVectorLocal = GetVectorForAxis(objectForwardAxis, objectForwardPositive);
			Quatn localRebasedRotation = Quatn.LookRotation(forwardVectorLocal, upVectorLocal);

			// this approach lets us define a regular old rotation,
			// by using our surface normal as up, a vector from the object as a 'forward',
			// and project that forward onto the plane of the surface normal to achieve a perfect alignment
			// to the surface
			Vect3 upVector = gridSnap.SurfaceNormal;
			Vect3 forwardVector = inputPose.rotation * (forwardVectorLocal);
			forwardVector = Vect3.ProjectOnPlane(forwardVector, gridSnap.SurfaceNormal);

			// to do angle snap, we can use basis vectors from the surface collider to provide snap angles
			// for the forward vector
			if (true) // formerly angleAroundSurfaceSnap
			{
				SurfaceAngleSnapDirections directions = GetAngleSnapDirectionsFromSurface(upVector, gridSnap.Grid.transform);
				forwardVector = SnapInputVector(forwardVector, directions, 44); // not quite 45 even though it should be because 45 exactly prevents hitting diagonals
			}

			Quatn surfaceRotation = Quatn.LookRotation(forwardVector, upVector);
			Quatn rotation = surfaceRotation * Quatn.Inverse(localRebasedRotation); // this operation lets us 'rebase' the simple surface rotation
																					// into our object's local coordinates, the object to align
																					// to the surface regardless of the object's entry orientation.

			Vect3 position = gridSnap.SurfacePoint + (gridSnap.SurfaceNormal * distance);
			snappedPose.position = position;
			snappedPose.rotation = rotation;

			return snappedPose;
		}

		void SetDebugPointsToGridSnap(GridSnapInfo2D snapInfo)
		{
			debugSphereA.transform.position = (Vector3)snapInfo.ObjectPoint;
			debugSphereB.transform.position = (Vector3)snapInfo.SurfacePoint;

			debugSphereA.SetActive(true);
			debugSphereB.SetActive(true);
		}

		PoseIC DoGridPose(PoseIC targetPose)
		{
			CheckGridSnap();

			float snapTValue = Mathf.InverseLerp(0, snapDuration, snapTimer);

			// conditional below is for guarding against no recent snaps
			// also important to note that this will not allow for switching surfaces at snap time
			// with the distances involved in snapping/unsnapping, this could make it hard to handle
			// scenarios with multiple surfaces in close proximity
			PoseIC snapPose = (currentGrid) ? GetGridSnapPose(targetPose) : targetPose;

			PoseIC lerpPose = new PoseIC(Vect3.Lerp(targetPose.position, snapPose.position, snapTValue),
				Quatn.Slerp(targetPose.rotation, snapPose.rotation, snapTValue));

			return lerpPose;
		}

        public override PoseIC DoConstraint(PoseIC targetPose)
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
#if UNITY
			Gizmos.color = Color.blue;
			Gizmos.DrawLine((Vector3)surfaceSnap.ObjectPoint, 
				(Vector3)(surfaceSnap.ObjectPoint + (surfaceSnap.ObjectNormal * 0.01f)));

			Gizmos.color = Color.green;
			Gizmos.DrawLine((Vector3)surfaceSnap.SurfacePoint, 
				(Vector3)(surfaceSnap.SurfacePoint + (surfaceSnap.SurfaceNormal * 0.01f)));
#endif
		}
	}
}