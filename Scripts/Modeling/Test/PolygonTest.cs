using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonTest : MonoBehaviour
{
    [Range(3, 32)]
    [SerializeField] int vertCount = 3;

    [SerializeField] int stripIterCount = 3;

    [SerializeField] bool indexAtZero = false;

    [SerializeField] bool drawHalfwayLine = false;

    [SerializeField] bool drawSegmentIDs = false;

    [Range(0, 2)]
    [SerializeField] int drawTriangleCount = 0;

    [SerializeField] bool debugTris = false;

    public int VertCount { get { return vertCount; } }
    public bool IndexAtZero { get { return indexAtZero; } }
    public int StripIterCount { get { return stripIterCount; } }

    public bool DrawHalfwayLine { get { return drawHalfwayLine; } }

    public bool DrawSegmentIDs { get { return drawSegmentIDs; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3[] Verts()
	{
        Vector3[] verts = new Vector3[vertCount];

        float angleIncrement = 360f / (float)vertCount;

        for (int i = 0; i < vertCount; i++)
        {
            Vector3 point = Vector3.up;

            Quaternion rotatiton = Quaternion.AngleAxis(angleIncrement * i, Vector3.forward);
            point = rotatiton * point;
            verts[i] = point;
        }

        return verts;
	}

	private void OnDrawGizmos()
	{
        Vector3[] verts = Verts();

		bool isOdd = (vertCount % 2 != 0);
		Gizmos.color = (isOdd) ? Color.green : Color.red;

        if (!debugTris)
        {
            // todo: restore seg and opp drawing
            for (int i = 0; i < vertCount; i++)
            {
                if (i < vertCount - 1) Gizmos.DrawLine(verts[i], verts[i + 1]);
                else Gizmos.DrawLine(verts[i], verts[0]);
            }
        }
        else
        {
            int halfwayPoint = (vertCount / 2);

            // draw our segments
            int segIter = halfwayPoint; //Mathf.Min(stripIterCount, halfwayPoint);

            // the next thing we need to do here is make it so that we can calculate the triangle count
            // ahead of time.
            int calculatedTriCount = 0;
            int predictedTriCount = (vertCount) - 2;//((isOdd) ? 2 : 0); // this is wrong now

            for (int i = 0; i < segIter; i++)
            {
                int opposite = (vertCount - 1) - i;

                // draw our current and opposite segments
                int segA, segB;
                segA = i;
                segB = i + 1;

                int oppA, oppB;
                oppA = (opposite == vertCount - 1) ? 0 : opposite;
                oppB = (opposite == vertCount - 1) ? opposite : opposite + 1;

                float colorIntensity = (i == stripIterCount - 1) ? 1 : 0.35f;

                bool isFirst = i == 0;
                bool isFirstOrLast = (isFirst) || (i == halfwayPoint - 1);
                bool vertCountLessThanFour = (vertCount <= 4);
                bool isTri = isFirst || vertCountLessThanFour || (isFirstOrLast && !isOdd);

                bool shouldDraw = (i == stripIterCount);
                if (isTri)
                {
                    if (shouldDraw)
                    {
                        if (drawTriangleCount == 1)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(verts[segA], verts[segB]);
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(verts[segB], verts[oppB]); // overdraw, degenerate line
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(verts[oppB], verts[segA]);
                        }
                    }

                    calculatedTriCount++;
                }
                else
                {
                    // first triangle
                    if (shouldDraw)
                    {
                        if (drawTriangleCount == 1)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(verts[segA], verts[segB]);
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(verts[segB], verts[oppB]); // overdraw, degenerate line
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(verts[oppB], verts[segA]);
                        }
                        else if (drawTriangleCount == 2)
                        {
                            // second triangle
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(verts[segB], verts[oppA]);
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(verts[oppA], verts[oppB]); // across
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(verts[oppB], verts[segB]); // diagonal
                        }
                    }

                    calculatedTriCount += 2;
                }
            }

            Debug.Log(string.Format("Processed {0} triangles, expected {1}", calculatedTriCount, predictedTriCount));
        }
    }
}
