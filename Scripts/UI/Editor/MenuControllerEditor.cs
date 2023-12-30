using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental.UI
{
    [CustomEditor(typeof(MenuController))]
    public class MenuControllerEditor : Editor
    {
        SerializedProperty menuCollectionProperty;

		private void OnEnable()
		{
			Init();
		}

		private void Start()
		{
			Init();
		}

		void Init()
		{
			menuCollectionProperty = serializedObject.FindProperty("menuCollection");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			// check for duplicates in the name list
			for(int i=0; i < menuCollectionProperty.arraySize; i++)
			{
				SerializedProperty menuEntryProperty = menuCollectionProperty.GetArrayElementAtIndex(i);
				SerializedProperty menuNameProperty = menuEntryProperty.FindPropertyRelative("Name");
				SerializedProperty menuMenuProperty = menuEntryProperty.FindPropertyRelative("Menu");

				for(int checkEntry =0; checkEntry < menuCollectionProperty.arraySize; checkEntry++)
				{
					if (checkEntry == i) continue;
					SerializedProperty checkEntryProperty = menuCollectionProperty.GetArrayElementAtIndex(checkEntry);
					SerializedProperty checkEntryNameProperty = checkEntryProperty.FindPropertyRelative("Name");
					SerializedProperty checkEntryMenuProperty = checkEntryProperty.FindPropertyRelative("Menu");

					bool stringMatch = (menuNameProperty.stringValue == checkEntryNameProperty.stringValue);
					bool objectMatch = (menuMenuProperty.objectReferenceInstanceIDValue == 
						checkEntryMenuProperty.objectReferenceInstanceIDValue);

					if(stringMatch && !objectMatch)
					{
						EditorGUILayout.HelpBox(string.Format("Unique menus with duplicate names {0} and {1}, this is not allowed.",
							menuNameProperty.stringValue, checkEntryNameProperty.stringValue), MessageType.Error);
					}
					else if (objectMatch && !stringMatch)
					{
						EditorGUILayout.HelpBox(string.Format("Menu {0} is duplicated in the list but has multiple names... this is not allowed",
							checkEntryMenuProperty.objectReferenceValue.name), MessageType.Error);
					}
					else if (objectMatch && stringMatch)
					{
						EditorGUILayout.HelpBox(string.Format("There is a completely duplicated entry for menu {0} {1}. This is not allowed.", 
							checkEntryNameProperty.stringValue, checkEntryMenuProperty.objectReferenceValue.name), MessageType.Error);
					}
				}
			}
		}
	}
}