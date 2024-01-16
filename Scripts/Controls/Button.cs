using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Interaction.Slottables;
using Instrumental.Modeling.ProceduralGraphics;
using Instrumental.Schema;

namespace Instrumental.Controls
{
    public class Button : UIControl
    {
		// events
		public delegate void ButtonEventHandler(Button sender);
		public event ButtonEventHandler ButtonActivated;
		public event ButtonEventHandler ButtonHovered;
		public event ButtonEventHandler ButtonHoverEnded;

		[SerializeField] ButtonModel buttonModel;
		[SerializeField] ButtonSchema buttonSchema = ButtonSchema.GetDefault();
		[SerializeField] BoxCollider boxCollider;
		[SerializeField] ButtonRuntime buttonRuntimeBehavior;

		[SerializeField] float hoverHeight = 0.03f;
		[SerializeField] float underFlow = 0.01f;

		#region Schema value accessors (Incomplete)
		public bool HasRim { get { return buttonSchema.HasRim; }
			set
			{
				if (HasRim != value) // don't do un-necessary work
				{
					buttonSchema.HasRim = value;
				}
			}
		}

		public int CornerVertCount { get { return buttonSchema.CornerVertCount; }
			set
			{
				if (CornerVertCount != value) { buttonSchema.CornerVertCount = value; RebuildMesh(); }
			}
		}

		public int WidthSliceCount { get { return buttonSchema.WidthVertCount;  }
			set { if (WidthSliceCount != value) { buttonSchema.WidthVertCount = value; RebuildMesh(); } }
		}

		public int BevelSliceCount { get { return buttonSchema.BevelSliceCount; }
			set { if (BevelSliceCount != value) { buttonSchema.BevelSliceCount = value; RebuildMesh(); } }
		}

		public float Depth { get { return buttonSchema.Depth; } 
			set { if (Depth != value) { buttonSchema.Depth = value; UpdateVertsOnly(); }  } }

		/// <summary>
		/// In round buttons, height is also radius!
		/// </summary>
		public float Height
		{
			get { return buttonSchema.Radius; }
			set
			{
				if (Height != value) { buttonSchema.Radius = value; UpdateVertsOnly(); }
			}
		}

		public float Width { get { return buttonSchema.Width; }
			set
			{
				if (Width != value) { buttonSchema.Width = value; UpdateVertsOnly(); }
			}
		}

		public float BevelRadius { get { return buttonSchema.BevelRadius; }
			set { if (BevelRadius != value) { buttonSchema.BevelRadius = value; UpdateVertsOnly(); } }
		}

		public float RimWidth { get { return buttonSchema.RimWidth; }
			set { if (RimWidth != value) { buttonSchema.RimWidth = value; UpdateVertsOnly(); } }
		}

		public float RimDepth { get { return buttonSchema.RimDepth; }
			set { if (RimDepth != value) { buttonSchema.RimDepth = value; UpdateVertsOnly(); } }
		}

		public float HoverHeight { get { return hoverHeight; } }
		#endregion

		public ButtonRuntime Runtime { get { return buttonRuntimeBehavior; } }

		private void EnsureRuntimeComponentExists()
		{
			if (!buttonRuntimeBehavior) buttonRuntimeBehavior = GetComponent<ButtonRuntime>();
			if (!buttonRuntimeBehavior)
			{
				buttonRuntimeBehavior = gameObject.AddComponent<ButtonRuntime>();

				float physDepth = (buttonSchema.Depth + (buttonSchema.Depth * buttonSchema.BevelDepth));
				buttonRuntimeBehavior.ButtonFaceDistance = physDepth;
				buttonRuntimeBehavior.ButtonFace = buttonRuntimeBehavior.transform.GetChild(0);
				buttonRuntimeBehavior.ThrowSource = buttonRuntimeBehavior.transform.GetChild(2).GetComponent<AudioSource>();
			}
		}

		private void ClearRuntimeComponents()
		{
			if (buttonRuntimeBehavior)
			{
				if (Application.isPlaying)
				{
					Destroy(buttonRuntimeBehavior);
				}
				else
				{
#if UNITY_EDITOR
					UnityEditor.EditorApplication.delayCall += () =>
					{
						UnityEditor.Undo.DestroyObjectImmediate(buttonRuntimeBehavior);
					};
#endif
				}
			}
		}

		private void EnsureBoxColliderExists()
		{
			if (!boxCollider) boxCollider = GetComponent<BoxCollider>();
			if (!boxCollider) boxCollider = gameObject.AddComponent<BoxCollider>();
		}

