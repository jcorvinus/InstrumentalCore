using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public class ItemRespawn : MonoBehaviour
    {
        [SerializeField] InteractiveItem item;
        Vector3 startingPosition;
        Quaternion startingRotation;

        // Start is called before the first frame update
        void Start()
        {
            startingPosition = item.transform.position;
            startingRotation = item.transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {

        }

		private void OnTriggerEnter(Collider other)
		{
			if(other.gameObject == item.gameObject)
			{
                item.RigidBody.velocity = Vector3.zero;
                item.RigidBody.angularVelocity = Vector3.zero;

                item.RigidBody.position = startingPosition;
                item.RigidBody.rotation = startingRotation;
			}
		}
	}
}