using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

namespace Instrumental.Schema
{
    public enum PanelType
    {
        Square,
        Fillet
    }

    public enum SpaceType
    {
        Rectilinear,
        Cylindrical,
        Spherical
    }

#if UNITY
	[CreateAssetMenu(fileName = "PanelSchema", menuName = "Instrumental/PanelSchema")]
#endif
	public class PanelSchema : ScriptableObject
    {
        public PanelType PanelType;
        public SpaceType SpaceType;
        public float SpaceCurveRadius;
        public sV2 PanelDimensions;
        public float Depth;
        public float Radius;
        public int RadiusSegments;
        public float BorderThickness;

        public sColor BorderColor;

        #region UI Controls
        public ControlSchema[] Controls;
        #endregion

        public static PanelSchema GetDefaults()
        {
			return new PanelSchema()
			{
				PanelType = PanelType.Fillet,
				SpaceType = SpaceType.Rectilinear,
				SpaceCurveRadius = 1,
				Depth = 0.0125f,
				PanelDimensions = new sV2(0.245f, 0.125f),
				Radius = 0.03f,
				BorderThickness = 0.437f,
				RadiusSegments = 5,
				BorderColor = new sColor(1, 1, 1, 1),
                Controls = new ControlSchema[0]
            };
        }
    }
}