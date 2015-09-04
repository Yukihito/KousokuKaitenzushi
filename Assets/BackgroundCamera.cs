using UnityEngine;
using System.Collections;

public class BackgroundCamera : MonoBehaviour {
	private static Camera bgCamera;
	// Use this for initialization
	void Start () {

		if (bgCamera != null)
			return;

		var backGroundCameraObject = new GameObject ("Background Color Camera");
		bgCamera = backGroundCameraObject.AddComponent<Camera> ();
		bgCamera.depth = -99;
		bgCamera.fieldOfView = 1;
		bgCamera.farClipPlane = 1.1f;
		bgCamera.nearClipPlane = 1; 
		bgCamera.cullingMask = 0;
		bgCamera.depthTextureMode = DepthTextureMode.None;
		bgCamera.backgroundColor = Color.black;
		bgCamera.renderingPath = RenderingPath.VertexLit;
		bgCamera.clearFlags = CameraClearFlags.SolidColor;
		bgCamera.useOcclusionCulling = false;
		backGroundCameraObject.hideFlags = HideFlags.NotEditable;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
