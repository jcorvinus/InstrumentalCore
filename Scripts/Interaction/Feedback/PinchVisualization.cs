using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Feedback
{
    public abstract class PinchVisualization : MonoBehaviour
    {
        [SerializeField] protected Handedness handedness;
        protected InstrumentalHand hand;

        [SerializeField] Material effectMaterial;
        GameObject visModel;
        MeshRenderer visRenderer;

		protected virtual void Awake()
		{
            visModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            SphereCollider visCollider = visModel.GetComponent<SphereCollider>();
            visRenderer = visModel.GetComponent<MeshRenderer>();
            visRenderer.material = effectMaterial;
            visRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            visRenderer.receiveShadows = false;
            Destroy(visCollider);

            visModel.transform.localScale = Vector3.zero;
            visModel.gameObject.SetActive(false);
		}

		// Start is called before the first frame update
		void Start()
        {
            hand = (handedness == Handedness.Left) ? InstrumentalHand.LeftHand :
                InstrumentalHand.RightHand;
        }

        protected abstract PinchInfo GetPinchInfo();

        // Update is called once per frame
        void Update()
        {
            PinchInfo pinchInfo = GetPinchInfo();
            visModel.transform.position = (Vector3)pinchInfo.PinchCenter;
            visModel.transform.localScale = Vector3.one * (pinchInfo.PinchDistance * 0.5f);
            visModel.SetActive(true);
        }
    }
}