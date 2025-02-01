using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Interaction;
using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.UI
{
    /// <summary>
    /// Just a simple key-value pair
    /// </summary>
    [System.Serializable]
    public struct MenuEntry
	{
        public string Name;
        public Menu Menu;
	}

    public class MenuController : MonoBehaviour
    {
        [SerializeField] MenuEntry[] menuCollection;

		private void Awake()
		{
            GetAllSubMenus();
		}

		public void GetAllSubMenus()
		{
            Menu[] childMenus = GetComponentsInChildren<Menu>(true);

            List<Menu> validUntrackedMenus = new List<Menu>();

            Debug.Log(string.Format("Found {0} child menus inside of MenuController {1}", childMenus.Length,
                this.name));

            for(int i=0; i < childMenus.Length; i++)
			{
                Menu childMenu = childMenus[i];
                bool menuHasName = childMenu.MenuName.Length > 0;
                bool childMenuAlreadyTracked = ContainsMenu(childMenu.MenuName);
                if (!menuHasName)
				{
                    Debug.LogWarning(string.Format("Menu on gameObject {0} doesn't have a specified MenuName, and can't be used by MenuCollection. Excluding", childMenu.name));
				}

                if(childMenuAlreadyTracked)
				{
                    Debug.LogWarning(string.Format("Menu {0} was already in menuCollection, ignoring", childMenu.MenuName));
				}
                else
				{
                    validUntrackedMenus.Add(childMenu);
				}
			}

            MenuEntry[] newChildMenus = new MenuEntry[menuCollection.Length + validUntrackedMenus.Count];

            for(int i=0; i < newChildMenus.Length; i++)
			{
                if(i < menuCollection.Length)
				{
                    newChildMenus[i] = menuCollection[i];
				}
				else
				{
                    int adjustedIndex = i - menuCollection.Length;
                    Menu childMenu = childMenus[adjustedIndex];
					newChildMenus[i] = new MenuEntry()
                    {
                        Menu = childMenu,
                        Name = childMenu.MenuName
                    };
				}
			}

            menuCollection = newChildMenus;
        }

        public bool ContainsMenu(string name)
		{
            for(int i=0; i < menuCollection.Length; i++)
			{
                if(menuCollection[i].Name == name)
				{
                    return true;
				}
			}

            return false;
		}

        public Menu FindMenuForName(string menuName)
		{
            for(int i=0; i < menuCollection.Length; i++)
			{
                if (menuCollection[i].Name == menuName)
				{
                    return menuCollection[i].Menu;
				}
			}

            return null;
		}

        public int GetNumberOfMenus()
		{
            return menuCollection.Length;
		}

        public void ShowMenu(string menuName, bool doPlacement, bool closeExisting)
		{
            // find our menu
            Menu menuToOpen = FindMenuForName(menuName);

            if(doPlacement)
			{
                PoseIC placementPose = GetPlacementPose();
                menuToOpen.transform.SetPositionAndRotation(
					(Vector3)placementPose.position, 
					(Quaternion)placementPose.rotation);
			}

            // find any menus that should be closed.
            if(closeExisting)
			{

			}
		}

        public static PoseIC GetPlacementPose()
		{
            InstrumentalBody body = InstrumentalBody.Instance;

            // our position should be just over one forearm's length from the torso,
            // and down far enough to have a 30 degree incline when rotated to face the user's neck

            // I think we can figure this out by rotating the forward direction down 30 degrees,
            // then pushing out the required distance.

            // in the future, we can worry about hip placement and if the user is lying down or in any other pose,
            // but for now let's do this simply.
            Quatn userRotation = Quatn.LookRotation(body.ForwardDirection, Vect3.up);
            Vect3 userRight = userRotation * Vect3.right;

			Vect3 userForwardDirection = Quatn.AngleAxis(30, userRight) * body.ForwardDirection;
			Vect3 placementPosition = (Vect3)body.Head.position + (userForwardDirection * 0.46f);
			Vect3 rotationDirection = ((Vect3)body.Head.position - placementPosition);
			Quatn rotation = Quatn.LookRotation(rotationDirection,
                -(Quatn.AngleAxis(120, userRight) * body.ForwardDirection));

            return new PoseIC(placementPosition, rotation);
        }
    }
}