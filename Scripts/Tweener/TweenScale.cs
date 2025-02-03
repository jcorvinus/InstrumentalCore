using UnityEngine;
using System.Collections;

using Instrumental.Core.Math;

namespace Instrumental.Tweening
{
	[AddComponentMenu("Instrumental/Tweening/Tween Scale")]
	public class TweenScale : Tweener
	{
		public Vector3 StartScale;
		public Vector3 GoalScale;
        public bool Unclamped;
        public bool RunWhileNotPlay = false;

		// Use this for initialization
		void Start()
		{
			base.Start();
		}

		// Update is called once per frame
		void Update()
		{
			base.Update();
            if (TweenerState == TweenState.Play || RunWhileNotPlay)
            {
                if (Unclamped) transform.localScale = Vector3.LerpUnclamped(StartScale, GoalScale, TValue);
                else transform.localScale = Vector3.Lerp(StartScale, GoalScale, TValue);
            }
		}
	}
}