using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Interaction.Slottables;

namespace Instrumental.Controls
{
    public class CloneSlot : MonoBehaviour
    {
        public delegate void CloneHandler();
        public event CloneHandler Cloned;

        float pullDist = 0.15f;
        ItemSlot anchor;
        GameObject anchorTarget;

        private void Awake()
        {
            anchor = GetComponent<ItemSlot>();
            //anchor.allowMultipleObjects = false;
        }

        // Use this for initialization
        IEnumerator Start()
        {
            //anchor.allowMultipleObjects = false;

            while (!anchor.AttachedItem) yield return null;

            anchorTarget = anchor.AttachedItem.gameObject; //anchor.anchoredObjects.First(item => true).gameObject;
            InteractiveItem targetInteraction = anchor.AttachedItem.GetComponent<InteractiveItem>();
            targetInteraction.OnGrasped += OnGraspBegin;
            targetInteraction.OnUngrasped += OnGraspEnd;
        }

        // Update is called once per frame
        void Update()
        {
            if (anchorTarget)
            {
                float dist = Vector3.Distance(anchorTarget.transform.position, transform.position);
                if (dist >= pullDist)
                {
                    InteractiveItem targetInteraction = anchorTarget.GetComponent<InteractiveItem>();
                    targetInteraction.OnUngrasped -= OnGraspEnd;
                    targetInteraction.OnGrasped -= OnGraspBegin;

                    // clone stuff, manage attachment
                    GameObject clone = GameObject.Instantiate(anchorTarget, anchorTarget.transform.parent);
                    InteractiveItem cloneInteraction = clone.GetComponent<InteractiveItem>();
                    cloneInteraction.OnUngrasped += OnGraspEnd;
                    cloneInteraction.OnGrasped += OnGraspBegin;

                    // enable our anchor
                    anchor.enabled = true;
                    InteractiveItem interaction = clone.GetComponent<InteractiveItem>();
                    SlottableItem anchorable = clone.GetComponent<SlottableItem>();
                    //anchorable.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
                    interaction.RigidBody.position = transform.position;
                    interaction.RigidBody.rotation = transform.rotation;

                    //anchorable.anchor = anchor;
                    //anchorable.TryAttach(true);
                    anchorTarget = clone.gameObject;
                }
            }
        }

        void OnGraspBegin(InteractiveItem item)
        {
            // disable our anchor so that nothing else can get shoved into the slot
            // while we're out
            anchor.enabled = false;
        }

        void OnGraspEnd(InteractiveItem item)
        {
            // return item to slot!
            anchor.enabled = true;
            SlottableItem anchorable = anchorTarget.GetComponent<SlottableItem>();
            //anchorable.anchor = anchor;
            //anchorable.TryAttach(true);
        }

        private void AnchorObject(SlottableItem interaction)
        {
            anchor.enabled = true;
            interaction.InteractiveItem.RigidBody.position = anchor.transform.position;
            interaction.InteractiveItem.RigidBody.rotation = anchor.transform.rotation;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, pullDist);
        }
    }
}