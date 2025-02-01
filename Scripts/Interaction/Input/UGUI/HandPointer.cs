using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;
using Instrumental.Core.Math;

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

        UICursor cursor;

#region Input Stuff
        [SerializeField] bool isLeft = false;
        private EventSystem eventSystem;
        private HandInputModule inputModule;

        private PointerState currentState;
        private PointerState previousState;
        private PointerEventData eventData;

        private Vector2 prevPosition;
        private Vector2 dragStartPosition;

        private GameObject previousHoverObject;
        private GameObject currentObject;
        private GameObject currentDragObject;
        private GameObject currentHoverObject;

        private bool previousPinching;

        private float enteredCanvasTime;

        private List<RaycastResult> raycastResults = new List<RaycastResult>();

        const float defaultRaycastLength = 1f;
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
            cursor = GetComponentInChildren<UICursor>();

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

            cursor.Init(hand, inputModule, eventSystem);
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
            RayIC handRay = (isLeft) ? body.LeftHandRay : body.RightHandRay;
            Vect3 aimPosition = (isLeft) ? body.LeftAimPosition : body.RightAimPosition;
            PoseIC palmPose = hand.GetAnchorPose(AnchorPoint.Palm);

            bool active = IsActive(); // todo: add palm direction filtering here

            if(active)
			{
                // set our transform position and rotation to that of the ray
                pinchConeRenderer.transform.SetPositionAndRotation((Vector3)aimPosition, 
					Quaternion.LookRotation((Vector3)handRay.direction, 
                    (Quaternion)palmPose.rotation * Vector3.back));

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
                Vect3 endpoint = handRay.position + (handRay.direction * defaultRaycastLength);
                if (cursor.SurfaceTarget) endpoint = (Vect3)cursor.transform.position;
                DoLine(aimPosition, endpoint);
			}
            else
			{
                // disable pinch cone
                pinchConeRenderer.enabled = false;
			}
        }

        void DoLine(Vect3 origin, Vect3 endPoint)
		{
            Vect3 startPoint = origin;

            for(int i = 0; i < lineRenderer.positionCount; i++)
			{
                if(i == 0)
				{
                    linePoints[0] = (Vector3)startPoint;
				}
                else if (i == lineRenderer.positionCount - 1)
				{
                    linePoints[lineRenderer.positionCount - 1] = (Vector3)endPoint;
				}
                else
				{
                    float tValue = Mathf.InverseLerp(0, lineRenderer.positionCount, i);
                    linePoints[i] = (Vector3)Vect3.Lerp(startPoint, endPoint, tValue);
				}
			}

            lineRenderer.SetPositions(linePoints);
		}

        private bool IsPinching()
		{
            PinchInfo indexPinch = hand.GetPinchInfo(Finger.Index);
            return indexPinch.PinchAmount > 0.9f;
        }

        bool DoRaycast()
		{
            eventData.Reset();
            eventData.button = PointerEventData.InputButton.Left;

            RayIC handRay = (isLeft) ? body.LeftHandRay : body.RightHandRay;

            // find our UISurfaceTarget if there is one
            UISurfaceTarget hitSurface = null;

            for(int i=0; i < UISurfaceTarget.SurfaceTargets.Count; i++)
			{
                if(UISurfaceTarget.SurfaceTargets[i])
				{
                    hitSurface = UISurfaceTarget.SurfaceTargets[i];
                    break;
                }
			}

            if (hitSurface)
            {
                // raycast against our hit target, send the cursor there
                UIRaycastHit uiRaycastHt;
				Ray uHandRay = new Ray((Vector3)handRay.position, (Vector3)handRay.direction);
                bool didHitSurface = hitSurface.DoRaycast(uHandRay, defaultRaycastLength, out uiRaycastHt);

                if(didHitSurface)
				{
                    cursor.RegisterSurfaceHit(hitSurface, uiRaycastHt);
				}
                else
				{
                    cursor.ClearSurfaceHit();
				}

                eventData.position = inputModule.ScreenCamera.WorldToScreenPoint(cursor.transform.position);
                eventData.delta = eventData.position - prevPosition;
                eventData.scrollDelta = Vector2.zero;

                eventSystem.RaycastAll(eventData, raycastResults);
                eventData.pointerCurrentRaycast = HandInputModule.FindFirstRaycast(raycastResults);
            }
			else
			{
                cursor.ClearSurfaceHit();
			}

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
                bool isPinching = IsPinching();

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
                if(currentState == PointerState.OnCanvas && previousState != PointerState.OffCanvas)
				{
                    enteredCanvasTime = Core.Time.time;
				}

                // todo: dispatch pointer state change event
                //if(OnStateChanged != null)
				//{
                //  OnStateChanged(previousState, currentState);
				//}
            }
        }

        public void Process()
		{
            // check our eligibility requirements
            if(IsActive())
			{
                // raycast against ui surface, update UICursor
                bool hit = DoRaycast();

                previousState = currentState;
                ProcessPointer(eventData);
                ProcessState();
                ProcessEvents();
			}
		}

        void ProcessEvents()
        {
            // don't process empty events
            if (eventData == null) return;

#region Raycast
			if (eventData.pointerCurrentRaycast.gameObject == null || currentState == PointerState.OffCanvas)
			{
                return;
			}

            previousHoverObject = currentHoverObject;
            currentHoverObject = eventData.pointerCurrentRaycast.gameObject;

            // handle in out events
            inputModule.HandlePointerExitAndEnterWrapper(eventData, currentHoverObject);

            if(!previousPinching && IsPinching())
			{
                previousPinching = true;

                // If we just entered this canvas
                if(Core.Time.time - enteredCanvasTime >= Core.Time.deltaTime)
				{
                    if(eventSystem.currentSelectedGameObject)
					{
                        eventSystem.SetSelectedGameObject(null);
					}

                    // update pointer stuff
                    eventData.pressPosition = eventData.position;
                    eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
                    eventData.pointerPress = null;
                    eventData.useDragThreshold = true;

                    if(currentHoverObject)
					{
                        currentObject = currentHoverObject;

                        // check for pointer down handlers
                        GameObject gameObjectJustPressed = ExecuteEvents.ExecuteHierarchy(currentObject,
                            eventData, ExecuteEvents.pointerDownHandler);

                        if(!gameObjectJustPressed)
						{
                            GameObject gameObjectJustClicked = ExecuteEvents.ExecuteHierarchy(currentObject,
                                eventData, ExecuteEvents.pointerClickHandler);

                            if(gameObjectJustClicked)
							{
                                currentObject = gameObjectJustClicked;
							}
						}
                        else
						{
                            currentObject = gameObjectJustPressed;
						}

                        if(gameObjectJustPressed)
						{
                            eventData.pointerPress = gameObjectJustPressed;
                            currentObject = gameObjectJustPressed;

                            // select it
                            if(ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject))
							{
                                eventSystem.SetSelectedGameObject(currentObject);
							}
						}

                        // look for anything that has dragging
                        eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentObject);

                        if(eventData.pointerDrag)
						{
                            IDragHandler dragHandler = eventData.pointerDrag.GetComponent<IDragHandler>();

                            if (dragHandler != null)
                            {
                                if(dragHandler is EventTrigger && eventData.pointerDrag.transform.parent)
								{
                                    eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(
                                        eventData.pointerDrag.transform.parent.gameObject);

                                    if(eventData.pointerDrag != null)
									{
                                        dragHandler = eventData.pointerDrag.GetComponent<IDragHandler>();

                                        if(dragHandler != null && !(dragHandler is EventTrigger))
										{
                                            currentDragObject = eventData.pointerDrag;
                                            dragStartPosition = eventData.position;

                                            if(currentObject && currentObject == currentDragObject)
											{
                                                ExecuteEvents.Execute(
                                                    eventData.pointerDrag,
                                                    eventData,
                                                    ExecuteEvents.beginDragHandler);

                                                eventData.dragging = true;
											}
										}
                                    }
								}
                            }
                            else
							{
                                currentDragObject = eventData.pointerDrag;
                                dragStartPosition = eventData.position;

                                if(currentObject && currentObject == currentDragObject)
								{
                                    ExecuteEvents.Execute(eventData.pointerDrag,
                                        eventData, ExecuteEvents.beginDragHandler);
                                    eventData.dragging = true;
								}
							}
						}
					}
				}
			}

