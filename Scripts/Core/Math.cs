﻿#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

namespace Instrumental.Core.Math
{
    /// <summary>
    /// Anything involving math that we want to be available in any engine gets put here.
    /// Contains neat features such as linear interpolation, cartesian->spherical projection,
    /// Vector averaging, and more.
    /// </summary>
	public static class Math
	{
        /// <summary>
        /// Simple integer 2d array.
        /// </summary>
        public struct Int2D
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Simple integer 3d array.
        /// </summary>
        public struct Int3D
        {
            public int x;
            public int y;
            public int z;
        }

		/// <summary>
		/// Flips a 0-1 value. Will always return positive.
		/// </summary>
		/// <param name="tValue">input value</param>
		/// <returns></returns>
		public static float UnitReciprocal(float tValue)
		{
			return Mathf.Abs(((tValue * 100) - 100) * 0.01f);
		}

#region Linear Interpolation Functions
		/// <summary>
		/// Lerp with a sine tvalue
		/// </summary>
		public static float Sinerp(float from, float to, float t)
		{
			return Mathf.Lerp(from, to, Mathf.Sin(t * Mathf.PI * 0.5f));
		}

		/// <summary>
		/// Lerp with a cosine tvalue
		/// </summary>
		public static float Coserp(float from, float to, float t)
		{
			return Mathf.Lerp(from, to, (1f - Mathf.Cos(t * Mathf.PI * 0.5f)));
		}

		/// <summary>
		/// lerp with an exponential tvalue
		/// </summary>
		public static float Exerp(float from, float to, float t)
		{
			return Mathf.Lerp(from, to, t * t);
		}
		
		public static float InverseLerp(float low, float high, float value)
		{
			return (value - low) / (high - low);
		}

        public static double InverseLerp(long low, long high, float value)
        {
            return (value - low) / (high - low);
        }

        public static double InverseLerp(double low, double high, double value)
		{
			return (value - low) / (high - low);
		}
		
		public static float Lerp(float A, float B, float t)
		{
			return A + (B - A) * t;
		}

		public static double Lerp(double A, double B, double t)
		{
			return A + (B - A) * t;
		}
#endregion

		// The vector3 interpolators below are basically the same as using some of NGUI's tweeners with preset animation
		// curve values. Still useful though.
#region Vector3 Interpolation Functions
		public static Vect3 Sinerp(Vect3 to, Vect3 from, float tValue)
		{
			return new Vect3(Sinerp(to.x, from.x, tValue),
				Sinerp(to.y, from.y, tValue),
				Sinerp(to.z, from.z, tValue));
		}

		public static Vect3 Coserp(Vect3 to, Vect3 from, float tValue)
		{
			return new Vect3(Coserp(to.x, from.x, tValue),
				Coserp(to.y, from.y, tValue),
				Coserp(to.z, from.z, tValue));
		}

		public static Vect3 Exerp(Vect3 to, Vect3 from, float tValue)
		{
			return new Vect3(Exerp(to.x, from.x, tValue),
				Exerp(to.y, from.y, tValue),
				Exerp(to.z, from.z, tValue));
		}

        public static float UnclampedLerp(float a, float b, float t)
        {
            return t * b + (1 - t) * a;
        }

        public static Vect3 UnclampedLerp(Vect3 to, Vect3 from, float tValue)
        {
            return new Vect3(
               UnclampedLerp(to.x, from.x, tValue),
               UnclampedLerp(to.y, from.y, tValue),
               UnclampedLerp(to.z, from.z, tValue));
        }
#endregion

#region Spherical Coordinates
		public static void SphericalToCartesian(float radius, float polar, float elevation, out Vect3 outCart)
		{
			float a = radius * Mathf.Cos(elevation);
			outCart = new Vect3(a * Mathf.Cos(polar), radius * Mathf.Sin(elevation), a * Mathf.Sin(polar));
		}

		public static void CartesianToSpherical(Vect3 cartCoords, out float outRadius, out float outPolar, out float outElevation)
		{
			if (cartCoords.x == 0)
				cartCoords.x = Mathf.Epsilon;
			outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x)
							+ (cartCoords.y * cartCoords.y)
							+ (cartCoords.z * cartCoords.z));
			outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
			if (cartCoords.x < 0)
				outPolar += Mathf.PI;
			outElevation = Mathf.Asin(cartCoords.y / outRadius);
		}
#endregion

