using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

using Instrumental.Controls;

namespace Instrumental.Schema
{
	[System.Serializable]
    public struct ControlVariable
    {
        public string Name;
        public System.Type Type;
        public string Value;
    }

    /// <summary>
    /// Because Unity serialization doesn't support inheritance,
    /// we're going to dump all the variables into a single struct and
    /// let the control classes handle loading and saving
    /// </summary>
#if UNITY
    [CreateAssetMenu(fileName = "ControlSchema", menuName = "Instrumental/ControlSchema")]
#endif
	public abstract class ControlSchema : ScriptableObject
    {
#if UNITY
		[Header("Common Variables")]
#endif
		public ControlType Type;
        public string Name;
        public sV3 Position;
        public sQuat Rotation;

#if UNITY
		[Header("Type-Specific Variables")]
#endif
		public List<ControlVariable> ControlVariables;
    }
}