using System.Collections;
using System.Collections.Generic;

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

