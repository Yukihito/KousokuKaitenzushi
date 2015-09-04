using UnityEngine;
using System.Collections;

public class StageSelectCamera : MonoBehaviour {
	float orbitRadius = 80f;
	float orbitAngle = 0f;
	float orbitSpeed = 0.003f;

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
		orbitAngle = (orbitAngle + orbitSpeed) % (2f * Mathf.PI);
		Vector3 preferedPosition = new Vector3 (Mathf.Cos (orbitAngle) * orbitRadius, 10f, Mathf.Sin (orbitAngle) * orbitRadius - 10f) + new Vector3(50f, 50f, 0f);
		transform.position = Vector3.Lerp (transform.position, preferedPosition, 0.1f);
		Vector3 forward = Vector3.zero - transform.position;
		Quaternion nextRotation = Quaternion.LookRotation (forward);
		transform.rotation = nextRotation;
	}
}
