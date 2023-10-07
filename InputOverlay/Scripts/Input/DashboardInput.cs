using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace Instrumental.Overlay
{
    public class DashboardInput : MonoBehaviour
    {
        InputManager inputManager;
        CVROverlay overlay;
        LensToken useAimToken;

		private void Awake()
		{
			overlay = SteamVR.instance.overlay;
            inputManager = GetComponent<InputManager>();
        }

		// Start is called before the first frame update
		void Start()
        {
            Lens<bool> useAimLens = new Lens<bool>(0, (bool a) => { return overlay.IsDashboardVisible(); });
            useAimToken = inputManager.UseAim.AddLens(useAimLens);
        }
    }
}