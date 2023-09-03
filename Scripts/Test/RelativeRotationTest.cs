using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeRotationTest : MonoBehaviour
{
    [SerializeField] Transform root;
    [SerializeField] Transform child;
    [SerializeField] Transform grandChild;
    [SerializeField] Transform tip;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnDrawGizmos()
	{
        // get the relative offset
        Quaternion inverse = child.rotation * Quaternion.Inverse(root.rotation);
        Vector3 inverseEuler = inverse.eulerAngles;
        Vector3 childLocalEuler = child.localRotation.eulerAngles;
        Debug.Log(string.Format("Inverse: {0} child: {1}",
            inverseEuler.x, childLocalEuler.x));

        Quaternion grandChildInverse = grandChild.rotation * Quaternion.Inverse(child.rotation);
        Vector3 grandChildInverseEuler = grandChildInverse.eulerAngles;
        Vector3 grandchildLocalEuler = grandChild.localRotation.eulerAngles;

        Debug.Log(string.Format("grandchild Inverse: {0} grandChild: {1}",
            grandChildInverseEuler.x, grandchildLocalEuler.x));

        Gizmos.DrawLine(root.position, child.position);
        Gizmos.DrawLine(child.position, grandChild.position);
        Gizmos.DrawLine(grandChild.position, tip.position);
    }
}
