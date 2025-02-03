using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

namespace Instrumental.Core.Math
{
#if UNITY
	[System.Serializable]
#endif
	public struct Vect2
	{
#if UNITY
		private UnityEngine.Vector2 v;
#elif STEREOKIT
		private Vec2 v;
#endif

		public float x
		{
			get { return v.x; }
			set { v.x = value; }
		}

		public float y
		{
			get { return v.y; }
			set { v.y = value; }
		}

		public Vect2(float x, float y)
		{
			v.x = x;
			v.y = y;
		}

		public float this[int index]
		{
			get
			{
				if (index == 0) return x;
				else if (index == 1) return y;
				else throw new System.IndexOutOfRangeException();
			}
			set
			{
				if (index == 0) x = value;
				else if (index == 1) y = value;
				else throw new System.IndexOutOfRangeException();
			}
		}

#if UNITY
		public static explicit operator Vect2(UnityEngine.Vector2 v2) =>
			new Vect2(v2.x, v2.y);

		public static explicit operator UnityEngine.Vector2(Vect2 v2) =>
			v2.v;
#elif STEREOKIT
		public static explicit operator Vector2(StereoKit.Vec2 v2) =>
			new Vector3(v2.x, v2.y);

		public static explicit operator StereoKit.Vec2(Vector2 v2) =>
			new Vector3(v2.x, v2.y);
#endif

		public static implicit operator Vect2(Vect3 _v)
		{
			return new Vect2(_v.x, _v.y);
		}

		public static implicit operator Vect3(Vect2 _v)
		{
			return new Vect3(_v.x, _v.y, 0);
		}

		public static readonly Vect2 right = new Vect2(1, 0);
		public static readonly Vect2 left = new Vect2(-1, 0);
		public static readonly Vect2 up = new Vect2(0, 1);
		public static readonly Vect2 down = new Vect2(0, -1);
		public static readonly Vect2 one = new Vect2(1, 1);
		public static readonly Vect2 zero = new Vect2(0, 0);

		public float sqrMagnitude
		{
			get
			{
#if UNITY
				return v.sqrMagnitude;
#elif STEREOKIT
				return v.LengthSq;
#endif
			}
		}

		public Vect2 normalized
		{
			get
			{
#if UNITY
				return (Vect2)v.normalized;
#elif STEREOKIT
				rerturn (Vect2)v.Normalized;
#endif
			}
		}

		public float magnitude
		{
			get
			{
#if UNITY
				return v.magnitude;
#elif STEREOKIT
				return v.Length;
#endif
			}
		}

		public static float Angle(Vect2 from, Vect2 to)
		{
#if UNITY
			return Vector2.Angle(
				(UnityEngine.Vector2)from,
				(UnityEngine.Vector2)to);
#elif STEREOKIT
			return Vec2.AngleBetween(
			(StereoKit.Vec2)from,
			(StereoKit.Vec2)to);
#endif
		}

		public static float Distance(Vect2 a, Vect2 b)
		{
#if UNITY
			return UnityEngine.Vector2.Distance(
				(UnityEngine.Vector2)a,
				(UnityEngine.Vector2)b);
#elif STEREOKIT
			return StereoKit.Vect2.Distance(
				(StereoKit.Vec2)a,
				(StereoKit.Vec2)b);
#endif
		}

		public static float Dot(Vect2 a, Vect2 b)
		{
#if UNITY
			return UnityEngine.Vector2.Dot(
				(UnityEngine.Vector2)a,
				(UnityEngine.Vector2)b);
#elif STEREOKIT
			return StereoKit.Vec2.Dot(
				(StereoKit.Vec2)a,
				(StereoKit.Vec2)b);
#endif
		}

		public static Vect2 Lerp(Vect2 a, Vect2 b, float t)
		{
			return new Vect2(
				Math.Lerp(a.x, b.x, t),
				Math.Lerp(a.y, b.y, t));
		}

		#region Operators
		public static Vect2 operator +(Vect2 a, Vect2 b)
		{
#if UNITY
			return (Vect2)((UnityEngine.Vector2)a + (UnityEngine.Vector2)b);
#elif STEREOKIT
			return (Vector2)((StereoKit.Vec2)a + (StereoKit.Vec2)b);
#endif
		}

		public static Vect2 operator -(Vect2 a)
		{
			return new Vect2(-a.x, -a.y);
		}

		public static Vect2 operator -(Vect2 a, Vect2 b)
		{
#if UNITY
			return (Vect2)((UnityEngine.Vector2)a - (UnityEngine.Vector2)b);
#elif STEREOKIT
			return (Vector2)((StereoKit.Vec2)a - (StereoKit.Vec2)b);
#endif
		}

