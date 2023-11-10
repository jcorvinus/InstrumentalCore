using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Schema
{
	// todo: probably going to need to handle different grip/push types
	public enum SliderType
	{
		SlotOnly, // fake 'hole' mesh that looks like a cutout from the panel
		SlotRim, // same kind of rim seen on buttons. There are extruded edge loops
		SolidRail // tube, kinda like the leap pinch sliders
	}

	public struct SliderSchema
	{
		#region Constraint Defines
		public const float MIN_WIDTH = 0;
		public const float MAX_WIDTH = 1;

		public const float MIN_RADIUS = 0;
		public const float MAX_RADIUS = 1;

		public const float MIN_BUTTON_HEIGHT = 0;
		public const float MAX_BUTTON_HEIGHT = 0.07f;

		public const int MIN_FACE_BEVEL_SLICE_COUNT = 2;
		public const int MAX_FACE_BEVEL_SLICE_COUNT = 8;

		public const int MIN_FACE_SLICE_COUNT = 3;
		public const int MAX_FACE_SLICE_COUNT = 32;

		public const float MIN_EXTRUSION_DEPTH = 0;
		public const float MAX_EXTRUSION_DEPTH = 0.04f;

		public const float MIN_BEVEL_EXTRUSION_DEPTH = 0;
		public const float MAX_BEVEL_EXTRUSION_DEPTH = 0.4f;

		public const float MIN_BEVEL_RADIUS = 0;
		public const float MAX_BEVEL_RADIUS = 1;

		public const float MIN_RAIL_RADIUS = 0;
		public const float MAX_RAIL_RADIUS=0.01f;

		public const float MIN_RAIL_FORWARD_DIST = 0;
		public const float MAX_RAIL_FORWARD_DIST = 0.01f;

		public const int MIN_RAIL_RADIUS_SLICE_COUNT = 3;
		public const int MAX_RAIL_RADIUS_SLICE_COUNT = 8;

		public const int MIN_RAIL_WIDTH_SLICE_COUNT = 0;
		public const int MAX_RAIL_WIDTH_SLICE_COUNT = 32;
		#endregion

		public float Width;
		public float Radius;
		public float ButtonHeight;
		public int FaceBevelSliceCount;
		public int FaceSliceCount;
		public float ExtrusionDepth;
		public float BevelExtrusionDepth;
		public float BevelRadius;

		public float RailRadius;
		public float RailForwardDistance;
		public int RailRadiusSliceCount;
		public int RailWidthSliceCount;

		public static SliderSchema CreateFromControl(ControlSchema control)
		{
			#region Width
			float width = MIN_WIDTH;
			if (control.ControlVariables.Any(item => item.Name == "Width" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
					item.Name == "Width" && item.Type == typeof(float));

				if (!float.TryParse(controlVariable.Value, out width))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Radius
			float radius = MIN_RADIUS;
			if(control.ControlVariables.Any(item => item.Name == "Radius" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
				item.Name == "Radius" && item.Type == typeof(float));

				if(!float.TryParse(controlVariable.Value, out radius))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region ButtonHeight
			float buttonHeight = MIN_BUTTON_HEIGHT;
			if(control.ControlVariables.Any(item => item.Name == "ButtonHeight" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
				item.Name == "ButtonHeight" && item.Type == typeof(float));

				if (!float.TryParse(controlVariable.Value, out buttonHeight))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region FaceBevelSliceCount
			int faceBevelSliceCount = MIN_FACE_BEVEL_SLICE_COUNT;
			if(control.ControlVariables.Any(item => item.Name == "FaceBevelSliceCount" && item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
				item.Name == "FaceBevelSliceCount" && item.Type == typeof(int));

				if (!int.TryParse(controlVariable.Value, out faceBevelSliceCount))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region FaceSliceCount
			int faceSliceCount = MIN_FACE_SLICE_COUNT;
			if(control.ControlVariables.Any(item => item.Name == "FaceSliceCount" && item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
				item.Name == "FaceSliceCount" && item.Type == typeof(int));

				if(!int.TryParse(controlVariable.Value, out faceSliceCount))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Extrusion Depth
			float extrusionDepth = MIN_EXTRUSION_DEPTH;
			if(control.ControlVariables.Any(item=>item.Name == "ExtrusionDepth" && item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
				item.Name == "ExtrusionDepth" && item.Type == typeof(float));

				if (!float.TryParse(controlVariable.Value, out extrusionDepth))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Bevel Extrusion Depth
			float bevelExtrusionDepth = MIN_BEVEL_EXTRUSION_DEPTH;
			if(control.ControlVariables.Any(item => item.Name == "BevelExtrusionDepth" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => item.Name == "BevelExtrusionDepth"
				&& item.Type == typeof(float));

				if(!float.TryParse(controlVariable.Value, out bevelExtrusionDepth))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Bevel Radius
			float bevelRadius = MIN_BEVEL_RADIUS;
			if(control.ControlVariables.Any(item => item.Name == "BevelRadius" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => item.Name == "BevelRadius"
				&& item.Type == typeof(float));

				if(!float.TryParse(controlVariable.Value, out bevelRadius))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Rail Radius
			float railRadius = MIN_RAIL_RADIUS;
			if(control.ControlVariables.Any(item => item.Name == "RailRadius" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => item.Name == "RailRadius"
				&& item.Type == typeof(float));

				if(!float.TryParse(controlVariable.Value, out railRadius))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region RailForwardDistance
			float railForwardDistance = MIN_RAIL_FORWARD_DIST;
			if(control.ControlVariables.Any(item => item.Name == "RailForwardDistance" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => item.Name == "RailForwardDistance" &&
				item.Type == typeof(float));

				if (!float.TryParse(controlVariable.Value, out railForwardDistance))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region RailRadiusSliceCount
			int railRadiusSliceCount = MIN_RAIL_RADIUS_SLICE_COUNT;
			if(control.ControlVariables.Any(item => item.Name == "RailRadiusSliceCount" && item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => item.Name == "RailRadiusSliceCount" &&
				item.Type == typeof(int));

				if(!int.TryParse(controlVariable.Value, out railRadiusSliceCount))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region RailWidthSliceCount
			int railWidthSliceCount = MIN_RAIL_WIDTH_SLICE_COUNT;
			if(control.ControlVariables.Any(item => item.Name == "RailRadiusWidthSliceCount" && item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => item.Name == "RailRadiusWidthSliceCount" &&
				item.Type == typeof(int));

				if(!int.TryParse(controlVariable.Value, out railWidthSliceCount))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			return new SliderSchema()
			{
				Width = width,
				Radius = radius,
				ButtonHeight = buttonHeight,
				FaceBevelSliceCount = faceBevelSliceCount,
				FaceSliceCount = faceSliceCount,
				ExtrusionDepth = extrusionDepth,
				BevelExtrusionDepth = bevelExtrusionDepth,
				BevelRadius = bevelRadius,
				RailRadius = railRadius,
				RailForwardDistance = railForwardDistance,
				RailRadiusSliceCount = railRadiusSliceCount,
				RailWidthSliceCount = railWidthSliceCount
			};

		}

		public static SliderSchema GetDefault()
		{
			return new SliderSchema()
			{
				Width = 0.05f,
				Radius = 0.022f,
				ButtonHeight = 0.017f,
				FaceBevelSliceCount = 4,
				ExtrusionDepth = 0.017f,
				BevelExtrusionDepth = 0.246f,
				BevelRadius = 0.697f,
				RailRadius = 0.005f,
				RailForwardDistance = 0.00158f,
				RailRadiusSliceCount = 6,
				RailWidthSliceCount = 6
			};			
		}

		private static void DoVariableError(ControlSchema control, ControlVariable variable)
		{
			Debug.LogWarning(string.Format("There was a problem with {0}'s {1} variable. It did not parse to {2} properly.",
				control.Name, variable.Name, variable.Type.ToString()));
		}

		public void SetControlSchema(ref ControlSchema control)
		{
			#region Width
			Func<ControlVariable, bool> widthFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "Width" && item.Type == typeof(float));
			if(control.ControlVariables.Any(widthFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(widthFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = Width.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Radius
			Func<ControlVariable, bool> radiusFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "Radius" && item.Type == typeof(float));
			if(control.ControlVariables.Any(radiusFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(radiusFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = Radius.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Button Height
			Func<ControlVariable, bool> buttonHeightFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "ButtonHeight" && item.Type == typeof(float));
			if(control.ControlVariables.Any(buttonHeightFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(buttonHeightFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = ButtonHeight.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Face Bevel Slice Count
			Func<ControlVariable, bool> faceBevelSliceCountFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "FaceBevelSliceCount" && item.Type == typeof(int));
			if(control.ControlVariables.Any(faceBevelSliceCountFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(faceBevelSliceCountFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = FaceBevelSliceCount.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Face Slice Count
			Func<ControlVariable, bool> faceSliceCountFunc = new Func<ControlVariable, bool>(item =>
		    item.Name == "FaceSliceCount" && item.Type == typeof(int));
			if(control.ControlVariables.Any(faceSliceCountFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(faceSliceCountFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = FaceSliceCount.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Extrusion Depth
			Func<ControlVariable, bool> extrusionDepthFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "ExtrusionDepth" && item.Type == typeof(float));
			if(control.ControlVariables.Any(extrusionDepthFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(extrusionDepthFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = ExtrusionDepth.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Bevel Extrusion Depth
			Func<ControlVariable, bool> bevelExtrusionDepthFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "BevelExtrusionDepth" && item.Type == typeof(float));
			if(control.ControlVariables.Any(bevelExtrusionDepthFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(bevelExtrusionDepthFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = BevelExtrusionDepth.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Bevel Radius
			Func<ControlVariable, bool> bevelRadiusFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "BevelRadius" && item.Type == typeof(float));
			if(control.ControlVariables.Any(bevelRadiusFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(control.ControlVariables.First(bevelRadiusFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = BevelRadius.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Rail Radius
			Func<ControlVariable, bool> railRadiusFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "RailRadius" && item.Type == typeof(float));

			if(control.ControlVariables.Any(railRadiusFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(control.ControlVariables.First(railRadiusFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = RailRadius.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Rail Forward Distance
			Func<ControlVariable, bool> railForwardFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "RailForwardDistance" && item.Type == typeof(float));
			if(control.ControlVariables.Any(railForwardFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(control.ControlVariables.First(railForwardFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = RailForwardDistance.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Rail Radius Slice Count
			Func<ControlVariable, bool> railRadiusSliceCountFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "RailRadiusSliceCount" && item.Type == typeof(int));
			if(control.ControlVariables.Any(railRadiusSliceCountFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(control.ControlVariables.First(railRadiusSliceCountFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = RailRadiusSliceCount.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion

			#region Rail Width Slice Count
			Func<ControlVariable, bool> railWidthSliceCountFunc = new Func<ControlVariable, bool>(item =>
			item.Name == "RailWidthSliceCount" && item.Type == typeof(int));
			if(control.ControlVariables.Any(railWidthSliceCountFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(control.ControlVariables.First(railWidthSliceCountFunc));
				ControlVariable controlVariable = control.ControlVariables[indexOf];
				controlVariable.Value = RailWidthSliceCount.ToString();
				control.ControlVariables[indexOf] = controlVariable;
			}
			#endregion
		}
	}
}