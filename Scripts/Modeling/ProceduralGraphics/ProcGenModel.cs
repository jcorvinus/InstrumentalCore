using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public abstract class ProcGenModel : MonoBehaviour
    {
        public delegate void ModelPropertiesHandler(ProcGenModel sender);
        public event ModelPropertiesHandler PropertiesChanged;

        public virtual void OnValidate()
		{
            GenerateModel();
            SetPropertiesChanged();
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