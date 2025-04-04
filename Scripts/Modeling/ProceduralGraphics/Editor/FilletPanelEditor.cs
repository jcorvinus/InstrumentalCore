﻿#if UNITY
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Modeling.ProceduralGraphics
{
    [CustomEditor(typeof(FilletPanel))]
    public class FilletPanelEditor : Editor
    {
		#region Properties

		// schema properties
		SerializedProperty testSchemaProperty;
		//SerializedProperty testSchemaDimensions;
		//SerializedProperty testSchemaDepth;
		//SerializedProperty testSchemaRadius;
		//SerializedProperty testSchemaRadiusSegments;
		//SerializedProperty testSchemaBorderThickness;


		// old properties
        SerializedProperty panelDimensionsProperty;
        SerializedProperty panelDepthProperty;
        SerializedProperty radiusProperty;
        SerializedProperty filletSegmentsProperty;
        SerializedProperty widthSegmentsProperty;
        SerializedProperty heightSegmentsProperty;
        SerializedProperty useVColorsProperty;
        SerializedProperty faceColorTypeProperty;
        SerializedProperty faceGradientProperty;
        SerializedProperty faceGradientInfoProperty;
        SerializedProperty faceColorProperty;

        SerializedProperty borderProperty;
        SerializedProperty borderInsetProperty;
        SerializedProperty borderColorProperty;

        SerializedProperty displayModeProperty;
        SerializedProperty displaySegmentLinesProperty;
        SerializedProperty displayVertIDsProperty;
        SerializedProperty doBackfaceProperty;
        SerializedProperty displayNormalsProperty;
        #endregion

        SerializedObject targetObject;
        GameObject targetGameObject;
        FilletPanel m_instance;

        // Use this for initialization
        void Start()
        {
            targetObject = new SerializedObject(target);
            targetGameObject = ((FilletPanel)target).gameObject;
            m_instance = target as FilletPanel;
            GetProperties();
        }

        private void OnEnable()
        {
            targetObject = new SerializedObject(target);
            targetGameObject = ((FilletPanel)target).gameObject;
            m_instance = target as FilletPanel;
            GetProperties();
        }

        void GetProperties()
        {
			testSchemaProperty = targetObject.FindProperty("testSchema");
            panelDimensionsProperty = targetObject.FindProperty("panelDimensions");
            panelDepthProperty = targetObject.FindProperty("depth");
            radiusProperty = targetObject.FindProperty("radius");
            filletSegmentsProperty = targetObject.FindProperty("filletSegments");
            widthSegmentsProperty = targetObject.FindProperty("widthSegments");
            heightSegmentsProperty = targetObject.FindProperty("heightSegments");

            useVColorsProperty = targetObject.FindProperty("useVColors");

            faceColorTypeProperty = targetObject.FindProperty("faceColorType");
            faceGradientProperty = targetObject.FindProperty("faceGradient");
            faceGradientInfoProperty = targetObject.FindProperty("faceGradientInfo");
            faceColorProperty = targetObject.FindProperty("faceColor");

            // border
            borderProperty = targetObject.FindProperty("border");
            borderInsetProperty = targetObject.FindProperty("borderInsetPercent");
            borderColorProperty = targetObject.FindProperty("borderColor");

            // debug & display
            displaySegmentLinesProperty = targetObject.FindProperty("displaySegmentLines");
            displayNormalsProperty = targetObject.FindProperty("displayNormals");
            displayModeProperty = targetObject.FindProperty("visualizationMode");
            displayVertIDsProperty = targetObject.FindProperty("displayVertIDs");
            doBackfaceProperty = targetObject.FindProperty("doBackFace");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            targetObject.Update();

			if(!Application.isPlaying)
			{
				EditorGUILayout.PropertyField(testSchemaProperty, true);
			}

            if(EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Cannot modify segment values at runtime.", MessageType.Info);
            }
            else
            {
                widthSegmentsProperty.intValue = EditorGUILayout.IntField("Width Segnments", widthSegmentsProperty.intValue);
                heightSegmentsProperty.intValue = EditorGUILayout.IntField("Height Segments", heightSegmentsProperty.intValue);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(useVColorsProperty);
            EditorGUILayout.PropertyField(faceColorTypeProperty);

            if ((ColorType)faceColorTypeProperty.enumValueIndex == ColorType.Gradient)
            {
                EditorGUILayout.PropertyField(faceGradientProperty);
                EditorGUILayout.PropertyField(faceGradientInfoProperty, true);
            }
            else if((ColorType)faceColorTypeProperty.enumValueIndex == ColorType.FlatColor)
            {
                EditorGUILayout.PropertyField(faceColorProperty);
            }

            EditorGUILayout.LabelField("Border", EditorStyles.boldLabel);
            if(EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Cannot modify border type at runtime.", MessageType.Info);
            }
            else
            {
                FilletPanel.BorderType borderType = (FilletPanel.BorderType)EditorGUILayout.EnumPopup("Border Type", (FilletPanel.BorderType)borderProperty.enumValueIndex);
                borderProperty.enumValueIndex = (int)borderType;
            }

            if ((FilletPanel.BorderType)borderProperty.enumValueIndex == FilletPanel.BorderType.Outline ||
                (FilletPanel.BorderType)borderProperty.enumValueIndex == FilletPanel.BorderType.OutlineAndExtrude)
            {
                borderInsetProperty.floatValue = EditorGUILayout.Slider("Border Inset Percent", borderInsetProperty.floatValue,
                    FilletPanel.MIN_INSET_PERCENT, FilletPanel.MAX_INSET_PERCENT);

                EditorGUILayout.PropertyField(borderColorProperty);
            }

            EditorGUILayout.LabelField("Display Options", EditorStyles.boldLabel);
            FilletPanel.VisualizationMode visMode = (FilletPanel.VisualizationMode)displayModeProperty.enumValueIndex;
            visMode = (FilletPanel.VisualizationMode)EditorGUILayout.EnumPopup("Visualization Mode", visMode);
            displayModeProperty.enumValueIndex = (int)visMode;

            displayVertIDsProperty.boolValue = EditorGUILayout.Toggle("Display Vert IDs", displayVertIDsProperty.boolValue);
            displayNormalsProperty.boolValue = EditorGUILayout.Toggle("Display Normals", displayNormalsProperty.boolValue);
            doBackfaceProperty.boolValue = EditorGUILayout.Toggle("Do Back Face", doBackfaceProperty.boolValue);

            if (GUILayout.Button("Generate Mesh"))
            {
                m_instance.GenerateModel();
            }

            if(GUILayout.Button("Generate VColors"))
            {
                m_instance.GenerateVertexColors();
            }

            if(GUILayout.Button("Break"))
            {
                m_instance.Break();
            }

            targetObject.ApplyModifiedProperties();
        }

        private void DrawCorner(Vector3 v1, Vector3 v2, Vector3 v3, float radius)
        {
            FilletPanel.CornerInfo cornerInfo = m_instance.GetCorner((Vect3)v1, (Vect3)v2, (Vect3)v3, radius);
            if (cornerInfo.Valid)
            {
                Handles.color = Color.cyan;
                Handles.DrawWireArc(
                    (Vector3)cornerInfo.Center,
                    (Vector3)cornerInfo.Normal,
                    (Vector3)cornerInfo.From,
                    cornerInfo.Angle, 
                    cornerInfo.Radius); // normal determines the direction of fill
            }
            else
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc((Vector3)cornerInfo.Center, (Vector3)cornerInfo.Normal, cornerInfo.Radius);
            }
        }

        private void DrawCornerSamples(Vector3 v1, Vector3 v2, Vector3 v3, float radius)
        {
            Vector3[] cornerVerts = new Vector3[m_instance.FilletSegments];

            FilletPanel.CornerInfo cornerInfo = m_instance.GetCorner((Vect3)v1, (Vect3)v2, (Vect3)v3,
                radius);

            float angleIncrement = cornerInfo.Angle / (cornerVerts.Length - 1);

            for(int i=0; i < cornerVerts.Length; i++)
            {
                cornerVerts[i] = (Vector3)cornerInfo.Center + (Quaternion.AngleAxis(angleIncrement * i, (Vector3)cornerInfo.Normal) *
                    (Vector3)cornerInfo.From) * cornerInfo.Radius;
            }

            Handles.color = Color.yellow;

            for(int i=0; i < cornerVerts.Length - 1; i++)
            {
                Handles.DrawLine(cornerVerts[i], cornerVerts[i + 1]);
            }
        }

        private void DrawWidthSegments(float inset)
        {
            Vect3[] upperSegments = m_instance.GetUpperPoints(inset);
			Vect3[] lowerSegments = m_instance.GetLowerPoints(inset);

            for(int i=0; i < upperSegments.Length; i++)
            {
                Handles.DrawLine((Vector3)upperSegments[i], (Vector3)lowerSegments[i]);
            }
        }

        private void DrawHeightSegments(float inset)
        {
            Vect3[] leftSegments = m_instance.GetLeftPoints(inset);
            Vect3[] rightSegments = m_instance.GetRightPoints(inset);

            for(int i=0; i < leftSegments.Length; i++)
            {
                Handles.DrawLine((Vector3)leftSegments[i], (Vector3)rightSegments[i]);
            }
        }

        private void DrawOutlines(Matrix4x4 transformMatrix, FilletPanel.VisualizationMode visMode,
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
            float radius, float inset)
        {
            Vector3 upperLeft = v1 + Vector3.right * (radius);
            Vector3 upperRight = v4 + Vector3.right * -(radius);
            Vector3 leftUpper = v1 + Vector3.up * -(radius);
            Vector3 leftLower = v2 + Vector3.up * (radius);
            Vector3 lowerLeft = v2 + Vector3.right * (radius);
            Vector3 lowerRight = v3 + Vector3.right * -(radius);
            Vector3 rightUpper = v4 + Vector3.up * -(radius);
            Vector3 rightLower = v3 + Vector3.up * (radius);

            // draw our wire cube
            Handles.matrix = transformMatrix;
            /*Handles.Label(v1, "c1");
            Handles.Label(v2, "c2");
            Handles.Label(v3, "c3");
            Handles.Label(v4, "c4");*/

            Handles.color = Color.white;

            Handles.color = Color.green;
            Handles.DrawLine(upperLeft, upperRight);
            Handles.DrawLine(leftUpper, leftLower);
            Handles.DrawLine(lowerLeft, lowerRight);
            Handles.DrawLine(rightUpper, rightLower);

            if (visMode == FilletPanel.VisualizationMode.ActualOutlines)
            {
                DrawCornerSamples(v4, v1, v2, radius);
                DrawCornerSamples(v1, v2, v3, radius);
                DrawCornerSamples(v2, v3, v4, radius);
                DrawCornerSamples(v3, v4, v1, radius);
            }
            else if (visMode == FilletPanel.VisualizationMode.IdealOutlines)
            {
                DrawCorner(v4, v1, v2, radius);
                DrawCorner(v1, v2, v3, radius);
                DrawCorner(v2, v3, v4, radius);
                DrawCorner(v3, v4, v1, radius);
            }
        }

        private void OnSceneGUI()
        {
            FilletPanel.VisualizationMode visMode = (FilletPanel.VisualizationMode)displayModeProperty.enumValueIndex;

            Matrix4x4 transformMatrix = targetGameObject.transform.localToWorldMatrix;

            if (visMode == FilletPanel.VisualizationMode.None) return;
            else if (visMode == FilletPanel.VisualizationMode.Mesh)
            {
                if (displayVertIDsProperty.boolValue)
                {
                    Handles.matrix = transformMatrix;
                    for (int i = 0; i < m_instance.Verts.Length; i++)
                    {
                        Vector3 vert = (Vector3)m_instance.Verts[i];

                        //int vertIndx = (i) / width;
                        //int horizIndx = (i) % width;

                        //Handles.Label(vert, string.Format("v{0},{1}", horizIndx, vertIndx));
                        Handles.Label(vert, string.Format("v{0}", i));
                    }
                }
                Handles.matrix = Matrix4x4.identity;

                return;
            }

			float depth = m_instance.Depth;
			float radius = m_instance.Radius;
            Vect3 frontPrimaryV1, frontPrimaryV2, frontPrimaryV3, frontPrimaryV4;
            m_instance.GetCorners(out frontPrimaryV1, out frontPrimaryV2, 
                out frontPrimaryV3, out frontPrimaryV4, 0);

            // draw our primary outline
            DrawOutlines(transformMatrix, visMode,
                (Vector3)frontPrimaryV1, (Vector3)frontPrimaryV2, (Vector3)frontPrimaryV3, (Vector3)frontPrimaryV4, 
                radius, 0);

            FilletPanel.BorderType border = (FilletPanel.BorderType)borderProperty.enumValueIndex;
            if(border == FilletPanel.BorderType.Outline)
            {
                float inset = radius * borderInsetProperty.floatValue;
                Vect3 frontInsetV1, frontInsetV2, frontInsetV3, frontInsetV4;
                m_instance.GetCorners(out frontInsetV1, out frontInsetV2, out frontInsetV3, out frontInsetV4,
                    inset);

                DrawOutlines(transformMatrix, visMode, (Vector3)frontInsetV1, (Vector3)frontInsetV2, (Vector3)frontInsetV3, (Vector3)frontInsetV4,
                    radius, inset);

                if (displaySegmentLinesProperty.boolValue)
                {
                    DrawWidthSegments(inset);
                    DrawHeightSegments(inset);
                }
            }
            else
            {
                if (displaySegmentLinesProperty.boolValue)
                {
                    DrawWidthSegments(0);
                    DrawHeightSegments(0);
                }
            }

            Vect3[] verts;
            FilletPanel.PanelInfo panelInfo;

            m_instance.GenerateVerts(out verts, out panelInfo);
            int width = widthSegmentsProperty.intValue + 2;
            int height = heightSegmentsProperty.intValue + 2;

            if (displayVertIDsProperty.boolValue)
            {
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 vert = (Vector3)verts[i];

                    int vertIndx = (i) / width;
                    int horizIndx = (i) % width;

                    Handles.Label(vert, string.Format("v{0}", i));
                }
            }
        }
    }
}
#endif