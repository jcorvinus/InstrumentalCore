using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
using UnityEditor;

namespace Instrumental.Test
{
	[CustomEditor(typeof(PolyInterfaceSerializationTest))]
	public class PolyInterfaceSerializationTestEditor : Editor
	{
		SerializedProperty openFilePathProperty;
		SerializedProperty saveFilePathProperty;

		private void OnEnable()
		{
			openFilePathProperty = serializedObject.FindProperty("openFilePath");
			saveFilePathProperty = serializedObject.FindProperty("saveFilePath");
		}

		void Start()
		{
			openFilePathProperty = serializedObject.FindProperty("openFilePath");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			if(GUILayout.Button("Open File Path"))
			{
				openFilePathProperty.stringValue = EditorUtility.OpenFilePanel("Get Open Path for File", Application.dataPath, "*.json");
				serializedObject.ApplyModifiedProperties();
			}

			if(GUILayout.Button("Save File Path"))
			{
				saveFilePathProperty.stringValue = EditorUtility.SaveFilePanel("Set Save Path For File", Application.dataPath,
					"testFile.json", "*.json");
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
#endif