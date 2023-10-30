using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonTest : MonoBehaviour
{
    [Range(3, 32)]
    [SerializeField] int vertCount = 3;

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

	}
}
