using System.Collections;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core.Math;

namespace Instrumental.Core
{
#if UNITY
	[System.Serializable]
#endif
	public struct PlaneIC
	{
#if UNITY
		private UnityEngine.Plane uPlane;
#elif STEREOKIT
		private StereoKit.Plane skPlane;
#endif

		public PlaneIC(Vect3 a, Vect3 b, Vect3 c)
		{
#if UNITY
			uPlane = new Plane((Vector3)a, (Vector3)b, (Vector3)c);
#elif STEREOKIT
			Vec3 skA, Vec3 skB, Vec3 skC;
			skA = (Vec3)a;
			skB = (Vec3)b;
			skC = (Vec3)c;
			skPlane = Plane.FromPoints(skA, skB, skC);
#endif
		}

		public Vect3 ClosestPointOnPlane(Vect3 point)
		{
#if UNITY
			Vector3 uPoint = (Vector3)point;
			return (Vect3)uPlane.ClosestPointOnPlane(uPoint);
#elif STEREOKIT
			Vec3 skPopint = (Vec3)point;
			return (Vect3)skPlane.Closest(skPoint);
#endif
		}

		public bool Raycast(RayIC ray, out float distance)
		{
#if UNITY
			Ray uRay = (Ray)ray;
			return uPlane.Raycast(uRay, out distance);
#elif STEREOKIT
#endif
		}
	}
}