using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

namespace Instrumental.Schema
{
	/// <summary>
	/// This is a surrogate class for serializing Vector2s
	/// We do this because I need a way of serializing this type
	/// That is independent of the Vect2's per-platform internal representation
	/// </summary>
	[System.Serializable]
	public struct sV2
	{
		public float x;
		public float y;

		public sV2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

#if UNITY
		public static explicit operator Vector2(sV2 v2) =>
			new Vector2(v2.x, v2.y);

		public static explicit operator sV2(Vector2 v2) =>
			new sV2(v2.x, v2.y);
#elif STEREOKIT
		public static explicit operator Vec2(sV2 v2) =>
			new Vec2(v2.x, v2.y);

		public static explicit operator sV2(Vec2 v2) =>
			new sV2(v2.x, v2.y);
#endif
	}

	/// <summary>
	/// This is a surrogate class for serializing Vector3s
	/// We do this because I need a way of serializing this type
	/// That is independent of the Vect3's per-platform internal representation
	/// </summary>
	[System.Serializable]
	public struct sV3
	{
		public float x;
		public float y;
		public float z;

		public sV3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

#if UNITY
		public static explicit operator Vector3(sV3 v3) =>
			new Vector3(v3.x, v3.y, v3.z);

		public static explicit operator sV3(Vector3 v3) =>
			new sV3(v3.x, v3.y, v3.z);
#elif STEREOKIT
		public static explicit operator Vec3(sV3 v3) =>
			new Vec3(v3.x, v3.y, v3.z);

		public static explicit operator sV3(Vec3 v3) =>
			new sV3(v3.x, v3.y, v3.z);
#endif
	}

	/// <summary>
	/// This is a surrogate class for serializing Quaternions
	/// We do this because I need a way of serializing this type
	/// That is independent of the Quatn's per-platform internal representation
	/// </summary>
	[System.Serializable]
	public struct sQuat
	{
		public float x;
		public float y;
		public float z;
		public float w;

		public sQuat(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

#if UNITY
		public static explicit operator Quaternion(sQuat quat) =>
			new Quaternion(quat.x, quat.y, quat.z, quat.w);

		public static explicit operator sQuat(Quaternion quat) =>
			new sQuat(quat.x, quat.y, quat.z, quat.w);
#elif STEREOKIT
		public static explicit operator Quat(sQuat quat) =>
			new Quat(quat.x, quat.y, quat.z, quat.w);

		public static explicit operator sQuat(Quat quat) =>
			new Quat(quat.x, quat.y, quat.z, quat.w);
#endif
	}

	/// <summary>
	/// This is a surrogate class for serializing colors
	/// we do this because I need a way of serializing this type
	/// that is independent of the future color struct's per-platform internal representation
	/// </summary>
	[System.Serializable]
	public struct sColor
	{
		public float r;
		public float g;
		public float b;
		public float a;

		public sColor(float r, float g, float b, float a)
		{
			this.r = r;
			this.b = b;
			this.g = g;
			this.a = a;
		}
	}
}

