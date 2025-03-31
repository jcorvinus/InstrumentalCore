using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Modeling;
using Instrumental.Core;
using Instrumental.Core.Math;
using Instrumental.Schema;

namespace Instrumental.Modeling.ProceduralGraphics
{
	/// <summary>
	/// This class generates a 3d model of a control panel.
	/// It supports a completely flat panel, outlines, extruded panel,
	/// extruded panel with a back side, and extruded panel with extruded outlines.
	/// 
	/// todo: there is a bug in this class where in some configurations it will generate
	/// too many triangles, and these have degenerate indices of 0,0,0
	/// </summary>
    public class FilletPanel : ProcGenModel
    { 
        public enum BorderType
        {
            None,
            Outline,
            OutlineAndExtrude
        }

        public enum VisualizationMode
        {
            None,
            IdealOutlines,
            ActualOutlines,
            Mesh
        }

        public struct CornerInfo
        {
            public Vect3 Center;
            public Vect3 Normal;
            public Vect3 From;
            public float Angle;
            public bool Valid;
            public float Radius;
        }

        public struct FaceVertexArrayInfo
        {
            public int UpperEdgeBaseID;
            public int LowerEdgeBaseID;
            public int LeftEdgeBaseID;
            public int RightEdgeBaseID;

            public int UpperLeftCornerBaseID;
            public int UpperRightCornerBaseID;
            public int LowerLeftCornerBaseID;
            public int LowerRightCornerBaseID;

            public int InnerGridEndID; // if this is -1, we don't have an inner grid
        }

        public struct PanelInfo
        {
            public FaceVertexArrayInfo FrontVertInfo;
            public FaceVertexArrayInfo BackVertInfo;
            public FaceVertexArrayInfo FrontPanelExtrudeVertInfo;
            public FaceVertexArrayInfo BackPanelExtrudeVertInfo;
            public FaceVertexArrayInfo FrontOuterVertOutlineExtrudeInfo;
            public FaceVertexArrayInfo FrontInnerVertOutlineExtrudeInfo;
        }

        public const int MIN_FILLET_SEGMENTS = 2;
        public const int MAX_FILLET_SEGMENTS = 8;

        public const float MIN_INSET_PERCENT = 0.1f;
        public const float MAX_INSET_PERCENT = 0.5f;

        public const int MIN_BORDER_SEGMENTS = 1;
        public const int MAX_BORDER_SEGMENTS = 4;

        public const float MIN_DEPTH = 0;
        public const float MAX_DEPTH = 0.2f;

        public const float MIN_DIMENSION_SIZE = 0.09f;
        public const float MAX_DIMENSION_WIDTH = 0.9f;
        public const float MAX_DIMENSION_HEIGHT = 0.6f;

		/// <summary>
		/// TestSchema stores variables that can be used for testing,
		/// such as in edit mode in Unity, or for automated testing later
		/// down the line. If you do not override these values via a
		/// Panel class and deliberate loading, these will just get used
		/// as a fallback.
		/// </summary>
#if UNITY
		[SerializeField]
		PanelSchema testSchema;
#elif STEREOKIT
		PanelSchema testSchema = PanelSchema.GetDefaults();
#endif

        Vect2 panelDimensions; // schema property

        // defaults are set here because even though we load whatever's in the
        // test schema on load if available, OnValidate and I think OnEnable 
        // run before Awake handles that, and even then, the testSchema may not be available
        // although we'll want to warn the user in that case.
        float depth = 0.01f; // schema property

        float radius = 0.01f; // schema property

        int filletSegments = MIN_FILLET_SEGMENTS; // schema property

		// This one isn't a schema property because I believe we were going to do it automatically?
		// maybe based off of distance, or if we're in a curved space or not
        [SerializeField]
        int widthSegments;

		// This one isn't a schema property because I believe we were going to do it automatically?
		// maybe based off of distance, or if we're in a curved space or not
		[SerializeField]
        int heightSegments;

        [SerializeField]
        bool useVColors = true;

        [SerializeField]
        ColorType faceColorType = ColorType.FlatColor;

        [SerializeField]
        Gradient faceGradient;

        [SerializeField]
        GradientInfo faceGradientInfo;

        [SerializeField]
        Color faceColor = Color.white;

        [Header("Border")]
        [SerializeField]
        BorderType border = BorderType.None;

        [Range(MIN_INSET_PERCENT, MAX_INSET_PERCENT)]
        [SerializeField]
        float borderInsetPercent = 0.1f; // schema property

        [Range(MIN_BORDER_SEGMENTS, MAX_BORDER_SEGMENTS)]
        [SerializeField]
        int borderSegments = 1; // appears to be unused. I think this was how many bevels the border would have

        [SerializeField]
        Color borderColor = Color.white; // schema property

        public ColorType FaceColorType { get { return faceColorType; } }

#if UNITY_EDITOR
		// editor inspector variables
		public float Depth
		{
			get
			{
				if (!Application.isPlaying)
				{
					return (testSchema) ? testSchema.Depth : depth;
				}
				else
				{
					return depth;
				}
			}
		}

		public float Radius
		{
			get
			{
				if (!Application.isPlaying)
				{
					return (testSchema) ? testSchema.Radius : radius;
				}
				else return radius;
			}
		}

		public Vector2 PanelDimensions
		{
			get
			{
				if (!Application.isPlaying)
				{
					return (testSchema) ? (Vector2)((Vect2)testSchema.PanelDimensions) : (Vector2)panelDimensions;
				}
				else return (Vector2)panelDimensions;
			}
		}

		public int FilletSegments
		{
			get
			{
				if (!Application.isPlaying)
				{
					return (testSchema) ? testSchema.RadiusSegments : filletSegments;
				}
				else return filletSegments;
			}
		}

		public float BorderInsetPercent
		{
			get
			{
				if (!Application.isPlaying)
				{
					return (testSchema) ? testSchema.BorderThickness : borderInsetPercent;
				}
				else return borderInsetPercent;
			}
		}
#endif

		#region Mesh Vars
		ProcGenMesh mesh;

        PanelInfo panelInfo;
		Vect3[] verts;
        Vect2[] uvs;
        int[] tris;
        ColorIC[] vColors;

#if UNITY
		public Mesh Mesh { get { return mesh.UnityMesh; } }
#elif STEREOKIT
		public Mesh Mesh { get { return mesh.StereoKitMesh; } }
#endif
		public Vect3[] Verts { get { return verts; } }
        public Vect2[] UVs { get { return uvs; } }
        public int[] Tris { get { return tris; } }
#endregion

#region Debug Variables
        [SerializeField]
        VisualizationMode visualizationMode = VisualizationMode.ActualOutlines;

        [SerializeField]
        bool displayVertIDs = false;

        [SerializeField]
        bool displaySegmentLines = true;

        [SerializeField]
        bool displayNormals = false;

        [SerializeField]
        bool doBackFace = true;

        public bool DoExtrusion { get { return depth > 0; } }
#endregion

        public override void Awake()
        {
            base.Awake();
			mesh = new ProcGenMesh();
			SetDefaultValues();
		}

        // Use this for initialization
        public override void Start()
        {
            
        }

		private void SetDefaultValues()
		{
			if(testSchema)
			{
				panelDimensions = testSchema.PanelDimensions;
				depth = testSchema.Depth;
				radius = testSchema.Radius;
				filletSegments = testSchema.RadiusSegments;
				borderInsetPercent = testSchema.BorderThickness;
				//borderColor = testSchema.BorderColor; // schema property
			}
		}

