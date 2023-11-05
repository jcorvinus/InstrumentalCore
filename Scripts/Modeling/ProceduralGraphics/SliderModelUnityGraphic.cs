using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public class SliderModelUnityGraphic : MonoBehaviour
    {
        SliderModel sliderModel;

        MeshFilter faceMeshFilter;
        MeshRenderer faceMeshRenderer;

        bool hasComponents = false;

        // Start is called before the first frame update
        void Start()
        {

        }

		private void OnValidate()
		{
            Regenerate();
		}

		void AcquireComponents()
		{
            if(!hasComponents)
			{
                sliderModel = GetComponent<SliderModel>();
                faceMeshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
                faceMeshRenderer = faceMeshFilter.GetComponent<MeshRenderer>();
                hasComponents = true;
			}
		}

        void Regenerate()
		{
            AcquireComponents();
            faceMeshFilter.sharedMesh = sliderModel.FaceMesh;
		}

        // Update is called once per frame
        void Update()
        {

        }
    }
}