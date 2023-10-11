using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Space
{
    public abstract class RadialSpace : TransformSpace
    {
		protected float radiansPerMeter;

		[SerializeField]
        protected float radius=1;
        public float Radius { get { return radius; } set { radius = value; } }

		public override Matrix4x4 GetTransformationMatrix(Vector3 localPosition)
		{
			throw new System.NotImplementedException();
		}

		public override Vector3 InverseTransformDirection(Vector3 position, Vector3 direction)
		{
			throw new System.NotImplementedException();
		}

		public override Vector3 InverseTransformPoint(Vector3 worldPoint)
		{
			throw new System.NotImplementedException();
		}

		public override Quaternion InverseTransformRotation(Vector3 position, Quaternion rotation)
		{
			throw new System.NotImplementedException();
		}

		public override Vector3 TransformDirection(Vector3 localPosition, Vector3 localDirection)
		{
			throw new System.NotImplementedException();
		}

		public override Vector3 TransformPoint(Vector3 localPoint)
		{
			throw new System.NotImplementedException();
		}

		public override Quaternion TransformRotation(Vector3 localPosition, Quaternion localRotation)
		{
			throw new System.NotImplementedException();
		}



		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}