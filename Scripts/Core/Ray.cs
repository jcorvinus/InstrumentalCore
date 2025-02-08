using System.Collections;
using System.Collections.Generic;
using System;

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
	public struct RayIC
	{
#if UNITY
		private UnityEngine.Ray uRay;
#elif STEREOKIT
		private StereoKit.Ray skRay;
#endif

#if UNITY
		public static explicit operator RayIC(Ray ray) =>
			new RayIC((Vect3)ray.origin, (Vect3)ray.direction);

		public static explicit operator Ray(RayIC ray) =>
			new Ray((Vector3)ray.position, (Vector3)ray.direction);
#elif STEREOKIT
		public static explicit operator RayIC(Ray ray) =>
			new RayIC((Vect3)ray.position, (Vect3)ray.direction);
#endif

		public RayIC(Vect3 origin, Vect3 direction)
		{
#if UNITY
			Vector3 uOrigin, uDirection;
			uOrigin = (Vector3)origin;
			uDirection = (Vector3)direction;
			uRay = new Ray(uOrigin, uDirection);
#elif STEREOKIT
			skRay = new Ray();
			skRay.position = (Vec3)origin;
			skRay.direction = (Vec3)direction;
#endif
		}

		public Vect3 position
		{
			get
			{
#if UNITY
				return (Vect3)uRay.origin;
#elif STEREOKIT
				return (Vect3)skRay.position;
#endif
			}
			set
			{
#if UNITY
				uRay.origin = (Vector3)value;
#elif STEREOKIT
				skRay.position = (Vec3)value;
#endif
			}
		}

		public Vect3 direction
		{
			get
			{
#if UNITY
				return (Vect3)uRay.direction;
#elif STEREOKIT
				return (Vect3)skRay.direction;
#endif
			}
			set
			{
#if UNITY
				uRay.direction = (Vector3)value;
#elif STEREOKIT
				skRay.direction = (Vec3)value;
#endif
			}
		}
	}
}