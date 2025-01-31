using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Core.Math;

namespace Instrumental.Interaction.Slottables
{
    public class SlottableItem : MonoBehaviour
    {
        InteractiveItem interactiveItem;
        ItemSlot ownerSlot;
        ItemSlot targetSlot; /// <summary>this is the slot we are currently the best fit for./// </summary>
        List<ItemSlot> nearbySlots;
        Lens<bool> slotGravityLens;
        bool hasSettled = false;
        float hoverTimer = 0;
        const float hoverDuration = 0.65f;

        // need to figure out if a slot can own us but not be attached (usually because this object has been
        // grasped and needs to reserve the slot for re-attach

        public bool IsAttached { get { return ownerSlot && ownerSlot.AttachedItem == this; } }
        public InteractiveItem InteractiveItem { get { return interactiveItem; } }

        private void Awake()
        {
            interactiveItem = GetComponent<InteractiveItem>();
            nearbySlots = new List<ItemSlot>();
        }

        // Start is called before the first frame update
        void Start()
        {
			// add events
			interactiveItem.OnGrasped += InteractiveItem_OnGrasped;
			interactiveItem.OnUngrasped += InteractiveItem_OnUngrasped;
            slotGravityLens = new Lens<bool>(2, (previousValue) => IsAttached ? false : previousValue);
            interactiveItem.Gravity.AddLens(slotGravityLens); // returning null
        }

		private void InteractiveItem_OnUngrasped(InteractiveItem sender)
		{
            float sqrDistance = 0;
            ItemSlot closestSlot = GetClosestSlot(out sqrDistance);
            float distance = Mathf.Sqrt(sqrDistance);

            bool didAttach = closestSlot && closestSlot.AttachDistance > distance;

            Debug.Log((didAttach ? "Attach! " : "No attach ") +
                "Slot: " + 
                ((closestSlot) ?  closestSlot.name : "null")
                + " distance " + distance);

            if (didAttach)
			{
                Attach(closestSlot);
			}
		}

		private void InteractiveItem_OnGrasped(InteractiveItem sender)
		{
            if (ownerSlot) ownerSlot.Detach();
		}



		// Update is called once per frame
		void Update()
        {

        }

        public void Attach(ItemSlot slot)
		{
            targetSlot = slot;

            if(ownerSlot)
			{
                ownerSlot.Detach();
			}

            targetSlot.ItemNotifyAttached(this);
            ownerSlot = targetSlot;
            hasSettled = false;
            hoverTimer = 0;
		}

        public void Detach()
		{
            ownerSlot = null;
            hasSettled = false;
            hoverTimer = 0;
		}
        
        /// <summary>
        /// Returns the square distance to the closest available slot
        /// </summary>
        /// <returns></returns>
        ItemSlot GetClosestSlot(out float sqrDistance)
		{
            ItemSlot closestSlot = null;
            sqrDistance = float.PositiveInfinity;

            foreach(ItemSlot currentSlot in ItemSlot.AllSlots)
			{
                if(!currentSlot.AttachedItem)
				{
					// this technique won't work on some sizes of objects - we might want to replace with distance
					// to the closest point on the object, but that's more computation. Figure this tradeoff later
					Vect3 slotPosition = (Vect3)currentSlot.transform.position;
					Vect3 itemCenter = (Vect3)interactiveItem.RigidBody.position;

					Vect3 offset = slotPosition - itemCenter;
                    float currentSqrDistance = offset.sqrMagnitude;

                    if(currentSqrDistance < sqrDistance)
					{
                        closestSlot = currentSlot;
                        sqrDistance = currentSqrDistance;
					}
				}
			}

            return closestSlot;
		}

        const float maxMovementSpeed = 2f;
        AnimationCurve distanceMotionCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0),
            new Keyframe(0.02f, 0.3f, 0, 0));

        private void FixedUpdate()
		{
            // is attached
            if (IsAttached)
            {
                Rigidbody rigidBody = interactiveItem.RigidBody;

                if (!hasSettled)
                {
					Vect3 ownerSlotPosition = (Vect3)ownerSlot.transform.position;
					Vect3 rigidBodyPosition = (Vect3)rigidBody.position;

					Vect3 targetVelocity = Core.Math.Math.CalculateSingleShotVelocity(ownerSlotPosition,
                        rigidBodyPosition, Core.Time.fixedDeltaTime);
					Vect3 targetAngularVelocity = Core.Math.Math.CalculateSingleShotAngularVelocity(ownerSlot.transform.rotation,
                        rigidBody.rotation, Core.Time.fixedDeltaTime);

                    float targetSpeedSquared = targetVelocity.sqrMagnitude;
                    if (targetSpeedSquared > maxMovementSpeed)
                    {
                        float percent = maxMovementSpeed / Mathf.Sqrt(targetSpeedSquared);
                        targetVelocity *= percent;
                        targetAngularVelocity *= percent;
                    }

                    float remainingDistance = Vector3.Distance(rigidBody.position, ownerSlot.transform.position);
                    float strength = distanceMotionCurve.Evaluate(remainingDistance);

					Vect3 rigidBodyVelocity = (Vect3)rigidBody.velocity;
					Vect3 rigidBodyAngularVelocity = (Vect3)rigidBody.angularVelocity;
					Vect3 lerpedVelocity = Vect3.Lerp(rigidBodyVelocity, targetVelocity, strength);
                    Vect3 lerpedAngularVelocity = Vect3.Lerp(rigidBodyAngularVelocity, targetAngularVelocity, strength);

                    rigidBody.velocity = (Vector3)lerpedVelocity;
                    rigidBody.angularVelocity = (Vector3)lerpedAngularVelocity;

                    if (remainingDistance <= Mathf.Epsilon)
                    {
                        hasSettled = true;
                    }
                }
                else
				{
                    // get our hover distance and nudge towards the hand
                    hoverTimer += (interactiveItem.IsHovering) ? Core.Time.fixedDeltaTime : -Core.Time.fixedDeltaTime;
                    hoverTimer = Mathf.Clamp(hoverTimer, 0, hoverDuration);
                    float hoverTValue = Mathf.InverseLerp(0, hoverDuration, hoverTimer);
                    Vector3 direction = interactiveItem.HoverPoint - ownerSlot.transform.position;

                    float distance = direction.magnitude;
                    direction /= distance;

                    float targetDistance = (distance * 0.5f);
                    float hoverDistance = (interactiveItem.HoverDistance * 0.5f);

                    Vector3 goalPosition = ownerSlot.transform.position + (direction * ((targetDistance < hoverDistance) ? targetDistance : hoverDistance));
                    rigidBody.position = Vector3.Lerp(ownerSlot.transform.position, goalPosition, hoverTValue);

                    rigidBody.angularVelocity = Vector3.zero;
                    rigidBody.rotation = ownerSlot.transform.rotation;
				}
            }
            else
            {
                // is not attached
                // this is where we might want to check against 'thrown' and do a trajectory attach check
            }
		}
	}
}