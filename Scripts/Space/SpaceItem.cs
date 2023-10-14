//using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;

namespace Instrumental.Space
{
    public class SpaceItem : MonoBehaviour
    {
        public delegate void SpaceChangeHandler(SpaceItem sender, TransformSpace oldSpace, TransformSpace newSpace);
        public event SpaceChangeHandler SpaceChanged;

        TransformSpace currentSpace;

        public TransformSpace CurrentSpace { get { return currentSpace; } }

		private void Awake()
		{
            currentSpace = transform.GetComponentInParent<TransformSpace>();
		}

		private void Start()
        {

        }

        public void ChangeSpaces(TransformSpace newSpace)
        {
            if (currentSpace == newSpace) return; // don't double process

            TransformSpace oldSpace = currentSpace;
            currentSpace = newSpace;

            if(SpaceChanged != null)
			{
                SpaceChangeHandler dispatch = SpaceChanged;
                dispatch(this, oldSpace, currentSpace);
			}
        }
    }
}