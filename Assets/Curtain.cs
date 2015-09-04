using UnityEngine;
using System.Collections;

public class Curtain : MonoBehaviour {
	Texture2D tex;
	float alpha = 0f;
	float startTime;
	float speed = 3f;
	public bool IsVisible;
	public bool IsFadeIn;
	public bool IsFinished;

	// Use this for initialization
	void Start () {
		tex = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		tex.SetPixel (0, 0, new Color32(0, 0, 0, 127));
		tex.Apply ();
		FadeIn ();
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if (Input.GetKeyDown (KeyCode.I)) {
			FadeIn ();
		}
		if (Input.GetKeyDown (KeyCode.O)) {
			FadeOut ();
		}
		*/
		if (!IsFinished) {
			if (IsFadeIn) {
				alpha = 1f - (Time.time - startTime) * speed;
				if (alpha < 0f) {
					alpha = 0f;
					IsVisible = false;
					IsFinished = true;
				}
			} else {
				alpha = (Time.time - startTime) * speed;
				if (alpha > 1f) {
					alpha = 1f;
					IsFinished = true;
				}
			}

			byte alphaByte = (byte)(alpha * 255);
			tex = new Texture2D (1, 1, TextureFormat.RGBA32, false);
			tex.SetPixel (0, 0, new Color32(0, 0, 0, alphaByte));
			tex.Apply ();
		}
	}

	void OnGUI() {
		if (IsVisible) {
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), tex);
		}
	}

	public void FadeIn() {
		IsFadeIn = true;
		IsVisible = true;
		IsFinished = false;
		startTime = Time.time;
	}

	public void FadeOut() {
		IsFadeIn = false;
		IsVisible = true;
		IsFinished = false;
		startTime = Time.time;
	}
}
