﻿ using UnityEngine;
using System.Collections;

public class TitleCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))	 {
			Debug.Log("test");
			Application.LoadLevel("SushiSelect");
		}
	}
}
