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
			Vector3[] verts = m_instance.Verts();

			if (m_instance.DrawSegmentIDs)
			{
				for (int i = 0; i < vertCount; i++)
				{
					Vector3 startPos = verts[i];
					Vector3 endPos = verts[i];
					if (i == verts.Length - 1)
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

					int drawIndex = (m_instance.IndexAtZero) ? i : i + 1;
					Handles.Label(center, drawIndex.ToString());
				}
			}

			if (m_instance.DrawHalfwayLine)
			{
				//get our bisecting line
				int halfwayPoint = (vertCount / 2);
				Handles.DrawLine(verts[0], verts[halfwayPoint]);
			}
		}
	}
}
