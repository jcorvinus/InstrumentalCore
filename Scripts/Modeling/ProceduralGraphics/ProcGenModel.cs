using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public abstract class ProcGenModel : MonoBehaviour
    {
        public delegate void ModelPropertiesHandler(ProcGenModel sender);
        public event ModelPropertiesHandler PropertiesChanged;

        // Start is called before the first frame update
        void Start()
        {

        }

        protected void SetPropertiesChanged()
        {
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        public abstract void GenerateModel();

        // Update is called once per frame
        void Update()
        {

        }
    }
}