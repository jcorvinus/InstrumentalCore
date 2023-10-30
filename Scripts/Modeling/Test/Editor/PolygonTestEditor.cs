using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PolygonTest))]
public class PolygonTestEditor : Editor
{
	PolygonTest m_instance;

	private void OnEnable()
	{
		m_instance = target as PolygonTest;
	}


	private void OnSceneGUI()
	{
		// we want to get a segment ID for every vertex in our polygon
		if(m_instance)
		{
			int vertCount = m_instance.VertCount;
			Vector3[] verts = new Vector3[vertCount];

			float angleIncrement = 360 / vertCount;

			for (int i = 0; i < vertCount; i++)
			{
				Vector3 point = Vector3.up;

				Quaternion rotatiton = Quaternion.AngleAxis(angleIncrement * i, Vector3.forward);
				point = rotatiton * point;
				verts[i] = point;
			}

			bool canQuad = (vertCount % 2 == 0);
			Handles.color = (canQuad) ? Color.green : Color.red;
			for (int i = 0; i < vertCount; i++)
			{
				
				if (i < vertCount - 1) Handles.DrawLine(verts[i], verts[i + 1]);
				else Handles.DrawLine(verts[i], verts[0]);
			}

			// we need to figure out how many triangles we need
			// then figure out connectivity for each one

			for (int i = 0; i < vertCount; i++)
			{
				Vector3 startPos = verts[i];
				Vector3 endPos = verts[i];
				if(i == verts.Length - 1)
				{
					endPos = verts[0];
				}
				else
				{
					endPos = verts[i + 1];
				}

				Vector3 center = (startPos + endPos) * 0.5f;
				Vector3 direction = center.normalized;
				center += (direction) * 0.1f;

				Handles.Label(center, i.ToString());
			}
		}
	}
}
