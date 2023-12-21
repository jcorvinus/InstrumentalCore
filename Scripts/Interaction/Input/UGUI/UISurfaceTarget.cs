using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Instrumental.Interaction.Input
{
	public struct UIRaycastHit
	{
        public Canvas UICanvas;
        public Vector3 WorldPoint;
        public Vector3 LocalPoint;
        public Transform Transform;
    }

	public class UISurfaceTarget : MonoBehaviour
    {
        private static List<UISurfaceTarget> surfaceTargets = new List<UISurfaceTarget>();
        public static List<UISurfaceTarget> SurfaceTargets { get { return surfaceTargets; } }

        private Canvas uiCanvas;
        RectTransform rectTransform;
        public Canvas UICanvas { get { return uiCanvas; } }

		private void Awake()
		{
            uiCanvas = GetComponent<Canvas>();
            rectTransform = GetComponent<RectTransform>();

            surfaceTargets.Add(this);
		}

        public bool DoRaycast(Ray ray, float length, out UIRaycastHit hitInfo)
		{
            hitInfo = new UIRaycastHit();

            Vector3 planeNormal = transform.forward * -1;
            float dot = Vector3.Dot(planeNormal, ray.direction);

            if (dot > 0) return false;

            Plane canvasPlane = new Plane(planeNormal, transform.position);

            float center = 0;

            bool hitPlane = canvasPlane.Raycast(ray, out center);

            if (!hitPlane) return false;

            Vector3 hitPosition = ray.origin + (ray.direction * center);
            Vector3 hitPositionLocal = transform.InverseTransformPoint(hitPosition);

            // check is in bounds
            if(hitPositionLocal.x > (rectTransform.sizeDelta.x * -0.5f) && hitPositionLocal.x < (rectTransform.sizeDelta.x * 0.5f) &&
                hitPositionLocal.y > (rectTransform.sizeDelta.y * -0.5f) && hitPositionLocal.y < (rectTransform.sizeDelta.y * 0.5f))
			{
                // fill our hitInfo with information
                hitInfo.UICanvas = UICanvas;
                hitInfo.WorldPoint = hitPosition;
                hitInfo.LocalPoint = hitPositionLocal;
                hitInfo.Transform = transform;

                return true;
			} 
            else return false;
        }
	}
}