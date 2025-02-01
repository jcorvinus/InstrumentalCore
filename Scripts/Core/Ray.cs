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
	public struct RayIC
	{
#if UNITY
		private UnityEngine.Ray uRay;
#elif STEREOKIT
		private StereoKit.Ray skRay;
#endif

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