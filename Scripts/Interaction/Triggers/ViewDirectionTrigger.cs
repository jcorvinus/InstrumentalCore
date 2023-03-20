using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental;

namespace Instrumental.Interaction.Triggers
{
    public class ViewDirectionTrigger : Trigger
    {
        InstrumentalBody body;

        [Range(0, 60)]
        [SerializeField] float activationAngle = 25f;
        [Range(0, 60)]
        [SerializeField] float deactivationAngle = 30f;
        [Range(0, 80)]
        [SerializeField] float feedbackActivationAngleOuter = 80f;
        float currentAngle;

		private void Awake()
		{
            body = FindObjectOfType<InstrumentalBody>();
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // get our angle
            Vector3 directionToTarget = transform.position - body.Head.position;
            Vector3 viewDirection = body.Head.forward;
            bool isCorrectSide = (Vector3.Dot(directionToTarget, viewDirection) > 0); // vector3.angle doesn't 
                                                                                      // have a notion of sidedness and I'm not screwing around with vector3.signed angle rn
            float angle = Vector3.Angle(viewDirection, directionToTarget);

            if(IsActive)
			{
                // should we disengage?
                if(!isCorrectSide || angle > deactivationAngle)
				{
                    Deactivate();
				}
                else
				{
                    feedback = Mathf.InverseLerp(0, deactivationAngle, angle);
				}
			}
            else
			{
                if(isCorrectSide && angle < activationAngle)
				{
                    Activate();
				}
                else
				{
                    feedback = 1 - Mathf.InverseLerp(activationAngle, feedbackActivationAngleOuter,
                        angle);
				}
			}            
        }
    }
}