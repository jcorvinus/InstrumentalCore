using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AssetImporters;

using UnityEngine.XR.Management;
using Unity.XR.OpenVR;

using UnityEditor;
using UnityEditor.XR.Management.Metadata;

namespace Instrumental
{
    public class AppModeSwitcher : EditorWindow
    {
		bool hasSettingsObject = false;
		SerializedObject settingsObject;
		SerializedProperty initializationTypeProperty;
		SerializedProperty mirrorViewProperty; // MirrorView
		bool hasMirrorViewProperty=false;

		[MenuItem("Instrumental/App Mode Switcher")]
        public static void ShowSwitcher()
		{
            AppModeSwitcher window = GetWindow<AppModeSwitcher>();
            window.titleContent = new GUIContent("App mode switcher");


			window.GetXRLoaderSettings();
            window.Show();
		}

		public void SetModeOverlay()
		{

		}

		public void SetModeScene()
		{

		}

		void GetXRLoaderSettings()
		{
			hasSettingsObject = false;
			string packageName = "com.valvesoftware.unity.openvr";
			string settingsName = "Open VR Settings";
			var metadata = XRPackageMetadataStore.GetMetadataForPackage(packageName);
			//string[] assets = AssetDatabase.FindAssets($"t.{metadata.settingsType}"); // this breaks because sometimes we don't have the UNITY_XR_MANAGEMENT flag set
			// I think 'sometimes' here might mean edit time possibly? idk
			string[] assets = AssetDatabase.FindAssets(settingsName + " t:ScriptableObject");
			string foundPath = "";

			for (int i = 0; i < assets.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assets[i]); // todo: print all asset strings so we can see how the search works
																			 // and if we can narrow it down

				if (assetPath.Contains(settingsName))
				{
					foundPath = assetPath;
					break;
				}
			}

			if (foundPath.Length > 0)
			{
				var directInstance = AssetDatabase.LoadAssetAtPath(foundPath, typeof(OpenVRSettings));

				// need to turn this into a serialized object so I can edit it the right way.
				settingsObject = new SerializedObject(directInstance);
				initializationTypeProperty = settingsObject.FindProperty("InitializationType");
				hasSettingsObject = true;

				mirrorViewProperty = settingsObject.FindProperty("MirrorView");
				hasMirrorViewProperty = true;
			}
		}

		private void OnGUI()
		{
			if (hasSettingsObject)
			{
				settingsObject.Update();
				//OpenVRSettings.InitializationTypes initMode = (OpenVRSettings.InitializationTypes)initializationTypeProperty.enumValueIndex;

				//initMode = (OpenVRSettings.InitializationTypes)EditorGUILayout.EnumPopup("Mode", initMode);

				EditorGUILayout.PropertyField(initializationTypeProperty);
				EditorGUILayout.PropertyField(mirrorViewProperty);

				settingsObject.ApplyModifiedProperties();
			}
			else
			{
				if (GUILayout.Button("Try getting xr loader settings"))
				{
					GetXRLoaderSettings();
				}
			}
		}
	}
}