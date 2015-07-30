using UnityEngine;
using System.Collections;

public class OkeAccelerator : MonoBehaviour {
	float angle = 0f;
	MeshRenderer renderer;
	// Use this for initialization
	void Start () {
		renderer = GetComponentInChildren<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		angle++;
		if (angle >= 360f) {
			angle = 0;
		}
		renderer.transform.localRotation = Quaternion.Euler(angle, 0f, 90f);
	}
}
