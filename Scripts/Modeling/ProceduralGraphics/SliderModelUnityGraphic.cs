using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public class SliderModelUnityGraphic : MonoBehaviour
    {
        Slider slider;
        SliderModel sliderModel;

        MeshFilter faceMeshFilter;
        MeshRenderer faceMeshRenderer;
        MeshFilter railMeshFilter;
        MeshRenderer railMeshRenderer;

        MaterialPropertyBlock faceMeshPropertyBlock;

        int glowAmountHash;
        int isPressingHash;
        int isGraspingHash;
        int isTouchingHash;
        int isHoveringHash;
        int useDistanceGlowHash;

        bool hasComponents = false;

        // Start is called before the first frame update
        void Start()
        {
            glowAmountHash = Shader.PropertyToID("_GlowAmount");
            isPressingHash = Shader.PropertyToID("_IsPressing");
            isGraspingHash = Shader.PropertyToID("_IsGrasping");
            isHoveringHash = Shader.PropertyToID("_IsHovering");
            isTouchingHash = Shader.PropertyToID("_IsTouching");
            useDistanceGlowHash = Shader.PropertyToID("_UseDistanceGlow");

            faceMeshPropertyBlock = new MaterialPropertyBlock();

            AcquireComponents();
        }

		private void OnValidate()
		{
            Regenerate();
		}

		void AcquireComponents()
		{
            if(!hasComponents)
			{
                slider = GetComponent<Slider>();
                sliderModel = GetComponent<SliderModel>();
                faceMeshFilter = transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>();
                faceMeshRenderer = faceMeshFilter.GetComponent<MeshRenderer>();
                railMeshFilter = transform.GetChild(1).GetComponent<MeshFilter>();
                railMeshRenderer = railMeshFilter.GetComponent<MeshRenderer>();
                hasComponents = true;
			}
		}

        void Regenerate()
		{
            AcquireComponents();
            faceMeshFilter.sharedMesh = sliderModel.FaceMesh;
            railMeshFilter.sharedMesh = sliderModel.RailMesh;
		}

        // Update is called once per frame
        void Update()
        {
            faceMeshRenderer.GetPropertyBlock(faceMeshPropertyBlock);

            faceMeshPropertyBlock.SetInteger(useDistanceGlowHash, 1); // I just realized that if I wanted to get
            faceMeshPropertyBlock.SetInteger(isPressingHash, slider.IsPressed ? 1 : 0); // really crazy
            faceMeshPropertyBlock.SetInteger(isHoveringHash, slider.IsHovering ? 1 : 0); // I could bitpack
            faceMeshPropertyBlock.SetInteger(isTouchingHash, slider.IsTouching ? 1 : 0); // these bools into a single integer

            faceMeshRenderer.SetPropertyBlock(faceMeshPropertyBlock);
        }
    }
}