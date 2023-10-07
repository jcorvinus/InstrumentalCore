using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;

namespace Instrumental.Overlay
{
    /// <summary>
    /// For applications that use a button to open a menu,
    /// use this to track the state, and set aim mode
    /// to true whenever we think the menu will be open.
    /// 
    /// Some applications close their application menu when locomotion
    /// activates, so we'll want to be able to disable our joystick as well.
    /// </summary>
    public class MenuButtonInputState : MonoBehaviour
    {
        [SerializeField] Button menuButton;
        InputManager inputManager;
        bool isActive = false;
        LensToken menuOpenToken;

		private void Awake()
		{
            inputManager = GetComponent<InputManager>();
		}

		// Start is called before the first frame update
		void Start()
        {
            menuButton.Runtime.ButtonActivated += (ButtonRuntime sender) => 
            {
                isActive = !isActive;
            };

            Lens<bool> menuOpenLens = new Lens<bool>(2, (bool useAim) => { return isActive || useAim; });
            menuOpenToken = inputManager.UseAim.AddLens(menuOpenLens); // we can add and remove the token
                    // as necessary to prevent it from being used, in case our OR operation doesn't
                    // behave the way we expect
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}