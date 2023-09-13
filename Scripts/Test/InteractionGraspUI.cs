using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Instrumental.Interaction
{
    public class InteractionGraspUI : MonoBehaviour
    {
        [SerializeField] InteractiveItem interactiveItem;
        [Range(0, 1)]
        [SerializeField] int handIndex;

        [SerializeField] Slider[] curlSliders;
        [SerializeField] TMPro.TMP_Text[] curlLabels;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(interactiveItem && interactiveItem.GraspableHands != null &&
                interactiveItem.GraspableHands.Count > handIndex)
			{
                InteractiveItem.GraspDataVars graspData = interactiveItem.GraspableHands[handIndex];

                for(int i=0; i < curlSliders.Length; i++)
				{
                    float curl = 0;
                    float previousCurl = 0;
                    string fingerName = "";

					switch (i)
					{
                        case (0):
                            curl = graspData.ThumbCurlCurrent;
                            previousCurl = graspData.ThumbCurlPrevious;
                            fingerName = "thumb";
                            break;

                        case (1):
                            curl = graspData.IndexCurlCurrent;
                            previousCurl = graspData.IndexCurlPrevious;
                            fingerName = "index";
                            break;

                        case (2):
                            curl = graspData.MiddleCurlCurrent;
                            previousCurl = graspData.MiddleCurlPrevious;
                            fingerName = "middle";
                            break;

                        case (3):
                            curl = graspData.RingCurlCurrent;
                            previousCurl = graspData.RingCurlPrevious;
                            fingerName = "ring";
                            break;

                        case (4):
                            curl = graspData.PinkyCurlCurrent;
                            previousCurl = graspData.PinkyCurlPrevious;
                            fingerName = "pinky";
                            break;

                        default:
							break;
					}

                    // set slider to curl amount
                    curlSliders[i].value = curl;

                    // set label string to indicate curl velocity
                    float curlVel = curl - previousCurl;
                    curlLabels[i].text = string.Format("{0} curl vel: {1}", fingerName,
                        curlVel);
				}
			}
        }
    }
}