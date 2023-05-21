using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental.Interaction.Feedback
{
    public class SquashFeedback : MonoBehaviour
    {
        GraspableItem item;
        SphereCollider sphereCollider; // we'll use this for our squash start distance at first
                                       // might need to come up with a different way of specifying it later
                                       // for things like the pinch cone

        float squashStartDistance = 0.05f;
        [SerializeField] Transform target;
        Vector3 startScale;

        [Range(0, 0.025f)]
        [SerializeField] float minSquishDistance = 0.01f;
        [Range(0.5f, 1f)]
        [SerializeField] float minFloatScale = 0.45f;

		private void Awake()
		{
            item = GetComponent<GraspableItem>();
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
            scaleFactor = Mathf.Clamp(scaleFactor, minFloatScale,
                1);

            float xScale = 1f - (1f - scaleFactor) * 2f;
            float yScale = 1f - (1f - scaleFactor) * 2f;
            float zScale = 1f / (xScale * yScale);
            Vector3 scale = new Vector3(xScale, yScale, zScale);

            target.transform.localScale = Vector3.Scale(scale, startScale);
        }

        // Update is called once per frame
        void Update()
        {
            if(item.IsGrasped)
			{
                float scale = 1 - Mathf.InverseLerp(minSquishDistance, squashStartDistance,
                    Mathf.Abs(item.GraspDistance)); // grasp distance will be negative when in
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