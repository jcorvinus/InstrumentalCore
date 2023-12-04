using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental.Overlay
{
	public class InputDataSourcePicker : EditorWindow
	{
		SerializedObject targetObject;
		SerializedProperty targetProperty;
		string inputName;
		bool isLeft;
		InputDataSources[] allSources;
		Vector2 scrollPosition;

		public static void StartPicking(SerializedObject configurationEditorObject, 
			SerializedProperty guidProperty, bool isLeft, string inputName)
		{
			InputDataSourcePicker window = GetWindow<InputDataSourcePicker>();
			window.titleContent = new GUIContent("Pick a Guid for this input data source");

			window.targetObject = configurationEditorObject;
			window.targetProperty = guidProperty;
			window.isLeft = isLeft;
			window.inputName = inputName;

			window.allSources = FindObjectsOfType<InputDataSources>(true);

			window.Show();
		}

		private void OnGUI()
		{
			targetObject.Update();

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			EditorGUILayout.LabelField("Choose a source for input " + inputName + ((isLeft) ? " left" : " right"));

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

					if(GUILayout.Button("Pick this input"))
					{
						targetProperty.stringValue = guidProperty.stringValue;
						targetObject.ApplyModifiedProperties();
						this.Close();
					}
					EditorGUILayout.Space();
				}

				EditorGUILayout.Space();
			}

			EditorGUILayout.EndScrollView();
		}
	}
}