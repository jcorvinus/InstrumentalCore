using UnityEngine;
using System.Collections;

using TMPro;
using Instrumental.Controls;

namespace VRKeyboard
{
    /// <summary>
    /// Connects to a button and relays those events to
    /// a virtual keyboard.
    /// </summary>
    public class KeyButton : MonoBehaviour
    {
        public delegate void KeyButtonEventHandler(KeyButton sender);
        public event KeyButtonEventHandler Activated;
        public event KeyButtonEventHandler HoverGained;
        public event KeyButtonEventHandler HoverLost;

        private Keyboard keyboard;

		#region Phys Keyboard Stuff
		private ButtonRuntime button;

        private UnityEngine.UI.Text characterText;
        public string Text
        {
            get { return characterText.text; }
        }
        public UnityEngine.UI.Text CharacterText
        {
            get { return characterText; }
            set { characterText = value; }
        }
        #endregion

        #region Fast Keyboard Stuff
        UnityEngine.UI.Button unityButton;
        UnityEngine.EventSystems.EventTrigger unityEventTrigger;
        UnityEngine.EventSystems.EventTrigger.Entry hoverStartEntry;
        UnityEngine.EventSystems.EventTrigger.Entry hoverEndEntry;

        TMP_Text tmpText;
		#endregion

		private bool upperCase = false;
        public bool UpperCase { get { return upperCase; } set { upperCase = value; SetText(); } }

        private KeyCode key;
        public KeyCode Key
        {
            get { return key; }
            set { key = value; SetText(); }
        }

        public bool HasRuntimeButton { get { return button; } }

        void Awake()
        {
            keyboard = GetComponentInParent<Keyboard>();

            button = GetComponent<ButtonRuntime>();

            unityButton = GetComponent<UnityEngine.UI.Button>();
            tmpText = GetComponentInChildren<TMP_Text>();

            if(unityButton)
			{
                unityEventTrigger = unityButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                hoverStartEntry = new UnityEngine.EventSystems.EventTrigger.Entry()
                {
                    eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter,
                };
                hoverStartEntry.callback.AddListener((data) => { OnPointerEnter((UnityEngine.EventSystems.PointerEventData)data); });

                hoverEndEntry = new UnityEngine.EventSystems.EventTrigger.Entry()
                {
                    eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
                };
                hoverEndEntry.callback.AddListener((data) => { OnPointerExit((UnityEngine.EventSystems.PointerEventData)data); });

            }

			if (keyboard == null)
            {
                enabled = false;
                return;
            }
        }

        void OnEnable()
        {
            SetText();

            if (button)
            {
                button.ButtonActivated += OnButtonActivated;
                button.ButtonHovered += OnButtonHoverGained;
                button.ButtonHoverEnded += OnButtonHoverLost;
            }

            if(unityButton)
			{
                unityButton.onClick.AddListener(Activate);

                unityEventTrigger.triggers.Add(hoverStartEntry);
                unityEventTrigger.triggers.Add(hoverEndEntry);
			}
        }

        private void SetText()
        {
            if (characterText != null)
            {
                if (!UpperCase)
                {
                    characterText.text = Keyboard.ConvertCodeToChar(key.ToString()).ToLower();
                }
                else
                {
                    characterText.text = Keyboard.ConvertCodeToChar(key.ToString());
                }
            }
        }

        void OnDisable()
        {
            if (button)
            {
                button.ButtonActivated -= OnButtonActivated;
                button.ButtonHovered -= OnButtonHoverGained;
                button.ButtonHoverEnded -= OnButtonHoverLost;
            }

            if(unityButton)
			{
                unityButton.onClick.RemoveListener(Activate);
                unityEventTrigger.triggers.Remove(hoverStartEntry);
                unityEventTrigger.triggers.Remove(hoverEndEntry);
			}
        }

        private void Update()
        {
            button.VolumeModifier = keyboard.VolumeAdjust;
        }

        void Activate()
		{
            if (Activated != null) Activated(this);
        }

        void StartHover()
		{
            if (HoverGained != null) HoverGained(this);
        }

        void EndHover()
		{
            if (HoverLost != null) HoverLost(this);
        }

        void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
		{
            StartHover();
		}

        void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
		{
            EndHover();
		}

        #region Phys Event Methods
        private void OnButtonActivated(ButtonRuntime sender)
        {
            Activate();
        }

        private void OnButtonHoverGained(ButtonRuntime sender)
        {
            StartHover();   
        }

        private void OnButtonHoverLost(ButtonRuntime sender)
        {
            EndHover();   
        }
        #endregion
    }
}