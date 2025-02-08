using System.Collections;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

namespace Instrumental.Core
{
#if UNITY
	[System.Serializable]
#endif
	public struct  ColorIC
	{
#if UNITY
		Color color;
#elif STEREOKIT
		Color color;
#endif

		public float r
		{
			get { return color.r; }
			set	{ color.r = value; }
		}

		public float g
		{
			get { return color.g; }
			set { color.g = value; }
		}

		public float b
		{
			get { return color.b; }
			set { color.b = value; }
		} 

		public float a
		{
			get { return color.a; }
			set { color.a = value; }
		}

		public ColorIC(float r, float g, float b, float a)
		{
			color.r = r;
			color.g = g;
			color.b = b;
			color.a = a;
		}

		public static explicit operator ColorIC(Color uColor) =>
			new ColorIC(uColor.r, uColor.g, uColor.b, uColor.a);

		public static explicit operator Color(ColorIC icColor) =>
			new Color(icColor.r, icColor.g, icColor.b, icColor.a);
	}
}