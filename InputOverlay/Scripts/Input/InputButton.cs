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

		// Start is called before the first frame update
		void Awake()
        {
            button = GetComponent<ButtonRuntime>();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}