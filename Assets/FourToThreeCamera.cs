using UnityEngine;
using System.Collections;

public class FourToThreeCamera : MonoBehaviour {

	float prevScreenWidth;
	float prevScreenHeight;

	void Awake() {
		Camera camera = GetComponent<Camera> ();
		camera.rect = getNextRect ();
	}

	Rect getNextRect() {
		float nextAspect = 4f / 3f;
		float currentAspect = (float)Screen.width / (float)Screen.height;
		float scaleHeight = currentAspect / nextAspect;
		float scaleWidth = 1f / scaleHeight;
		if (scaleHeight < 1f) {
			return new Rect (0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
		}
		return new Rect ((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
	}

	bool isScreenSizeChanged() {
		return (float)Screen.width != prevScreenWidth || (float)Screen.height != prevScreenHeight;
	}

	void updateScreenSize() {
		if (isScreenSizeChanged()) {
			Camera camera = GetComponent<Camera> ();
			camera.rect = getNextRect ();
		}
		prevScreenWidth = (float)Screen.width;
		prevScreenHeight = (float)Screen.height;
	}

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
		updateScreenSize ();
	}
}
