using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental
{
    public class FindObjectsInLayer : EditorWindow
    {
        [SerializeField] LayerMask layerToFind;

        [MenuItem("Instrumental/Find Objects In Layer")]
        public static void ShowWindow()
		{
            FindObjectsInLayer window = EditorWindow.CreateWindow<FindObjectsInLayer>();
            window.Show();
		}

		public void OnGUI()
		{
			layerToFind = EditorGUILayout.LayerField("Layer", layerToFind.value);

			if(GUILayout.Button("Find all objects in layer"))
			{
				GameObject[] objects = FindObjectsOfType<GameObject>();
				int matches = 0;

				for(int i=0; i < objects.Length; i++)
				{
					if(objects[i].layer == (int)layerToFind)
					{
						matches++;
						Debug.Log(string.Format("Found object {0} in layer {1}", objects[i].name, LayerMask.LayerToName(layerToFind)));
					}
				}

				if (matches == 0) Debug.Log("No objects found in layer " + LayerMask.LayerToName(layerToFind));
			}
		}
	}
}