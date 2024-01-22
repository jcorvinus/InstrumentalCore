using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

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
			Quaternion userRotation = Quaternion.LookRotation(body.ForwardDirection, Vector3.up);
			Vector3 userRight = userRotation * Vector3.right;

			Vector3 userForwardDirection = Quaternion.AngleAxis(30, userRight) * body.ForwardDirection;
			Vector3 placementPosition = body.Head.position + (userForwardDirection * 0.46f);
			Vector3 rotationDirection = (body.Head.position - placementPosition);
			Quaternion rotation = Quaternion.LookRotation(rotationDirection,
				-(Quaternion.AngleAxis(120, userRight) * body.ForwardDirection));

			transform.SetPositionAndRotation(placementPosition, rotation);
			keyboards[currentKeboardIndex].gameObject.SetActive(true);

			keyboards[currentKeboardIndex].Show();
		}

		public void HideKeyboard()
		{
			keyboards[currentKeboardIndex].Hide();
		}
    }
}