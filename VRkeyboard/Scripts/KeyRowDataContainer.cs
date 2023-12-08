using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace VRKeyboard
{
    /// <summary>
    /// Just storing things here so we can keep them even with serialization
    /// changes in other classes
    /// </summary>
    public class KeyRowDataContainer : MonoBehaviour
    {
        [SerializeField] KeyRow[] rows;
        [SerializeField] protected Keyboard.KeyInfo returnButton;
        [SerializeField] protected Image returnImage;

        [SerializeField] protected Keyboard.KeyInfo caseButton;
        [SerializeField] protected Image caseImage;

        [SerializeField] protected Keyboard.KeyInfo symbolButton;
        [SerializeField] protected GameObject symbolImage;

        [SerializeField] protected Keyboard.KeyInfo backspaceButton;
        [SerializeField] protected GameObject backspaceFace;
    }
}