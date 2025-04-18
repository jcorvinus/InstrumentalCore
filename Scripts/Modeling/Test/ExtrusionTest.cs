using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

#if UNITY
using UnityEngine;
using Instrumental.Modeling.ProceduralGraphics;

namespace Instrumental.Modeling
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class ExtrusionTest : MonoBehaviour
	{
		// thinking about this - I wonder if we can separate our eventual generated code from 
		// the user-specified stuff by using partial structs.
		[System.Serializable]
		public struct ExtrusionTestVariables
		{
			[Range(0,1)]
			public float extrusionDepth;
			[Range(0, 1)]
			public float radius;

			[ProcGenMesh.BufferSizeRegen(CausesRegen = true)]
			public bool closeLoop;

			[ProcGenMesh.BufferSizeRegen(CausesRegen = true)]
			public bool fillFace;

			[Range(2, 8)]
			[ProcGenMesh.BufferSizeRegen(CausesRegen = true)]
			public int cornerVertCount;

			[Range(0, 8)]
			[ProcGenMesh.BufferSizeRegen(CausesRegen = true)]
			public int widthVertCount;

			public float width;

			public int EdgeLoopVertCount { get { return (cornerVertCount * 4) + (widthVertCount * 2); } }

			// look into auto-code generating this with an attribute on the struct or something
			public enum VariableType
			{
				extrusionDepth = 0,
				radius = 1,
				closeLoop = 2,
				fillFace = 3,
				cornerVertCount = 4,
				widthVertCount = 5,
				width = 6,
				Count = 6
			}

			// this is really weird and I don't like how we have to manually code this
			// however, I think in the future we might be able to get around this using ienumerator
			public ValueType this[int index]
			{
				get
				{
					switch (index)
					{
						case 0: return extrusionDepth;
						case 1: return radius;
						case 2: return closeLoop;
						case 3: return fillFace;
						case 4: return cornerVertCount;
						case 5: return widthVertCount;
						case 6: return width;
						default: throw new System.IndexOutOfRangeException("Invalid index for ExtrusionTestVariables"); // TODO: make this a custom exception
					}
				}

				set
				{
					switch (index)
					{
						case 0: 
							extrusionDepth = (float)value;
							break;
						case 1: 
							radius = (float) value;
							break;
						case 2: 
							closeLoop = (bool)value;
							break;
						case 3: 
							fillFace = (bool)value;
							break;
						case 4: 
							cornerVertCount = (int)value;
							break;
						case 5: 
							widthVertCount = (int)value;
							break;
						case 6:
							width = (float)value;
							break;
						default: throw new System.IndexOutOfRangeException("Invalid index for ExtrusionTestVariables"); // TODO: make this a custom exception
					}
				}
			}

			// need to have a static 'regen attribute map'
			private static Dictionary<VariableType, bool> regenMap = new Dictionary<VariableType, bool>();
			
			public static bool GetRegenAttribute(VariableType type)
			{
				if (regenMap.ContainsKey(type))
				{
					return regenMap[type];
				}
				else
				{
					throw new Exception("Invalid variable type for regen attribute");
				}
			}

			private static bool hasMapInitialized = false; // might not be necessary if our static constructor runs before everything

			static ExtrusionTestVariables()
			{
				Debug.Assert(hasMapInitialized == false, "Map has already been initialized, dono why a static constructor is getting called more than once.");

				// reflection on our type
				Type extrusionTestType = typeof(ExtrusionTestVariables);

				// get all the fields
				FieldInfo[] fields = extrusionTestType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

				// loop through the fields
				foreach (FieldInfo field in fields)
				{
					// check if the field has the attribute
					var attributes = field.GetCustomAttributes(typeof(ProcGenMesh.BufferSizeRegenAttribute), false);
					if (attributes.Length == 1)
					{
						// we have an attribute, but we need to check it for specific regen behavior
						ProcGenMesh.BufferSizeRegenAttribute regenAttribute = (ProcGenMesh.BufferSizeRegenAttribute)attributes[0];

						// check if the attribute causes regen
						if (regenAttribute.CausesRegen)
						{
							regenMap.Add((VariableType)Enum.Parse(typeof(VariableType), field.Name), true);
						}
					}
					else if(attributes.Length > 1)
					{
						throw new Exception("I have no idea how you did this but there should never be more than one BufferSizeRegenAttribute on a field");
					}
					else
					{
						regenMap.Add((VariableType)Enum.Parse(typeof(VariableType), field.Name), false);
					}
				}

				hasMapInitialized = true;
			}
		}

		[SerializeField]
		ExtrusionTestVariables initialVariables;
		const bool useInitialVariables = true; // change this approach later in 'actual' classes
			// since those will use solos and possibly not even [serializefield] tagged structs

		ExtrusionTestVariables previousVariables;
		ExtrusionTestVariables currentVariables; // use this so we store variables
			// to allow for clients to set variables individually

		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		Mesh _mesh;

		EdgeLoop backLoop;
		EdgeLoop frontLoop;
		EdgeBridge backFrontBridge;
		LinearEdgeLoopFaceFill faceFill;
		Vector3[] vertices;
		int[] triangles;
		
		// debug stuff
		[Header("Debug Variables")]
		[SerializeField] bool drawLoops;
		[SerializeField] bool drawMesh;
		[SerializeField] int drawSegmentID = 0;

		private void Awake()
		{
			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();

			if(useInitialVariables)
			{
				currentVariables = initialVariables; // by setting initial to current instead of 
					// previous, we should get the expected update right away
			}
		}

		// Use this for initialization
		void Start()
		{
			if (_mesh == null) _mesh = new Mesh();
			_mesh.MarkDynamic();

			meshFilter.mesh = _mesh;
		}

		private void OnValidate()
		{
			drawSegmentID = Mathf.Clamp(drawSegmentID, 0, frontLoop.GetSegmentCount() - 1);
		}

		private void OnEnable()
		{
			
		}

		#region Generation & Update
		void GenerateMesh()
		{
			int baseID = 0;
			backLoop = ModelUtils.CreateEdgeLoop(ref baseID, 
				currentVariables.closeLoop,
				currentVariables.EdgeLoopVertCount);

			frontLoop = ModelUtils.CreateEdgeLoop(ref baseID, 
				currentVariables.closeLoop,
				currentVariables.EdgeLoopVertCount);

			vertices = new Vector3[backLoop.VertCount + frontLoop.VertCount];

			SetVertices();

			int triangleBaseID = 0;

			backFrontBridge = ModelUtils.CreateExtrustion(ref triangleBaseID,
				frontLoop, backLoop);

			bool shouldFillFace = currentVariables.closeLoop && currentVariables.fillFace;
			if (shouldFillFace)
			{
				faceFill = ModelUtils.CreateLinearFaceFill(ref triangleBaseID,
					frontLoop, 
					currentVariables.cornerVertCount, 
					currentVariables.widthVertCount);
			}

			int bridgeTriangleIndexCount = backFrontBridge.GetTriangleIndexCount();
			int faceFillTriangleIndexCount = faceFill.GetTriangleIndexCount();
			triangles = new int[bridgeTriangleIndexCount + faceFillTriangleIndexCount];
			backFrontBridge.TriangulateBridge(ref triangles, true);
			if (shouldFillFace) faceFill.TriangulateFace(ref triangles, false);

			if (_mesh == null) _mesh = new Mesh();
			_mesh.MarkDynamic();

			_mesh.vertices = vertices;
			_mesh.triangles = triangles;
			_mesh.RecalculateNormals();
			meshFilter.sharedMesh = _mesh;
		}

		void LoopSide(int baseID, bool isLeft, float depth, float sideRadius,
			int cornerVertCount, float width)
		{
			float angleIncrement = 180f / (((float)cornerVertCount * 2f) - 1);

			for (int i = 0; i < cornerVertCount * 2; i++)
			{
				float angle = angleIncrement * i;

				Vector3 vertex = Vector3.up * ((isLeft) ? sideRadius : -sideRadius);
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				vertex += Vector3.right * (width * ((isLeft) ? -0.5f : 0.5f));
				vertex += Vector3.forward * depth;
				vertices[baseID + i] = vertex;
			}
		}

		void LoopEdge(int baseID, bool isBottom, float depth, float sideRadius,
			int widthVertCount, float width)
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

			// this hacky weird setup gives us even distribution of the allocated points, without doubling up
			int iterator = 0;
			int totalVertCount = widthVertCount + 2;
			for (int i = 0; i < totalVertCount - 1; i++)
			{
				float tValue = Mathf.InverseLerp(0, totalVertCount - 1, i);
				if (i > 0 && i < totalVertCount - 1)
				{
					vertices[baseID + iterator] = Vector3.Lerp(startEdge, endEdge, tValue);
					iterator++;
				}
			}
		}

		void SetVertices()
		{
			int cornerVertCount = currentVariables.cornerVertCount;
			float radius = currentVariables.radius;
			int widthVertCount = currentVariables.widthVertCount;
			float extrusionDepth = currentVariables.extrusionDepth;
			float width = currentVariables.width;

			LoopSide(backLoop.VertexBaseID, true, 0, radius, cornerVertCount, width);
			LoopEdge(backLoop.VertexBaseID + cornerVertCount * 2, true, 0, radius, widthVertCount, width);
			LoopSide((backLoop.VertexBaseID + cornerVertCount * 2) + widthVertCount, false, 0, radius, cornerVertCount, width);
			LoopEdge((backLoop.VertexBaseID + cornerVertCount * 4) + widthVertCount, false, 0, radius, widthVertCount, width);

			LoopSide(frontLoop.VertexBaseID, true, extrusionDepth, radius, cornerVertCount, width);
			LoopEdge(frontLoop.VertexBaseID + cornerVertCount * 2, true, extrusionDepth, radius, widthVertCount, width);
			LoopSide((frontLoop.VertexBaseID + cornerVertCount * 2) + widthVertCount, false, extrusionDepth, radius, cornerVertCount, width);
			LoopEdge((frontLoop.VertexBaseID + cornerVertCount * 4) + widthVertCount, false, extrusionDepth, radius, widthVertCount, width);
		}
		#endregion

		#region Updating
		#region External Update Intake
		public void BatchSet(ExtrusionTestVariables newVariables)
		{
			currentVariables.extrusionDepth = newVariables.extrusionDepth;
			currentVariables.radius = newVariables.radius;
			currentVariables.closeLoop = newVariables.closeLoop;
			currentVariables.fillFace = newVariables.fillFace;
			currentVariables.cornerVertCount = newVariables.cornerVertCount;
			currentVariables.widthVertCount = newVariables.widthVertCount;
			currentVariables.width = newVariables.width;
		}

		public void SetExtrusionDepth(float newDepth)
		{
			currentVariables.extrusionDepth = newDepth;
		}
		public void SetRadius(float newRadius)
		{
			currentVariables.radius = newRadius;
		}
		public void SetCloseLoop(bool newCloseLoop)
		{
			currentVariables.closeLoop = newCloseLoop;
		}
		public void SetFillFace(bool newFillFace)
		{
			currentVariables.fillFace = newFillFace;
		}
		public void SetCornerVertCount(int newCount)
		{
			currentVariables.cornerVertCount = newCount;
		}
		public void SetWidthVertCount(int newCount)
		{
			currentVariables.widthVertCount = newCount;
		}
		public void SetWidth(float newWidth)
		{
			currentVariables.width = newWidth;
		}
		#endregion

		// Update is called once per frame
		void Update()
		{
			// todo: not sure how but we need to make sure that we only run this after
			// all potential manipulating external classes have submitted their
			// updates
			
			// compare current variables to previous variables
			bool regenNeeded = false;
			bool refillNeeded = false;

			for (int i = 0; i < (int)ExtrusionTestVariables.VariableType.Count; i++)
			{
				ExtrusionTestVariables.VariableType type = (ExtrusionTestVariables.VariableType)i;

				ValueType currentVariable = currentVariables[i];
				ValueType previousVariable = previousVariables[i];
				
				bool variableChanged = !ValueType.Equals(currentVariable, previousVariable); // don't compare as reference types

				if (variableChanged)
				{
					// check our map for the correct type of regen
					bool typeHasRegenAttr = ExtrusionTestVariables.GetRegenAttribute(type);
					if (typeHasRegenAttr)
					{
						regenNeeded = true;
						Debug.Log(string.Format("Field {0} indicated full mesh regen",
							type.ToString()));
					}
					else
					{
						refillNeeded = true;
						Debug.Log(string.Format("Field {0} indicated buffer refill",
							type.ToString()));
					}

					if (regenNeeded && refillNeeded) break;
				}
			}

			if (regenNeeded)
			{
				Debug.Log("property update required full mesh regen"); // these may eventually cause perf issues
				GenerateMesh();
			}
			else if (refillNeeded)
			{
				Debug.Log("Property update required buffer refill");
				SetVertices();
			}

			previousVariables = currentVariables;
		}

		#endregion

		void DrawLoopWithSegment(EdgeLoop loop, ExtrusionTestVariables varsTouse)
		{
			int loopSegmentCount = loop.GetSegmentCount();
			int cornerVertCount = varsTouse.cornerVertCount;
			int widthVertCount = varsTouse.widthVertCount;

			// need to find the index of our bisection plane
			// first one is corner count, second one is cornercount * 3 + width count
			int bisectFirst = cornerVertCount;
			int bisectSecond = (cornerVertCount * 3) + widthVertCount;

			int bufferBisectFirst = loop.VertexBaseID + bisectFirst;
			int bufferBisectSecond = loop.VertexBaseID + bisectSecond;

			Gizmos.color = Color.red;
			Gizmos.DrawLine(vertices[bufferBisectFirst], vertices[bufferBisectSecond]);

			int nextBisect = 0;
			nextBisect = (drawSegmentID < bisectFirst) ? bisectFirst : bisectSecond;
			// get our distance to the bisect. Subtraction
			// then, apply that as an offset. Use mathf.repeat
			int offset = nextBisect - (drawSegmentID + 2);
			int adjacentSegment = nextBisect + offset;

			// ok so I think we can figure out if we've been processed already.
			// segment ID above first bisect, less than first bisect * 2?
			// segment ID above second bisect but below second bisect + bisectfirst?
			bool isInFirstRange(int id)
			{
				return (id < bisectFirst - 1);
			}

			bool isInSecondRange(int id)
			{
				return (id > bisectSecond - 1);
			}

			bool inFirstRange = isInFirstRange(drawSegmentID);
			bool inSecondRange = isInSecondRange(drawSegmentID);
			bool hasProcessedAlready = !inFirstRange && !inSecondRange;

			for (int i = 0; i < loopSegmentCount; i++)
			{
				int currentVert = 0;
				int nextVert = 0;

				loop.GetVertsForSegment(i, out currentVert, out nextVert);

				int currentBufferID = loop.GetBufferIndexForVertIndex(currentVert);
				int nextBufferID = loop.GetBufferIndexForVertIndex(nextVert);

				Gizmos.color = (i == drawSegmentID) ? Color.blue : Color.green;
				if (i == adjacentSegment) Gizmos.color = Color.yellow;
				if (i == drawSegmentID && hasProcessedAlready) Gizmos.color = Color.red;
				Gizmos.DrawLine(vertices[currentBufferID], vertices[nextBufferID]);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if(drawLoops)
			{
				/*Gizmos.color = Color.blue;
				ModelUtils.DrawEdgeLoopGizmo(vertices, frontLoop);

				Gizmos.color = Color.yellow;
				ModelUtils.DrawEdgeLoopGizmo(vertices, backLoop);*/

				DrawLoopWithSegment(frontLoop, (Application.isPlaying) ? initialVariables : currentVariables);
			}

			if(drawMesh && Application.isPlaying)
			{
				Gizmos.color = Color.green;
				ModelUtils.DrawMesh(vertices, triangles);
			}
		}
	}
}
#endif