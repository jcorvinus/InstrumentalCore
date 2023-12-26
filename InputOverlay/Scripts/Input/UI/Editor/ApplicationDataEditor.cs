using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Instrumental.Overlay
{
    [CustomEditor(typeof(ApplicationData))]
    public class ApplicationDataEditor : Editor
    {
        ApplicationData m_instance;

		private void OnEnable()
		{
			m_instance = target as ApplicationData;
		}

		void Start()
		{
			m_instance = target as ApplicationData;
		}

		void DrawApplicationMetaData(ApplicationMetaData metaData)
		{
			EditorGUILayout.LabelField(string.Format("App Name: {0}", metaData.Name));
			EditorGUILayout.LabelField(string.Format("Key: {0}", metaData.Key));
			EditorGUILayout.LabelField(string.Format("IsHidden: {0}", metaData.IsHidden));
			EditorGUILayout.LabelField(string.Format("IsInternal: {0}", metaData.IsInternal));
			EditorGUILayout.LabelField(string.Format("Image URL: {0}", metaData.ImageURL));
			EditorGUILayout.LabelField(string.Format("Action Manifest: {0}", metaData.ActionManifestPath));
			EditorGUILayout.Space();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (ApplicationData.ApplicationDictionary != null)
			{
				// draw all the dictionary stuff
				foreach (string key in ApplicationData.SortedKeys)
				{
					DrawApplicationMetaData(ApplicationData.ApplicationDictionary[key]);
				}

				// draw buttons for any test commands
				if (GUILayout.Button("Find Eligible Applications"))
				{
					m_instance.FindEligibleApplications();
				}

				if(GUILayout.Button("Download Images For Applications"))
				{
					m_instance.DownloadImagesForApplications();
				}
			}
		}
	}
}