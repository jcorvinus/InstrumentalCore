﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Core.Math;
using Instrumental.Schema;
using Instrumental.Modeling.ProceduralGraphics;

namespace Instrumental.Controls
{
    public class Slider : UIControl
    {
        #region Runtime Events
        public delegate void SliderRuntimeEventHandler(Slider sender);
        public event SliderRuntimeEventHandler OnTouched;
        public event SliderRuntimeEventHandler OnUntouched;
        public event SliderRuntimeEventHandler OnPressed;
        public event SliderRuntimeEventHandler OnUnpressed;
        public event SliderRuntimeEventHandler OnHovered;
        public event SliderRuntimeEventHandler OnUnhovered;
        public event SliderRuntimeEventHandler HorizontalAmountChanged;
        #endregion

        [SerializeField] SliderModel sliderModel;
#if UNITY
		[SerializeField] SliderSchema sliderSchema;
#elif STEREOKIT
		SliderSchema sliderSchema = SliderSchema.GetDefault();
#endif
		GameObject faceObject;

        [SerializeField] BoxCollider runtimeFaceCollider;
        BoxCollider designTimeFullCollider;

        [SerializeField] float hoverHeight = 0.03f;
        [SerializeField] float underFlow = 0.025f;

        InstrumentalHand leftHand;
        InstrumentalHand rightHand;

        // runtime vars
        bool isLeftInBounds;
        bool isRightInBounds;

        Vect3 furthestPushPoint;

        float horizontalPercent = 0;

        public float HorizontalPercent 
        {
            get { return horizontalPercent; }
            set 
            {
                horizontalPercent = value;
                SetHorizPosForValue();
            }
        }

        public Vect3 FurthestPushPoint { get { return furthestPushPoint; } }
        public float FurthestPushDistance { get { return furthestPushPoint.z; } }
        private List<float> fingerDots;
        private bool waitingForReactivation;

        public float SliderFaceDistance 
        {
            get 
            {
                return (sliderSchema.ExtrusionDepth + (sliderSchema.ExtrusionDepth *
                sliderSchema.BevelExtrusionDepth));
            }
        }

        public float SliderPressDistance
		{
            get
			{
                return SliderFaceDistance * 0.95f;
			}
		}
		bool isTouching;
        bool isHovering;
        bool isPressed;

        public bool IsTouching { get { return isTouching; } }
        public bool IsHovering { get { return isHovering; } }
        public bool IsPressed { get { return isPressed; } }

        // design time vars

        public override void SetSchema(ControlSchema controlSchema)
        {
			// set stuff like our press depth, mesh generation, etc...
			// based off the data in the schema
#if UNITY
			transform.localPosition = (Vector3)controlSchema.Position;
            transform.localRotation = (Quaternion)controlSchema.Rotation;
#elif STEREOKIT
			throw new System.NotImplementedException();
#endif
			_name = controlSchema.Name;

            sliderSchema = SliderSchema.CreateFromControl(controlSchema);
        }

        protected override void Awake()
        {
            _name = "Slider";

            base.Awake();

            faceObject = runtimeFaceCollider.gameObject;

            designTimeFullCollider = GetComponent<BoxCollider>();
        }

        private void SetFaceColliderRuntimeValues()
		{
			if (sliderSchema != null)
			{
				float physDepth = SliderFaceDistance;
				float physAndHoverDepth = physDepth + (hoverHeight);
				float totalDepth = physAndHoverDepth + underFlow;

				if (runtimeFaceCollider) // currently, some design mode prototypes of buttons don't have this
				{
					runtimeFaceCollider.center = new Vector3(0, 0, (physAndHoverDepth * 0.5f) - (underFlow * 0.5f));
					runtimeFaceCollider.size = new Vector3(sliderSchema.Radius * 2, (sliderSchema.Radius * 2),
						totalDepth);
				}
			}
        }

