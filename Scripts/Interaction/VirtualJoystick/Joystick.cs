using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.VirtualJoystick
{ 
    public class Joystick : MonoBehaviour
    {
        LeftMasterJoystick joystickMasterControl;
        [SerializeField] GraspableItem bulb;
        [SerializeField] MeshRenderer outerCylinder;
        [SerializeField] LineRenderer lineRenderer;
        Vector3[] linePositions;

        /// <summary>
        /// Forward this to input simulator so that it knows
        /// when the virtual touchpad should be 'touched'
        /// </summary>
        public bool InputActive { get { return bulb.IsGrasped; } }

        private float signifierValue=1;
        public float SignifierValue { set { signifierValue = value; } }

		private void Awake()
		{
            joystickMasterControl = GetComponentInParent<LeftMasterJoystick>();
            linePositions = new Vector3[2];
		}

		// Start is called before the first frame update
		void Start()
        {
        
        }

		private void OnEnable()
		{
            signifierValue = 1;
		}

		private void OnDisable()
        {
            signifierValue = 1;
        }

        void UpdateGraspable()
		{
            // maybe constraint goes here? idk
            if (!bulb.IsGrasped)
            {
                //bulb.RigidBody.position = transform.position;
                bulb.transform.position = transform.position;
            }
		}

		private void FixedUpdate()
		{
            UpdateGraspable();
		}

		void UpdateLineRenderer()
		{
            if(bulb.IsGrasped)
			{
                // draw the line
                linePositions[0] = bulb.transform.position;
                linePositions[1] = transform.position;
                lineRenderer.SetPositions(linePositions);
                lineRenderer.enabled = true;
			}
            else
			{
                lineRenderer.enabled = false;
			}

		}

        // Update is called once per frame
        void Update()
        {
            UpdateLineRenderer();

            if (bulb.IsGrasped) signifierValue = 1;
            else
			{
                outerCylinder.transform.localScale = Vector3.one * signifierValue;
			}

            if (signifierValue == 0) gameObject.SetActive(false);
        }
    }
}