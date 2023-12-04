using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental.Overlay
{
    [CustomEditor(typeof(InputConfiguration))]
    public class InputConfigurationEditor : Editor
    {
		SerializedProperty hookupsArrayProperty;

		Vector2 scrollPosition;

		private void OnEnable()
		{
			hookupsArrayProperty = serializedObject.FindProperty("Hookups");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			EditorGUILayout.LabelField("Hookups");
			for(int i=0; i < hookupsArrayProperty.arraySize; i++)
			{
				SerializedProperty hookupProperty = hookupsArrayProperty.GetArrayElementAtIndex(i);
				SerializedProperty dataProperty = hookupProperty.FindPropertyRelative("Data");
				SerializedProperty typeProperty = dataProperty.FindPropertyRelative("Type");
				SerializedProperty guidProperty = hookupProperty.FindPropertyRelative("DataSourceGuid");
				SerializedProperty nameProperty = dataProperty.FindPropertyRelative("Name");
				SerializedProperty isLeftProperty = dataProperty.FindPropertyRelative("IsLeftController");
				SerializedProperty vec2IsActiveDataSourceGuid = hookupProperty.FindPropertyRelative("Vec2IsActiveDataSourceGuid");

				DataType type = (DataType)typeProperty.enumValueIndex;

				EditorGUILayout.PropertyField(dataProperty);

				// need to get our type and see if we should expose a second guid button for 
				// the vector2 isactive bit.
				System.Guid guid;
				bool isvalidGuid = System.Guid.TryParse(guidProperty.stringValue, out guid);
				string guidButtonLabel = (isvalidGuid) ? string.Format("data source: {0}", guidProperty.stringValue) : "Pick data source";
				if (GUILayout.Button(guidButtonLabel))
				{
					InputDataSourcePicker.StartPicking(serializedObject, guidProperty, isLeftProperty.boolValue,
						nameProperty.stringValue);
				}

				if(type == DataType.Vec2)
				{
					// isactive guid picking
					System.Guid isActiveGuid;
					bool isActiveValidGuid = System.Guid.TryParse(vec2IsActiveDataSourceGuid.stringValue, out isActiveGuid);
					string isActiveGuidLabel = (isActiveValidGuid) ? string.Format("data source: {0}", vec2IsActiveDataSourceGuid.stringValue) :
						"Pick is active data source";

					if(GUILayout.Button(isActiveGuidLabel))
					{
						InputDataSourcePicker.StartPicking(serializedObject, vec2IsActiveDataSourceGuid,
							isLeftProperty.boolValue,
							nameProperty.stringValue + " is active data source");
					}
				}

				if (GUILayout.Button("Delete hookup"))
				{
					hookupsArrayProperty.DeleteArrayElementAtIndex(i);
				}
			}

			if(GUILayout.Button("Add new Hookup"))
			{
				hookupsArrayProperty.InsertArrayElementAtIndex(hookupsArrayProperty.arraySize);
			}

			EditorGUILayout.EndScrollView();
		}
	}
}