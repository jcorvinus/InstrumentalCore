using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Input
{
    public class HandPointer : MonoBehaviour
    {
        InstrumentalBody body;
        MeshRenderer pinchConeRenderer;
        [SerializeField] bool isLeft = false;

		private void Awake()
		{
            pinchConeRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
		}

		// Start is called before the first frame update
		void Start()
        {
            body = InstrumentalBody.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            InstrumentalHand hand = (isLeft) ? InstrumentalHand.LeftHand : InstrumentalHand.RightHand;
            Ray handRay = (isLeft) ? body.LeftHandRay : body.RightHandRay;
            Pose palmPose = hand.GetAnchorPose(AnchorPoint.Palm);

            bool active = (hand.IsTracking); // todo: add palm direction filtering here

            if(active)
			{
                // set our transform position and rotation to that of the ray
                transform.SetPositionAndRotation(handRay.origin, Quaternion.LookRotation(handRay.direction, 
                    palmPose.rotation * Vector3.back));

                // enable pinch cone
                pinchConeRenderer.enabled = true;
                
                // animate pinch cone
			}
            else
			{
                // disable pinch cone
                pinchConeRenderer.enabled = false;
			}
        }
    }
}