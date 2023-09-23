using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Slottables
{
    public class ItemSlot : MonoBehaviour
    {
        // events?
        // on attached
        // on detached

        private static HashSet<ItemSlot> allSlots;
        public static HashSet<ItemSlot> AllSlots 
        {
            get 
            {
                if (allSlots == null) allSlots = new HashSet<ItemSlot>();
                return allSlots;
            }
        }

        // add a reference for the slottable item here
        SlottableItem attachedItem;
        public SlottableItem AttachedItem { get { return attachedItem; } }

        // add a reference to the bounds object here
        MeshRenderer boundsPreview;
        [Tooltip("If true, the bounds preview mesh will be hidden when an item is attached")]
        [SerializeField] bool boundsPreviewHideOnAttached = true;

        // is attached state

        // size handling
        // size of slot
        [SerializeField] float size = 0.3f;
        [Range(0, 0.3f)]
        [SerializeField] float extraSlotRadius = 0.01f;
		// size category (infer from slot size)
		// does resize

        public float Size { get { return size; } }
        public float AttachDistance { get { return size + extraSlotRadius; } }

		// does consume

		private void Awake()
		{
            boundsPreview = GetComponentInChildren<MeshRenderer>();
		}

		private void OnEnable()
		{
            AllSlots.Add(this);
		}

		private void OnDisable()
		{
            AllSlots.Remove(this);
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        /// <summary>
        /// Items are in charge of attaching to us.
        /// This might be a weak or bad idea but I just want to get it working now.
        /// </summary>
        /// <param name="item"></param>
        public void ItemNotifyAttached(SlottableItem item)
		{
            attachedItem = item;
		}

        public void Detach()
		{
            attachedItem.Detach();
            attachedItem = null;
		}

		// Update is called once per frame
		void Update()
        {
            boundsPreview.enabled = !attachedItem;
        }

		private void OnDrawGizmos()
		{
            Gizmos.DrawWireSphere(transform.position, size);
            Gizmos.DrawWireSphere(transform.position, AttachDistance);
        }
	}
}