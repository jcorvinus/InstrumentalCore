using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Instrumental.Core;
using Instrumental.Core.Math;

public class ICPoseTest : MonoBehaviour
{
	private Vector3 basePosition = new Vector3(0.1707176f, 0.7975423f, 0.3463634f);
	private Quaternion baseRotation = new Quaternion(-0.02928568f, -0.1187174f, 0.05819362f, 0.9907886f);

	// steamVR palm offsets
	const float palmForwardOffset = 0.0153f;
	const float palmUpOffset = 0.06f;
	const float palmRightOffset = 0.0074f;

	bool isLeft = true;
	[SerializeField] bool doTest = false;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

	void DoTest()
	{
		Vector3 uVect = new Vector3(0.1f, 0.2f, 0.3f);
		Vect3 castVect = (Vect3)uVect;
		Vector3 castBack = (Vector3)castVect;

		// base unity struct pass
		Quaternion combinedRotation = baseRotation;
		float palmDirOffset = (isLeft) ? palmRightOffset : -palmRightOffset;
		Vector3 palmOffset = (Vector3.right * palmDirOffset) +
			(Vector3.up * -palmForwardOffset) + (Vector3.forward * palmUpOffset);
		palmOffset = combinedRotation * palmOffset;
		Pose pose = new Pose(basePosition + palmOffset,
			Quaternion.LookRotation(combinedRotation * Vector3.up * -1,
			combinedRotation * Vector3.forward));

		// IC structs pass
		Quatn combinedQuat = (Quatn)baseRotation;

		float icPalmDirOffset = (isLeft) ? palmRightOffset : -palmRightOffset;
		Vect3 icPalmBase = (Vect3)basePosition;
		Vect3 icPalmOffset = (Vect3.right * icPalmDirOffset) +
			(Vect3.up * -palmForwardOffset) + (Vect3.forward * palmUpOffset);
		icPalmOffset = combinedQuat * icPalmOffset;

		Quatn icPalmRotation = Quatn.LookRotation(combinedQuat * (Vect3.up * -1),
			combinedQuat * Vect3.forward);
		PoseIC icPose = new PoseIC();
		icPose.position = icPalmBase + icPalmOffset;
		icPose.rotation = icPalmRotation;

		Debug.Break();
	}

    // Update is called once per frame
    void Update()
    {

    }

	private void OnDrawGizmos()
	{
		if (doTest)
		{
			doTest = false;
			DoTest();
		}
	}
}
