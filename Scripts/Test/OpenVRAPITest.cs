using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

public class OpenVRAPITest : MonoBehaviour
{
    [SerializeField] bool getApplicationCount;
    [SerializeField] int appIndex;
    [SerializeField] bool getApplicationByIndex;
    [SerializeField] bool identifyApplication;
    [SerializeField] bool isApplicationInstalled;

    CVRApplications applicationsAPI;

    // Start is called before the first frame update
    void Start()
    {
        //SteamVR.instance
        //EVRInitError error = EVRInitError.None;
        //OpenVR.GetGenericInterface(OpenVR.IVRApplications_Version, ref error);

        //Debug.Log("CVRApplications: " + error.ToString());
        applicationsAPI = OpenVR.Applications;
    }

    void GetApplicationCount()
	{
        getApplicationCount = false;

        Debug.Log(string.Format("Application count: {0}", applicationsAPI.GetApplicationCount()));
	}

    void GetApplicationByIndex()
    {
        getApplicationByIndex = false;

        uint appCount = applicationsAPI.GetApplicationCount();

        System.Text.StringBuilder keyStringBuilder = new System.Text.StringBuilder(256);
        System.Text.StringBuilder propertyStringBuilder = new System.Text.StringBuilder(256);

        for (uint i = 0; i < appCount; i++)
        {
            keyStringBuilder.Clear();
            EVRApplicationError error = applicationsAPI.GetApplicationKeyByIndex(i, keyStringBuilder, 256);

            if (error == EVRApplicationError.None)
            {
                string key = keyStringBuilder.ToString();
                Debug.Log(string.Format("Application at {0} had an key of {1}", i, key));

                // lets get some properties
                applicationsAPI.GetApplicationPropertyString(key, EVRApplicationProperty.ImagePath_String,
                    propertyStringBuilder, 256, ref error);

                if (error == EVRApplicationError.None)
                {
                    string imageURL = propertyStringBuilder.ToString();
                    Debug.Log(string.Format("App: {0} imageURL: {1}", key, imageURL));
                }
                else
				{
                    Debug.Log(string.Format("Error getting imageURL for {0}, {1}", key, error.ToString()));
				}

                propertyStringBuilder.Clear();
                applicationsAPI.GetApplicationPropertyString(key, EVRApplicationProperty.ActionManifestURL_String,
                    propertyStringBuilder, 256, ref error);

                if(error == EVRApplicationError.None)
				{
                    string actionManifestURL = propertyStringBuilder.ToString();
                    Debug.Log(string.Format("Application {0} has manifest at {1}", key, actionManifestURL));
				}
            }
            else
            {
                Debug.Log(error);
            }
        }
	}

    void IdentifyApplication()
	{
        identifyApplication = false;
	}

    void IsApplicationInstalled()
	{
        isApplicationInstalled = false;
	}

    // Update is called once per frame
    void Update()
    {
        if (getApplicationCount) GetApplicationCount();
        if (getApplicationByIndex) GetApplicationByIndex();
        if (identifyApplication) IdentifyApplication();
        if (isApplicationInstalled) IsApplicationInstalled();
    }
}
