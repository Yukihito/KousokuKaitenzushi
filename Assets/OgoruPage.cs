using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OgoruPage : MonoBehaviour {
	Curtain curtain;
	SoundManager soundManager;
	SushiFactory sushiFactory;
	KeyConfig keyConfig;
	bool isFinished;
	WrapedSushi sushi;
	Button ogoruButton;
	RectTransform ogoruButtonRect;
	Text caption;
	Text thanks;
	Canvas canvas;
	bool isJumping;
	bool isThanks;
	float jumpStartTime;

	// Use this for initialization
	void Start () {
		curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		soundManager = GameObject.Find ("Sounds").GetComponent<SoundManager> ();
		sushiFactory = GameObject.Find ("SushiFactory").GetComponent<SushiFactory> ();
		ogoruButton = GameObject.Find ("KojikiLink").GetComponent<Button> ();
		caption = GameObject.Find ("Caption").GetComponent<Text> ();
		thanks = GameObject.Find ("Thanks").GetComponent<Text> ();
		ogoruButton.onClick.AddListener (() => {
			Application.OpenURL("http://www.amazon.co.jp/gp/registry/wishlist/2BNAH23MYZ5TM/ref=cm_wl_huc_view");
			isThanks = true;
		});
		ogoruButtonRect = GameObject.Find ("KojikiLink").GetComponent<RectTransform> ();
		canvas = GameObject.Find ("Canvas").GetComponent<Canvas> ();
		keyConfig = KeyConfig.Current;
		isFinished = false;
		isJumping = false;
		isThanks = false;
		thanks.enabled = false;
		sushi = sushiFactory.Create ((int)SushiType.Toro, Vector3.zero, Quaternion.Euler (new Vector3 (-30f, -30f, 0f)));
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 mPos;
		Rect rect = ogoruButtonRect.rect;
		rect.y += ogoruButtonRect.anchoredPosition.y;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (
			canvas.transform as RectTransform,
			Input.mousePosition,
			canvas.worldCamera,
			out mPos);
		Debug.Log (mPos);
		if (rect.Contains (mPos)) {
			if (!isJumping) {
				jumpStartTime = Time.time;
			}
			isJumping = true;
		} else {
			isJumping = false;
		}

		if (isThanks) {
			isJumping = true;
			ogoruButton.enabled = false;
			ogoruButton.transform.position = Vector3.up * 1000f;
			caption.enabled = false;
			thanks.enabled = true;
		}

		if (isJumping) {
			sushi.Underlying.transform.position = new Vector3 (0.0f, Mathf.Sin (((Time.time - jumpStartTime) * 10f) % Mathf.PI) * 0.1f, 0.0f);
		} else {
			sushi.Underlying.transform.position = Vector3.zero;
		}

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
