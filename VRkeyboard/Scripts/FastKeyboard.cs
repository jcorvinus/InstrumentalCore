using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRKeyboard
{
    public class FastKeyboard : Keyboard
    {
		bool isShowing = false;
		bool isAnimating = false;
		const float duration = 0.25f;
		float animTimer = 0;


		protected override KeyInfo ButtonInit(KeyInfo button, bool textFace)
		{
			//return base.ButtonInit(button, textFace);

			AddKeyEvents(button);
			return button;
		}

		public override void Show()
		{
			isShowing = true;
			animTimer = 0;
			gameObject.SetActive(true);
			visible = true;
			isAnimating = true;
		}

		public override void Hide()
		{
			animTimer = 0;
			isShowing = false;
			gameObject.SetActive(false);
			visible = false;
			isAnimating = true;
		}

		private void Update()
		{
			if(isAnimating)
			{
				animTimer += (isShowing) ?  Time.deltaTime : -Time.deltaTime;
				animTimer = Mathf.Clamp(animTimer, 0, duration);
				float tValue = Mathf.InverseLerp(0, duration, animTimer);

				transform.localScale = Vector3.one * tValue;

				if ((isShowing && animTimer == duration) ||
					(!isShowing && animTimer == 0)) isAnimating = false;
			}
		}
	}
}