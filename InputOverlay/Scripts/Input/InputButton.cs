using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;
using TMPro;

namespace Instrumental.Overlay
{
    public class InputButton : MonoBehaviour
    {
        [SerializeField] TMP_Text inputLabel;
        ButtonRuntime button;
        InputDataSources inputDataSources;

		// Start is called before the first frame update
		void Awake()
        {
            button = GetComponent<ButtonRuntime>();
            inputDataSources = GetComponent<InputDataSources>();
        }

        // Update is called once per frame
        void Update()
        {
            inputDataSources.SetData(0, button.IsPressed);
        }
    }
}