#region Spatial Relationships
		/// <summary>
		/// Gets a normalized direction from one point to another.
		/// </summary>
		/// <param name="to">Destination</param>
		/// <param name="from">source</param>
		/// <returns>A normalized direction from the source to the destination</returns>
		public static Vect3 Direction(Vect3 to, Vect3 from)
		{
			return (to - from).normalized; // dono why but this is backwards?
		}

		public static Vect3 CenterOfPoints(Vect3[] points)
		{
			Vect3 average = new Vect3();

			foreach (Vect3 target in points) average += target;

			average /= points.Length;

			return average;
		}

		public static Vect3 CenterOfGameObjects(GameObject[] objects)
		{
			Vect3 average = new Vect3();

			foreach (GameObject target in objects) average += (Vect3)target.transform.position;

			average /= objects.Length;

			return average;
		}

		/// <summary>
		/// Find the distance from the center to the furthest point in the array.
		/// </summary>
		/// <param name="points">Input vector 3 array.</param>
		/// <returns>The distance from the center to the furthest point.</returns>
		public static float DistanceOfPoints(Vect3[] points)
		{
			Vect3 center = CenterOfPoints(points);

			float dist = 0;

			foreach (Vect3 point in points)
			{
				float newDist = Vect3.Distance(point, center);
				if (newDist > dist) dist = newDist;
			}

			return dist;
		}

        public static Vect3 GetClosestPointOnLineSegment(Vect3 A, Vect3 B, Vect3 P)
        {
            Vect3 AP = P - A;       //Vector from A to P   
            Vect3 AB = B - A;       //Vector from A to B  

            float magnitudeAB = AB.sqrMagnitude;    //Magnitude of AB vector (it's length squared)
                                // NOTE: sqrMag above might just be mag.
            float ABAPproduct = Vect2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;
            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }

		//public static Vect2 GetClosestPointOnLineSegment(Vect2 A, Vect2 B, Vect2 P)
		//{
		//	Vect2 AP = P - A;       //Vector from A to P   
		//	Vect2 AB = B - A;       //Vector from A to B  

		//	float magnitudeAB = AB.LengthSquared();     //Magnitude of AB vector (it's length squared)     
		//	float ABAPproduct = Vect2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
		//	float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

		//	if (distance < 0)     //Check if P projection is over vectorAB     
		//	{
		//		return A;

		//	}
		//	else if (distance > 1)
		//	{
		//		return B;
		//	}
		//	else
		//	{
		//		return A + AB * distance;
		//	}
		//}
		#endregion

		public static Vect3 CalculateSingleShotVelocity(Vect3 position, Vect3 previousPosition,
            float deltaTime)
        {
            float velocityFactor = 1.0f / deltaTime;
            return velocityFactor * (position - previousPosition);
        }

        public static Vect3 CalculateSingleShotAngularVelocity(Quatn rotation, Quatn previousRotation,
            float deltaTime)
        {
			Quatn deltaRotation = rotation * Quatn.Inverse(previousRotation);

            Vect3 _deltaAxis;
            Vect3 deltaAxis;
            float deltaAngle;

            deltaRotation.ToAngleAxis(out deltaAngle, out _deltaAxis);
			deltaAxis = (Vect3)_deltaAxis;

            if (float.IsInfinity(deltaAxis.x) || float.IsInfinity(deltaAxis.y) ||
                float.IsNaN(deltaAxis.x) || float.IsNaN(deltaAxis.y) || float.IsNaN(deltaAxis.z))
            {
                deltaAxis = Vect3.zero;
                deltaAngle = 0;
            }

            if (deltaAngle > 180)
            {
                deltaAngle -= 360.0f;
            }

            Vect3 angularVelocity = deltaAxis * deltaAngle * Mathf.Deg2Rad / deltaTime;

            return angularVelocity;
        }

        /// <summary>
        /// Performs a division operation (a / b),
        /// and stuffs the remainder into an output parameter
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="remainder"></param>
        /// <returns>quotient</returns>
        public static int DivRem(int dividend, int divisor, out int remainder)
        {
            remainder = dividend % divisor;
            return dividend / divisor;
        }

        /// <summary>
        /// If you have a 1d representation of a 2d array, you can supply
        /// this function with the 1d index, and the 2d array width and this
        /// function will return a struct giving you x,y coords for the 2d array.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arrayWidth"></param>
        /// <returns></returns>
        public static Int2D Wrap1DArrayTo2D(int index, int arrayWidth)
        {
            return new Int2D() { X = (int)Mathf.Floor(index / arrayWidth), Y = index % arrayWidth };
        }

        public static Int3D Wrap1DArrayTo3D(int index, int arrayWidth, int arrayHeight, int arrayDepth)
        {
            int x=0, y=0, z=0;

            if (index < arrayWidth * arrayHeight * arrayDepth)
            {
                int zQuotient = DivRem(index, arrayDepth, out z);
                int yQuotient = DivRem(zQuotient, arrayHeight, out y);
                x = yQuotient % arrayWidth;
            }

            return new Int3D() { x = x,y = y, z = z };
        }

        public static void GetTopRowEnds(int baseIndex, int gridWidth, out int rowStart, out int rowEnd)
        {
            rowStart = baseIndex;
            rowEnd = baseIndex + gridWidth;
        }

        public static void GetBottomRowEnds(int baseIndex, int arrayLength, int gridWidth, out int rowStart, out int rowEnd)
        {
            rowStart = ((arrayLength) - gridWidth) + baseIndex;
            rowEnd = rowStart + gridWidth + baseIndex;
        }

        public static int GetLeftEdgeForHeightIndex(int baseIndex, int heightIndex, int gridWidth)
        {
            return baseIndex + heightIndex * gridWidth;
        }

        public static int GetRightEdgeForHeightIndex(int baseIndex, int heightIndex, int gridWidth)
        {
            return baseIndex + (gridWidth - 1) + (gridWidth * heightIndex);
        }

		public static bool IndexInRange(int index, int length)
		{
			return (index > -1 && index < length);
		}

        public static Vect2 GetNormal(Vect2 a, Vect2 b)
        {
            Vect3 v = b - a; // why is this a vect3? It was a vector3 before?

            return new Vect2(-v.y, v.x) / Mathf.Sqrt((v.x * v.x) + (v.y * v.y));
        }

        public static float Repeat(float t, float length)
        {
            return t - Mathf.Floor(t / length) * length;
        }
    }
}