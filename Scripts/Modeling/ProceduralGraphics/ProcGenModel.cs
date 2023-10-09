using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public abstract class ProcGenModel : MonoBehaviour
    {
        public delegate void ModelPropertiesHandler(ProcGenModel sender);
        public event ModelPropertiesHandler PropertiesChanged;

        protected TransformSpace space;

        public virtual void OnValidate()
		{
            GenerateModel();
            SetPropertiesChanged();
		}

        public void SetSpace(TransformSpace space)
		{
            this.space = space;

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