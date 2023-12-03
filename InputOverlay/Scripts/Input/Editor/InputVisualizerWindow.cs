using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace Instrumental.Overlay
{
    public class InputVisualizerWindow : EditorWindow
    {
        [MenuItem("Instrumental/Input Visualizer")]
        public static void ShowInputVisualizer()
		{
            InputVisualizerWindow window = GetWindow<InputVisualizerWindow>();
            window.titleContent = new GUIContent("Input Visualizer");

            window.Show();
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Input Data Sources");

			if(InputDataSources.LiveDataSources != null)
			{
				foreach(string key in InputDataSources.LiveDataSources.Keys)
				{
					InputDataSources.LiveSource source = InputDataSources.LiveDataSources[key];
					EditorGUILayout.LabelField("Type", source.Source.Type.ToString());
					EditorGUILayout.LabelField("Name", source.Source.Name);
					EditorGUILayout.LabelField("Guid", source.Source.Guid);

					switch (source.Source.Type)
					{
						case DataType.None:
							EditorGUILayout.LabelField("Null type, no value");
							break;
						case DataType.Bool:
							EditorGUILayout.LabelField("Value: " + source.GetBool());
							break;
						case DataType.Float:
							EditorGUILayout.LabelField("Value: " + source.GetFloat());
							break;
						case DataType.Vec2:
							EditorGUILayout.LabelField("Value: " + source.GetVec2().ToString());
							break;
						default:
							break;
					}
				}
			}
		}
	}
}