#endregion

#region Scrolling
            if(!eventData.dragging && currentDragObject && 
                Vector2.Distance(eventData.position, dragStartPosition) * 100f >
                EventSystem.current.pixelDragThreshold)
			{
                if(currentObject && !currentObject.GetComponent<ScrollRect>()) // I don't like this GetComponent
                    // call inside of an update sub method, but I can't think of a better way
				{
                    ExecuteEvents.Execute(eventData.pointerDrag,
                        eventData, ExecuteEvents.beginDragHandler);
                    eventData.dragging = true;

                    ExecuteEvents.Execute(currentObject, eventData,
                        ExecuteEvents.pointerUpHandler);

                    eventData.rawPointerPress = null;
                    eventData.pointerPress = null;
                    currentObject = null;
				}
			}
#endregion

#region End Interaction
            if(!IsPinching())
			{
                previousPinching = false;

                if(currentDragObject)
				{
                    ExecuteEvents.Execute(currentDragObject, eventData, ExecuteEvents.endDragHandler);

                    if(currentObject && currentDragObject == currentObject)
					{
                        ExecuteEvents.ExecuteHierarchy(currentHoverObject, eventData,
                            ExecuteEvents.dropHandler);
					}

                    eventData.pointerDrag = null;
                    eventData.dragging = false;
                    currentDragObject = null;
				}

                if(currentObject)
				{
                    ExecuteEvents.Execute(currentObject, eventData, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(currentObject, eventData, ExecuteEvents.pointerClickHandler);
                    eventData.rawPointerPress = null;
                    eventData.pointerPress = null;
                    currentObject = null;
                    currentDragObject = null;
                }
			}
#endregion

			// dragging
            if(eventData.pointerDrag != null && eventData.dragging)
			{
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
			}
		}
	}
}