using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental.Interaction.Feedback
{
    public class SquashFeedback : MonoBehaviour
    {
        InteractiveItem item;
        SphereCollider sphereCollider; // we'll use this for our squash start distance at first
                                       // might need to come up with a different way of specifying it later
                                       // for things like the pinch cone

        float squashStartDistance = 0.05f;
        [SerializeField] Transform target;
        Vector3 startScale;

        [Range(0, 0.025f)]
        [SerializeField] float minSquishDistance = 0.01f;
        [Range(0f, 1f)]
        [SerializeField] float minFloatScale = 0.45f;

        [SerializeField] AnimationCurve squishBlend = AnimationCurve.Linear(0, 0, 1, 1);

		private void Awake()
		{
            item = GetComponent<InteractiveItem>();
            sphereCollider = GetComponent<SphereCollider>();

            if (sphereCollider) squashStartDistance = sphereCollider.radius;
            startScale = target.localScale;
		}

		// Start is called before the first frame update
		void Start()
        {

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
            float zScaleUniform = Mathf.SmoothStep(minFloatScale, 1f, scaleFactor);

            // Interpolate between the two zScale values using smoothstep
            float zScale = Mathf.Lerp(zScaleVol, zScaleUniform, squishBlend.Evaluate(t)); //Mathf.SmoothStep(0f, 1f, t)

            Vector3 scale = new Vector3(xScale, yScale, zScale);

            target.transform.localScale = Vector3.Scale(scale, startScale);
        }

        // Update is called once per frame
        void Update()
        {
            if(item.IsGrasped)
			{
                float scale = Mathf.InverseLerp(minSquishDistance, squashStartDistance,
                    Mathf.Abs(item.CurrentGraspDistance)); // grasp distance will be negative when in
                        // grasping mode, positive when in near grasp mode

                SetScaleFactor(scale);
			}
            else
			{
                SetScaleFactor(1);
			}
        }
    }
}