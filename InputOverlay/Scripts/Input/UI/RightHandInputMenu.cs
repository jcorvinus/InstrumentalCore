using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRKeyboard;

using Instrumental.Controls;

namespace Instrumental.Overlay
{
    public class RightHandInputMenu : MonoBehaviour
    {
        [SerializeField] ButtonRuntime settingsButton;

		private void Start()
		{
			settingsButton.ButtonActivated += (ButtonRuntime sender) =>
			{
				KeyboardManager.Instance.ShowKeyboard();
			};
		}
	}
}