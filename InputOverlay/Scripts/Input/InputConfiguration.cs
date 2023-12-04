using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Overlay
{
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Instrumental/InputConfiguration")]
    public class InputConfiguration : ScriptableObject
    {
        public InputHookup[] Hookups;
    }
}