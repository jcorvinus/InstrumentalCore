using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental.Interaction.Feedback
{
    public class HoverMaterialFeedback : MonoBehaviour
    {
        [SerializeField] Color noInteractColor = Color.white;
        [SerializeField] Color hoverColor = Color.cyan;
        [SerializeField] Color graspColor = Color.green;

        GraspableItem item;

        MeshRenderer meshRenderer;

		private void Awake()
		{
            item = GetComponent<GraspableItem>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Color _hoverColor = Color.Lerp(noInteractColor, hoverColor, item.HoverTValue);
            Color graspColor = Color.green;
            Color color = noInteractColor;

            if (item.IsGrasped) color = graspColor;
            else if (item.IsHovering) color = _hoverColor;

            meshRenderer.material.color = color;
        }
    }
}