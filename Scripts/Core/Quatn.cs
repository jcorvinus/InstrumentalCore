using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

namespace Instrumental.Core.Math
{
	public struct Quatn
	{
#if UNITY
		private Quaternion q;
#elif STEREOKIT
	private Quat q;
#endif

		public float x
		{
			get
			{
				return q.x;
			}
			set
			{
				q.x = value;
			}
		}

		public float y
		{
			get
			{
				return q.y;
			}
			set
			{
				q.y = value;
			}
		}

		public float z
		{
			get
			{
				return q.z;
			}
			set
			{
				q.z = value;
			}
		}

		public float w
		{
			get
			{
				return q.w;
			}
			set
			{
				q.w = value;
			}
		}

		public Quatn(float x, float y, float z, float w)
		{
			q.x = x;
			q.y = y;
			q.z = z;
			q.w = w;
		}

		// conversion operators
#if UNITY
		// these are explicit for now so that any weirdness shows up
		// as an error, will probably make them implicit later
		public static explicit operator Quatn(UnityEngine.Quaternion _q) =>
			new Quatn(_q.x, _q.y, _q.z, _q.w);

		public static explicit operator UnityEngine.Quaternion(Quatn _q) =>
			new Quaternion(_q.x, _q.y, _q.z, _q.w);
#elif STEREOKIT
		public static explicit operator Quatn(StereoKit.Quat _q) =>
			new Quatn(_q.x, _q.y, _q.z, _q.w);

		public static explicit operator StereoKit.Quat(Quatn _q) =>
			new StereoKit.Quat(_q.x, _q.y, _q.z, _q.w);
#endif

		public static readonly Quatn identity = new Quatn(0, 0, 0, 1);

		// note: we will probably have to alter any code that uses this, since
		// this functionality doesn't exist in stereokit
		public Vect3 eulerAngles
		{
			get
			{
#if UNITY
				return (Vect3)q.eulerAngles;
#elif STEREOKIT
				throw new System.NotImplementedException(); // gonna have to figure out what to do here
#endif
			}
		}

		public Quatn normalized
		{
			get
			{
#if UNITY
				return (Quatn)q.normalized;
#elif STEREOKIT
				return (Quatn)q.Normalized;
#endif
			}
		}

		public static float Angle(Quatn a, Quatn b)
		{
#if UNITY
			return Quaternion.Angle(
				(UnityEngine.Quaternion)a,
				(UnityEngine.Quaternion)b);
#elif STEREOKIT
			throw new System.NotImplementedException(); // going to have to implement angle difference between two quaternions
#endif
		}

		// get back to the other methods later, I was having problems with 
		// feature parity between unity and stereokit.
		// for now I need operator overloads so I can do multiplications

		public static Vect3 operator *(Quatn rotation, Vect3 point)
		{
#if UNITY
			Quaternion uRotation = (Quaternion)rotation;
			Vector3 uPoint = (Vector3)point;

			return (Vect3)(uRotation * uPoint);
#elif STEREOKIT
			Quat skRotation = (Quat)rotation;
			Vec3 skPoint = (Vec3)point;
			return (Vect3)(skRotation * skPoint);
#endif
		}

		public static Quatn operator *(Quatn lhs, Quatn rhs)
		{
#if UNITY
			Quaternion uLhs, uRhs;
			uLhs = (Quaternion)lhs;
			uRhs = (Quaternion)rhs;

			return (Quatn)(uLhs * uRhs);
#elif STEREOKIT
			Quat skLhs, skRhs;
			skLhs = (Quat)lhs;
			skRhs = (Quat)rhs;

			return (Quatn)(skLhs * skRhs);
#endif
		}
	}
}