		private void SetBoxColliderRuntimeValues()
		{
			float physDepth = (buttonSchema.Depth + (buttonSchema.Depth * buttonSchema.BevelDepth));
			float physAndHoverDepth = physDepth + (hoverHeight);
			float totalDepth = physAndHoverDepth + underFlow;

			boxCollider.center = new Vector3(0, 0, (physAndHoverDepth * 0.5f) - (underFlow * 0.5f));
			boxCollider.size = new Vector3(buttonSchema.Width + (buttonSchema.Radius * 2), buttonSchema.Radius * 2,
				totalDepth);
			boxCollider.isTrigger = true;
		}

		private void SetGraspableColliderValues()
		{
			float physDepth = (buttonSchema.Depth + (buttonSchema.Depth * buttonSchema.BevelDepth));
			boxCollider.center = new Vector3(0, 0, physDepth * 0.5f);
			boxCollider.size = new Vector3(buttonSchema.Width + (buttonSchema.Radius * 2),
				buttonSchema.Radius * 2, physDepth);
			boxCollider.isTrigger = false;
		}

		void SetupRuntimeComponents()
		{
			EnsureBoxColliderExists();
			EnsureRuntimeComponentExists();
			SetBoxColliderRuntimeValues();
		}

		private void OnValidate()
		{
#if UNITY_EDITOR
			buttonModel.SetNewButtonSchema(buttonSchema);

			// handle our component differentiation
			if(!Application.isPlaying) // only change components at edit time.
			{
				switch (Mode)
				{
					case ControlMode.Runtime:
						SetupRuntimeComponents();
						ClearAnyGraspable();
						ClearSlottable();

						break;
					case ControlMode.Design:
						if (buttonRuntimeBehavior)
						{
							UnityEditor.EditorApplication.delayCall += () =>
							{
								UnityEditor.Undo.DestroyObjectImmediate(buttonRuntimeBehavior);
							};
						}

						// set collider to fit button visual,
						// do not include extrea hover distance
						EnsureBoxColliderExists();
						SetGraspableColliderValues();
						EnsureGraspableExists();
						ClearRuntimeComponents();
						ClearSlottable();
						break;

					case ControlMode.Design_Palette:
						EnsureBoxColliderExists();
						SetGraspableColliderValues();
						EnsureGraspableExists();
						ClearRuntimeComponents();
						EnsureSlottableExists();
						EnsureSpaceChangeColliderExists(transform);
						
						break;

					default:
						break;
				}
			}
#endif
		}

		public override void SwitchMode(ControlMode newMode)
		{
			ControlMode oldMode = Mode;

			base.SwitchMode(newMode);

			switch (oldMode)
			{
				case ControlMode.Runtime:
					if(newMode == ControlMode.Design)
					{
						SetGraspableColliderValues();
					}
					else if (newMode == ControlMode.Design_Palette)
					{
						SetGraspableColliderValues();
					}
					break;

				case ControlMode.Design:
					if(newMode == ControlMode.Runtime)
					{
						SetupRuntimeComponents();
					}
					else if (newMode == ControlMode.Design_Palette)
					{
						SetGraspableColliderValues();
					}
					break;

				case ControlMode.Design_Palette:
					if(newMode == ControlMode.Runtime)
					{
						SetupRuntimeComponents();
					}	
					else if (newMode == ControlMode.Design)
					{
						SetGraspableColliderValues();
					}
					break;
				default:
					break;
			}
		}

		public override void SetSchema(ControlSchema controlSchema)
        {
            // set things based off the schema
            transform.localPosition = controlSchema.Position;
            transform.localRotation = controlSchema.Rotation;
            _name = controlSchema.Name;

			buttonSchema = ButtonSchema.CreateFromControl(controlSchema);
        }

		protected override void Awake()
        {
            _name = "Button";

			buttonRuntimeBehavior = GetComponent<ButtonRuntime>();

            base.Awake();

			if(buttonRuntimeBehavior)
			{
				buttonRuntimeBehavior.ButtonActivated += (ButtonRuntime sender) =>
				{
					if (ButtonActivated != null) ButtonActivated(this);
				};

				buttonRuntimeBehavior.ButtonHovered += (ButtonRuntime sender) =>
				{
					if (ButtonHovered != null) ButtonHovered(this);
				};

				buttonRuntimeBehavior.ButtonHoverEnded += (ButtonRuntime sender) =>
				{
					if (ButtonHoverEnded != null) ButtonHoverEnded(this);
				};
			}
        }

        protected override void Start()
        {
            base.Start();
        }

        public override ControlSchema GetSchema()
        {
            ControlSchema schema = new ControlSchema()
            {
                Name = _name,
                Position = transform.localPosition,
                Rotation = transform.localRotation,
                Type = GetControlType()
            };

			buttonSchema.SetControlSchema(ref schema);

            return schema;
        }

		#region Meshing
		void RebuildMesh()
		{

		}

		void UpdateVertsOnly()
		{
			
		}
		#endregion

		public override ControlType GetControlType()
        {
            return ControlType.Button;
        }
    }
}