        private void SetDesignColliderValues()
		{
			if (sliderSchema != null)
			{
				float physDepth = SliderFaceDistance;
				float physAndHoverDepth = physDepth + (hoverHeight);

				designTimeFullCollider = GetComponent<BoxCollider>();
				if (designTimeFullCollider)
				{
					designTimeFullCollider.center = new Vector3(0, 0, physDepth * 0.5f);
					designTimeFullCollider.size = new Vector3(sliderSchema.Width, sliderSchema.Radius * 2, physDepth);
				}
			}
        }

		private void OnValidate()
		{
#if UNITY_EDITOR
            if(sliderModel && sliderSchema != null) sliderModel.SetNewSliderSchema(sliderSchema);

			switch (Mode)
			{
				case ControlMode.Runtime:
                    SetFaceColliderRuntimeValues();
                    designTimeFullCollider = GetComponent<BoxCollider>();
                    ClearAnyGraspable();
                    ClearSlottable();
                    break;

				case ControlMode.Design:
                    SetDesignColliderValues();
                    designTimeFullCollider = GetComponent<BoxCollider>();
                    EnsureGraspableExists();
                    ClearSlottable();
                    break;

				case ControlMode.Design_Palette:
                    SetDesignColliderValues();
                    designTimeFullCollider = GetComponent<BoxCollider>();
                    EnsureGraspableExists();
                    EnsureSlottableExists();
                    EnsureSpaceChangeColliderExists(transform);
                    break;

				default:
					break;
			}
#endif
        }

		protected override void Start()
        {
            base.Start();

            leftHand = InstrumentalHand.LeftHand;
            rightHand = InstrumentalHand.RightHand;
        }

        public override ControlSchema GetSchema()
        {
            /*ControlSchema schema = new ControlSchema()
            {
                Name = _name,
#if UNITY
				Position = (sV3)transform.localPosition,
                Rotation = (sQuat)transform.localRotation,
#elif STEREOKIT
				throw new System.NotImplementedException();
#endif
				Type = GetControlType()
            };*/

            return sliderSchema;
        }

        public override ControlType GetControlType()
        {
            return ControlType.Slider;
        }

#region Runtime Behavior
		bool IsInBounds(Vect3 point)
		{
            Vector3 closestPointOnBounds = runtimeFaceCollider.ClosestPoint((Vector3)point);
			Vector3 uPoint = (Vector3)point;
            return closestPointOnBounds == uPoint;
		}

        Vect3 GetTipPosition(InstrumentalHand hand)
        {
            return hand.GetAnchorPose(AnchorPoint.IndexTip).position;
        }

        void SetHorizPosForValue()
		{
            float lowExtent = sliderSchema.Width * -0.5f;
            float highExtent = sliderSchema.Width * 0.5f;
            faceObject.transform.localPosition = new Vector3(Mathf.Lerp(lowExtent, highExtent, horizontalPercent),
                0, faceObject.transform.localPosition.z);
		}

        float GetHorizValueFromPos()
		{
            float lowExtent = sliderSchema.Width * -0.5f;
            float highExtent = sliderSchema.Width * 0.5f;

            float horizPos = faceObject.transform.localPosition.x;

            return Mathf.InverseLerp(lowExtent, highExtent, horizPos);
        }

