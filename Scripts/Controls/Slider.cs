using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Schema;
using Instrumental.Modeling.ProceduralGraphics;

namespace Instrumental.Controls
{
    public class Slider : UIControl
    {
        [SerializeField] SliderModel sliderModel;
        [SerializeField] SliderSchema sliderSchema = SliderSchema.GetDefault();
        GameObject physicsObject;
        GameObject rimObject;

        [SerializeField] BoxCollider boxCollider;

        [SerializeField] float hoverHeight = 0.03f;
        [SerializeField] float underFlow = 0.01f;

        public override void SetSchema(ControlSchema controlSchema)
        {
            // set stuff like our press depth, mesh generation, etc...
            // based off the data in the schema
            transform.localPosition = controlSchema.Position;
            transform.localRotation = controlSchema.Rotation;
            _name = controlSchema.Name;

            sliderSchema = SliderSchema.CreateFromControl(controlSchema);
        }

        protected override void Awake()
        {
            _name = "Slider";

            base.Awake();

            physicsObject = transform.Find("Physics").gameObject;
            rimObject = transform.Find("Rim").gameObject;

            // also get our graphics so we can do hover animations
        }

		private void OnValidate()
		{
			
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

            return schema;
        }

        public override ControlType GetControlType()
        {
            return ControlType.Slider;
        }
    }
}