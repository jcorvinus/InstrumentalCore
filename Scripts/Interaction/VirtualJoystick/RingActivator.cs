using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class RingActivator : MonoBehaviour
    {
        [System.Serializable]
        public struct ClickState
        {
            public bool InRegion;
            public float InPlaneDistance;
            public float HeightValue; // negative means we are on the valid side for activation
            public bool OnCorrectSide;
        }

        public System.Action Activated;

        [Range(0, 1)]
        [SerializeField] float radius = 0.1f;
        [SerializeField] float verticalOffset = 0.1f;
        [SerializeField] float outerRingOffset = 0.05f;
        [SerializeField] float dampAmount = 6f;
        Vector3 innerRingVelocity;
        GameObject outerTarget;

        [SerializeField] AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Range(0.1f, 0.3f)]
        [SerializeField]
        float introAnimDuration = 0.15f;
        [Range(0, 0.2f)]
        [SerializeField]
        float innerRingTimeOffset = 0.05f;
        float enabledTime = 0;
        ClickState currentState;

        [Range(0, 1f)]
        [SerializeField] float scale = 1f;

        // components
        [SerializeField] MeshRenderer innerRing;
        [SerializeField] MeshRenderer outerRing;
        Color outerRingDefaultGlowColor;
        int emissionHash;

        AudioSource audioSource;

        public float Radius { get { return radius; } }

		private void Awake()
		{
            audioSource = GetComponent<AudioSource>();
            outerTarget = new GameObject("InnerTarget");
            outerTarget.transform.parent = transform;
            outerRing.transform.parent = null;

            emissionHash = Shader.PropertyToID("_EmissionColor");
            outerRingDefaultGlowColor = outerRing.material.GetColor(emissionHash);
		}

		// Start is called before the first frame update
		void Start()
        {

        }

		private void OnEnable()
		{
            enabledTime = 0;
            outerRing.enabled = true;
            outerRing.transform.localScale = Vector3.zero;
            innerRing.enabled = true;
            innerRing.transform.localScale = Vector3.zero;
            outerRing.transform.position = transform.position;

			currentState = new ClickState()
			{ 
                HeightValue = (outerRingOffset),
                InPlaneDistance = 0,
                InRegion = true,
                OnCorrectSide = true
            };
		}

		private void OnDisable()
		{
            enabledTime = 0;
            if(innerRing) innerRing.enabled = false;
            if(outerRing) outerRing.enabled = false;
		}

		// Update is called once per frame
		void Update()
        {
            // doing local here so we can take advantage of parenting
            // and not get glitchy poses
            Vector3 outerRingPoint = (Vector3.up * (verticalOffset + outerRingOffset));
            Vector3 innerRingPoint = (Vector3.up * verticalOffset);

            if (enabledTime < introAnimDuration)
			{
                enabledTime += Time.deltaTime;

                float outerTValue = Mathf.InverseLerp(0, introAnimDuration, enabledTime);
                float innerTValue = Mathf.InverseLerp(innerRingTimeOffset, introAnimDuration, enabledTime);

                outerRing.transform.position = transform.TransformPoint(Vector3.Lerp(Vector3.zero, outerRingPoint, outerTValue));
                outerRing.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * scale, scaleCurve.Evaluate(outerTValue));
                outerRing.transform.rotation = innerRing.transform.rotation;

                innerRing.transform.localPosition = Vector3.Lerp(Vector3.zero, innerRingPoint, innerTValue);
                innerRing.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * scale, scaleCurve.Evaluate(outerTValue));
                outerTarget.transform.SetPositionAndRotation(innerRing.transform.position, innerRing.transform.rotation);
            }
            else
			{
                innerRing.transform.localPosition = innerRingPoint;
                outerTarget.transform.position = transform.TransformPoint(outerRingPoint);
                outerRing.transform.position = Vector3.SmoothDamp(outerRing.transform.position, 
                    outerTarget.transform.position, 
                    ref innerRingVelocity, dampAmount);
                outerRing.transform.rotation = innerRing.transform.rotation;

                // get our is in region
                Vector3 outerTransformedToInnerSpace = innerRing.transform.InverseTransformPoint(outerRing.transform.position);
                outerTransformedToInnerSpace = new Vector3(outerTransformedToInnerSpace.x, outerTransformedToInnerSpace.z, outerTransformedToInnerSpace.y);
                Vector3 transformedInPlane = new Vector3(outerTransformedToInnerSpace.x, 0, outerTransformedToInnerSpace.z);
                float y = outerTransformedToInnerSpace.y;
                float distance = transformedInPlane.magnitude;

                bool inRegion = transformedInPlane.sqrMagnitude < radius;

                ClickState newClickState = new ClickState()
				{
					HeightValue = y,
                    InPlaneDistance = distance,
                    OnCorrectSide = y < 0,
                    InRegion = inRegion
                };

                // update our clickstate
                if(newClickState.OnCorrectSide && !currentState.OnCorrectSide)
				{
                    // we've gone from one side to the other
                    if(newClickState.InPlaneDistance < radius &&
                        currentState.InPlaneDistance < radius)
					{
                        Activate(); // undo this later when we're good with the behavior
					}
				}

                Color emissionColor = (newClickState.OnCorrectSide) ? Color.cyan : outerRingDefaultGlowColor;
                outerRing.material.SetColor(emissionHash, emissionColor);

                currentState = newClickState;
			}
        }

        void Activate()
		{
            if (Activated != null)
            {
                Activated();
            }

            audioSource.Play();
            enabled = false;
        }

        public Vector3 GetChildSpawnPosition()
        {
            return innerRing.transform.position;
        }

        private void OnDrawGizmos()
		{
            Vector3 outerRingPoint = (Vector3.up * (verticalOffset + outerRingOffset));
            Vector3 innerRingPoint = (Vector3.up * verticalOffset);

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            DebugExtension.DrawCircle(outerRingPoint, radius * scale);
            DebugExtension.DrawCircle(innerRingPoint, radius * scale);
            Gizmos.matrix = Matrix4x4.identity;
        }
	}
}