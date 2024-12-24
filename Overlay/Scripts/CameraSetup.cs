using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Valve.VR;

namespace Instrumental.Overlay
{
    public class CameraSetup : MonoBehaviour
    {
		public UnityEvent OnSetupComplete;

		[SerializeField] RenderTexture renderTexture;
		public RenderTexture RenderTexture { get { return renderTexture; } }

		Camera leftEyeCamera;
		Camera rightEyeCamera;
		[SerializeField] Camera screenCamera;
		Camera centerEyeCamera;

		[Range(0, 2)]
		[SerializeField]
		float perCameraIPDMultiplier = 1.0f; // 1.116f
		[Range(0, 2)]
		[SerializeField]
		float perCameraFovMultiplier = 1.0f; //1.284f
		[Range(0, 2)]
		[SerializeField] float viewportHeight=1;
		float fieldOfView=90;
		float aspect=1;

		[Range(0,10)]
		[SerializeField] float steamVRNearPlane = 0.11f;
		public float SteamVRNearPlane { get { return steamVRNearPlane; } }
		float combinedWidth = 0.1f;
		public float CombinedWidth { get { return combinedWidth; } }

		[SerializeField] bool useFixedDimensions = false;

		[SerializeField] SteamVR_Overlay debugOverlay;

		[SerializeField] MeshFilter leftHiddenAreaMeshFilter;
		[SerializeField] MeshFilter rightHiddenAreaMeshFilter;

		public float FieldOfView { get { return fieldOfView; } }
		public float Aspect { get { return aspect; } }
		public float nearPlane { get { return screenCamera.nearClipPlane; } }

		const UnityEngine.Experimental.Rendering.GraphicsFormat renderTextureFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
		const UnityEngine.Experimental.Rendering.GraphicsFormat depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;

		private void Awake()
		{
			Camera[] cameras = GetComponentsInChildren<Camera>();

			for(int i=0; i < cameras.Length; i++)
			{
				if (cameras[i].stereoTargetEye == StereoTargetEyeMask.Left) leftEyeCamera = cameras[i];
				else if (cameras[i].stereoTargetEye == StereoTargetEyeMask.Right) rightEyeCamera = cameras[i];
				else if (screenCamera == null && cameras[i].stereoTargetEye == StereoTargetEyeMask.None) screenCamera = cameras[i];
			}

			Vector2 tanHalfFov;
			CVRSystem hmd = SteamVR.instance.hmd;

			float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
			hmd.GetProjectionRaw(EVREye.Eye_Left, ref l_left, ref l_right, ref l_top, ref l_bottom);

			float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
			hmd.GetProjectionRaw(EVREye.Eye_Right, ref r_left, ref r_right, ref r_top, ref r_bottom);

			tanHalfFov = new Vector2(
				Mathf.Max(-l_left, l_right, -r_left, r_right),
				Mathf.Max(-l_top, l_bottom, -r_top, r_bottom));

			float leftTanHalfFov = Mathf.Max(-l_top, l_bottom);
			float rightTanHalfFov = Mathf.Max(-r_top, r_bottom);

			float leftFieldOfView = 2.0f * Mathf.Atan(leftTanHalfFov) * Mathf.Rad2Deg;
			float rightfieldOfView = 2.0f * Mathf.Atan(rightTanHalfFov) * Mathf.Rad2Deg;

			fieldOfView = 2.0f * Mathf.Atan(tanHalfFov.y) * Mathf.Rad2Deg;
			screenCamera.fieldOfView = fieldOfView;
			screenCamera.aspect = 2.358722f; // maybe 2.245614?
			aspect = screenCamera.aspect;
			Debug.Log("Starting fov: " + fieldOfView);

			// get our target resolution
			int textureWidth = (int)SteamVR.instance.sceneWidth;
			int textureHeight = (int)SteamVR.instance.sceneHeight;
			
			VRTextureBounds_t[] textureBounds = new VRTextureBounds_t[2];

			textureBounds[0].uMin = 0.5f + 0.5f * l_left / tanHalfFov.x;
			textureBounds[0].uMax = 0.5f + 0.5f * l_right / tanHalfFov.x;
			textureBounds[0].vMin = 0.5f - 0.5f * l_bottom / tanHalfFov.y;
			textureBounds[0].vMax = 0.5f - 0.5f * l_top / tanHalfFov.y;

			textureBounds[1].uMin = 0.5f + 0.5f * r_left / tanHalfFov.x;
			textureBounds[1].uMax = 0.5f + 0.5f * r_right / tanHalfFov.x;
			textureBounds[1].vMin = 0.5f - 0.5f * r_bottom / tanHalfFov.y;
			textureBounds[1].vMax = 0.5f - 0.5f * r_top / tanHalfFov.y;

			// Grow the recommended size to account for the overlapping fov
			textureWidth = (int)((float)textureWidth / Mathf.Max(textureBounds[0].uMax - textureBounds[0].uMin, textureBounds[1].uMax - textureBounds[1].uMin));
			textureHeight = (int)((float)textureHeight / Mathf.Max(textureBounds[0].vMax - textureBounds[0].vMin, textureBounds[1].vMax - textureBounds[1].vMin));

			float multiplier = 0.5f; // adding the hidden area mesh has made rendering at 1x reasonable.
			textureWidth = (int)(textureWidth * 2 * multiplier);
			textureHeight = (int)(textureHeight * multiplier);

			if (useFixedDimensions)
			{
				textureWidth = 5786;
				textureHeight = 2869;
			}

			Debug.Log("Creating render texture of resolution:" + textureWidth + " x " + textureHeight);

			renderTexture = new RenderTexture(textureWidth,
				textureHeight,
				renderTextureFormat,
				depthStencilFormat,
				0);

			if (leftEyeCamera && rightEyeCamera)
			{
				leftEyeCamera.targetTexture = renderTexture;
				rightEyeCamera.targetTexture = renderTexture;

				HmdMatrix34_t leftEyeMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Left);
				leftEyeCamera.transform.localPosition = leftEyeMatrix.GetPosition() / perCameraIPDMultiplier;
				leftEyeCamera.fieldOfView = leftFieldOfView * perCameraFovMultiplier;

				HmdMatrix34_t rightEyeMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Right);
				rightEyeCamera.transform.localPosition = rightEyeMatrix.GetPosition() / perCameraIPDMultiplier;
				rightEyeCamera.fieldOfView = rightfieldOfView * perCameraFovMultiplier;

