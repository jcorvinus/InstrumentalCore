using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction.Triggers;
using Instrumental.Tweening;

namespace Instrumental.Interaction
{
    public class HandUIOpener : MonoBehaviour
    {
        PalmDirectionTrigger palmDirection;
        Transform container;
        TweenScale containerScale;

		// todo: rotation tweener
		// find a way to make it look at the head?

		private void Awake()
		{
            container = transform.GetChild(0);
			containerScale = container.GetComponent<TweenScale>();
			palmDirection = GetComponent<PalmDirectionTrigger>();
		}

		// Start is called before the first frame update
		void Start()
		{
			container.gameObject.SetActive(false);

			palmDirection.OnActivated += () =>
			{
				Activate();
			};

			palmDirection.OnDeactivated += () =>
			{
				Deactivate();
			};
		}

		void Activate()
		{
			// set container initial conditions
			// set initial rotation
			// set initial scale
			// activate container
			// play tweens, set animations into motion
			container.transform.localScale = Vector3.zero;
			container.gameObject.SetActive(true);
			containerScale.Play();
		}

		void Deactivate()
		{
			// deactivate container
			container.gameObject.SetActive(false);
		}

		// Update is called once per frame
		void Update()
        {

        }

	}
}