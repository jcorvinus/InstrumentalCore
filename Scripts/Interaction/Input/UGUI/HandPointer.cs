using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Input
{
    public class HandPointer : MonoBehaviour
    {
        InstrumentalBody body;
        MeshRenderer pinchConeRenderer;
        int pinchDiffuseHash;
        int pinchEmissHash;
        Color defaultColor;
        Color defaultEmissionColor;
        [SerializeField] bool isLeft = false;

        [Range(0, 0.025f)]
        [SerializeField] float minSquishDistance = 0.01f;
        [Range(0f, 1f)]
        [SerializeField] float minFloatScale = 0.45f;
        float squashStartDistance = 0.05f;

        [SerializeField] AnimationCurve squishBlend = AnimationCurve.Linear(0, 0, 1, 1);

        private void Awake()
		{
            pinchConeRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            pinchDiffuseHash = Shader.PropertyToID("_Color");
            pinchEmissHash = Shader.PropertyToID("_EmissionColor");
            defaultColor = pinchConeRenderer.material.GetColor(pinchDiffuseHash);
            defaultEmissionColor = pinchConeRenderer.material.GetColor(pinchEmissHash);
		}

		// Start is called before the first frame update
		void Start()
        {
            body = InstrumentalBody.Instance;
        }

        void SetScaleFactor(float scaleFactor)
        {
            // Calculate how far scaleFactor is between 1 and 0.5
            float t = Mathf.InverseLerp(1f, 0.5f, scaleFactor);

            // Smoothstep the scale values to the limit defined by minFloatScale
            float xScale = Mathf.SmoothStep(minFloatScale, 1f, scaleFactor);
            float yScale = Mathf.SmoothStep(minFloatScale, 1f, scaleFactor);

            // Calculate zScale for volume preservation
            float zScaleVol = Mathf.Pow(1f / (Mathf.Pow(xScale, 2)), 1f / 3f);

            // Smoothstep zScale equal to scaleFactor
            //float zScaleUniform = Mathf.SmoothStep(minFloatScale, 1f, scaleFactor);

            // Interpolate between the two zScale values using smoothstep
            //float zScale = Mathf.Lerp(zScaleVol, zScaleUniform, squishBlend.Evaluate(t)); //Mathf.SmoothStep(0f, 1f, t)

            Vector3 scale = new Vector3(xScale, yScale, zScaleVol);

            pinchConeRenderer.transform.localScale = Vector3.Scale(scale, Vector3.one);
        }

        // Update is called once per frame
        void Update()
        {
            InstrumentalHand hand = (isLeft) ? InstrumentalHand.LeftHand : InstrumentalHand.RightHand;
            Ray handRay = (isLeft) ? body.LeftHandRay : body.RightHandRay;
            Pose palmPose = hand.GetAnchorPose(AnchorPoint.Palm);

            bool active = (hand.IsTracking); // todo: add palm direction filtering here

            if(active)
			{
                // set our transform position and rotation to that of the ray
                transform.SetPositionAndRotation(handRay.origin, Quaternion.LookRotation(handRay.direction, 
                    palmPose.rotation * Vector3.back));

                // enable pinch cone
                pinchConeRenderer.enabled = true;

                PinchInfo indexPinch = hand.GetPinchInfo(Finger.Index);

                // animate pinch cone
                float scale = Mathf.InverseLerp(minSquishDistance, squashStartDistance,
                    indexPinch.PinchDistance);
                float invScale = 1 - scale;
                SetScaleFactor(scale);

                pinchConeRenderer.material.SetColor(pinchDiffuseHash, Color.Lerp(defaultColor, Color.white, invScale));
                pinchConeRenderer.material.SetColor(pinchEmissHash, Color.Lerp(defaultEmissionColor, Color.white, invScale));
			}
            else
			{
                // disable pinch cone
                pinchConeRenderer.enabled = false;
			}
        }
    }
}