        public override void OnValidate()
        {
			if (!Application.isPlaying) SetDefaultValues();

            widthSegments = Mathf.Max(0, widthSegments);
            heightSegments = Mathf.Max(0, heightSegments);

            if(panelDimensions.x < MIN_DIMENSION_SIZE)
            {
                panelDimensions = new Vect2(MIN_DIMENSION_SIZE, panelDimensions.y);
            }

            if(panelDimensions.y < MIN_DIMENSION_SIZE)
            {
                panelDimensions = new Vect2(MIN_DIMENSION_SIZE, 0.001f);
            }

            radius = Mathf.Max(radius, 0);

            depth = (depth >= 0) ? depth : 0;

            base.OnValidate();
        }

        public void SetDimensions(Vect2 dimensions)
        {
            panelDimensions = dimensions;
            SetPropertiesChanged();
        }

        public void SetDepth(float depth)
        {
            this.depth = depth;
            SetPropertiesChanged();
        }

        public void SetRadius(float radius)
        {
            this.radius = radius;
            SetPropertiesChanged();
        }

        public void SetBorderColor(Color color)
        {
            borderColor = color;

            if(useVColors)
            {
                GenerateVColors(out vColors);
				mesh.Update(null, null, null, null, vColors, false, false);
            }

            SetPropertiesChanged();
        }

        public void SetBorderInsetPercent(float percent)
        {
            borderInsetPercent = percent;
            SetPropertiesChanged();
        }

        public void SetFaceGradient(Gradient surfaceGradient)
        {
            faceGradient = surfaceGradient;

            if (useVColors)
            {
                GenerateVColors(out vColors);
				mesh.Update(null, null, null, null, vColors, false, false);
			}

            SetPropertiesChanged();
        }

        public void SetFaceColor(Color surfaceColor)
        {
            faceColor = surfaceColor;

            if (useVColors)
            {
                GenerateVColors(out vColors);
                mesh.Update(null, null, null, null, vColors, false, false);
            }

            SetPropertiesChanged();
        }

#region Panel Shape Methods
        // assuming both vector3s are on a plane (and as such, actually vector2s),
        // get the normal that points 'towards' the reference point
        private Vect3 GetNormal(Vect3 position1, Vect3 position2, Vect3 reference, out Vect3 center)
        {
			Vect3 direction = (position2 - position1).normalized;
            center = (position1 + position2) * 0.5f;

			Vect3 directionToReference = (reference - center).normalized;

			Vect3 normal1 = Quatn.AngleAxis(90, Vect3.forward) * direction;
			Vect3 normal2 = Quatn.AngleAxis(-90, Vect3.forward) * direction;

            return (Vect3.Dot(normal1, directionToReference) > Vect3.Dot(normal2, directionToReference)) ?
                normal1 : normal2;
        }

        public void GetCorners(out Vect3 v1, out Vect3 v2, out Vect3 v3, out Vect3 v4,
            float insetValue)
        {
            v1 = Vect3.up * (panelDimensions.y - insetValue) + Vect3.right * -1 * (panelDimensions.x - insetValue);
            v1 *= 0.5f;
            v2 = Vect3.down * (panelDimensions.y - insetValue) + Vect3.right * -1 * (panelDimensions.x - insetValue);
            v2 *= 0.5f;
            v3 = Vect3.down * (panelDimensions.y - insetValue) + Vect3.right * (panelDimensions.x - insetValue);
            v3 *= 0.5f;
            v4 = Vect3.up * (panelDimensions.y - insetValue) + Vect3.right * (panelDimensions.x - insetValue);
            v4 *= 0.5f;

            return;
        }

        public Vect3[] GetUpperPoints(float inset = 0)
        {
			Vect3 upperLeft, upperRight;
            upperLeft = Vect3.up * (panelDimensions.y - inset) + Vect3.right * -1 * (panelDimensions.x - inset);
            upperLeft *= 0.5f;
            upperLeft = upperLeft + Vect3.right * radius;

            upperRight = Vect3.up * (panelDimensions.y - inset) + Vect3.right * (panelDimensions.x - inset);
            upperRight *= 0.5f;
            upperRight = upperRight + Vect3.right * -radius;

			Vect3[] points = new Vect3[widthSegments + 2];

            points[0] = upperLeft;
            points[points.Length - 1] = upperRight;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(widthSegments + 1f)) * i;

                points[i] = Vect3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public Vect3[] GetLowerPoints(float inset=0)
        {
			Vect3 lowerLeft, lowerRight;
            lowerLeft = Vect3.down * (panelDimensions.y - inset) + Vect3.right * -1 * (panelDimensions.x - inset);
            lowerLeft *= 0.5f;
            lowerLeft = lowerLeft + Vect3.right * radius;

            lowerRight = Vect3.down * (panelDimensions.y - inset) + Vect3.right * (panelDimensions.x - inset);
            lowerRight *= 0.5f;
            lowerRight = lowerRight + Vect3.right * -radius;

			Vect3[] points = new Vect3[widthSegments + 2];

            points[0] = lowerLeft;
            points[points.Length - 1] = lowerRight;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(widthSegments + 1f)) * i;

                points[i] = Vect3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public Vect3[] GetLeftPoints(float inset=0)
        {
			Vect3 upperLeft, lowerLeft;

            upperLeft = Vect3.up * (panelDimensions.y - inset) + Vect3.right * -1 * (panelDimensions.x - inset);
            upperLeft *= 0.5f;
            upperLeft = upperLeft + Vect3.up * -radius;

            lowerLeft = Vect3.down * (panelDimensions.y - inset) + Vect3.right * -1 * (panelDimensions.x - inset);
            lowerLeft *= 0.5f;
            lowerLeft = lowerLeft + Vect3.up * radius;

			Vect3[] points = new Vect3[heightSegments + 2];
            points[0] = upperLeft;
            points[points.Length - 1] = lowerLeft;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(heightSegments + 1f)) * i;

                points[i] = Vect3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public Vect3[] GetRightPoints(float inset=0)
        {
			Vect3 upperRight, lowerRight;

            upperRight = Vect3.up * (panelDimensions.y - inset) + Vect3.right * (panelDimensions.x - inset);
            upperRight *= 0.5f;
            upperRight = upperRight + Vect3.up * -radius;

            lowerRight = Vect3.down * (panelDimensions.y - inset) + Vect3.right * (panelDimensions.x - inset);
            lowerRight *= 0.5f;
            lowerRight = lowerRight + Vect3.up * radius;

			Vect3[] points = new Vect3[heightSegments + 2];
            points[0] = upperRight;
            points[points.Length - 1] = lowerRight;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(heightSegments + 1f)) * i;

