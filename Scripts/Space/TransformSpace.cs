using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Space
{
    public abstract class TransformSpace : MonoBehaviour
    {
        public Transform OriginRef;

        protected Transform Origin { get { return (OriginRef) ? OriginRef : transform; } }

        public abstract Vector3 TransformPoint(Vector3 localPoint);
        public abstract Vector3 InverseTransformPoint(Vector3 worldPoint);
        public abstract Quaternion TransformRotation(Vector3 localPosition, Quaternion localRotation);
        public abstract Quaternion InverseTransformRotation(Vector3 position, Quaternion rotation);
        public abstract Vector3 TransformDirection(Vector3 localPosition, Vector3 localDirection);
        public abstract Vector3 InverseTransformDirection(Vector3 position, Vector3 direction);
        public abstract Matrix4x4 GetTransformationMatrix(Vector3 localPosition);
    }
}