using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental.Interaction.Slottables
{
    public class SlottableItem : MonoBehaviour
    {
        InteractiveItem interactiveItem;
        ItemSlot ownerSlot;
        ItemSlot targetSlot; /// <summary>this is the slot we are currently the best fit for./// </summary>
        List<ItemSlot> nearbySlots;
        Lens<bool> slotGravityLens;

        // need to figure out if a slot can own us but not be attached (usually because this object has been
        // grasped and needs to reserve the slot for re-attach

        public bool IsAttached { get { return ownerSlot && ownerSlot.AttachedItem == this; } }

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

            if(closestSlot && closestSlot.AttachDistance > distance)
			{
                Debug.Log("Attach! Slot: " + closestSlot.name + " distance " + distance);
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

        void Attach(ItemSlot slot)
		{
            targetSlot = slot;

            if(ownerSlot)
			{
                ownerSlot.Detach();
			}

            targetSlot.ItemNotifyAttached(this);
            ownerSlot = targetSlot;
		}

        public void Detach()
		{
            ownerSlot = null;
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
                    Vector3 offset = currentSlot.transform.position - interactiveItem.RigidBody.centerOfMass;
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

        // these also exist in slottable item - consider refactoring so we're not duplicating code.
        Vector3 CalculateSingleShotVelocity(Vector3 position, Vector3 previousPosition,
            float deltaTime)
        {
            float velocityFactor = 1.0f / deltaTime;
            return velocityFactor * (position - previousPosition);
        }

        Vector3 CalculateSingleShotAngularVelocity(Quaternion rotation, Quaternion previousRotation,
            float deltaTime)
        {
            Quaternion deltaRotation = rotation * Quaternion.Inverse(previousRotation);

            Vector3 deltaAxis;
            float deltaAngle;

            deltaRotation.ToAngleAxis(out deltaAngle, out deltaAxis);

            if (float.IsInfinity(deltaAxis.x))
            {
                deltaAxis = Vector3.zero;
                deltaAngle = 0;
            }

            if (deltaAngle > 180)
            {
                deltaAngle -= 360.0f;
            }

            Vector3 angularVelocity = deltaAxis * deltaAngle * Mathf.Deg2Rad / deltaTime;

            return angularVelocity;
        }

        const float maxMovementSpeed = 6f;
        AnimationCurve distanceMotionCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0),
            new Keyframe(0.02f, 0.3f, 0, 0));

        private void FixedUpdate()
		{
            // is attached
            if (IsAttached)
            {
                Rigidbody rigidBody = interactiveItem.RigidBody;

                // has settled
                // is hovering
                Vector3 targetVelocity = CalculateSingleShotVelocity(ownerSlot.transform.position,
                    rigidBody.position, Time.fixedDeltaTime);
                Vector3 targetAngularVelocity = CalculateSingleShotAngularVelocity(ownerSlot.transform.rotation,
                    rigidBody.rotation, Time.fixedDeltaTime);

                float targetSpeedSquared = targetVelocity.sqrMagnitude;
                if (targetSpeedSquared > maxMovementSpeed)
                {
                    float percent = maxMovementSpeed / Mathf.Sqrt(targetSpeedSquared);
                    targetVelocity *= percent;
                    targetAngularVelocity *= percent;
                }

                float remainingDistance = Vector3.Distance(rigidBody.position, ownerSlot.transform.position);
                float strength = distanceMotionCurve.Evaluate(remainingDistance);

                Vector3 lerpedVelocity = Vector3.Lerp(rigidBody.velocity, targetVelocity, strength);
                Vector3 lerpedAngularVelocity = Vector3.Lerp(rigidBody.angularVelocity, targetAngularVelocity, strength);

                rigidBody.velocity = lerpedVelocity;
                rigidBody.angularVelocity = lerpedAngularVelocity;
            }
            else
            {
                // is not attached
                
            }
		}
	}
}