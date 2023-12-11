using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Input
{
    public class HandPointer : MonoBehaviour
    {
        MeshRenderer pinchConeRenderer;
        [SerializeField] bool isLeft = false;

		private void Awake()
		{
            pinchConeRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}