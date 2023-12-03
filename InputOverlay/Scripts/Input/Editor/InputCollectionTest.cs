using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental.Overlay
{
    public class InputCollectionTest : EditorWindow
    {
		InputDataSources[] allSources;

        [MenuItem("Instrumental/Input Overview")]
        public static void ShowInputOverview()
		{
            InputCollectionTest window = GetWindow<InputCollectionTest>();
            window.titleContent = new GUIContent("Input Overview");

			window.allSources = new InputDataSources[0];
		}

		void FindDataSources()
		{
			allSources = FindObjectsOfType<InputDataSources>(true);

			Debug.Log("");
		}

		private void OnGUI()
		{
			if(GUILayout.Button("Find sources"))
			{
				FindDataSources();
			}

			for(int i=0; i < allSources.Length; i++)
			{
				InputDataSources sources = allSources[i];

				SerializedObject sourcesObject = new SerializedObject(sources);
				SerializedProperty sourcesDescriptionArray = sourcesObject.FindProperty("dataSources");

				// we need to list the data sources, there's more than just one and the sources description is an array

				for (int sourceIndx = 0; sourceIndx < sourcesDescriptionArray.arraySize; sourceIndx++)
				{
					SerializedProperty sourceDescriptionProperty = sourcesDescriptionArray.GetArrayElementAtIndex(sourceIndx);

					// find our sub properties
					EditorGUILayout.LabelField(sources.name);
					SerializedProperty typeProperty = sourceDescriptionProperty.FindPropertyRelative("Type");
					SerializedProperty nameProperty = sourceDescriptionProperty.FindPropertyRelative("Name");
					SerializedProperty guidProperty = sourceDescriptionProperty.FindPropertyRelative("Guid");

					EditorGUILayout.PropertyField(typeProperty);
					EditorGUILayout.PropertyField(nameProperty);
					EditorGUILayout.PropertyField(guidProperty);
					EditorGUILayout.Space();
				}

				EditorGUILayout.Space();
			}
		}
	}
}