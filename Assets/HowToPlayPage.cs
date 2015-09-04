using UnityEngine;
using System.Collections;

public class HowToPlayPage : MonoBehaviour {
	Curtain curtain;
	SoundManager soundManager;
	KeyConfig keyConfig;
	bool isFinished;

	// Use this for initialization
	void Start () {
		curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		soundManager = GameObject.Find ("Sounds").GetComponent<SoundManager> ();
		keyConfig = KeyConfig.Current;
		isFinished = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isFinished) {
			if (keyConfig.IsPushed (ConfigurableKeyType.Enter) ||
			    keyConfig.IsPushed (ConfigurableKeyType.Space) ||
			    keyConfig.IsPushed (ConfigurableKeyType.Esc)) {
				curtain.FadeOut ();
				soundManager.BackAudio.Play ();
				isFinished = true;
			}
		} else if(curtain.IsFinished) {
			Application.LoadLevel ("Title");
		}
	}
}
