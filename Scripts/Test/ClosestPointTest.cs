using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Core.Math;

namespace Instrumental.Interaction
{
    public class ClosestPointTest : MonoBehaviour
    {
        [SerializeField] Transform referencePoint;

        Collider[] itemColliders;
        [SerializeField] Rigidbody rigidBody;

        // used for doing the stack walk test
        [SerializeField] Collider stackWalkCollider;
        float radius;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            itemColliders = GetComponentsInChildren<Collider>();
        }

        // Start is called before the first frame update
        void Start()
        {
            CalculateRadius(itemColliders);
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

                    if (closestDistance > squareDistance)
                    {
                        closestPoint = closestPointOnCollider;
                    }

                    foundValidCollider = true;
                }

                isPointInside = false;
                return foundValidCollider;
            }
        }

        void StackWalk(Transform currentTransform, ref List<Transform> stack, Rigidbody rootBody)
        {
            if (currentTransform == null)
            {
                Debug.Log("Stackwalk hit scene root, this is bad");
            }
            else if (currentTransform.gameObject.GetInstanceID() != rootBody.gameObject.GetInstanceID())
            {
                stack.Add(currentTransform);
                Debug.Log("Added " + currentTransform.name + " to stack, now has " + stack.Count);
                StackWalk(currentTransform.parent, ref stack, rigidBody);
            }
            else
            {
                //stack.Add(currentTransform);
                Debug.Log("Finished stack at " + currentTransform.name + ", now has " + stack.Count);
            }
        }

        Bounds GetColliderLocalBounds(Collider _collider)
        {
            Vector3 center = Vector3.zero;
            Vector3 size = Vector3.zero;

            if (_collider is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)_collider;
                center = boxCollider.center;
                size = boxCollider.size;
            }
            else if (_collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)_collider;
                center = sphereCollider.center;
                size = Vector3.one * sphereCollider.radius * 2;
            }
            else if (_collider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider = (CapsuleCollider)_collider;
                center = capsuleCollider.center;

                switch (capsuleCollider.direction)
                {
                    case (0): // x
                        size = new Vector3(capsuleCollider.height, capsuleCollider.radius * 2, capsuleCollider.radius * 2);
                        break;
                    case (1): // y
                        size = new Vector3(capsuleCollider.radius * 2, capsuleCollider.height, capsuleCollider.radius * 2);
                        break;
                    case (2): // z
                        size = new Vector3(capsuleCollider.radius * 2, capsuleCollider.radius * 2, capsuleCollider.height);
                        break;

                    default:
                        break;
                }
            }
            else if (_collider is MeshCollider)
            {
                MeshCollider meshCollider = (MeshCollider)_collider;
                center = meshCollider.sharedMesh.bounds.center;
                size = meshCollider.sharedMesh.bounds.size;
            }

            return new Bounds(center, size);
        }

        BoundsVertices GetVerticesForCollider(Collider _collider)
        {
            // we might not need this anymore, look into optimizing it out
            Bounds bounds = GetColliderLocalBounds(_collider);

            // transform our vertices stack walked upwards to the root
            List<Transform> transformStack = new List<Transform>();

            StackWalk(_collider.transform, ref transformStack, rigidBody);

            // in a bounds configuration:
            // v1 is down front left, v2 is down front right, v3 is down back left, v4 is down back right
            Vector3 v1=bounds.center, v2=bounds.center, v3=bounds.center, v4=bounds.center,
                // v5 is up front left, v6 is up front right, v7 is up back left, v8 is up back right
                v5=bounds.center, v6=bounds.center, v7=bounds.center, v8=bounds.center;

            if (_collider is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)_collider;
                v1 = sphereCollider.center + (Vector3.up * sphereCollider.radius);
                v2 = sphereCollider.center + (Vector3.down * sphereCollider.radius);
                v3 = sphereCollider.center + (Vector3.forward * sphereCollider.radius);
                v4 = sphereCollider.center + (Vector3.back * sphereCollider.radius);
                v5 = sphereCollider.center + (Vector3.left * sphereCollider.radius);
                v6 = sphereCollider.center + (Vector3.right * sphereCollider.radius);
                v7 = sphereCollider.center;
                v8 = sphereCollider.center;
            }
            else if (_collider is CapsuleCollider)
			{
                CapsuleCollider capsuleCollider = (CapsuleCollider)_collider;

                Vector3 direction = Vector3.zero;
                Vector3 radiusDirA=Vector3.zero, radiusDirB=Vector3.zero;
				switch (capsuleCollider.direction)
				{
                    case (0):
                        direction = Vector3.right;
                        radiusDirA = Vector3.forward;
                        radiusDirB = Vector3.up;
                        break;

                    case (1):
                        direction = Vector3.up;
                        radiusDirA = Vector3.right;
                        radiusDirB = Vector3.forward;
                        break;

                    case (2):
                        direction = Vector3.right;
                        radiusDirA = Vector3.forward;
                        radiusDirB = Vector3.up;
                        break;

					default:
						break;
				}

                v1 = capsuleCollider.center + (direction * capsuleCollider.height * 0.5f);
                v2 = capsuleCollider.center - (direction * capsuleCollider.height * 0.5f);
                v3 = capsuleCollider.center + (radiusDirA * capsuleCollider.radius);
                v4 = capsuleCollider.center - (radiusDirA * capsuleCollider.radius);
                v5 = capsuleCollider.center + (radiusDirB * capsuleCollider.radius);
                v6 = capsuleCollider.center - (radiusDirB * capsuleCollider.radius);
                v7 = capsuleCollider.center;
                v8 = capsuleCollider.center;
            }
            else
            {
                // if mesh collider
                // todo: in the future what we can do is calculate this by making a cage around the mesh, then
                // fitting those points to the closest point on mesh.
                // we can then store this for a given collider so that we never have to re-calculate this 
                // fitted sparse collection of points for a given mesh collider.
                v1 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v2 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);
                v3 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v4 = bounds.center + (Vector3.down * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);

                v5 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v6 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.forward * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);
                v7 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.left * bounds.size.x * 0.5f);
                v8 = bounds.center + (Vector3.up * bounds.size.y * 0.5f) + (Vector3.back * bounds.size.z * 0.5f) + (Vector3.right * bounds.size.x * 0.5f);
            }

            for (int i = 0; i < transformStack.Count; i++)
            {
                Transform currentTransform = transformStack[i];
                Matrix4x4 trs = Matrix4x4.TRS(currentTransform.localPosition, currentTransform.localRotation, currentTransform.localScale);
                v1 = trs.MultiplyPoint3x4(v1);
                v2 = trs.MultiplyPoint3x4(v2);
                v3 = trs.MultiplyPoint3x4(v3);
                v4 = trs.MultiplyPoint3x4(v4);
                v5 = trs.MultiplyPoint3x4(v5);
                v6 = trs.MultiplyPoint3x4(v6);
                v7 = trs.MultiplyPoint3x4(v7);
                v8 = trs.MultiplyPoint3x4(v8);
            }

            return new BoundsVertices()
            {
                v1 = (Vect3)v1,
                v2 = (Vect3)v2,
                v3 = (Vect3)v3,
                v4 = (Vect3)v4,
                v5 = (Vect3)v5,
                v6 = (Vect3)v6,
                v7 = (Vect3)v7,
                v8 = (Vect3)v8
            };
        }

        void CalculateRadius(Collider[] colliders)
        {
            Vector3 furthestPoint = rigidBody.centerOfMass;
            float furthestSqrDist = 0;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider currentCollider = colliders[i];
                BoundsVertices boundsVertices = GetVerticesForCollider(currentCollider);

                for (int v = 0; v < 8; v++)
                {
                    Vector3 vertex = (Vector3)boundsVertices.GetVertex(v);
                    Vector3 offset = vertex - rigidBody.centerOfMass;
                    float sqrMag = offset.sqrMagnitude;

                    if (sqrMag > furthestSqrDist)
                    {
                        furthestPoint = vertex;
                        furthestSqrDist = sqrMag;
                    }
                }
            }

            radius = (furthestSqrDist > 0) ? Vector3.Distance(rigidBody.centerOfMass, furthestPoint) : 0; //Mathf.Sqrt(furthestSqrDist)
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if (referencePoint)
                {
                    Vector3 closestPoint = referencePoint.position;
                    bool isInside = false;

                    if (ClosestPointOnItem(referencePoint.position, out closestPoint, out isInside))
                    {
                        Gizmos.DrawLine(referencePoint.position, closestPoint);
                        Gizmos.color = isInside ? Color.blue : Color.green;
                        Gizmos.DrawWireSphere(closestPoint, 0.01f);
                    }
                }
            }

            if (stackWalkCollider && rigidBody)
            {
                BoundsVertices stackWalkColliderVertices = GetVerticesForCollider(stackWalkCollider);

                // then set our gizmo transform to the root's object2world matrix
                Gizmos.matrix = rigidBody.transform.localToWorldMatrix;

                // then draw the vertices
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v1, 0.01f);
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v2, 0.01f);
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v3, 0.01f);
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v4, 0.01f);
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v5, 0.01f);
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v6, 0.01f);
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v7, 0.01f);
                Gizmos.DrawWireSphere((Vector3)stackWalkColliderVertices.v8, 0.01f);
            }

            if (rigidBody)
            {
                Gizmos.DrawWireSphere(rigidBody.worldCenterOfMass, radius * rigidBody.transform.lossyScale.x);
            }
        }
    }
}