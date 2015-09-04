using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreditPage : MonoBehaviour {
	Curtain curtain;
	SoundManager soundManager;
	KeyConfig keyConfig;
	bool isFinished;

	// Use this for initialization
	void Start () {
		curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		soundManager = GameObject.Find ("Sounds").GetComponent<SoundManager> ();
		GameObject.Find ("MaoudamasiiLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("http://maoudamashii.jokersounds.com/");
		});
		GameObject.Find ("JapaneseSushiLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("https://www.assetstore.unity3d.com/jp/#!/content/37401");
		});
		GameObject.Find ("NebulaLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("https://www.assetstore.unity3d.com/jp/#!/content/2967");
		});
		GameObject.Find ("ToonySkiesLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("https://www.assetstore.unity3d.com/jp/#!/content/11020");
		});
		GameObject.Find ("FurnitureLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("https://www.assetstore.unity3d.com/jp/#!/content/11859");
		});
		GameObject.Find ("HorseLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("https://www.assetstore.unity3d.com/jp/#!/content/16687");
		});
		GameObject.Find ("AkibaLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("https://www.assetstore.unity3d.com/jp/#!/content/20359");
		});
		GameObject.Find ("UnityLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("http://unity3d.com/");
		});
		GameObject.Find ("MyLink").GetComponent<Button> ().onClick.AddListener(() => {
			Application.OpenURL("https://twitter.com/ye_ey");
		});
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
