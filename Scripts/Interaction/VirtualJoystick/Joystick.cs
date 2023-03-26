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

		private void Awake()
		{
            joystickMasterControl = GetComponentInParent<LeftMasterJoystick>();
            linePositions = new Vector3[2];
		}

		private void OnDisable()
		{
			
		}

		// Start is called before the first frame update
		void Start()
        {
        
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
        }
    }
}