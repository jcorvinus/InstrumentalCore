using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Space
{
    public class GlobalSpace : MonoBehaviour
    {
        UICommonElements uiCommon;

        public UICommonElements UICommon { get { return uiCommon; } }

        private static GlobalSpace instance;
        public static GlobalSpace Instance { get 
            {
                if(!instance)
				{
                    instance = FindObjectOfType<GlobalSpace>();
                    instance.Load();
				}

                return instance; 
            } 
        }

        void Load()
		{
            uiCommon = Resources.Load<UICommonElements>("UICommon");

            if (uiCommon == null)
            {
                Debug.LogError("could not load UI common");
            }

            instance = this;
        }

        private void Awake()
        {
            Load();
        }
    }
}