        void RuntimeUpdate()
		{
            runtimeFaceCollider.enabled = true;
            designTimeFullCollider.enabled = false;

            // check to see if there are any fingers in this button's region
            bool oldIsInBounds = isLeftInBounds || isRightInBounds;

            isLeftInBounds = leftHand != null && IsInBounds(GetTipPosition(leftHand));
            isRightInBounds = rightHand != null && IsInBounds(GetTipPosition(rightHand));

            bool isInBoundsThisFrame = isLeftInBounds || isRightInBounds;

            if (isInBoundsThisFrame != oldIsInBounds)
            {
                if (isInBoundsThisFrame) Hover();
                else CancelHover();
            }

            isHovering = isInBoundsThisFrame;

            if(isInBoundsThisFrame)
			{
                furthestPushPoint.z = SliderFaceDistance; // setting a decent initial value

                if (isLeftInBounds)
                {
                    Vect3 leftTipPosition = GetTipPosition(leftHand);
                    leftTipPosition = (Vect3)transform.InverseTransformPoint((Vector3)leftTipPosition);

                    if (FurthestPushDistance > leftTipPosition.z) furthestPushPoint = leftTipPosition;
                }

                if (isRightInBounds)
                {
                    Vect3 rightTipPosition = GetTipPosition(rightHand);
                    rightTipPosition = (Vect3)transform.InverseTransformPoint((Vector3)rightTipPosition);

                    if (FurthestPushDistance > rightTipPosition.z) furthestPushPoint = rightTipPosition;
                }

                bool wasTouching = isTouching;
                bool wasPressed = isPressed;

                isTouching = FurthestPushDistance < SliderFaceDistance;
                isPressed = FurthestPushDistance < 0.001f;

                if(wasTouching != isTouching)
				{
                    if (isTouching) StartTouch();
                    else EndTouch();
				}

                if(isPressed != wasPressed)
				{
                    if (isPressed) StartPress();
                    else EndPress();
				}

                // reactivation and activation used to happen here
            }
            else
			{
                isTouching = false;
                isPressed = false;
                isHovering = false;
			}

            // handle movement
            if(isTouching)
			{
                float pushInDistance = Mathf.Min(furthestPushPoint.z, SliderFaceDistance);
                pushInDistance = Mathf.Clamp(pushInDistance, 0, SliderFaceDistance);

                float oldXValue = faceObject.transform.localPosition.x;
                float xValue = furthestPushPoint.x;

                xValue = Mathf.Clamp(xValue, -sliderSchema.Width * 0.5f, sliderSchema.Width * 0.5f);

                faceObject.transform.localPosition = new Vector3(
                    xValue, 0, pushInDistance);
			}
            else
			{
                // spring back to the original position
                faceObject.transform.localPosition = Vector3.Lerp(faceObject.transform.localPosition,
                    new Vector3(faceObject.transform.localPosition.x, 0, SliderFaceDistance), 5 * Core.Time.deltaTime);
			}
        }

        void StartTouch()
		{
            if(OnTouched != null)
			{
                SliderRuntimeEventHandler dispatch = OnTouched;
                dispatch(this);
			}
		}

        void EndTouch()
		{
            if(OnUntouched != null)
			{
                SliderRuntimeEventHandler dispatch = OnUntouched;
                dispatch(this);
			}
		}

        void StartPress()
		{
            if(OnPressed != null)
			{
                SliderRuntimeEventHandler dispatch = OnPressed;
                dispatch(this);
			}
		}

        void EndPress()
		{
            if(OnUnpressed != null)
			{
                SliderRuntimeEventHandler dispatch = OnUnpressed;
                dispatch(this);
			}
		}

        void Hover()
		{
            isHovering = true;

            if(OnHovered != null)
			{
                SliderRuntimeEventHandler dispatch = OnHovered;
                dispatch(this);
			}
		}

        void CancelHover()
		{
            isHovering = false;
		}
#endregion

		void DesigntimeUpdate()
		{
            runtimeFaceCollider.enabled = false;
            designTimeFullCollider.enabled = true;
        }

        void DesignPaletteUpdate()
		{
            runtimeFaceCollider.enabled = false;
            designTimeFullCollider.enabled = true;
        }

		private void Update()
		{
			switch (Mode)
			{
				case ControlMode.Runtime:
                    RuntimeUpdate();
					break;
				case ControlMode.Design:
                    DesigntimeUpdate();
					break;
				case ControlMode.Design_Palette:
                    DesignPaletteUpdate();
					break;
				default:
					break;
			}
		}
	}
}