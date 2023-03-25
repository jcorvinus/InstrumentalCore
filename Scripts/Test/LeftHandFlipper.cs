using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

using Instrumental.Interaction;

namespace Instrumental.Test
{
    public class LeftHandFlipper : MonoBehaviour
    {
        InstrumentalHand hand;
        [SerializeField] SteamVR_Behaviour_Skeleton[] visualSkeletons;
        [SerializeField] SteamVR_Behaviour_Skeleton dataSkeleton;

        [SerializeField] KeyCode visualFlipKey = KeyCode.F3;
        [SerializeField] KeyCode dataFlipKey = KeyCode.F4;

        bool visualflip = false;
        bool dataFlip = false;

		private void Awake()
		{
            hand = GetComponent<InstrumentalHand>();
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyUp(visualFlipKey))
			{
                visualflip = !visualflip;

                foreach (SteamVR_Behaviour_Skeleton visSkeleton in visualSkeletons) visSkeleton.mirroring = visualflip ? SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft : SteamVR_Behaviour_Skeleton.MirrorType.None;

            }
        }
    }
}