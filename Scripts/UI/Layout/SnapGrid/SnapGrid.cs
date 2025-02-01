using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Instrumental.Core;
using Instrumental.Core.Math;

// why is SnapGrid not in a namespace?
public class SnapGrid : MonoBehaviour
{
    private static List<SnapGrid> snapGrids;

    public static int SnapGridCount()
    {
        if (snapGrids != null) return snapGrids.Count;
        else return 0;
    }

    public static SnapGrid GetGridForIndex(int index)
    {
        return snapGrids[index];
    }

    public enum GridType { TwoDimensional = 0, ThreeDimensional=1 }

    [SerializeField] GridType type = GridType.TwoDimensional;
    [SerializeField] Vect3 bounds = new Vect3 { x = 0.3f, y = 0, z = 0.15f };
    [SerializeField] float cellDimension = 0.02f;
    [SerializeField] Transform referencePoint;

    public GridType Type { get { return type; } }


	private void Awake()
	{
		if(snapGrids == null)
		{
            snapGrids = new List<SnapGrid>();
		}

        snapGrids.Add(this);
	}

	private void OnValidate()
	{
        transform.localScale = Vector3.one; // do not let the scale change
    }

	private void OnDestroy()
	{
		if(snapGrids != null)
		{
            snapGrids.Remove(this);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.one; // do not let the scale change
    }

    int GetWidthSteps()
	{
        return (int)(bounds.x / cellDimension);
	}

    int GetDepthSteps()
	{
        return (int)(bounds.z / cellDimension);
	}

    int GetHeightSteps()
	{
        return (int)(bounds.y / cellDimension);
	}

    public Vect3 GetSnappedPosition(Vect3 input)
	{
		Vect3 localPosition = (Vect3)transform.InverseTransformPoint((Vector3)input);
        int xSteps = Mathf.RoundToInt(localPosition.x / cellDimension); // one thing I think we can do is divide localposition.x by cell dimensions?
        float x = xSteps * cellDimension;

        int ySteps = Mathf.RoundToInt(localPosition.y / cellDimension);
        float y = (type == GridType.TwoDimensional) ? 0 : ySteps * cellDimension;

        int zSteps = Mathf.RoundToInt(localPosition.z / cellDimension);
        float z = zSteps * cellDimension;
        return (Vect3)transform.TransformPoint(new Vector3(x, y, z));
	}

    public bool IsInBounds(Vect3 localPoint)
	{
        Bounds gridBounds = new Bounds(Vector3.zero, 
            (type == GridType.TwoDimensional) ? new Vector3(bounds.x, 0.1f, bounds.z) : (Vector3)bounds);

        return gridBounds.Contains((Vector3)localPoint);
	}

	private void OnDrawGizmos()
	{
#if UNITY
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        // draw width
        float widthStartPos = -bounds.x * 0.5f;
        int widthSteps = GetWidthSteps();

        Gizmos.color = Color.red;
        for(int w=0; w < widthSteps; w++)
		{
            float withPos = widthStartPos + ( w * cellDimension);
            Vector3 start, end;
            start = new Vector3(withPos, 0, -bounds.z * 0.5f);
            end = new Vector3(withPos, 0, bounds.z * 0.5f);
            Gizmos.DrawLine(start, end);
		}

        // draw depth
        float depthStartPos = -bounds.z * 0.5f;
        int depthSteps = GetDepthSteps();

        Gizmos.color = Color.blue;
        for(int d=0; d < depthSteps; d++)
		{
            float depthPos = depthStartPos + (d * cellDimension);
            Vector3 start, end;
            start = new Vector3(-bounds.x * 0.5f, 0, depthPos);
            end = new Vector3(bounds.x * 0.5f, 0, depthPos);
            Gizmos.DrawLine(start, end);
        }
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere((Vector3)GetSnappedPosition((Vect3)referencePoint.position), 0.01f);
#endif
	}
}
