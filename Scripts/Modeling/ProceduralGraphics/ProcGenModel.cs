using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Space;
using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Modeling.ProceduralGraphics
{
	public class ProcGenMesh
	{
		public enum BufferType
		{
			Unknown = -1,
			Verts = 0,
			Normals = 1,
			Triangles = 3,
			UV1 = 4
		}
#if UNITY
		// unity buffers
		private Vector2[] uvs;
		private Vector3[] verts;
		private Vector3[] normals;

		private Color[] colors;

		Mesh mesh;
		public Mesh UnityMesh { get { return mesh; } }
#elif STEREOKIT
			private Vertex[] meshVerts;

			Mesh mesh;
			public Mesh StereoKitMesh { get { return mesh; } }
#endif

		private int[] tris;

		private string name = "";

		public void SetName(string name)
		{
			this.name = name;
#if UNITY
			mesh.name = this.name;
#endif
		}

		public ProcGenMesh()
		{
#if UNITY
			mesh = new Mesh();
			mesh.MarkDynamic();
#elif STEREOKIT
			mesh = new Mesh();
#endif
		}

		public int GetVertexCount()
		{
#if UNITY
			return (verts != null) ? verts.Length : 0;
#elif STEREOKIT
			return (meshVerts != null) ? meshVerts.Length : 0;
#endif
		}

#if UNITY
		public Vector3 GetVertexAtIndex(int index)
		{
			return verts[index];
		}

		public Vector3 GetNormalAtIndex(int index)
		{
			return mesh.normals[index];
		}
#endif

		/// <summary>
		/// Clears the mesh, and starts fresh with the data you send it
		/// </summary>
		public void Create(Vect3[] verts, int[] tris,
			Vect3[] normals, Vect2[] uvs, ColorIC[] colors)
		{
#if UNITY
			this.mesh.Clear();
			this.verts = new Vector3[verts.Length];
			for(int v=0; v < verts.Length; v++)
			{
				this.verts[v] = (Vector3)verts[v];
			}
			mesh.vertices = this.verts;
			mesh.RecalculateBounds();

			this.tris = new int[tris.Length];
			for (int t = 0; t < tris.Length; t++)
			{
				this.tris[t] = tris[t];
			}

			mesh.triangles = this.tris;

			if (normals != null && normals.Length > 0)
			{
				this.normals = new Vector3[normals.Length];
				for(int n=0; n < normals.Length; n++)
				{
					this.normals[n] = (Vector3)normals[n];
				}

				mesh.normals = this.normals;
			}
			else
			{
				this.normals = new Vector3[0];
				mesh.normals = null;
				mesh.RecalculateNormals();
			}

			if(uvs != null && uvs.Length > 0)
			{
				this.uvs = new Vector2[uvs.Length];

				for(int uv=0; uv < uvs.Length; uv++)
				{
					this.uvs[uv] = (Vector2)uvs[uv];
				}

				mesh.uv = this.uvs;
			}
			else
			{
				this.uvs = new Vector2[0];
				mesh.uv = null;
			}

			if(colors != null && colors.Length > 0)
			{
				this.colors = new Color[colors.Length];

				for(int c=0; c < colors.Length; c++)
				{
					this.colors[c] = (Color)colors[c];
				}

				mesh.colors = this.colors;
			}
			else
			{
				this.colors = new Color[0];
				mesh.colors = null;
			}
#elif STEREOKIT
			meshVerts = new Vertex[verts.Length];

			for(int v=0; v < verts.Length; v++)
			{
				meshVerts[v].pos = (Vect3)verts[v];
			}

			if(normals != null && normals.Length > 0)
			{
				for(int n=0; n < normals.length; n++)
				{
					meshVerts[n].norm = (Vect3)normals[n];
				}
			}

			if(uvs != null && uvs.Length > 0)
			{
				for(int uv=0; uv < uvs.Length; uv++)
				{
					meshVerts[uv].uv (Vec2)uvs[uv];
				}
			}

			if(colors != null && colors.Length > 0)
			{
				for(int c=0; c < colors.Length; c++)
				{
					meshVerts[c].col = (Color32)colors[c];
				}
			}

			mesh.SetVerts(meshVerts);
			mesh.SetInds(tris);
#endif
		}

		/// <summary>
		///  Updates an existing mesh. If the buffers you send don't match
		/// the size of the existing data, unknown bad things can happen!
		/// </summary>
		/// <param name="verts">vertices to update (can be null to skip)</param>
		/// <param name="normals">normals to update (can be null to skip)</param>
		/// <param name="uvs">uvs to update (can be null to skip)</param>
		/// <param name="tris">triangle indices to update (you probably don't want this)</param>
		/// <param name="colors">colors to update (can be null to skip)</param>
		/// <param name="recalculateBounds">if vertices are supplied and this value is true, recalculates the mesh bounds</param>
		/// <param name="recalculateNormals">if no normals are supplied, use this if you want them to be recalculated</param>
		public void Update(Vect3[] verts, Vect3[] normals, Vect2[] uvs,
			int[] tris, ColorIC[] colors, bool recalculateBounds, bool recalculateNormals)
		{
#if UNITY
			if (verts != null && verts.Length > 0)
			{
				for (int v = 0; v < verts.Length; v++)
				{
					this.verts[v] = (Vector3)verts[v];
				}
				mesh.vertices = this.verts;
				if (recalculateBounds) mesh.RecalculateBounds();
			}

			if (tris != null && tris.Length > 0)
			{
				for (int t = 0; t < tris.Length; t++)
				{
					this.tris[t] = tris[t];
				}

				mesh.triangles = this.tris;
			}

			if (normals != null && normals.Length > 0)
			{
				for (int n = 0; n < normals.Length; n++)
				{
					this.normals[n] = (Vector3)normals[n];
				}

				mesh.normals = this.normals;
			}
			else
			{
				mesh.normals = null;
				if(recalculateNormals) mesh.RecalculateNormals();
			}

			if (uvs != null && uvs.Length > 0)
			{
				for (int uv = 0; uv < uvs.Length; uv++)
				{
					this.uvs[uv] = (Vector2)uvs[uv];
				}

				mesh.uv = this.uvs;
			}
			else
			{
				this.uvs = new Vector2[0];
				mesh.uv = null;
			}

			if (colors != null && colors.Length > 0)
			{
				for (int c = 0; c < colors.Length; c++)
				{
					this.colors[c] = (Color)colors[c];
				}

				mesh.colors = this.colors;
			}
			else
			{
				this.colors = new Color[0];
				mesh.colors = null;
			}
#elif STEREOKIT
			for(int v=0; v < verts.Length; v++)
			{
				meshVerts[v].pos = (Vect3)verts[v];
			}

			if(normals != null && normals.Length > 0)
			{
				for(int n=0; n < normals.length; n++)
				{
					meshVerts[n].norm = (Vect3)normals[n];
				}
			}

			if(uvs != null && uvs.Length > 0)
			{
				for(int uv=0; uv < uvs.Length; uv++)
				{
					meshVerts[uv].uv (Vec2)uvs[uv];
				}
			}

			if(colors != null && colors.Length > 0)
			{
				for(int c=0; c < colors.Length; c++)
				{
					meshVerts[c].col = (Color32)colors[c];
				}
			}

			mesh.SetVerts(meshVerts);
			if (tris != null && tris.Length > 0)
			{
				mesh.SetInds(tris);
			}
#endif
		}
	}

	public abstract class ProcGenModel : MonoBehaviour
    {
        public delegate void ModelPropertiesHandler(ProcGenModel sender);
        public event ModelPropertiesHandler PropertiesChanged;

        protected SpaceItem spaceItem;
        TransformSpace _space;
        protected TransformSpace space { get { return (spaceItem) ? spaceItem.CurrentSpace : _space; } }

        public virtual void Awake()
		{
            spaceItem = GetComponentInParent<SpaceItem>();

            if (!spaceItem)
            {
                TransformSpace spaceInParent = GetComponentInParent<TransformSpace>();
                if (spaceInParent) SetSpace(spaceInParent);
            }
		}

        public virtual void Start()
		{

		}

        public virtual void OnValidate()
		{
            GenerateModel();
            SetPropertiesChanged();
		}

        public void SetSpace(TransformSpace space)
		{
            // hook this into space item change event?
            _space = space;

            // we need to update the verts.
            // not sure if there's a good way of doing this across all models rn
            // we can obviously regenerate the model
            // but that might be slow if we re-do all of it,
            // want to selectively update the verts
		}

        protected Vector3 WarpVertex(Vector3 vertex, Transform meshTransform)
		{
            if(!space) return vertex;
            else
			{
                // get the world space rectilinear transform of the vertex
                // we can probably avoid going all the way up to global space
                // with transform point by doing a stack walk,
                // although I should profile the difference.
                vertex = meshTransform.TransformPoint(vertex);
                vertex = space.transform.InverseTransformPoint(vertex);
                vertex = space.TransformPoint(vertex); // this is where we do our rect 2 warp
                vertex = space.transform.TransformPoint(vertex);
                vertex = meshTransform.InverseTransformPoint(vertex);
                return vertex;
			}
		}

        protected void SetPropertiesChanged()
        {
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        public abstract void GenerateModel();
    }
}