                points[i] = Vect3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public CornerInfo GetCorner(Vect3 v1, Vect3 v2, Vect3 v3, float _radius)
        {
			Vect3 v2ToV1Dir = (v1 - v2).normalized;
			Vect3 v2ToV3Dir = (v3 - v2).normalized;

			Vect3 avgDir = (v2ToV1Dir + v2ToV3Dir) * 0.5f;
            avgDir = avgDir.normalized;

			// drawing normals
			Vect3 v2ToV1Center;
			Vect3 v2ToV3Center;

			Vect3 v2ToV1Normal = GetNormal(v2, v1, (v2 + avgDir * _radius), out v2ToV1Center);
			Vect3 v2ToV3Normal = GetNormal(v2, v3, (v2 + avgDir * _radius), out v2ToV3Center);


			Vect3 offsetL1V1 = v2 + (v2ToV1Normal * _radius);
			Vect3 offsetL1V2 = v1 + (v2ToV1Normal * _radius);

			Vect3 offsetL2V1 = v2 + (v2ToV3Normal * _radius);
			Vect3 offsetL2V2 = v3 + (v2ToV3Normal * _radius);

            // let's turn l2 into a Plane and then do Plane.raycast
            PlaneIC l2Plane = new PlaneIC(offsetL2V1, offsetL2V2, offsetL2V1 + Vect3.forward);
            RayIC l1Ray = new RayIC(offsetL1V1, offsetL1V2 - offsetL1V1);

			Vect3 center = Vect3.zero;

            float intersect = 0f;
            if (l2Plane.Raycast(l1Ray, out intersect))
            {
                center = l1Ray.position + l1Ray.direction * intersect;

				// get our intersect points by walking back up our normals to the
				// original lines
				PlaneIC v2ToV1Plane = new PlaneIC(v1, v2, v1 + Vect3.forward);
				PlaneIC v2ToV3Plane = new PlaneIC(v2, v3, v3 + Vect3.forward);

                RayIC arcStartRay = new RayIC(center, v2ToV1Normal * -1);
                float arcStartDistance = 0f;
				Vect3 arcStartPoint = v2ToV1Center;
                if (v2ToV1Plane.Raycast(arcStartRay, out arcStartDistance))
                {
                    arcStartPoint = arcStartRay.position + arcStartRay.direction * arcStartDistance;

                    float normalMult = 1; // we need to figure out when to flip this!

                    return new CornerInfo()
                    {
                        Angle = Vect3.Angle(v2ToV1Dir, v2ToV3Dir),
                        Center = center,
                        From = (arcStartPoint - center).normalized,
                        Normal = Vect3.forward * normalMult,
                        Radius = _radius,
                        Valid = true
                    };
                }
                else
                {
                    return new CornerInfo()
                    {
                        Center = center,
                        Normal = Vect3.forward,
                        Radius = _radius,
                        Valid = false
                    };
                }
            }
            else
            {
                return new CornerInfo()
                {
                    Center = center,
                    Normal = Vect3.forward,
                    Radius = _radius,
                    Valid = false
                };
            }
        }

#endregion

#region Meshing Methods

        public override void GenerateModel()
        {
            PanelInfo panelInfo;

			if(mesh == null) mesh = new ProcGenMesh();

			mesh.SetName("fillet");

            GenerateVerts(out verts, out panelInfo);
            GenerateUVs(verts, out uvs);

            if(useVColors)
            {
                GenerateVColors(out vColors);
            }

            int[] triangles;
            GenerateTriangles(panelInfo, out triangles);

            tris = triangles;

			mesh.Create(verts, tris, null, uvs, ((useVColors) ? vColors : null));

            this.panelInfo = panelInfo;
        }

        public void GenerateVerts(out Vect3[] verts, out PanelInfo panelInfo)
        {
            verts = new Vect3[GetTotalVertBufferSize()];

            FaceVertexArrayInfo frontVertInfo;
            FaceVertexArrayInfo backVertInfo;
            FaceVertexArrayInfo frontPanelExtrudeVertInfo;
            FaceVertexArrayInfo backPanelExtrudeVertInfo;
            FaceVertexArrayInfo frontOuterVertOutlineExtrudeInfo;

            switch (border)
            {
                case BorderType.Outline:
                    GenerateVertsOutline(ref verts, out frontVertInfo,
                        out backVertInfo, out frontOuterVertOutlineExtrudeInfo, out frontPanelExtrudeVertInfo,
                        out backPanelExtrudeVertInfo);

                    panelInfo = new PanelInfo()
                    {
                        FrontVertInfo = frontVertInfo,
                        BackVertInfo = backVertInfo,
                        FrontOuterVertOutlineExtrudeInfo = frontOuterVertOutlineExtrudeInfo,
                        FrontPanelExtrudeVertInfo = frontPanelExtrudeVertInfo,
                        BackPanelExtrudeVertInfo = backPanelExtrudeVertInfo
                    };
                    break;
                case BorderType.OutlineAndExtrude:
                    FaceVertexArrayInfo frontInnerExtrudeInfo;
                    GenerateVertsOutlineExtrude(ref verts, out frontVertInfo,
                        out backVertInfo, out frontOuterVertOutlineExtrudeInfo, out frontInnerExtrudeInfo,
                        out frontPanelExtrudeVertInfo, out backPanelExtrudeVertInfo);

                    panelInfo = new PanelInfo()
                    {
                        FrontVertInfo = frontVertInfo,
                        BackVertInfo = backVertInfo,
                        FrontOuterVertOutlineExtrudeInfo = frontOuterVertOutlineExtrudeInfo,
                        FrontInnerVertOutlineExtrudeInfo = frontInnerExtrudeInfo,
                        FrontPanelExtrudeVertInfo = frontPanelExtrudeVertInfo,
                        BackPanelExtrudeVertInfo = backPanelExtrudeVertInfo
                    };
                    break;
                default:
                    GenerateVertsNoOutline(ref verts, out frontVertInfo, out backVertInfo,
                        out frontPanelExtrudeVertInfo, out backPanelExtrudeVertInfo);

                    panelInfo = new PanelInfo()
                    {
                        FrontVertInfo = frontVertInfo,
                        BackVertInfo = backVertInfo,
                        FrontPanelExtrudeVertInfo = frontPanelExtrudeVertInfo,
                        BackPanelExtrudeVertInfo = backPanelExtrudeVertInfo
                    };
                    break;
            }

            WarpVerts();
        }

        void WarpVerts()
		{
            for(int i=0; i < verts.Length; i++)
			{
                verts[i] = (Vect3)WarpVertex((Vector3)verts[i], transform);
			}
		}

        public void GenerateUVs(Vect3[] verts, out Vect2[] uvs)
        {
            uvs = new Vect2[verts.Length];

            // come up with a method of normalizing the vert locations
            // according to the panel dimensions
            for (int i = 0; i < verts.Length; i++)
            {
                uvs[i] = new Vect2(verts[i].x, verts[i].y);
            }
        }

        void GenerateVertsNoOutline(ref Vect3[] verts, out FaceVertexArrayInfo frontVertInfo,
            out FaceVertexArrayInfo backVertInfo, out FaceVertexArrayInfo frontPanelExtrudeVertInfo,
            out FaceVertexArrayInfo backPanelExtrudeVertInfo)
        {
            Vect3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, 0);

            int baseID = 0;

            baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, false,
                radius, 0, out frontVertInfo);

            if (DoExtrusion)
            {
                // get front extrusion edge
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontPanelExtrudeVertInfo);

