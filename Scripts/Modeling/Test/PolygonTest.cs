using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonTest : MonoBehaviour
{
    [Range(3, 32)]
    [SerializeField] int vertCount = 3;

    Vector3[] verts;
    float angleIncrement;

    public int VertCount { get { return vertCount; } }

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
        verts = new Vector3[vertCount];

        angleIncrement = 360 / vertCount;

        for(int i=0; i < vertCount; i++)
		{
            Vector3 point = Vector3.up;

            Quaternion rotatiton = Quaternion.AngleAxis(angleIncrement * i, Vector3.forward);
            point = rotatiton * point;
            verts[i] = point;
		}

        bool canQuad = (vertCount % 2 == 0);
        Gizmos.color = (canQuad) ? Color.green : Color.red;
        for(int i=0; i < vertCount; i++)
		{
            if (i < vertCount - 1) Gizmos.DrawLine(verts[i], verts[i + 1]);
            else Gizmos.DrawLine(verts[i], verts[0]);
		}

        // we need to figure out how many triangles we need
        // then figure out connectivity for each one
	}
}