		public static Vect2 operator *(float d, Vect2 a)
		{
#if UNITY
			return (Vect2)(d *
				(UnityEngine.Vector2)a);
#elif STEREOKIT
			return (Vector2)(d *
				(StereoKit.Vec2)a);
#endif
		}

		public static Vect2 operator *(Vect2 a, float d)
		{
#if UNITY
			return (Vect2)((UnityEngine.Vector2)a * d);
#elif STEREOKIT
			return (Vector2)((StereoKit.Vec2)a * d);
#endif
		}

		public static Vect2 operator /(Vect2 a, float d)
		{
#if UNITY
			return (Vect2)((UnityEngine.Vector2)a / d);
#elif STEREOKIT
			return (Vector2)((StereoKit.Vec2)a / d);
#endif
		}
		#endregion
	}

#if UNITY
	[System.Serializable]
#endif
	public struct Vect3
	{
#if UNITY
		private UnityEngine.Vector3 v;
#elif STEREOKIT
		private Vec3 v;
#endif

		public float x
		{
			get { return v.x; }
			set { v.x = value; }
		}

		public float y
		{
			get { return v.y; }
			set { v.y = value; }
		}

		public float z
		{
			get { return v.z; }
			set { v.z = value; }
		}

		public float this[int index]
		{
			get
			{
				if (index == 0) return x;
				else if (index == 1) return y;
				else if (index == 2) return z;
				else throw new System.IndexOutOfRangeException();
			}
			set
			{
				if (index == 0) x = value;
				else if (index == 1) y = value;
				else if (index == 2) z = value;
				else throw new System.IndexOutOfRangeException();
			}
		}

		public Vect3(float x, float y, float z)
		{
			v.x = x;
			v.y = y;
			v.z = z;
		}

#if UNITY
		public static explicit operator Vect3(UnityEngine.Vector3 v3) =>
			new Vect3(v3.x, v3.y, v3.z);

		public static explicit operator UnityEngine.Vector3(Vect3 v3) =>
			v3.v;
#elif STEREOKIT
		public static explicit operator Vector3(StereoKit.Vec3 v3) =>
			new Vector3(v3.x, v3.y, v3.z);

		public static explicit operator StereoKit.Vec3(Vector3 v3) =>
			new Vector3(v3.x, v3.y, v3.z);
#endif

		public static readonly Vect3 right =
			new Vect3(1, 0, 0);
		public static readonly Vect3 left =
			new Vect3(-1, 0, 0);
		public static readonly Vect3 up =
			new Vect3(0, 1, 0);
		public static readonly Vect3 back =
#if UNITY
			new Vect3(-1, 0, 0);
#elif STEREOKIT
			new Vector3(1, 0, 0);
#endif
		public static readonly Vect3 forward =
#if UNITY
			new Vect3(0, 0, 1);
#elif STEREOKIT
			new Vector3(0, 0, -1);
#endif
		public static readonly Vect3 one =
			new Vect3(1, 1, 1);
		public static readonly Vect3 zero =
			new Vect3(0, 0, 0);
		public static readonly Vect3 down =
			new Vect3(0, -1, 0);

		public Vect3 normalized
		{
			get
			{
#if UNITY
				return (Vect3)v.normalized;
#elif STEREOKIT
				return (Vector3)v.Normalized;
#endif
			}
		}

		public float magnitude
		{
			get
			{
#if UNITY
				return v.magnitude;
#elif STEREOKIT
				return v.Length;
#endif
			}
		}

		public float sqrMagnitude
		{
			get
			{
#if UNITY
				return v.sqrMagnitude;
#elif STEREOKIT
				return v.LengthSq;
#endif
			}
		}

