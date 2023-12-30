using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}