using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TitlePage : MonoBehaviour {
	GameObject[] sushis;
	Text selector;
	float rotation = 0f;
	float rotationSpeed = 0.05f;
	float radius = 0f;
	float maxRadius = 0.5f;
	int selecting = 0;
	KeyConfig keyConfig;
	Curtain curtain;
	bool IsSelected;
	bool IsReturn;
	SoundManager soundManager;

	// Use this for initialization
	void Start () {
		keyConfig = KeyConfig.Current;
		sushis = new GameObject[14];
		for (int i = 0; i < 14; i++) {
			sushis[i] = GameObject.Find ("" + i);
		}
		selector = GameObject.Find ("Selector").GetComponent<Text> ();
		curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		soundManager = GameObject.Find ("Sounds").GetComponent<SoundManager> ();
		IsSelected = false;
		IsReturn = false;
	}
	
	// Update is called once per frame
	void Update () {
		radius = radius * 0.9f + maxRadius * 0.1f;
		rotation += rotationSpeed;
		rotation %= Mathf.PI * 2f;
		float p = Mathf.PI * 2f / 14f;
		for (int i = 0; i < 14; i++) {
			sushis[i].transform.position = (new Vector3(Mathf.Sin(rotation + p * (float)i), Mathf.Cos(rotation + p * (float)i), 0f)) * radius;
			sushis [i].transform.rotation = Quaternion.LookRotation (sushis [i].transform.position, -Vector3.forward);
		}

		selector.rectTransform.anchoredPosition = new Vector2 (-110f + ((Time.time * 3f) % 1f) * 5f, -60f - ((float)selecting * 24));

		if (!IsSelected) {
			if (keyConfig.IsPushed (ConfigurableKeyType.Down)) {
				selecting++;
				if (selecting > 5) {
					selecting = 0;
				}
				soundManager.PiAudioSource.Play ();
			}

			if (keyConfig.IsPushed (ConfigurableKeyType.Up)) {
				selecting--;
				if (selecting < 0) {
					selecting = 5;
				}
				soundManager.PiAudioSource.Play ();
			}

			if (Input.GetKey (KeyCode.KeypadEnter) || Input.GetKey (KeyCode.Return)) {
				IsSelected = true;
				curtain.FadeOut ();
				soundManager.ClickAudioSource.Play ();
			}

			if (keyConfig.IsPushed (ConfigurableKeyType.Esc)) {
				IsSelected = true;
				IsReturn = true;
				curtain.FadeOut ();
				soundManager.BackAudio.Play ();
			}
		}

		if (IsSelected && curtain.IsFinished) {
			if (!IsReturn) {
				if (selecting == 0) {
					Application.LoadLevel ("SushiSelect");
				} else if (selecting == 1) {
					Application.LoadLevel ("HowToPlay");
				} else if (selecting == 2) {
					Application.LoadLevel ("credit");
				} else if (selecting == 3) {
					Application.LoadLevel ("Ogoru");
				} else if (selecting == 4) {
					Application.LoadLevel ("CleanPage");
				} else if (selecting == 5) {
					Application.Quit ();
				}
			} else {
				Application.Quit ();
			}
		}
	}
}