				SetUpHiddenAreaMesh();

				combinedWidth = GetCombinedWidth(steamVRNearPlane, out var height); // used to be left cam near plane
			}
			else
			{
				centerEyeCamera = GetComponent<Camera>();
				centerEyeCamera.targetTexture = renderTexture;
				debugOverlay.texture = renderTexture;
			}

			if(OnSetupComplete != null)
			{
				OnSetupComplete.Invoke();
			}
		}

		Mesh GetMeshForVRMeshData(HiddenAreaMesh_t src, VRTextureBounds_t bounds)
		{
			if (src.unTriangleCount == 0)
				return null;

			var data = new float[src.unTriangleCount * 3 * 2]; //HmdVector2_t
			System.Runtime.InteropServices.Marshal.Copy(src.pVertexData, data, 0, data.Length);

			var vertices = new Vector3[src.unTriangleCount * 3 + 12];
			var indices = new int[src.unTriangleCount * 3 + 24];

			var x0 = 2.0f * bounds.uMin - 1.0f;
			var x1 = 2.0f * bounds.uMax - 1.0f;
			var y0 = 2.0f * bounds.vMin - 1.0f;
			var y1 = 2.0f * bounds.vMax - 1.0f;

			for (int i = 0, j = 0; i < src.unTriangleCount * 3; i++)
			{
				var x = Mathf.Lerp(x0, x1, data[j++]);
				var y = Mathf.Lerp(y0, y1, data[j++]);
				vertices[i] = new Vector3(x, y, 0.0f);
				indices[i] = i;
			}

			// Add border
			var offset = (int)src.unTriangleCount * 3;
			var iVert = offset;
			vertices[iVert++] = new Vector3(-1, -1, 0);
			vertices[iVert++] = new Vector3(x0, -1, 0);
			vertices[iVert++] = new Vector3(-1, 1, 0);
			vertices[iVert++] = new Vector3(x0, 1, 0);
			vertices[iVert++] = new Vector3(x1, -1, 0);
			vertices[iVert++] = new Vector3(1, -1, 0);
			vertices[iVert++] = new Vector3(x1, 1, 0);
			vertices[iVert++] = new Vector3(1, 1, 0);
			vertices[iVert++] = new Vector3(x0, y0, 0);
			vertices[iVert++] = new Vector3(x1, y0, 0);
			vertices[iVert++] = new Vector3(x0, y1, 0);
			vertices[iVert++] = new Vector3(x1, y1, 0);

			var iTri = offset;
			indices[iTri++] = offset + 0;
			indices[iTri++] = offset + 1;
			indices[iTri++] = offset + 2;
			indices[iTri++] = offset + 2;
			indices[iTri++] = offset + 1;
			indices[iTri++] = offset + 3;
			indices[iTri++] = offset + 4;
			indices[iTri++] = offset + 5;
			indices[iTri++] = offset + 6;
			indices[iTri++] = offset + 6;
			indices[iTri++] = offset + 5;
			indices[iTri++] = offset + 7;
			indices[iTri++] = offset + 1;
			indices[iTri++] = offset + 4;
			indices[iTri++] = offset + 8;
			indices[iTri++] = offset + 8;
			indices[iTri++] = offset + 4;
			indices[iTri++] = offset + 9;
			indices[iTri++] = offset + 10;
			indices[iTri++] = offset + 11;
			indices[iTri++] = offset + 3;
			indices[iTri++] = offset + 3;
			indices[iTri++] = offset + 11;
			indices[iTri++] = offset + 6;

			var mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = indices;
			mesh.bounds = new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)); // Prevent frustum culling from culling this mesh
			return mesh;
		}

		void SetUpHiddenAreaMesh()
		{
			HiddenAreaMesh_t leftEyeMesh = OpenVR.System.GetHiddenAreaMesh(EVREye.Eye_Left, EHiddenAreaMeshType.k_eHiddenAreaMesh_Standard);
			VRTextureBounds_t leftBounds = SteamVR.instance.textureBounds[0];
			HiddenAreaMesh_t rightEyeMesh = OpenVR.System.GetHiddenAreaMesh(EVREye.Eye_Right, EHiddenAreaMeshType.k_eHiddenAreaMesh_Standard);
			VRTextureBounds_t rightBounds = SteamVR.instance.textureBounds[1];

			Mesh leftMesh = GetMeshForVRMeshData(leftEyeMesh, leftBounds);
			Mesh rightMesh = GetMeshForVRMeshData(rightEyeMesh, rightBounds);

			leftHiddenAreaMeshFilter.mesh = leftMesh;
			rightHiddenAreaMeshFilter.mesh = rightMesh;
		}

		float GetCombinedWidth(float distance, out float height)
		{
			float leftHeight = 2.0f * distance * Mathf.Tan(leftEyeCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
			float leftWidth = leftHeight * leftEyeCamera.aspect;
			float rightHeight = 2.0f * distance * Mathf.Tan(rightEyeCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
			float rightWidth = rightHeight * rightEyeCamera.aspect;
			height = rightHeight;

			// might have to update this for canted displays
			float width = ((leftWidth * 0.5f) + (rightWidth * 0.5f)) +
				(rightEyeCamera.transform.localPosition.x + -leftEyeCamera.transform.localPosition.x);

			return width;
		}

		[SerializeField] bool setFieldOfView;
		private void Update()
		{
			if(setFieldOfView)
			{
				CVRSystem hmd = SteamVR.instance.hmd;

				float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
				hmd.GetProjectionRaw(EVREye.Eye_Left, ref l_left, ref l_right, ref l_top, ref l_bottom);

				float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
				hmd.GetProjectionRaw(EVREye.Eye_Right, ref r_left, ref r_right, ref r_top, ref r_bottom);

				float leftTanHalfFov = Mathf.Max(-l_top, l_bottom);
				float rightTanHalfFov = Mathf.Max(-r_top, r_bottom);

				float leftFieldOfView = 2.0f * Mathf.Atan(leftTanHalfFov) * Mathf.Rad2Deg;
				float rightfieldOfView = 2.0f * Mathf.Atan(rightTanHalfFov) * Mathf.Rad2Deg;

				HmdMatrix34_t leftEyeMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Left);
				leftEyeCamera.transform.localPosition = leftEyeMatrix.GetPosition() / perCameraIPDMultiplier;
				leftEyeCamera.fieldOfView = leftFieldOfView * perCameraFovMultiplier;

				HmdMatrix34_t rightEyeMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Right);
				rightEyeCamera.transform.localPosition = rightEyeMatrix.GetPosition() / 1.116f;
				rightEyeCamera.fieldOfView = rightfieldOfView * perCameraFovMultiplier;
			}

			// set camera viewport
			leftEyeCamera.rect = new Rect(leftEyeCamera.rect.x,
				((1 - viewportHeight) * 0.5f), leftEyeCamera.rect.width, viewportHeight);
			rightEyeCamera.rect = new Rect(rightEyeCamera.rect.x,
				((1 - viewportHeight) * 0.5f), rightEyeCamera.rect.width, viewportHeight);

			combinedWidth = GetCombinedWidth(steamVRNearPlane, out var height); // used to be left cam near plane
		}

		void OnDrawGizmos()
		{
			if (!leftEyeCamera || !rightEyeCamera)
			{
				Camera[] cameras = GetComponentsInChildren<Camera>();

				for (int i = 0; i < cameras.Length; i++)
				{
					if (cameras[i].stereoTargetEye == StereoTargetEyeMask.Left) leftEyeCamera = cameras[i];
					else if (cameras[i].stereoTargetEye == StereoTargetEyeMask.Right) rightEyeCamera = cameras[i];
					else if (screenCamera == null && cameras[i].stereoTargetEye == StereoTargetEyeMask.None) screenCamera = cameras[i];
				}
			}

			float width, height, distance;
			distance = steamVRNearPlane; //leftEyeCamera.nearClipPlane;
			width = GetCombinedWidth(distance, out height);

			Gizmos.color = Color.red;
			Gizmos.matrix = Matrix4x4.TRS(transform.position +
				(transform.forward * distance),
				transform.rotation,
				Vector3.one);

			Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, height, 0));

			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}
	}
}