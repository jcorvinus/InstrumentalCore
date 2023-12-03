using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Overlay;

namespace Instrumental.Interaction.VirtualJoystick
{ 
    public class Joystick : MonoBehaviour
    {
        public System.Action OnMoved;

        LeftMasterJoystick joystickMasterControl;
        [SerializeField] InteractiveItem bulb;
        [SerializeField] MeshRenderer outerCylinder;
        [SerializeField] LineRenderer lineRenderer;

        [Range(0, 1)]
        [SerializeField] float deadzone = 0.1f;

        Vector3[] linePositions;
        float ungraspTime = 0;
        const float returnToCenterDuration = 0.15f;
        Vector3 ungraspStartLocation;
        Transform headTransform;

        Vector2 joystickValue;

        [SerializeField] GameObject headRelativeVisualizer;

        /// <summary>
        /// Forward this to input simulator so that it knows
        /// when the virtual touchpad should be 'touched'
        /// </summary>
        public bool InputActive { get { return bulb.IsGrasped; } }

        private float signifierValue=1;
        public float SignifierValue { set { signifierValue = value; } }

        public Vector2 Value { get { return joystickValue; } }

		private void Awake()
		{
            joystickMasterControl = GetComponentInParent<LeftMasterJoystick>();
            linePositions = new Vector3[2];
		}

		// Start is called before the first frame update
		void Start()
        {
			bulb.OnUngrasped += Bulb_OnUngrasped;
            headTransform = InstrumentalBody.Instance.Head;
        }

		private void Bulb_OnUngrasped(InteractiveItem sender)
		{
            transform.position = bulb.transform.position;
            if (OnMoved != null) OnMoved();
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
                ungraspTime += Time.fixedDeltaTime;
                ungraspTime = Mathf.Clamp(ungraspTime, 0, returnToCenterDuration);

                float returnTValue = Mathf.InverseLerp(0, returnToCenterDuration,
                    ungraspTime);
                //bulb.transform.localPosition = Vector3.Lerp(ungraspStartLocation, Vector3.zero, returnTValue); // uncomment if we ever want to do our ungrasp return-to-center
                bulb.transform.localPosition = Vector3.zero;
                joystickValue = Vector2.zero;

                if(headRelativeVisualizer)
				{
                    headRelativeVisualizer.SetActive(false);
                }
            }
            else
			{
                ungraspTime = 0;
                ungraspStartLocation = bulb.transform.localPosition;

                // transform joystick into head space
                Vector3 rawDirection = (bulb.transform.position - transform.position);
                rawDirection = Vector3.Scale(rawDirection, new Vector3(1, 0, 1));
                float distance = rawDirection.magnitude;
                rawDirection /= distance;

                float normalizedDistance = Mathf.InverseLerp(0, joystickMasterControl.GetInnerRadius(),
                    distance);

                Vector3 forward = Vector3.forward;

                float absDeadzone = (joystickMasterControl.GetInnerRadius() * deadzone);
                if (distance > absDeadzone)
                {
                    Vector3 headForwardFlattened = Vector3.Scale(headTransform.forward, new Vector3(1, 0, 1));
                    Quaternion rotation = Quaternion.FromToRotation(headForwardFlattened, rawDirection);
                    forward = rotation * Vector3.forward;
                }

                joystickValue = new Vector2(forward.x, forward.z) * normalizedDistance;

                if(headRelativeVisualizer)
				{
                    Vector3 headLocal = new Vector3(joystickValue.x, joystickValue.y, 3);
                    headRelativeVisualizer.transform.position =
                        headTransform.TransformPoint(headLocal * 0.1f);
                    headRelativeVisualizer.SetActive(true);
				}
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

            if (bulb.IsGrasped)
            {
                signifierValue = 1;
            }
            else
            {
                outerCylinder.transform.localScale = Vector3.one * signifierValue;
            }

            if (signifierValue == 0) gameObject.SetActive(false);
        }
	}
}