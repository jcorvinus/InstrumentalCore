using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.UI
{
    public class Menu : MonoBehaviour
    {
        public delegate void MenuEventHandler(Menu sender);
        public event MenuEventHandler OnMenuOpenStarted;
        public event MenuEventHandler OnMenuOpenFinished;
        public event MenuEventHandler OnMenuCloseStarted;
        public event MenuEventHandler OnMenuCloseFinished;

        [Header("Unique name for searching display")]
        [SerializeField]
        private string menuName;

        public string MenuName { get { return menuName; } }

        public virtual void Open()
		{

		}

        public virtual void Close()
		{

		}
    }
}