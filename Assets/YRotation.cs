using UnityEngine;
using System.Collections;

public class YRotation : MonoBehaviour {
	public float Speed = 1f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (Vector3.up * Speed);
	}
}
