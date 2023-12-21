using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Instrumental.Interaction.Input
{
	public class HandInputModule : BaseInputModule
	{
		private Camera screenCamera;
		private HandPointer leftPointer;
		private HandPointer rightPointer;

		public Camera ScreenCamera { get { return screenCamera; } }

		protected override void Start()
		{
			base.Start();

			Transform cameraCandidate = InstrumentalBody.Instance.Head.Find("ScreenCamera");

			if(cameraCandidate)
			{
				screenCamera = cameraCandidate.GetComponent<Camera>();
			}

			Canvas[] canvases = FindObjectsOfType<Canvas>();
			for(int i=0; i < canvases.Length; i++)
			{
				if (!canvases[i].worldCamera) canvases[i].worldCamera = screenCamera;
			}

			leftPointer = transform.GetChild(0).GetComponent<HandPointer>();
			rightPointer = transform.GetChild(1).GetComponent<HandPointer>();
		}

		public override void Process()
		{
			leftPointer.Process();
			rightPointer.Process();
		}

		public new static RaycastResult FindFirstRaycast(List<RaycastResult> results)
		{
			return BaseInputModule.FindFirstRaycast(results);
		}

		public override bool ShouldActivateModule()
		{
			return true;
		}
	}
}