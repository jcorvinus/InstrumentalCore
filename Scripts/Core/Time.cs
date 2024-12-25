#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

namespace Instrumental.Core
{
	public static class Time
	{
		#region Total Time
		public static float time
		{
			get
			{
#if UNITY
				return UnityEngine.Time.time;
#elif STEREOKIT
				return StereoKit.Time.Totalf;
#endif
			}
		}

		public static float unscaledTime
		{
			get
			{
#if UNITY
				return UnityEngine.Time.unscaledTime;
#elif STEREOKIT
				return StereoKit.Time.TotalUnscaledf;
#endif
			}
		}
		#endregion

		#region Delta Time
		public static float deltaTime
		{
			get
			{
#if UNITY
				return UnityEngine.Time.deltaTime;
#elif STEREOKIT
				return StereoKit.Time.Stepf;
#endif
			}
		}

		public static float unscaledDeltaTime
		{
			get
			{
#if UNITY
				return UnityEngine.Time.fixedUnscaledTime;
#elif STEREOKIT
				return StereoKit.Time.StepUnscaledf;
#endif
			}
		}
		#endregion

		#region Fixed Time
		public static float fixedTime
		{
			get
			{
#if UNITY
				return UnityEngine.Time.fixedTime;
#elif STEREOKIT
				return StereoKit.Time.Totalf; // note: stereokit does not innately have physics
#endif
			}
		}
#endregion

		#region Fixed DeltaTime
		public static float fixedDeltaTime
		{
			get
			{
#if UNITY
				return UnityEngine.Time.fixedDeltaTime;
#elif STEREOKIT
				return StereoKit.Time.Scaledf; // note: stereokit does not innately have physics
#endif
			}
		}
		#endregion

		#region TimeScale
		public static float timeScale
		{
			get
			{
#if UNITY
				return UnityEngine.Time.timeScale;
#elif STEREOKIT
				return StereoKit.Time.Scale;
#endif
			}
			set
			{
#if UNITY
				UnityEngine.Time.timeScale = value;
#elif STEREOKIT
				StereoKit.Time.Scale = value;
#endif
			}
		}
#endregion
	}
}
