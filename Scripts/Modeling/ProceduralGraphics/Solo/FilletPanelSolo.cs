using System.Collections;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

using Instrumental.Schema;

namespace Instrumental.Modeling.ProceduralGraphics.Solo
{
    /// <summary>
    /// This is a 'solo' class. Solos are intended to be used to control the generation of a
    /// model when the model is not owned by a Control, or specifically in the case of panels,
    /// a panel.
    /// </summary>
    public class FilletPanelSolo :
#if UNITY
        MonoBehaviour
#elif STEREOKIT
        IStepper
#endif
    {
#if UNITY
        [SerializeField]
#endif
        PanelSchema panelSchema; // with stereokit we'll need a way of loading these

        FilletPanelModel filletPanelModel;

        // called on startup
#if UNITY
        void Start()
#elif STEREOKIT
        public bool Initialize()
#endif
        {
            AcquireComponents();
            SetSchemaValues();
        }

        void AcquireComponents()
        {
#if UNITY
            filletPanelModel = GetComponent<FilletPanelModel>();
#elif STEREOKIT
            throw new System.NotImplementedException("FilletPanelSolo not implemented in stereokit yet");
#endif
        }

        void SetSchemaValues()
        {
            filletPanelModel.SetDimensions(panelSchema.PanelDimensions);
            filletPanelModel.SetDepth(panelSchema.Depth);
            filletPanelModel.SetRadius(panelSchema.Radius);
            filletPanelModel.SetFilletSegments(panelSchema.RadiusSegments);
            filletPanelModel.SetBorderInsetPercent(panelSchema.BorderThickness);
        }

        // called once per frame
#if UNITY
        void Update()
#elif STEREOKIT
        public void Step()
#endif
        {

        }
    }
}