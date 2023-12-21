using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Instrumental.Interaction.Input
{
	public enum PointerState
	{ 
        OnCanvas=0,
        OnElement=1,
        PinchCanvas=2,
        PinchControl=3,
        OffCanvas=4
    }

	public class HandPointer : MonoBehaviour
    {
        InstrumentalBody body;
        InstrumentalHand hand;

        #region Input Stuff
        [SerializeField] bool isLeft = false;
        private EventSystem eventSystem;
        private HandInputModule inputModule;

        private PointerState currentState;
        private PointerState previousState;
        private PointerEventData eventData;

        private Vector2 prevPosition;
        private Vector2 dragStartPosition;

        private GameObject previousObject;
        private GameObject currentObject;
        private GameObject currentDragObject;
        private GameObject hoverObject;

        private List<RaycastResult> raycastResults = new List<RaycastResult>();
		#endregion

		#region Feedback Stuff
		MeshRenderer pinchConeRenderer;
        int pinchDiffuseHash;
        int pinchEmissHash;
        Color defaultColor;
        Color defaultEmissionColor;

        [Range(0, 0.025f)]
        [SerializeField] float minSquishDistance = 0.01f;
        [Range(0f, 1f)]
        [SerializeField] float minFloatScale = 0.45f;
        float squashStartDistance = 0.05f;

        [SerializeField] AnimationCurve squishBlend = AnimationCurve.Linear(0, 0, 1, 1);

        LineRenderer lineRenderer;
        Vector3[] linePoints;
		#endregion

		private void Awake()
		{
            inputModule = GetComponentInParent<HandInputModule>();
            eventSystem = GetComponentInParent<EventSystem>();

			#region Feedback Stuff
			pinchConeRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            pinchDiffuseHash = Shader.PropertyToID("_Color");
            pinchEmissHash = Shader.PropertyToID("_EmissionColor");
            defaultColor = pinchConeRenderer.material.GetColor(pinchDiffuseHash);
            defaultEmissionColor = pinchConeRenderer.material.GetColor(pinchEmissHash);

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;

            linePoints = new Vector3[lineRenderer.positionCount];
			#endregion
		}

		// Start is called before the first frame update
		void Start()
        {
            body = InstrumentalBody.Instance;
            hand = (isLeft) ? InstrumentalHand.LeftHand : InstrumentalHand.RightHand;

            eventData = new PointerEventData(eventSystem);
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

            Vector3 scale = new Vector3(xScale, yScale, zScaleVol);

            pinchConeRenderer.transform.localScale = Vector3.Scale(scale, Vector3.one);
        }

        // keep track of this, we might need to de-conflict reasons for deactivation
        bool IsActive()
		{
            return hand.IsTracking;
		}

        // Update is called once per frame
        void Update()
        {
            Ray handRay = (isLeft) ? body.LeftHandRay : body.RightHandRay;
            Vector3 aimPosition = (isLeft) ? body.LeftAimPosition : body.RightAimPosition;
            Pose palmPose = hand.GetAnchorPose(AnchorPoint.Palm);

            bool active = IsActive(); // todo: add palm direction filtering here

            if(active)
			{
                // set our transform position and rotation to that of the ray
                pinchConeRenderer.transform.SetPositionAndRotation(aimPosition, Quaternion.LookRotation(handRay.direction, 
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

                // do our line renderer feedback
                lineRenderer.enabled = true;
                DoLine(aimPosition, handRay.direction, 0.5f);
			}
            else
			{
                // disable pinch cone
                pinchConeRenderer.enabled = false;
			}
        }

        void DoLine(Vector3 origin, Vector3 direction, float distance)
		{
            Vector3 startPoint = origin;
            Vector3 endPoint = origin + (direction * distance);

            for(int i = 0; i < lineRenderer.positionCount; i++)
			{
                if(i == 0)
				{
                    linePoints[0] = startPoint;
				}
                else if (i == lineRenderer.positionCount - 1)
				{
                    linePoints[lineRenderer.positionCount - 1] = endPoint;
				}
                else
				{
                    float tValue = Mathf.InverseLerp(0, lineRenderer.positionCount, i);
                    linePoints[i] = Vector3.Lerp(startPoint, endPoint, tValue);
				}
			}

            lineRenderer.SetPositions(linePoints);
		}

        bool DoRaycast()
		{
            eventData.Reset();
            eventData.button = PointerEventData.InputButton.Left;

            Ray handRay = (isLeft) ? body.LeftHandRay : body.RightHandRay;
            Vector3 aimPosition = (isLeft) ? body.LeftAimPosition : body.RightAimPosition;
            Vector3 localizedPosition = inputModule.ScreenCamera.transform.position - handRay.origin + aimPosition;

            eventData.position = inputModule.ScreenCamera.WorldToScreenPoint(localizedPosition);
            eventData.delta = eventData.position - prevPosition;
            eventData.scrollDelta = Vector2.zero;

            eventSystem.RaycastAll(eventData, raycastResults);
            eventData.pointerCurrentRaycast = HandInputModule.FindFirstRaycast(raycastResults);

            raycastResults.Clear();

            return eventData.pointerCurrentRaycast.gameObject != null;
		}

        void ProcessPointer(PointerEventData hitData)
		{
            GameObject pointerHitObject = eventData.pointerCurrentRaycast.gameObject;

            if(pointerHitObject)
			{
                RectTransform dragRef = eventData.pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();

                if(RectTransformUtility.ScreenPointToWorldPointInRectangle(dragRef,  hitData.position,
                    hitData.enterEventCamera, out Vector3 globalLookPosition))
				{
                    GameObject hoverObject = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(pointerHitObject);

                    if (hoverObject)
					{
                        Vector3 pointerInDragRef = hoverObject.transform.InverseTransformPoint(globalLookPosition);
                        pointerInDragRef = new Vector3(pointerInDragRef.x, pointerInDragRef.y, 0);
                        transform.position = hoverObject.transform.TransformPoint(pointerInDragRef);
					}
                    else
					{
                        transform.position = globalLookPosition - transform.forward * 0.01f;
					}

                    transform.rotation = dragRef.rotation;
				}
			}
		}

        void ProcessState()
		{
            if(eventData.pointerCurrentRaycast.gameObject)
			{
                PinchInfo indexPinch = hand.GetPinchInfo(Finger.Index);
                bool isPinching = indexPinch.PinchAmount < 0.01f;

                if (ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    eventData.pointerCurrentRaycast.gameObject))
				{
                    // get pointer state for pinching element or on element
                    currentState = isPinching ? PointerState.PinchControl : PointerState.OnElement;
				}
                else
				{
                    // do the same as above, but for the canvas
                    currentState = isPinching ? PointerState.PinchCanvas : PointerState.OnCanvas;
				}
			}
            else
            {
                currentState = PointerState.OffCanvas;
            }

            if (currentState != previousState)
            {
                // todo: dispatch pointer state change event
            }
        }

        public void Process()
		{
            // check our eligibility requirements
            if(IsActive())
			{
                bool hit = DoRaycast();

                previousState = currentState;
                ProcessPointer(eventData);
                ProcessState();
			}
		}
    }
}