using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;

namespace Instrumental.Modeling.ProceduralGraphics
{
	/// <summary>
	/// A proc gen model for our slider
	/// Remember that just like the button, since this is a pressable, the x direction is flipped, because
	/// activation conditions are 'lower z value of fingertip to activate'
	/// </summary>
	public class SliderModel : ProcGenModel
	{
		// todo: rail type:
		// slot only (make a fake 'hole' mesh that looks like a cutout from the panel)
		// slot with rim (same way we do rims for buttons, extruded edgeloops)
		// solid rail (tube)

		// schema stuff
		[Range(0,1)]
		[SerializeField] float width = 0.05f;
		[Range(0,1)]
		[SerializeField] float radius = 0.022f;
		[SerializeField] float buttonHeight = 0.017f;
		//[SerializeField] float railWidth = 0.01f; // maybe change this to an over-extension amount?
		[Range(0, 1)]
		[SerializeField] float railRadius = 0.005f;
		[Range(0, 0.01f)]
		[SerializeField] float railForwardDistance=0;

		[SerializeField] int faceBevelSliceCount=4;
		[Range(4, 32)]
		[SerializeField] int faceSliceCount = 12;

		[Range(0, 0.04f)]
		[SerializeField] float extrusionDepth = 0.017f;

		[Range(0, 0.4f)]
		[SerializeField] float bevelExtrusionDepth = 0.246f;

		[Range(0,1)]
		[SerializeField] float bevelRadius = 0.697f;


		// face mesh stuff
		[Header("Face Color")]
		[SerializeField]
		ColorType faceColorType = ColorType.FlatColor;
		[SerializeField]
		Gradient faceGradient;
		[SerializeField]
		GradientInfo faceGradientInfo;
		[SerializeField]
		Color faceColor = Color.white;

		Mesh _faceMesh;
		Vector3[] faceVertices;
		int[] faceTriangles;
		Color[] faceColors;

		EdgeLoop[] faceBevelLoops;
		EdgeBridge[] faceBevelBridges;
		LinearEdgeLoopFaceFill faceFill;

		public Mesh FaceMesh { get { return _faceMesh; } }

		// rim mesh stuff
		Mesh _railMesh;


		Vector3 GetLeftExtent()
		{
			return Vector3.right * (width * 0.5f);
		}

		Vector3 GetRightExtent()
		{
			return Vector3.left * (width * 0.5f);
		}

		void Loop(ref Vector3[] vertices,
			int baseID, bool isBottom, float depth, float sideRadius)
		{
			int loopVertCount = faceSliceCount;
			float angleIncrement = 360f / ((loopVertCount) - 1);

			for(int i = 0; i < loopVertCount; i++)
			{
				float angle = angleIncrement * i;
				Vector3 vertex = Vector3.up * sideRadius;
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				vertex += Vector3.right * (width * 0.5f);
				vertex += Vector3.forward * depth;
				vertices[baseID + i] = vertex;
			}
		}

		void SetFaceVertices()
		{
			float extraExtrudeDepth = extrusionDepth * bevelExtrusionDepth;
			float totalExtrudeDepth = extrusionDepth + extraExtrudeDepth;
			float innerRadius = radius;
			for(int i=0; i < faceBevelSliceCount; i++)
			{
				float depthTValue = ((float)i + 1) / (float)faceBevelSliceCount;
				float tValue = (float)i / (float)faceBevelSliceCount;
				int startIndex = faceBevelLoops[i].VertexBaseID;


				float sliceRadius = MathSupplement.Sinerp(innerRadius, radius, 1 - depthTValue);
				float sliceDepth = (i == faceBevelSliceCount - 1) ? Mathf.Lerp(extrusionDepth, totalExtrudeDepth, ((tValue) + (depthTValue)) * 0.5f) : Mathf.Lerp(extrusionDepth, totalExtrudeDepth, depthTValue);

				Loop(ref faceVertices, startIndex, false, sliceDepth, sliceRadius);
			}
		}

		void GenerateFaceColors(out Color[] vertexColors)
		{
			vertexColors = new Color[faceVertices.Length];

			for (int i = 0; i < vertexColors.Length; i++)
			{
				switch (faceColorType)
				{
					case ColorType.FlatColor:
						vertexColors[i] = faceColor;
						break;
					case ColorType.Gradient:
						vertexColors[i] = faceColor; // todo: replace this with a radial gradient
						break;
					default:
						break;
				}
			}
		}

		[SerializeField] int tempLoopCount = 0;

		void GenerateFaceMesh()
		{
			if(_faceMesh == null)
			{
				_faceMesh = new Mesh();
				_faceMesh.MarkDynamic();
			}

			int baseID = 0;
			int bridgeCount=faceBevelSliceCount - 1; // add another one back in here for the fill?

			faceBevelLoops = new EdgeLoop[faceBevelSliceCount];
			for(int i=0; i < faceBevelLoops.Length; i++)
			{
				faceBevelLoops[i] = ModelUtils.CreateEdgeLoop(ref baseID, true, faceSliceCount);
			}

			int vertexCount = faceSliceCount * faceBevelSliceCount;

			faceVertices = new Vector3[vertexCount];

			SetFaceVertices();
			GenerateFaceColors(out faceColors);

			// triangle meshing
			int triangleBaseID = 0;
			faceBevelBridges = new EdgeBridge[bridgeCount];
			for (int i = 0; i < bridgeCount; i++) // this starting at 1 is our problem
			{
				EdgeLoop firstLoop = faceBevelLoops[i];
				EdgeLoop secondLoop = faceBevelLoops[i + 1];
				faceBevelBridges[i] = ModelUtils.CreateExtrustion(ref triangleBaseID, firstLoop, secondLoop);
			}

			int bridgeTriangleIndexCount = faceBevelBridges[0].GetTriangleIndexCount() * bridgeCount;

			// todo: face fill?

			faceTriangles = new int[bridgeTriangleIndexCount];
			for(int i=0; i < faceBevelBridges.Length; i++)
			{
				faceBevelBridges[i].TriangulateBridge(ref faceTriangles, false);
			}
		}

		void GenerateRimMesh()
		{

		}

		public override void GenerateModel()
		{
			GenerateFaceMesh();
			GenerateRimMesh();
		}

		// debug drawing stuff
		[SerializeField] bool drawLoops;
		[SerializeField] bool drawMesh;

		private void OnDrawGizmos()
		{
			Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			Gizmos.matrix = localToWorld;
			Gizmos.DrawWireCube(Vector3.forward * railForwardDistance, new Vector3(width, railRadius));
			DebugExtension.DrawCircle(GetLeftExtent() + (Vector3.forward * buttonHeight), Vector3.forward, radius);
			Gizmos.matrix = Matrix4x4.identity;

			if(drawLoops)
			{
				Gizmos.color = Color.green;
				for (int i = 0; i < faceBevelLoops.Length; i++)
				{
					ModelUtils.DrawEdgeLoopGizmo(faceVertices, faceBevelLoops[i]);
				}
			}

			if (drawMesh)
			{
				Gizmos.color = Color.green;
				ModelUtils.DrawMesh(faceVertices, faceTriangles);
			}
		}
	}
}