                int backExtrusionID = baseID;
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out backPanelExtrudeVertInfo);

                for (int i = backExtrusionID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vect3.forward * depth;
                }
            }
            else
            {
                frontPanelExtrudeVertInfo = new FaceVertexArrayInfo();
                backPanelExtrudeVertInfo = new FaceVertexArrayInfo();
            }

            if (doBackFace)
            {
                int backSideBaseID = baseID;
                baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, true,
                    radius, 0, out backVertInfo);

                for (int i = backSideBaseID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vect3.forward * depth;
                }
            }
            else
            {
                backVertInfo = new FaceVertexArrayInfo();
            }
        }

        void GenerateVertsOutline(ref Vect3[] verts, out FaceVertexArrayInfo frontVertInfo,
            out FaceVertexArrayInfo backVertInfo, out FaceVertexArrayInfo frontOutlineVertInfo,
            out FaceVertexArrayInfo frontPanelExtrudeVertInfo, out FaceVertexArrayInfo backPanelExtrudeVertInfo)
        {
            float inset = radius * borderInsetPercent;

			Vect3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, inset);

            int baseID = 0;

            baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, false, radius, inset, out frontVertInfo);

            GetCorners(out c1, out c2, out c3, out c4, 0);

            int frontOutlineBaseID = baseID;
            // do our front outline here
            baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontOutlineVertInfo);

            if (DoExtrusion)
            {
                // get front extrusion edge
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontPanelExtrudeVertInfo, 0);

                int backExtrusionID = baseID;
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out backPanelExtrudeVertInfo);

                for (int i = backExtrusionID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vect3.forward * depth;
                }
            }
            else
            {
                frontPanelExtrudeVertInfo = new FaceVertexArrayInfo();
                backPanelExtrudeVertInfo = new FaceVertexArrayInfo();
            }

            if (doBackFace)
            {
                int backSideBaseID = baseID;
                baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, true,
                    radius, 0, out backVertInfo);

                for (int i = backSideBaseID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vect3.forward * depth;
                }
            }
            else
            {
                backVertInfo = new FaceVertexArrayInfo();
            }
        }

        void GenerateVertsOutlineExtrude(ref Vect3[] verts, out FaceVertexArrayInfo frontVertInfo,
            out FaceVertexArrayInfo backVertInfo, out FaceVertexArrayInfo frontOutlineVertInfo,
            out FaceVertexArrayInfo frontInnerExtrudeVertInfo, out FaceVertexArrayInfo frontPanelExtrudeVertInfo,
            out FaceVertexArrayInfo backPanelExtrudeVertInfo)
        {
            float inset = radius * borderInsetPercent;

			Vect3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, inset);

            int baseID = 0;

            baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, false,
                radius, inset, out frontVertInfo);

            GetCorners(out c1, out c2, out c3, out c4, 0);

            int frontOutlineBaseID = baseID;

            // do our front outer outline here
            baseID = GenerateBorderEdge(ref verts, baseID, radius,
                out frontPanelExtrudeVertInfo);

            // adjust front outline here
            for (int i = frontOutlineBaseID; i < baseID; i++)
            {
                verts[i] += Vect3.back * depth;
            }

            // re-purposing our frontOutlineVertInfo to be the duplicate edge
            // of the front panel
            int frontInnerFaceDuplicateVerts = baseID;
            baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontOutlineVertInfo, inset);

            int frontInnerExtrudeBaseID = baseID;
            baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontInnerExtrudeVertInfo, inset);

            for (int i = frontInnerExtrudeBaseID; i < baseID; i++)
            {
                // push these verts forward
                verts[i] += Vect3.back * depth;
            }

            if (DoExtrusion)
            {
                int backExtrusionID = baseID;
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out backPanelExtrudeVertInfo);

                for (int i = backExtrusionID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vect3.forward * depth;
                }
            }
            else
            {
                frontPanelExtrudeVertInfo = new FaceVertexArrayInfo();
                backPanelExtrudeVertInfo = new FaceVertexArrayInfo();
            }

            if (doBackFace)
            {
                int backSideBaseID = baseID;
                baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, true,
                    radius, 0, out backVertInfo);

                for (int i = backSideBaseID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vect3.forward * depth;
                }
            }
            else
            {
                backVertInfo = new FaceVertexArrayInfo();
            }
        }

        private int GenerateFaceVerts(ref Vect3[] verts, int baseID,
			Vect3 c1, Vect3 c2, Vect3 c3, Vect3 c4, bool isBack,
            float _radius, float inset, out FaceVertexArrayInfo faceInfo)
        {
            baseID = GetGridInnerVerts(ref verts, baseID, inset);

            int innerGridEndID = baseID;

            int upBaseID, downBaseID, leftBaseID, rightBaseID;

            baseID = GetGridEdgeVerts(ref verts, baseID, out upBaseID,
                out downBaseID, out leftBaseID, out rightBaseID, inset);

            // getting our vertex fans
            //corner 0: (v4, v1, v2); // upper left
            //corner 1: (v1, v2, v3); // upper right
            //corner 2: (v2, v3, v4); // lower left
            //corner 3: (v3, v4, v1); // lower right

            int upperLeftCornerBaseID = 0, lowerLeftCornerBaseID = 0, lowerRightCornerBaseID = 0, upperRightCornerBaseID = 0;

            if (filletSegments > 2)
            {
                upperLeftCornerBaseID = baseID;
                baseID = GetCornerFanVerts(ref verts, baseID, c4, c1, c2, _radius);

                lowerLeftCornerBaseID = baseID;
                baseID = GetCornerFanVerts(ref verts, baseID, c1, c2, c3, _radius);

                lowerRightCornerBaseID = baseID;
                baseID = GetCornerFanVerts(ref verts, baseID, c2, c3, c4, _radius);

                upperRightCornerBaseID = baseID; // actually upper right
                baseID = GetCornerFanVerts(ref verts, baseID, c3, c4, c1, _radius);
            }
            else
            {
                upperLeftCornerBaseID = leftBaseID;
                upperRightCornerBaseID = upBaseID + widthSegments + 1;
                lowerLeftCornerBaseID = downBaseID;
                lowerRightCornerBaseID = rightBaseID + heightSegments + 1;
            }

            faceInfo = new FaceVertexArrayInfo()
            {
                UpperEdgeBaseID = upBaseID,
                LowerEdgeBaseID = downBaseID,
                LeftEdgeBaseID = leftBaseID,
                RightEdgeBaseID = rightBaseID,
                UpperLeftCornerBaseID = upperLeftCornerBaseID,
                UpperRightCornerBaseID = upperRightCornerBaseID,
                LowerLeftCornerBaseID = lowerLeftCornerBaseID,
                LowerRightCornerBaseID = lowerRightCornerBaseID,
                InnerGridEndID = innerGridEndID
            };

            return baseID;
        }

        int GetCornerFanVerts(ref Vect3[] verts, int baseID, Vect3 c1, Vect3 c2, Vect3 c3, float _radius)
        {
            int trackID = baseID;

            int cornerVertsCount = filletSegments;

            CornerInfo cornerInfo = GetCorner(c1, c2, c3, _radius);

            float angleIncrement = cornerInfo.Angle / (cornerVertsCount - 1);

            for (int i = 1; i < cornerVertsCount - 1; i++)
            {
                verts[trackID] = cornerInfo.Center + (Quatn.AngleAxis(angleIncrement * (i), cornerInfo.Normal) *
                    cornerInfo.From) * cornerInfo.Radius;

                trackID++;
            }

            return trackID;
        }

        /// <summary>
        /// Add the inner grid of verts to the vertex buffer
        /// </summary>
        /// <param name="verts">Vertes budffer</param>
        /// <param name="baseID">starting offset</param>
        /// <returns>index of last placed vertex</returns>
        int GetGridInnerVerts(ref Vect3[] verts, int baseID, float inset = 0)
        {
            int innerWidthSegments = widthSegments + 2;
            int innerHeightSements = heightSegments + 2;

            Vect2 innerDimensions = panelDimensions - (Vect2.one * radius * 2) - (Vect2.one * inset);

            Vect3 startPos = (Vect3.left * 0.5f * (panelDimensions.x - inset)) +
                (Vect3.up * 0.5f * (panelDimensions.y - inset)) + new Vect3(1, -1, 0) * radius;

            float widthIncrement = innerDimensions.x / (float)(widthSegments + 1);
            float heightIncrement = innerDimensions.y / (float)(heightSegments + 1);

            int trackedIndx = baseID;
            for (int vertIndx = 0; vertIndx < innerHeightSements; vertIndx++)
            {
                for (int horizIndx = 0; horizIndx < innerWidthSegments; horizIndx++)
                {
                    verts[trackedIndx] = startPos + (Vect3.right * widthIncrement * horizIndx) +
                        (Vect3.down * heightIncrement * vertIndx);

                    trackedIndx++;
                }
            }

            return trackedIndx;
        }

        /// <summary>
        /// Add the outer edge verts to the vertex buffer
        /// </summary>
        /// <param name="verts">Vertes budffer</param>
        /// <param name="baseID">starting offset</param>
        /// <returns>index of last placed vertex</returns>
        int GetGridEdgeVerts(ref Vect3[] verts, int baseID, out int upBaseID, out int downBaseID,
            out int leftBaseID, out int rightBaseID, float inset = 0)
        {
            int horizVertCount = widthSegments + 2;
            int verticalVertCount = heightSegments + 2;

            int vertTrackID = baseID;
            upBaseID = baseID;
            downBaseID = 0;
            leftBaseID = 0;
            rightBaseID = 0;

            Vect2 innerDimensions = panelDimensions - (Vect2.one * radius * 2) - (Vect2.one * inset);
            float widthIncrement = innerDimensions.x / (float)(widthSegments + 1);
            float heightIncrement = innerDimensions.y / (float)(heightSegments + 1);

            // up verts
            for (int i = 0; i < horizVertCount; i++)
            {
				Vect3 startPos = (Vect3.left * 0.5f * (panelDimensions.x - inset)) +
                    (Vect3.up * 0.5f * (panelDimensions.y - inset)) + new Vect3(1, 0, 0) * radius;

                verts[vertTrackID] = startPos + (Vect3.right * widthIncrement * i);
                vertTrackID++;
            }

            // down verts
            downBaseID = vertTrackID;
            for (int i = 0; i < horizVertCount; i++)
            {
				Vect3 startPos = (Vect3.left * 0.5f * (panelDimensions.x - inset)) +
                    (Vect3.up * 0.5f * -(panelDimensions.y - inset)) + new Vect3(1, 0, 0) * radius;

                verts[vertTrackID] = startPos + (Vect3.right * widthIncrement * i);
                vertTrackID++;
            }

            // left verts
            leftBaseID = vertTrackID;
            for (int i = 0; i < verticalVertCount; i++)
            {
				Vect3 startPos = (Vect3.left * 0.5f * (panelDimensions.x - inset)) +
                    (Vect3.up * 0.5f * (panelDimensions.y - inset)) + new Vect3(0, -1, 0) * radius;

                verts[vertTrackID] = startPos + (Vect3.down * heightIncrement * i);
                vertTrackID++;
            }

            // right verts
            rightBaseID = vertTrackID;
            for (int i = 0; i < verticalVertCount; i++)
            {
				Vect3 startPos = (Vect3.right * 0.5f * (panelDimensions.x - inset)) +
                    (Vect3.up * 0.5f * (panelDimensions.y - inset)) + new Vect3(0, -1, 0) * radius;

                verts[vertTrackID] = startPos + (Vect3.down * heightIncrement * i);
                vertTrackID++;
            }

            return vertTrackID;
        }

        int GenerateBorderEdge(ref Vect3[] _verts, int baseID, float _radius,
            out FaceVertexArrayInfo edgeInfo, float inset = 0)
        {
			Vect3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, inset);

            int upBaseID, downBaseID, leftBaseID, rightBaseID;

            baseID = GetGridEdgeVerts(ref _verts, baseID, out upBaseID,
                out downBaseID, out leftBaseID, out rightBaseID, inset);

            int upperLeftCornerBaseID = baseID;
            baseID = GetCornerFanVerts(ref _verts, baseID, c4, c1, c2, _radius);

            int lowerLeftCornerBaseID = baseID;
            baseID = GetCornerFanVerts(ref _verts, baseID, c1, c2, c3, _radius);

            int lowerRightCornerBaseID = baseID;
            baseID = GetCornerFanVerts(ref _verts, baseID, c2, c3, c4, _radius);

            int upperRightCornerBaseID = baseID; // actually upper right
            baseID = GetCornerFanVerts(ref _verts, baseID, c3, c4, c1, _radius);

            edgeInfo = new FaceVertexArrayInfo
            {
                UpperEdgeBaseID = upBaseID,
                LowerEdgeBaseID = downBaseID,
                LeftEdgeBaseID = leftBaseID,
                RightEdgeBaseID = rightBaseID,
                UpperLeftCornerBaseID = upperLeftCornerBaseID,
                UpperRightCornerBaseID = upperRightCornerBaseID,
                LowerLeftCornerBaseID = lowerLeftCornerBaseID,
                LowerRightCornerBaseID = lowerRightCornerBaseID,
                InnerGridEndID = -1
            };

            return baseID;
        }

        void GenerateVColors(out ColorIC[] vertexColors)
        {
            vertexColors = new ColorIC[verts.Length]; // todo: update this to be more efficient

            int frontStartIndex = 0;
            int frontEndIndex = frontStartIndex + GetVertexCountForFaceSide(true);

            for(int i=0; i < verts.Length; i++)
            {
                if(i >= frontStartIndex && i < frontEndIndex)
                {
                    switch (faceColorType)
                    {
                        case ColorType.FlatColor:
                            // do our front face color
                            vertexColors[i] = (ColorIC)faceColor;
                            break;
                        case ColorType.Gradient:
                            float gradientValue = 0;
                            if(faceGradientInfo.Type == GradientType.Horizontal)
                            {
                                gradientValue = Mathf.InverseLerp(-panelDimensions.x * 0.5f, panelDimensions.x * 0.5f, verts[i].x);
                            }
                            else if (faceGradientInfo.Type == GradientType.Vertical)
                            {
                                gradientValue = Mathf.InverseLerp(-panelDimensions.y * 0.5f, panelDimensions.y * 0.5f, verts[i].y);
                            }
                            else if (faceGradientInfo.Type == GradientType.Radial)
                            {
                                gradientValue = uvs[i].magnitude;
                            }

                            if (faceGradientInfo.Invert) gradientValue = 1 - gradientValue;
                            vertexColors[i] = (ColorIC)faceGradient.Evaluate(gradientValue);

                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // just do our border color
                    vertexColors[i] = (ColorIC)borderColor;
                }
            }
        }

        public void GenerateVertexColors()
        {
            if(useVColors)
            {
                GenerateVColors(out vColors);
				mesh.Update(null, null, null, null, vColors, false, false);
            }
        }

        //
        // Triangles
        //

        private void GenerateTriangles(PanelInfo panelInfo, out int[] triangles)
        {
            triangles = new int[GetTotalTriangleCount() * 3];

            int trackID = 0;

            GenerateFaceTriangles(panelInfo.FrontVertInfo, true, ref trackID, ref triangles);
            if (doBackFace) GenerateFaceTriangles(panelInfo.BackVertInfo, false, ref trackID, ref triangles); // generating back faces

            switch (border)
            {
                case BorderType.Outline:
                    if (DoExtrusion)
                    {
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.BackPanelExtrudeVertInfo, true, ref trackID, ref triangles);
                        TriangulateExtrusion(panelInfo.FrontOuterVertOutlineExtrudeInfo, panelInfo.FrontVertInfo, false, ref trackID, ref triangles);
                    }
                    break;
                case BorderType.OutlineAndExtrude:
                    if (DoExtrusion)
                    {
                        // from back panel to front outline extrusion
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.BackPanelExtrudeVertInfo, true, ref trackID, ref triangles);
                        // front panel outer extrusion outline to outline forward inset
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.FrontInnerVertOutlineExtrudeInfo, false, ref trackID, ref triangles);
                        // outline forward inset to front face duplicate verts (warning: second name has been remapped)
                        TriangulateExtrusion(panelInfo.FrontInnerVertOutlineExtrudeInfo, panelInfo.FrontOuterVertOutlineExtrudeInfo, false, ref trackID, ref triangles);
                    }
                    break;
                default:
                    // generate extrusion triangles
                    if (DoExtrusion)
                    {
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.BackPanelExtrudeVertInfo, true, ref trackID, ref triangles);
                    }
                    break;
            }
        }

        private void GenerateFaceTriangles(FaceVertexArrayInfo vertInfo, bool flip, ref int trackID, ref int[] triangles)
        {
            int innerGridBase = vertInfo.InnerGridEndID - GetInnerGridVertexCount();

            // triangulate inner grid
            int numberOfQuads = InnerGridTriangleCount() / 2;
            for (int i = 0; i < numberOfQuads; i++) // dividing by two so we can iterate by quad
            {
                // we lose accuracy every loop, so we'll need a 'bump' counter that goes up
                // after every width iteration
                int bumpCounter = i / (widthSegments + 1);

                // upper left vert id = base * quad ID
                // upper right vert id = (base * quad ID) + 1

                // lower left vert = base * lower quad ID (lower quad ID is quad ID + quad grid width)
                // lower right vert = (base * lower quad ID) + 1

                // bottom left, upper left, upper right
                // bottom left, upper right, lower left

                // we need to find a way to get the next row's base id
                int upperRight = (i + 1);
                int upperLeft = i;
                int lowerLeft = (i + widthSegments + 2);
                int lowerRight = (lowerLeft + 1);

                triangles[trackID] = ((flip) ? (lowerLeft + bumpCounter) : (upperRight + bumpCounter)) + innerGridBase;
                triangles[trackID + 1] = (upperLeft + bumpCounter) + innerGridBase;
                triangles[trackID + 2] = ((flip) ? (upperRight + bumpCounter) : (lowerLeft + bumpCounter)) + innerGridBase;

                triangles[trackID + 3] = ((flip) ? (lowerLeft + bumpCounter) : (lowerRight + bumpCounter)) + innerGridBase;
                triangles[trackID + 4] = (upperRight + bumpCounter) + innerGridBase;
                triangles[trackID + 5] = ((flip) ? (lowerRight + bumpCounter) : (lowerLeft + bumpCounter)) + innerGridBase;

                trackID += 6;
            }

#region Outer Edges
            // triangulate outer edges
            // do upper edge
            for (int i = 0; i < widthSegments + 1; i++)
            {
                int upperLeft = vertInfo.UpperEdgeBaseID + i;
                int upperRight = upperLeft + 1;
                int lowerLeft = innerGridBase + i;
                int lowerRight = innerGridBase + i + 1;

                triangles[trackID] = (flip) ? lowerLeft : upperRight;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? upperRight : lowerLeft;

                triangles[trackID + 3] = (flip) ? lowerLeft : lowerRight;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerRight : lowerLeft;

                trackID += 6;
            }

            // do lower edge
            for (int i = 0; i < widthSegments + 1; i++)
            {
                int innerGridLowEdgeBase = vertInfo.InnerGridEndID - (widthSegments + 2);

                int upperLeft = vertInfo.LowerEdgeBaseID + i;
                int upperRight = upperLeft + 1;
                int lowerLeft = innerGridLowEdgeBase + i;
                int lowerRight = innerGridLowEdgeBase + i + 1;

                triangles[trackID] = (flip) ? upperRight : lowerLeft;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? lowerLeft : upperRight;

                triangles[trackID + 3] = (flip) ? lowerRight : lowerLeft;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerLeft : lowerRight;

                trackID += 6;
            }

            // do left edge
            for (int i = 0; i < heightSegments + 1; i++)
            {
                int upperLeft = vertInfo.LeftEdgeBaseID + i;
                int upperRight = innerGridBase + Math.GetLeftEdgeForHeightIndex(0, i, widthSegments + 2);

                int lowerLeft = upperLeft + 1;
                int lowerRight = innerGridBase + Math.GetLeftEdgeForHeightIndex(0, i + 1, widthSegments + 2);

                triangles[trackID] = (flip) ? lowerLeft : upperRight;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? upperRight : lowerLeft;

                triangles[trackID + 3] = (flip) ? lowerLeft : lowerRight;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerRight : lowerLeft;

                trackID += 6;
            }

            // do right edge
            for (int i = 0; i < heightSegments + 1; i++)
            {
                int upperLeft = vertInfo.RightEdgeBaseID + i;
                int upperRight = innerGridBase + Math.GetRightEdgeForHeightIndex(0, i, widthSegments + 2);

                int lowerLeft = upperLeft + 1;
                int lowerRight = innerGridBase + Math.GetRightEdgeForHeightIndex(0, i + 1, widthSegments + 2);

                triangles[trackID] = (flip) ? upperRight : lowerLeft;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? lowerLeft : upperRight;

                triangles[trackID + 3] = (flip) ? lowerRight : lowerLeft;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerLeft : lowerRight;

                trackID += 6;
            }
#endregion

#region Triangulate Corner Fans
            // upper left
            TriangulateFan(vertInfo.UpperEdgeBaseID, vertInfo.LeftEdgeBaseID,
                innerGridBase, vertInfo.UpperLeftCornerBaseID, flip, ref trackID, ref triangles);

            // lower left
            TriangulateFan(
                vertInfo.LeftEdgeBaseID + heightSegments + 1,
                vertInfo.LowerEdgeBaseID,
                innerGridBase + Math.GetLeftEdgeForHeightIndex(0, heightSegments + 1, widthSegments + 2),
                vertInfo.LowerLeftCornerBaseID,
                flip,
                ref trackID,
                ref triangles);

            // upper right
            TriangulateFan(
                vertInfo.RightEdgeBaseID,
                vertInfo.UpperEdgeBaseID + widthSegments + 1,
                innerGridBase + Math.GetRightEdgeForHeightIndex(0, 0, widthSegments + 2),
                vertInfo.UpperRightCornerBaseID,
                flip,
                ref trackID,
                ref triangles);

            // lower right
            TriangulateFan(
                vertInfo.LowerEdgeBaseID + widthSegments + 1,
                vertInfo.RightEdgeBaseID + heightSegments + 1,
                innerGridBase + Math.GetRightEdgeForHeightIndex(0, heightSegments + 1, widthSegments + 2),
                vertInfo.LowerRightCornerBaseID,
                flip,
                ref trackID,
                ref triangles);
#endregion
        }

        private void TriangulateFan(int lowEdgeID, int highEdgeID, int centerID,
            int baseID, bool flip, ref int trackID, ref int[] triangles)
        {
            int triangleCount = filletSegments - 2;

            // do our entry triangle
            triangles[trackID] = (flip) ? baseID : centerID;
            triangles[trackID + 1] = lowEdgeID;
            triangles[trackID + 2] = (flip) ? centerID : baseID;
            trackID += 3;

            // do our fan triangles
            for (int triIndx = 0; triIndx < triangleCount; triIndx++)
            {
                int lastID = (triIndx == triangleCount - 1) ? highEdgeID : (triIndx + 1) + baseID;
                triangles[trackID] = (flip) ? lastID : centerID;
                triangles[trackID + 1] = (triIndx) + baseID;
                triangles[trackID + 2] = (flip) ? centerID : lastID;
                trackID += 3;
            }
        }

        private void TriangulateExtrusion(FaceVertexArrayInfo frontVertInfo, FaceVertexArrayInfo backVertInfo,
            bool flip, ref int trackID, ref int[] triangles)
        {
            int widthQuadCount = widthSegments + 1;
            int heightQuadCount = heightSegments + 1;

            // triangulate upper edge
            for (int i = 0; i < widthQuadCount; i++)
            {
                int triA0 = frontVertInfo.UpperEdgeBaseID + (i + 1);
                int triA1 = backVertInfo.UpperEdgeBaseID + i;
                int triA2 = frontVertInfo.UpperEdgeBaseID + i;

                int triB0 = backVertInfo.UpperEdgeBaseID + i;
                int triB1 = frontVertInfo.UpperEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.UpperEdgeBaseID + (i + 1);

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate lower edge
            for (int i = 0; i < widthQuadCount; i++)
            {
                int triA0 = frontVertInfo.LowerEdgeBaseID + i;
                int triA1 = backVertInfo.LowerEdgeBaseID + i;
                int triA2 = frontVertInfo.LowerEdgeBaseID + (i + 1);

                int triB0 = backVertInfo.LowerEdgeBaseID + (i + 1);
                int triB1 = frontVertInfo.LowerEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.LowerEdgeBaseID + i;

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate left edge
            for (int i = 0; i < heightQuadCount; i++)
            {
                int triA0 = frontVertInfo.LeftEdgeBaseID + i;
                int triA1 = backVertInfo.LeftEdgeBaseID + i;
                int triA2 = frontVertInfo.LeftEdgeBaseID + (i + 1);

                int triB0 = backVertInfo.LeftEdgeBaseID + (i + 1);
                int triB1 = frontVertInfo.LeftEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.LeftEdgeBaseID + i;

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate right edge
            for (int i = 0; i < heightQuadCount; i++)
            {
                int triA0 = frontVertInfo.RightEdgeBaseID + (i + 1);
                int triA1 = backVertInfo.RightEdgeBaseID + i;
                int triA2 = frontVertInfo.RightEdgeBaseID + i;

                int triB0 = backVertInfo.RightEdgeBaseID + i;
                int triB1 = frontVertInfo.RightEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.RightEdgeBaseID + (i + 1);

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate UL fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.UpperLeftCornerBaseID, frontVertInfo.LeftEdgeBaseID, frontVertInfo.UpperEdgeBaseID,
                backVertInfo.UpperLeftCornerBaseID, backVertInfo.LeftEdgeBaseID, backVertInfo.UpperEdgeBaseID,
                ref trackID, ref triangles);

            // triangulate LL fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.LowerLeftCornerBaseID, frontVertInfo.LowerEdgeBaseID, frontVertInfo.LeftEdgeBaseID + (heightSegments + 1),
                backVertInfo.LowerLeftCornerBaseID, backVertInfo.LowerEdgeBaseID, backVertInfo.LeftEdgeBaseID + (heightSegments + 1),
                ref trackID, ref triangles);

            // triangulate UR fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.UpperRightCornerBaseID, frontVertInfo.UpperEdgeBaseID + (widthSegments + 1), frontVertInfo.RightEdgeBaseID,
                backVertInfo.UpperRightCornerBaseID, backVertInfo.UpperEdgeBaseID + (widthSegments + 1), backVertInfo.RightEdgeBaseID,
                ref trackID, ref triangles);

            // triangulate LR fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.LowerRightCornerBaseID, frontVertInfo.RightEdgeBaseID + (heightSegments + 1), frontVertInfo.LowerEdgeBaseID + (widthSegments + 1),
                backVertInfo.LowerRightCornerBaseID, backVertInfo.RightEdgeBaseID + (heightSegments + 1), backVertInfo.LowerEdgeBaseID + (widthSegments + 1),
                ref trackID, ref triangles);
        }

        private void TriangulateExtrusionFan(bool flip,
            int frontCornerBaseID, int frontCornerHighID, int frontCornerLowID,
            int backCornerBaseID, int backCornerHighID, int backCornerLowID,
            ref int trackID, ref int[] triangles)
        {
            int fanQuadCount = filletSegments;

            if (filletSegments > 2)
            {
                // do our leading triangles
                int leadingTriA0 = frontCornerLowID;
                int leadingTriA1 = backCornerBaseID;
                int leadingTriA2 = frontCornerBaseID;

                int leadingTriB0 = backCornerBaseID;
                int leadingTriB1 = frontCornerLowID;
                int leadingTriB2 = backCornerLowID;

                triangles[trackID] = (!flip) ? leadingTriA0 : leadingTriA2;
                triangles[trackID + 1] = leadingTriA1;
                triangles[trackID + 2] = (!flip) ? leadingTriA2 : leadingTriA0;

                triangles[trackID + 3] = (!flip) ? leadingTriB0 : leadingTriB2;
                triangles[trackID + 4] = leadingTriB1;
                triangles[trackID + 5] = (!flip) ? leadingTriB2 : leadingTriB0;

                trackID += 6;

                for (int i = 0; i < fanQuadCount - 3; i++)
                {
                    int triA0 = frontCornerBaseID + i;
                    int triA1 = backCornerBaseID + i;
                    int triA2 = frontCornerBaseID + (i + 1);

                    int triB0 = backCornerBaseID + (i + 1);
                    int triB1 = frontCornerBaseID + (i + 1);
                    int triB2 = backCornerBaseID + i;

                    triangles[trackID] = (!flip) ? triA0 : triA2;
                    triangles[trackID + 1] = triA1;
                    triangles[trackID + 2] = (!flip) ? triA2 : triA0;

                    triangles[trackID + 3] = (!flip) ? triB0 : triB2;
                    triangles[trackID + 4] = triB1;
                    triangles[trackID + 5] = (!flip) ? triB2 : triB0;

                    trackID += 6;
                }

                // do our trailing triangles
                int offset = (filletSegments - 3);
                int trailingTriA0 = frontCornerBaseID + offset;
                int trailingTriA1 = backCornerBaseID + offset;
                int trailingTriA2 = frontCornerHighID;

                int trailingTriB0 = backCornerHighID;
                int trailingTriB1 = frontCornerHighID;
                int trailingTriB2 = backCornerBaseID + offset;

                triangles[trackID] = (!flip) ? trailingTriA0 : trailingTriA2;
                triangles[trackID + 1] = trailingTriA1;
                triangles[trackID + 2] = (!flip) ? trailingTriA2 : trailingTriA0;

                triangles[trackID + 3] = (!flip) ? trailingTriB0 : trailingTriB2;
                triangles[trackID + 4] = trailingTriB1;
                triangles[trackID + 5] = (!flip) ? trailingTriB2 : trailingTriB0;

                trackID += 6;
            }
            else
            {
                // custom handling for direct reference of border edges,
                // no fillet segments

                // do our leading triangles
                int leadingTriA0 = frontCornerLowID;
                int leadingTriA1 = backCornerHighID;
                int leadingTriA2 = frontCornerHighID;

                int leadingTriB0 = backCornerHighID;
                int leadingTriB1 = frontCornerLowID;
                int leadingTriB2 = backCornerLowID;

                triangles[trackID] = (!flip) ? leadingTriA0 : leadingTriA2;
                triangles[trackID + 1] = leadingTriA1;
                triangles[trackID + 2] = (!flip) ? leadingTriA2 : leadingTriA0;

                triangles[trackID + 3] = (!flip) ? leadingTriB0 : leadingTriB2;
                triangles[trackID + 4] = leadingTriB1;
                triangles[trackID + 5] = (!flip) ? leadingTriB2 : leadingTriB0;

                trackID += 6;
            }
        }

        private int InnerGridTriangleCount()
        {
            int widthQuads = widthSegments + 1;
            int heightQuads = heightSegments + 1;
            return (widthQuads * heightQuads) * 2;
        }

        private int ExtrusionEdgeTriangleCount()
        {
            int widthQuadCount = widthSegments + 1;
            int heightQuadCount = heightSegments + 1;

            int filletQuadCount = filletSegments + 2;

            int totalQuadCount = (widthQuadCount * 2) + (heightQuadCount * 2) +
                (filletQuadCount * 4);

            return (totalQuadCount * 2);
        }

        private int EdgeGridTriangleCount()
        {
            return (((widthSegments + 1) * 2) + ((heightSegments + 1) * 2)) * 2;
        }

        private int CornerFanTriangleCount()
        {
            return filletSegments - 1;
        }

        private int FaceTriangleCount()
        {
            int innerGridTriangleCount = InnerGridTriangleCount();

            int edgeGridTriangleCount = EdgeGridTriangleCount();

            int cornerFanCount = CornerFanTriangleCount();

            return innerGridTriangleCount + edgeGridTriangleCount + (cornerFanCount * 4);
        }

        private int GetTotalTriangleCount()
        {
            int faceCount = FaceTriangleCount();

            if (doBackFace) faceCount *= 2;

            int extrusionCount = 1;

            switch (border)
            {
                case BorderType.None:
                    extrusionCount = (DoExtrusion) ? 1 : 0;
                    break;
                case BorderType.Outline:
                    extrusionCount = (DoExtrusion) ? 2 : 0;
                    break;
                case BorderType.OutlineAndExtrude:
                    extrusionCount = (DoExtrusion) ? 4 : 0;
                    break;
                default:
                    break;
            }

            int extrusionTriangleCount = ExtrusionEdgeTriangleCount() * extrusionCount;

            return faceCount + extrusionTriangleCount;
        }

        private int GetTotalVertBufferSize()
        {
            int faceSide = GetVertexCountForFaceSide(true);
            int edgeSide = GetVertexCountForFaceSide(false);

            int numberOfEdgeSides = 0;

            if (border == BorderType.OutlineAndExtrude) numberOfEdgeSides = 2;
            else if (border == BorderType.Outline) numberOfEdgeSides = 2;
            if (DoExtrusion) numberOfEdgeSides += 2;

            int faceCount = (doBackFace) ? 2 : 1;

            return (faceSide * faceCount) + (edgeSide * numberOfEdgeSides);
        }

        private int GetInnerGridVertexCount()
        {
            int innerWidthSegments = widthSegments + 2;
            int innerHeightSements = heightSegments + 2;

            return innerWidthSegments * innerHeightSements;
        }

        private int GetVertexCountForFaceSide(bool includeInnerGridVerts)
        {
            int innerWidthSegments = widthSegments + 2;
            int innerHeightSements = heightSegments + 2;

            int innerGridSize = (includeInnerGridVerts) ? GetInnerGridVertexCount() : 0;

            int outerEdgesSize = (innerWidthSegments * 2) + (innerHeightSements * 2);

            int vertexFanCount = (filletSegments - 2) * 4;

            return innerGridSize + outerEdgesSize + vertexFanCount;
        }
