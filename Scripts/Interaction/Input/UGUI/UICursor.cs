using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Instrumental.Interaction.Input
{
    public class UICursor : MonoBehaviour
    {
        const float smallScale = 0.02f;
        const float largeScale = 0.04f;
        SpriteRenderer spriteRenderer;
        InstrumentalHand hand;
        UISurfaceTarget surfaceTarget;
        Vector3 localSeekPosition;

        const float minSquishDistance = 0.01f;
        const float squashStartDistance = 0.05f;

        HandInputModule inputModule;
        EventSystem eventSystem;

        public UISurfaceTarget SurfaceTarget { get { return surfaceTarget; } }
        
        bool hasInit = false;

		private void Awake()
		{
            spriteRenderer = GetComponent<SpriteRenderer>();
		}

		public void Init(InstrumentalHand hand, HandInputModule inputModule, EventSystem eventSystem)
		{
            this.hand = hand;
            this.inputModule = inputModule;
            this.eventSystem = eventSystem;
            hasInit = true;
		}

        public void RegisterSurfaceHit(UISurfaceTarget surfaceTarget, UIRaycastHit hit)
		{
            bool surfaceChanged = this.surfaceTarget != surfaceTarget;

            this.surfaceTarget = surfaceTarget;
            localSeekPosition = hit.LocalPoint;

            if (surfaceChanged)
            {
                transform.SetPositionAndRotation(surfaceTarget.UICanvas.transform.TransformPoint(localSeekPosition),
                    surfaceTarget.UICanvas.transform.rotation);
            }

            spriteRenderer.enabled = true;
		}

        public void ClearSurfaceHit()
		{
            spriteRenderer.enabled = false;
            this.surfaceTarget = null;
		}

        // Update is called once per frame
        void Update()
        {
            if (!hasInit) return;

            if(hand.IsTracking)
			{
                PinchInfo indexPinch = hand.GetPinchInfo(Finger.Index);
                float scale = Mathf.InverseLerp(minSquishDistance, squashStartDistance,
                    indexPinch.PinchDistance);
                transform.localScale = Vector3.Lerp(Vector3.one * largeScale, Vector3.one * smallScale, scale);

                if(surfaceTarget)
				{
                    transform.SetPositionAndRotation(surfaceTarget.UICanvas.transform.TransformPoint(localSeekPosition),
                        surfaceTarget.UICanvas.transform.rotation);
                }
            }
        }
    }
}