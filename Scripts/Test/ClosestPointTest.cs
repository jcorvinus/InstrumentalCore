using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestPointTest : MonoBehaviour
{
    [SerializeField] Transform referencePoint;

    Collider[] itemColliders;
    Rigidbody rigidBody;

	private void Awake()
	{
        rigidBody = GetComponent<Rigidbody>();
        itemColliders = GetComponentsInChildren<Collider>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool ClosestPointOnItem(Vector3 position, out Vector3 closestPoint,
        out bool isPointInside)
    {
        float closestDistance = float.PositiveInfinity;
        closestPoint = position;

        if (!rigidBody || itemColliders == null ||
            itemColliders.Length == 0)
        {
            isPointInside = false;
            return false;
        }
        else
        {
            bool foundValidCollider = false;

            for (int i = 0; i < itemColliders.Length; i++)
            {
                Collider testCollider = itemColliders[i];
                Vector3 closestPointOnCollider = testCollider.ClosestPoint(closestPoint);

                isPointInside = (closestPointOnCollider == position);

                if (isPointInside)
				{
                    return true;
				}

                float squareDistance = (position - closestPointOnCollider).sqrMagnitude;

                if(closestDistance > squareDistance)
				{
                    closestPoint = closestPointOnCollider;
				}

                foundValidCollider = true;
            }

            isPointInside = false;
            return foundValidCollider;
        }
    }

	private void OnDrawGizmos()
	{
		if(Application.isPlaying)
		{
            if(referencePoint)
			{
                Vector3 closestPoint = referencePoint.position;
                bool isInside = false;

                if(ClosestPointOnItem(referencePoint.position, out closestPoint, out isInside))
				{
                    Gizmos.DrawLine(referencePoint.position, closestPoint);
                    Gizmos.color = isInside ? Color.blue : Color.green;
                    Gizmos.DrawWireSphere(closestPoint, 0.01f);
				}
			}
		}
	}
}
