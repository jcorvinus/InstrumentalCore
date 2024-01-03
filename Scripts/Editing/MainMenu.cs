using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Editing
{
    public class MainMenu : MonoBehaviour
    {
        PanelEditor panelEditor;
        BoxCollider controlZoneCollider;
        float leftSideBuffer;
        float originalXPosition;

        [SerializeField] string panelName;
        [SerializeField] bool savePanel;
        [SerializeField] bool loadPanel;

        private void Awake()
        {
            panelEditor = FindObjectOfType<PanelEditor>();
            controlZoneCollider = GetComponent<BoxCollider>();
        }

        // Use this for initialization
        void Start()
        {
            Vector3 panelLeftPoint = panelEditor.Panel.GetPositionForHandle(Controls.PanelHandle.HandleType.LeftRail);
            leftSideBuffer = panelLeftPoint.x - GetSliderPoint().x;

            originalXPosition = transform.position.x;
        }

        // Update is called once per frame
        void Update()
        {
            if (panelEditor.Panel.IsResizing)
            {
                Vector3 panelLeftPoint = GetLeftSidePoint();
                //Vector3 sliderPoint = GetSliderPoint();

                transform.position = new Vector3(Mathf.Min(originalXPosition, panelLeftPoint.x), transform.position.y,
                    transform.position.z);
            }

            // debug commands
            if(savePanel)
			{
                savePanel = false;
                SavePanel();
			}
        }

        void SavePanel()
		{
            if(panelName != null && panelName.Length > 0)
			{
                string savedPanelPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                savedPanelPath = System.IO.Path.Combine(savedPanelPath, "Instrumental");
                savedPanelPath = System.IO.Path.Combine(savedPanelPath, string.Format("{0}.uiPanel", panelName));

                panelEditor.Save(savedPanelPath);
			}
            else
			{
                Debug.LogError("Cannot save panel, name was not specified");
			}
		}

        void LoadPanel()
		{
            Debug.LogError("Not yet implemented.");
		}

        Vector3 GetLeftSidePoint()
        {
            return panelEditor.Panel.GetPositionForHandle(Controls.PanelHandle.HandleType.LeftRail) +
                    (Vector3.left * (leftSideBuffer + (controlZoneCollider.size.y * 0.5f)));
        }

        Vector3 GetSliderPoint()
        {
            return transform.TransformPoint(new Vector3(
                0, (controlZoneCollider.size.y) * 0.5f, 0) + controlZoneCollider.center);
        }

        private void OnDrawGizmosSelected()
        {
            if (controlZoneCollider == null) controlZoneCollider = GetComponent<BoxCollider>();
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(GetSliderPoint(), 0.02f);

            if(panelEditor != null && panelEditor.Panel != null)
            {
                Vector3 panelLeftPoint = GetLeftSidePoint();

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(panelLeftPoint, 0.02f);
            }
        }
    }
}