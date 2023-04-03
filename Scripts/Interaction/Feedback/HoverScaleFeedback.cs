using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Feedback
{
    public class HoverScaleFeedback : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float minScale = 0;
        [SerializeField] float maxScale = 1;
        [SerializeField] GameObject optionalDisableOnMax;
        GraspableItem item;

		private void Awake()
		{
            item = GetComponent<GraspableItem>();
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // todo: figure out what to do when 
            // !hovering
            // grasping
            float scale = Mathf.Lerp(minScale, maxScale, item.HoverTValue);
            target.transform.localScale = Vector3.one * scale;
        }
    }
}