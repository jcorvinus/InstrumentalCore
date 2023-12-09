using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRKeyboard
{
    public class FastKeyboard : Keyboard
    {

		public override void Show()
		{
			gameObject.SetActive(true);
			visible = true;
		}

		public override void Hide()
		{
			gameObject.SetActive(false);
			visible = false;
		}
	}
}