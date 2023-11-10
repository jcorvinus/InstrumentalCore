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
		[SerializeField] int faceBevelSliceCount=4;
		[Range(3, 32)]
		[SerializeField] int faceSliceCount = 12;
		[Range(0, 0.04f)]
		[SerializeField] float extrusionDepth = 0.017f;
		[Range(0, 0.4f)]
		[SerializeField] float bevelExtrusionDepth = 0.246f;
		[Range(0,1)]
		[SerializeField] float bevelRadius = 0.697f;

		//[SerializeField] float railWidth = 0.01f; // maybe change this to an over-extension amount?
		[Range(0, 0.01f)]
		[SerializeField] float railRadius = 0.005f;
		[Range(0, 0.01f)]
		[SerializeField] float railForwardDistance = 0.00158f;
		[Range(3, 8)]
		[SerializeField] int railRadiusSliceCount = 6;
		[Range(0, 32)]
		[SerializeField] int railWidthSliceCount = 6;


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
		EdgeloopFaceFill faceFill;

		public Mesh FaceMesh { get { return _faceMesh; } }

		// rim mesh stuff
		Mesh _railMesh;
		Vector3[] railVertices;
		int[] railTriangles;
		Color[] railColors;
		EdgeLoop railLoop;
		LinearEdgeLoopFaceFill railFill;

		public Vector3[] RailVertices { get { return railVertices; } }

		public Mesh RailMesh { get { return _railMesh; } }

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
			float angleIncrement = 360f / (float)loopVertCount;

			for (int i = 0; i < loopVertCount; i++)
			{
				float angle = angleIncrement * i;
				Vector3 vertex = Vector3.up * sideRadius;
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				//vertex += Vector3.right * (width * 0.5f);
				vertex += Vector3.forward * depth;
				vertices[baseID + i] = vertex;
			}
		}

		void SetFaceVertices()
		{
			float extraExtrudeDepth = extrusionDepth * bevelExtrusionDepth;
			float totalExtrudeDepth = extrusionDepth + extraExtrudeDepth;
			float innerRadius = radius * bevelRadius;
			for(int i=0; i < faceBevelSliceCount; i++)
			{
				float depthTValue = ((float)i + 1) / (float)faceBevelSliceCount;
				float tValue = (float)i / (float)faceBevelSliceCount;
				int startIndex = faceBevelLoops[i].VertexBaseID;


				float sliceRadius = MathSupplement.Sinerp(innerRadius, radius, 1 - depthTValue);
				float sliceDepth = (i == faceBevelSliceCount - 1) ? Mathf.Lerp(extrusionDepth, totalExtrudeDepth, ((tValue) + (depthTValue)) * 0.5f) : Mathf.Lerp(extrusionDepth, totalExtrudeDepth, depthTValue);

				Loop(ref faceVertices, startIndex, false, sliceDepth, sliceRadius);
			}

			WarpFaceVerts();
		}

		void WarpFaceVerts()
		{
			for (int i = 0; i < faceVertices.Length; i++)
			{
				faceVertices[i] = WarpVertex(faceVertices[i], transform);
			}
		}

		void WarpRailVerts()
		{
			for (int i = 0; i < railVertices.Length; i++)
			{
				railVertices[i] = WarpVertex(railVertices[i], transform);
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

			_faceMesh.colors = null;
			_faceMesh.triangles = null;
			_faceMesh.vertices = null;

			int vertexBaseID = 0;
			int bridgeCount = faceBevelSliceCount - 1; // add another one back in here for the fill?

			faceBevelLoops = new EdgeLoop[faceBevelSliceCount];
			for (int i = 0; i < faceBevelLoops.Length; i++)
			{
				faceBevelLoops[i] = ModelUtils.CreateEdgeLoop(ref vertexBaseID, true, faceSliceCount);
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

			// face fill
			faceFill = ModelUtils.CreateFaceFill(ref triangleBaseID, faceBevelLoops[bridgeCount],
				faceSliceCount);
			int faceFillTriangleIndexCount = faceFill.GetTriangleIndexCount();

			faceTriangles = new int[bridgeTriangleIndexCount + faceFillTriangleIndexCount];
			for (int i = 0; i < faceBevelBridges.Length; i++)
			{
				faceBevelBridges[i].TriangulateBridge(ref faceTriangles, false);
			}

			faceFill.TriangulateFace(ref faceTriangles, false);

			_faceMesh.vertices = faceVertices;
			_faceMesh.triangles = faceTriangles;
			_faceMesh.RecalculateNormals();

			// do I have to mark meshes as modified? Will that help?
		}

		void LoopSideRail(ref Vector3[] vertices,
			int baseID, bool isLeft, float depth, float sideRadius)
		{
			float angleIncrement = 180f / (((float)railRadiusSliceCount * 2f) - 1);

			for (int i = 0; i < railRadiusSliceCount * 2; i++)
			{
				float angle = angleIncrement * i;

				Vector3 vertex = Vector3.up * ((isLeft) ? sideRadius : -sideRadius);
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				vertex += Vector3.right * (width * ((isLeft) ? -0.5f : 0.5f));
				vertex += Vector3.forward * depth;
				vertices[baseID + i] = vertex;
			}
		}

		void LoopEdgeRail(ref Vector3[] vertices,
			int baseID, bool isBottom, float depth, float sideRadius)
		{
			Vector3 leftEdge, rightEdge;
			leftEdge = Vector3.right * -(width * 0.5f); // we might also have to subtract the radius here? Not sure.
			rightEdge = Vector3.right * (width * 0.5f);

			leftEdge += Vector3.up * ((isBottom) ? -sideRadius : sideRadius);
			rightEdge += Vector3.up * ((isBottom) ? -sideRadius : sideRadius);
			leftEdge += Vector3.forward * depth;
			rightEdge += Vector3.forward * depth;

			Vector3 startEdge = (isBottom) ? leftEdge : rightEdge;
			Vector3 endEdge = (isBottom) ? rightEdge : leftEdge;

			for (int i = 0; i < railWidthSliceCount; i++)
			{
				vertices[baseID + i] = Vector3.Lerp(startEdge, endEdge, (float)i / railWidthSliceCount);
			}
		}

		void SetRailVerts()
		{
			LoopSideRail(ref railVertices, railLoop.VertexBaseID, true, railForwardDistance, railRadius);
			LoopEdgeRail(ref railVertices, railLoop.VertexBaseID + (railRadiusSliceCount * 2), true, railForwardDistance, railRadius);
			LoopSideRail(ref railVertices, (railLoop.VertexBaseID + (railRadiusSliceCount * 2)) + railWidthSliceCount, false, railForwardDistance, railRadius);
			LoopEdgeRail(ref railVertices, (railLoop.VertexBaseID + (railRadiusSliceCount * 4)) + railWidthSliceCount, false, railForwardDistance, railRadius);

			WarpRailVerts();
		}

		void GenerateRailMesh()
		{
			if(_railMesh == null)
			{
				_railMesh = new Mesh();
				_railMesh.MarkDynamic();
			}

			_railMesh.colors = null;
			_railMesh.triangles = null;
			_railMesh.vertices = null;

			int vertexBaseID = 0;
			int loopVertexCount = (railRadiusSliceCount * 4) + (railWidthSliceCount * 2);
			railLoop = ModelUtils.CreateEdgeLoop(ref vertexBaseID, true, loopVertexCount);

			railVertices = new Vector3[loopVertexCount];
			railColors = new Color[loopVertexCount];

			SetRailVerts();

			int triangleBaseID = 0;
			railFill = ModelUtils.CreateLinearFaceFill(ref triangleBaseID, railLoop, railRadiusSliceCount, railWidthSliceCount);

			railTriangles = new int[railFill.GetTriangleIndexCount()];
			railFill.TriangulateFace(ref railTriangles, false);

			_railMesh.vertices = railVertices;
			_railMesh.colors = railColors;
			_railMesh.triangles = railTriangles;
			_railMesh.RecalculateNormals();
		}

		public override void GenerateModel()
		{
			GenerateFaceMesh();
			GenerateRailMesh();
		}

		// debug drawing stuff
		[SerializeField] bool drawProxy;
		[SerializeField] bool drawLoops;
		[SerializeField] bool drawMesh;

		public bool DrawLoops { get { return drawLoops; } }

		private void OnDrawGizmos()
		{
			if (drawProxy)
			{
				Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
				Gizmos.matrix = localToWorld;
				Gizmos.DrawWireCube(Vector3.forward * railForwardDistance, new Vector3(width, railRadius));
				DebugExtension.DrawCircle(GetLeftExtent() + (Vector3.forward * buttonHeight), Vector3.forward, radius);
				Gizmos.matrix = Matrix4x4.identity;
			}

			if(drawLoops)
			{
				Gizmos.color = Color.green;
				for (int i = 0; i < faceBevelLoops.Length; i++)
				{
					ModelUtils.DrawEdgeLoopGizmo(faceVertices, faceBevelLoops[i]);
				}

				Gizmos.color = Color.grey;
				ModelUtils.DrawEdgeLoopGizmo(railVertices, railLoop);
			}

			if (drawMesh)
			{
				Gizmos.color = Color.green;
				ModelUtils.DrawMesh(faceVertices, faceTriangles);
			}
		}
	}
}