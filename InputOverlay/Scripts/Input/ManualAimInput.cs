using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;

namespace Instrumental.Overlay
{
    public class ManualAimInput : MonoBehaviour
    {
        [SerializeField] Button aimButton;
        [SerializeField] TMPro.TMP_Text modeLable;
        InputManager inputManager;
        bool isActive = false;
        LensToken manualAimToken;

        bool inputAimPrevious;

		private void Awake()
		{
            inputManager = GetComponent<InputManager>();
		}

		// Start is called before the first frame update
		void Start()
        {
            aimButton.Runtime.ButtonActivated += (ButtonRuntime sender) =>
            {
                isActive = !isActive;
            };

            Lens<bool> aimLens = new Lens<bool>(1, (bool useAim) => { return isActive; });
                manualAimToken = inputManager.UseAim.AddLens(aimLens);
        }

        // Update is called once per frame
        void Update()
        {
            bool currentAim = inputManager.UseAim.GetValue();
            if (inputAimPrevious != currentAim)
			{
                modeLable.text = (currentAim) ? "Aim Mode" : "Hand Mode";
			}
        }
    }
}