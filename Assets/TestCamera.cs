using UnityEngine;
using System.Collections;

public class TestCamera : MonoBehaviour {
	GameObject carGameObject;
	Car car;
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

	void updateWorldMat() {
		if (car.IsJumping) {

			Vector3 preferedPosition = carGameObject.transform.position + car.JumpDir * 15.5f;
			Vector3 nextPosition = Vector3.Lerp (transform.position, preferedPosition, 0.7f);
			transform.position = nextPosition;

			Vector3 forward = (carGameObject.transform.position - transform.position).normalized;
			Vector3 right = Vector3.Cross (car.JumpDir, forward).normalized;
			Vector3 up = Vector3.Cross (forward, right);
			Quaternion preferedRotation = Quaternion.LookRotation(forward, up);
			Quaternion nextRotation = Quaternion.Lerp (transform.rotation, preferedRotation, 0.1f);

			transform.rotation = nextRotation;
		} else {
			Quaternion preferedRotation = carGameObject.transform.rotation;
			Quaternion nextRotation = Quaternion.Lerp (transform.rotation, preferedRotation, 0.13f);
			Vector3 preferedPosition = carGameObject.transform.position - carGameObject.transform.forward * 3.5f + carGameObject.transform.up * 1.5f;
			Vector3 nextPosition = Vector3.Lerp (transform.position, preferedPosition, 0.3f);
			transform.position = nextPosition;
			transform.rotation = nextRotation;
		}
	}

	// Use this for initialization
	void Start () {
		carGameObject = GameObject.Find ("Player");
		car = carGameObject.GetComponent<Car> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		updateScreenSize ();
		updateWorldMat ();
	}
}
