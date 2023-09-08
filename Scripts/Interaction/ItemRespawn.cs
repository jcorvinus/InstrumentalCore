using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public class ItemRespawn : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

		private void OnTriggerEnter(Collider other)
		{
            InteractiveItem item = other.GetComponentInParent<InteractiveItem>();
            if (item) item.Respawn();
		}
	}
}