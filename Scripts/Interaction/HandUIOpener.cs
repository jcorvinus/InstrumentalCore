using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction.Triggers;
using Instrumental.Tweening;

namespace Instrumental.Interaction
{
    public class HandUIOpener : MonoBehaviour
    {
        ViewDirectionTrigger viewDirection;
        Transform container;
        TweenScale containerScale;

		// todo: rotation tweener
		// find a way to make it look at the head?

		private void Awake()
		{
            container = transform.GetChild(0);
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}