#endregion

        // Update is called once per frame
        void Update()
        {
            UpdateVerts();
        }

        void UpdateVerts()
        {
            if (mesh == null)
            {
                GenerateModel();
            }
            else
            {
                GenerateVerts(out verts, out panelInfo);
            }

			mesh.Update(verts, null, null, null, null, true, false);
        }

#if UNITY_EDITOR
        public void Break()
        {
            Debug.Break();
        }
#endif

        private void DrawGizmoMesh()
        {
#if UNITY
			GenerateVerts(out verts, out panelInfo);
            GenerateTriangles(panelInfo, out tris);

            Color[] colors = { Color.green, Color.white };
            Gizmos.matrix = transform.localToWorldMatrix;

            for (int i = 0; i < tris.Length; i += 3)
            {
                Gizmos.color = (tris[i] > panelInfo.BackVertInfo.InnerGridEndID - GetInnerGridVertexCount()) ? Color.green : Color.yellow;

                int vertA, vertB, vertC;

                vertA = i;
                vertB = vertA + 1;
                vertC = vertB + 1;

                try
                {
                    Gizmos.DrawLine((Vector3)verts[tris[vertA]], (Vector3)verts[tris[vertB]]);
                }
                catch (System.IndexOutOfRangeException e)
                {
                    Debug.Log(string.Format("IOOR in triangle edge 0. Index: {0} {1}", tris[vertA], tris[vertB]));
                }

                try
                {
                    Gizmos.DrawLine((Vector3)verts[tris[vertB]], (Vector3)verts[tris[vertC]]);
                }
                catch (System.IndexOutOfRangeException e)
                {
                    Debug.Log(string.Format("IOOR in triangle edge 1. Index: {0} {1}", tris[vertB], tris[vertC]));
                }

                try
                {
                    Gizmos.DrawLine((Vector3)verts[tris[vertA]], (Vector3)verts[tris[vertC]]);
                }
                catch(System.IndexOutOfRangeException e)
                {
                    Debug.Log(string.Format("IOOR in triangle edge 2. Index: {0} {1}", tris[vertA], tris[vertC]));
                }
            }

            float normalDisplayLength = Mathf.Min(panelDimensions.x, panelDimensions.y) * 0.2f;

            if (displayNormals && mesh.UnityMesh != null)
            {
                for(int i=0; i < mesh.GetVertexCount(); i++)
                {
                    Vector3 start = (Vector3)verts[i];
                    Vector3 direction = mesh.GetNormalAtIndex(i) * normalDisplayLength * 0.5f;

                    Gizmos.DrawLine(start, start + direction);
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
#endif
		}

        private void OnDrawGizmosSelected()
        {
#if UNITY
			if (visualizationMode == VisualizationMode.Mesh)
            {
                DrawGizmoMesh();
            }
#endif
		}
    }
}