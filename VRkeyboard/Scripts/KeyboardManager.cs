using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

using Instrumental.Interaction;
using Instrumental.Core;
using Instrumental.Core.Math;

namespace VRKeyboard
{
    public class KeyboardManager : MonoBehaviour
    {
		private static KeyboardManager instance;
		public static KeyboardManager Instance { get { return instance; } }

		public bool IsShown()
		{
			return keyboards[currentKeboardIndex].Visible;
		}

        int currentKeboardIndex = 0;
        [SerializeField] Keyboard[] keyboards;
        InstrumentalBody body;

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			body = InstrumentalBody.Instance;
		}

		public void ShowKeyboard()
		{
			// our position should be just over one forearm's length from the torso,
			// and down far enough to have a 30 degree incline when rotated to face the user's neck

			// I think we can figure this out by rotating the forward direction down 30 degrees,
			// then pushing out the required distance.

			// in the future, we can worry about hip placement and if the user is lying down or in any other pose,
			// but for now let's do this simply.
			Quatn userRotation = Quatn.LookRotation(body.ForwardDirection, Vect3.up);
			Vect3 userRight = userRotation * Vect3.right;

			Vect3 userForwardDirection = Quatn.AngleAxis(30, userRight) * body.ForwardDirection;
			Vect3 placementPosition = (Vect3)body.Head.position + (userForwardDirection * 0.46f);
			Vect3 rotationDirection = ((Vect3)body.Head.position - placementPosition);
			Quatn rotation = Quatn.LookRotation(rotationDirection,
				-(Quatn.AngleAxis(120, userRight) * body.ForwardDirection));

			transform.SetPositionAndRotation((Vector3)placementPosition, (Quaternion)rotation);
			keyboards[currentKeboardIndex].gameObject.SetActive(true);

			keyboards[currentKeboardIndex].Show();
		}

		public void HideKeyboard()
		{
			keyboards[currentKeboardIndex].Hide();
		}
    }
}