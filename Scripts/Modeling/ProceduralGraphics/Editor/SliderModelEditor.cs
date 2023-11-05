using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental.Modeling.ProceduralGraphics
{
    [CustomEditor(typeof(SliderModel))]
    public class SliderModelEditor : Editor
    {
        SliderModel m_instance;
		private void OnEnable()
		{
			m_instance = target as SliderModel;
		}

		private void OnSceneGUI()
		{
			if(m_instance && m_instance.DrawLoops)
			{
				for(int i=0; i < m_instance.RailVertices.Length; i++)
				{
					Handles.Label(m_instance.RailVertices[i], i.ToString());
				}
			}
		}
	}
}