using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental.Overlay
{
    [CustomEditor(typeof(InputDataSources), editorForChildClasses:true)]
    public class InputDataSourcesEditor : Editor
    {
		bool dataSourcesFoldout = true;

		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();

			serializedObject.Update();

			SerializedProperty dataSourceArrayProperty = serializedObject.FindProperty("dataSources");

			dataSourcesFoldout = EditorGUILayout.Foldout(dataSourcesFoldout, "Data Sources");

			if (dataSourcesFoldout)
			{
				for (int i = 0; i < dataSourceArrayProperty.arraySize; i++)
				{
					// draw our data source fields
					SerializedProperty dataSource = dataSourceArrayProperty.GetArrayElementAtIndex(i);
					//EditorGUILayout.PropertyField(dataSource);

					SerializedProperty typeProperty = dataSource.FindPropertyRelative("Type");
					SerializedProperty nameProperty = dataSource.FindPropertyRelative("Name");
					SerializedProperty guidProperty = dataSource.FindPropertyRelative("Guid");

					EditorGUILayout.PropertyField(typeProperty);
					EditorGUILayout.PropertyField(nameProperty);
					EditorGUILayout.LabelField("guid: " + guidProperty.stringValue);


					// add a delete button for removing this array element
					if(GUILayout.Button("Delete data source"))
					{
						dataSourceArrayProperty.DeleteArrayElementAtIndex(i);
					}
					EditorGUILayout.Space();
				}

				// add a button for adding a new array element
				if (GUILayout.Button("Add data source"))
				{
					int insertionIndex = dataSourceArrayProperty.arraySize;
					dataSourceArrayProperty.InsertArrayElementAtIndex(insertionIndex);

					SerializedProperty dataSource = dataSourceArrayProperty.GetArrayElementAtIndex(insertionIndex);
					SerializedProperty guidProperty = dataSource.FindPropertyRelative("Guid");
					guidProperty.stringValue = System.Guid.NewGuid().ToString();
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}