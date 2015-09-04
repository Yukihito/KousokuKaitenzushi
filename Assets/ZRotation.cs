using UnityEngine;
using System.Collections;

public class ZRotation : MonoBehaviour {
	float speed = 0.3f;
	float rotation = 0f;
	GameObject mainCamera;

	// Use this for initialization
	void Start () {
		mainCamera = GameObject.FindWithTag ("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {
		rotation += speed;
		rotation = rotation % 360f;
		transform.rotation = Quaternion.LookRotation ((mainCamera.transform.position - transform.position).normalized);
		transform.RotateAround (transform.position, transform.forward, rotation);
	}
}
