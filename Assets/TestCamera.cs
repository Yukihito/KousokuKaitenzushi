using UnityEngine;
using System.Collections;

public class TestCamera : MonoBehaviour {
	GameObject carGameObject;
	Car car;

	// Use this for initialization
	void Start () {
		carGameObject = GameObject.Find ("Car");
		car = carGameObject.GetComponent<Car> ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 forward = carGameObject.transform.position - transform.position;
		Quaternion preferedRotation = carGameObject.transform.rotation;
		Quaternion nextRotation = Quaternion.Lerp (transform.rotation, preferedRotation, 0.03f);
		Vector3 preferedPosition = carGameObject.transform.position - carGameObject.transform.forward * 3.5f + carGameObject.transform.up * 1.5f;
		Vector3 nextPosition = Vector3.Lerp (transform.position, preferedPosition, 0.3f);
		transform.position = nextPosition;
		transform.rotation = nextRotation;
	}
}
