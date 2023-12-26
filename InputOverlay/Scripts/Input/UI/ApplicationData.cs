using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Valve.VR;

namespace Instrumental.Overlay
{
    public struct ApplicationMetaData
	{
        public string Key;
        public string Name;
        public string ImageURL;
        public string ActionManifestPath;
        public bool IsHidden;
        public bool IsInternal;
	}

    public class ApplicationData : MonoBehaviour
    {
        static List<string> sortedKeyList;
        static Dictionary<string, Texture2D> applicationImageDictionary;
        static Dictionary<string, ApplicationMetaData> applicationDictionary;
		public static Dictionary<string, ApplicationMetaData> ApplicationDictionary { get { return applicationDictionary; } }
        public static List<string> SortedKeys { get { return sortedKeyList; } }

        CVRApplications applicationsAPI;

        private void Awake()
		{
			applicationDictionary = new Dictionary<string, ApplicationMetaData>();
            sortedKeyList = new List<string>();
		}

		private void Start()
		{
            applicationsAPI = OpenVR.Applications;
        }

		public void FindEligibleApplications()
		{
            uint appCount = applicationsAPI.GetApplicationCount();

            System.Text.StringBuilder keyStringBuilder = new System.Text.StringBuilder(256);
            System.Text.StringBuilder propertyStringBuilder = new System.Text.StringBuilder(256);

            for (uint i = 0; i < appCount; i++)
            {
                ApplicationMetaData appMetaData = new ApplicationMetaData();

                keyStringBuilder.Clear();
                EVRApplicationError error = applicationsAPI.GetApplicationKeyByIndex(i, keyStringBuilder, 256);

                if (error == EVRApplicationError.None)
                {
                    string key = keyStringBuilder.ToString();
                    Debug.Log(string.Format("Application at {0} had an key of {1}", i, key));
                    appMetaData.Key = key;

					// lets get some properties
					#region Image URL
					applicationsAPI.GetApplicationPropertyString(key, EVRApplicationProperty.ImagePath_String,
                        propertyStringBuilder, 256, ref error);

                    if (error == EVRApplicationError.None)
                    {
                        string imageURL = propertyStringBuilder.ToString();
                        Debug.Log(string.Format("App: {0} imageURL: {1}", key, imageURL));
                        appMetaData.ImageURL = imageURL;
                    }
                    else
                    {
                        Debug.Log(string.Format("Error getting imageURL for {0}, {1}", key, error.ToString()));
                    }
					#endregion

					#region Action Manifest
					propertyStringBuilder.Clear();
                    applicationsAPI.GetApplicationPropertyString(key, EVRApplicationProperty.ActionManifestURL_String,
                        propertyStringBuilder, 256, ref error);

                    if (error == EVRApplicationError.None)
                    {
                        string actionManifestURL = propertyStringBuilder.ToString();
                        Debug.Log(string.Format("Application {0} has manifest at {1}", key, actionManifestURL));

                        if (actionManifestURL == "none") actionManifestURL = "";
                        appMetaData.ActionManifestPath = actionManifestURL;
                    }
					#endregion

					#region Application Name
					propertyStringBuilder.Clear();
                    applicationsAPI.GetApplicationPropertyString(key, EVRApplicationProperty.Name_String,
                        propertyStringBuilder, 256, ref error);

                    if (error == EVRApplicationError.None)
					{
                        string applicationName = propertyStringBuilder.ToString();
                        Debug.Log(string.Format("Application {0} had name of {1}", key, applicationName));
                        appMetaData.Name = applicationName;
					}
                    #endregion

                    #region IsInternal
                    bool isInternal = applicationsAPI.GetApplicationPropertyBool(key, EVRApplicationProperty.IsInternal_Bool, ref error);
                    if(error != EVRApplicationError.None)
					{
                        Debug.Log(string.Format("Error getting internal flag for application {0}", error.ToString()));
					}
                    appMetaData.IsInternal = isInternal;
                    #endregion

                    #region IsHidden
                    bool isHidden = applicationsAPI.GetApplicationPropertyBool(key, EVRApplicationProperty.IsHidden_Bool,
                        ref error);

                    if(error != EVRApplicationError.None)
					{
                        Debug.Log(string.Format("Error getting hidden flag for application {0}", error.ToString()));
					}
                    appMetaData.IsHidden = isHidden;
					#endregion

					bool isValid = !(isInternal || isHidden);

                    if(isValid)
					{
                        applicationDictionary.Add(key, appMetaData);
					}
				}
				else
                {
                    Debug.Log(error);
                }
            }

            sortedKeyList = SortedKeyList();
        }

        List<string> SortedKeyList()
		{
            List<string> keyList = new System.Collections.Generic.List<string>();
            foreach(string key in applicationDictionary.Keys)
			{
                keyList.Add(key);
			}

            keyList = keyList.OrderBy<string, int>(item => 
            {
                int value = 3;
                if (item.Contains("application.generated") && 
                !item.Contains("application.generated.unity.instrumental")) value = 0;
                else if (item.Contains("steam.app")) value = 1;
                else if (item.Contains("openvr.tool")) value = 2;
                return value;
            }).ToList<string>();

            return keyList;
		}

        public void DownloadImageForKey(string key)
		{

		}

        public void DownloadImagesForApplications()
		{
            for(int i=0; i < sortedKeyList.Count; i++)
			{
                string key = sortedKeyList[i];
                ApplicationMetaData metaData = applicationDictionary[key];

                bool urlIsValid = metaData.ImageURL != null && metaData.ImageURL.Length > 0;
                bool textureAlreadyLoaded = applicationImageDictionary.ContainsKey(key) &&
                    applicationImageDictionary[key];

                if (textureAlreadyLoaded)
                {
                    // todo: return that this texture is ready already
                }
                else if (urlIsValid)
                {
                    UnityWebRequestTexture.GetTexture(metaData.ImageURL);
                }
			}
		}
	}
}