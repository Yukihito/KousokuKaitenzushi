using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CleanPage : MonoBehaviour {

	Curtain curtain;
	SoundManager soundManager;
	SushiFactory sushiFactory;
	KeyConfig keyConfig;
	bool isFinished;
	bool isYes;
	WrapedSushi sushi;
	Text cursor;
	bool isDeleting;

	// Use this for initialization
	void Start () {
		curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		soundManager = GameObject.Find ("Sounds").GetComponent<SoundManager> ();
		sushiFactory = GameObject.Find ("SushiFactory").GetComponent<SushiFactory> ();
		cursor = GameObject.Find ("Cursor").GetComponent<Text> ();
		keyConfig = KeyConfig.Current;
		isFinished = false;
		isYes = false;
		isDeleting = false;
		sushi = sushiFactory.Create ((int)SushiType.Toro, Vector3.zero, Quaternion.Euler (new Vector3 (-30f, -30f, 0f)));
	}

	// Update is called once per frame
	void Update () {
		if (!isFinished) {
			sushi.Underlying.transform.position = new Vector3 (Mathf.Sin ((Time.time * 60f) % (Mathf.PI * 2f)) * 0.002f, 0f, 0f);
			if (keyConfig.IsPushed (ConfigurableKeyType.Esc)) {
				curtain.FadeOut ();
				soundManager.BackAudio.Play ();
				isYes = false;
				isFinished = true;
			}

			if ((!isYes && keyConfig.IsPushed (ConfigurableKeyType.Right)) ||
			    (isYes && keyConfig.IsPushed (ConfigurableKeyType.Left))) {
				isYes = !isYes;
				soundManager.PiAudioSource.Play ();
			}

			if (isYes) {
				cursor.rectTransform.anchoredPosition = new Vector2 (90f - ((Time.time * 2f) % 1f) * 10f, -180f);
			} else {
				cursor.rectTransform.anchoredPosition = new Vector2 (-230f - ((Time.time * 2f) % 1f) * 10f, -180f);
			}

			if (keyConfig.IsPushed (ConfigurableKeyType.Enter) || keyConfig.IsPushed (ConfigurableKeyType.Space)) {
				if (isYes) {
					isDeleting = true;
					soundManager.ClickAudioSource.Play ();
				} else {
					soundManager.BackAudio.Play ();
					curtain.FadeOut ();
				}
				isFinished = true;
			}
		} else if (isDeleting) {
			sushi.Alpha = sushi.Alpha * 0.9f;
			if (sushi.Alpha < 0.001f) {
				sushi.Alpha = 0f;
				isDeleting = false;
				curtain.FadeOut ();
			}
		} else if (curtain.IsFinished) {
			if (isYes) {
				SaveData.DeleteAll ();
				SaveData.Save ();
			}
			Application.LoadLevel ("Title");
		}
	}
}