		public static float Angle(Vect3 from, Vect3 to)
		{
#if UNITY
			return UnityEngine.Vector3.Angle(
				(UnityEngine.Vector3)from,
				(UnityEngine.Vector3)to);
#elif STEREOKIT
			return StereoKit.Vec3.AngleBetween(
				(StereoKit.Vec3)from,
				(StereoKit.Vec3)to);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Distance(Vect3 a, Vect3 b)
		{
#if UNITY
			return UnityEngine.Vector3.Distance(
				(UnityEngine.Vector3)a,
				(UnityEngine.Vector3)b);
#elif STEREOKIT
			return StereoKit.Vec3.Distance(
				(StereoKit.Vec3)a,
				(StereoKit.Vec3)b);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Dot(Vect3 a, Vect3 b)
		{
#if UNITY
			return UnityEngine.Vector3.Dot(
				(UnityEngine.Vector3)a,
				(UnityEngine.Vector3)b);
#elif STEREOKIT
			return StereoKit.Vec3.Dot(
				(StereoKit.Vec3)a,
				(StereoKit.Vec3)b);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vect3 Cross(Vect3 a, Vect3 b)
		{
#if UNITY
			return (Vect3)UnityEngine.Vector3.Cross(
				(UnityEngine.Vector3)a,
				(UnityEngine.Vector3)b);
#elif STEREOKIT
			return (Vector3)StereoKit.Vec3.Cross(
				(StereoKit.Vec3)a,
				(StereoKit.Vec3)b);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vect3 ProjectOnPlane(Vect3 vector, Vect3 onNormal)
		{
#if UNITY
			Vector3 uVector, uOnNormal;
			uVector = (Vector3)vector;
			uOnNormal = (Vector3)onNormal;

			return (Vect3)Vector3.ProjectOnPlane(uVector, uOnNormal);
#elif STEREOKIT
			throw new System.NotImplementedException();
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SignedAngle(Vect3 from, Vect3 to, Vect3 axis)
		{
#if UNITY
			Vector3 uFrom, uTo, uAxis;
			uFrom = (Vector3)from;
			uTo = (Vector3)to;
			uAxis = (Vector3)axis;

			return Vector3.SignedAngle(uFrom, uTo, uAxis);
#elif STEREOKIT
			throw new System.NotImplementedException();
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vect3 Lerp(Vect3 a, Vect3 b, float t)
		{
#if UNITY
			return (Vect3)UnityEngine.Vector3.Lerp(
				(UnityEngine.Vector3)a,
				(UnityEngine.Vector3)b,
				t);
#elif STEREOKIT
			return (Vector3)StereoKit.Vec3.Lerp(
			(StereoKit.Vec3)a,
			(StereoKit.Vec3)b,
			t);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vect3 LerpUnclamped(Vect3 a, Vect3 b, float t)
		{
			return new Vect3(
				Math.Lerp(a.x, b.x, t),
				Math.Lerp(a.y, b.y, t),
				Math.Lerp(a.z, b.z, t));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vect3 Scale(Vect3 a, Vect3 b)
		{
#if UNITY
			Vector3 uA, uB;
			uA = (Vector3)a;
			uB = (Vector3)b;

			return (Vect3)Vector3.Scale(uA, uB);
#elif STEREOKIT
			return new Vect3(
			a.x * b.x,
			a.y * b.y,
			a.z * b.z);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vect3 Slerp(Vect3 a, Vect3 b, float t)
		{
#if UNITY
			Vector3 uA, uB;
			uA = (Vector3)a;
			uB = (Vector3)b;

			return (Vect3)Vector3.Slerp(uA, uB, t);
#elif STEREOKIT
			throw new System.NotImplementedException();
#endif
		}

#region Operators
		public static Vect3 operator +(Vect3 a, Vect3 b)
		{
#if UNITY
			return (Vect3)((UnityEngine.Vector3)a + (UnityEngine.Vector3)b);
#elif STEREOKIT
			return (Vector3)((StereoKit.Vec3)a + (StereoKit.Vec3)b);
#endif
		}

		public static Vect3 operator -(Vect3 a)
		{
			return new Vect3(-a.x, -a.y, -a.z);
		}

		public static Vect3 operator -(Vect3 a, Vect3 b)
		{
#if UNITY
			return (Vect3)((UnityEngine.Vector3)a - (UnityEngine.Vector3)b);
#elif STEREOKIT
			return (Vector3)((StereoKit.Vec3)a - (StereoKit.Vec3)b);
#endif
		}

		public static Vect3 operator *(float d, Vect3 a)
		{
#if UNITY
			return (Vect3)(d *
				(UnityEngine.Vector3)a);
#elif STEREOKIT
			return (Vector3)(d *
				(StereoKit.Vec3)a);
#endif
		}

		public static Vect3 operator *(Vect3 a, float d)
		{
#if UNITY
			return (Vect3)((UnityEngine.Vector3)a * d);
#elif STEREOKIT
			return (Vector3)((StereoKit.Vec3)a * d);
#endif
		}

		public static Vect3 operator /(Vect3 a, float d)
		{
#if UNITY
			return (Vect3)((UnityEngine.Vector3)a / d);
#elif STEREOKIT
			return (Vector3)((StereoKit.Vec3)a / d);
#endif
		}
#endregion
	}
}