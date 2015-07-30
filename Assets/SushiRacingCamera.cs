using UnityEngine;
using System.Collections;

public class SushiRacingCamera : MonoBehaviour {
	GameObject sushi;
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
		sushi = GameObject.Find ("RacingSushi");
	}
	
	// Update is called once per frame
	void Update () {
		updateScreenSize ();
		Vector3 forward = sushi.transform.position - transform.position;
		forward.y = sushi.transform.forward.y;
		Quaternion preferedRotation = Quaternion.LookRotation (forward);
		Quaternion nextRotation = Quaternion.Lerp (transform.rotation, preferedRotation, 0.1f);
		Vector3 preferedPosition = sushi.transform.position - sushi.transform.forward * 3.5f;
		preferedPosition.y = sushi.transform.position.y + 1.2f;
		Vector3 nextPosition = Vector3.Lerp (transform.position, preferedPosition, 0.3f);
		transform.position = nextPosition;
		transform.rotation = nextRotation;
	}
}
