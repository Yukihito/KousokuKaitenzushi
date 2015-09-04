using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class SushiSelectCamera : MonoBehaviour {
	SwitchableSushi switchableSushi;
	KeyConfig keyConfig;
	int maxSushi;
	public static SushiType currentSushi;
	float orbitRadius = 2f;
	float orbitAngle = 0f;
	float orbitSpeed = 0.01f;
	GameObject message;
	GameObject fuda;
	public Sprite AmaebiSprite;
	public Sprite EbiSprite;
	public Sprite HamachiSprite;
	public Sprite HokkiSprite;
	public Sprite IkaSprite;
	public Sprite IkuraSprite;
	public Sprite KohadaSprite;
	public Sprite MaguroSprite;
	public Sprite OotoroSprite;
	public Sprite SulmonSprite;
	public Sprite TakoSprite;
	public Sprite TamagoSprite;
	public Sprite UniSprite;
	Text maxSpeedText;
	Text accelerationText;
	Text weightText;
	Text maxSpeedMeterText;
	Text accelerationMeterText;
	Text weightMeterText;
	SoundManager soundManager;
	Sprite[] fudaSprites;
	int frame;
	Vector3 hiddenFudaPosition = new Vector2(500f, 100f);
	Vector3 preferedFudaPositon = new Vector2(300f, 100f);

	float prevScreenWidth;
	float prevScreenHeight;
	bool isSelected;
	bool isReturn;
	Curtain curtain;

	void Awake() {
		Camera camera = GetComponent<Camera> ();
		camera.rect = getNextRect ();
	}

	Rect getNextRect() {
		float nextAspect = 4f / 3f;
		float currentAspect = (float)Screen.width / (float)Screen.height;
		float scaleHeight = currentAspect / nextAspect;
		float scaleWidth = 1f / scaleHeight;
		if (scaleHeight < 1f) {
			return new Rect (0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
		}
		return new Rect ((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
	}

	bool isScreenSizeChanged() {
		return (float)Screen.width != prevScreenWidth || (float)Screen.height != prevScreenHeight;
	}

	void updateScreenSize() {
		if (isScreenSizeChanged()) {
			Camera camera = GetComponent<Camera> ();
			camera.rect = getNextRect ();
		}
		prevScreenWidth = (float)Screen.width;
		prevScreenHeight = (float)Screen.height;
	}

	void hideSpecs() {
		maxSpeedText.enabled = false;
		accelerationText.enabled = false;
		weightText.enabled = false;
		maxSpeedMeterText.enabled = false;
		accelerationMeterText.enabled = false;
		weightMeterText.enabled = false;
	}

	void showSpecs() {
		maxSpeedText.enabled = true;
		accelerationText.enabled = true;
		weightText.enabled = true;
		maxSpeedMeterText.enabled = true;
		accelerationMeterText.enabled = true;
		weightMeterText.enabled = true;
	}

	void updateSpecs() {
		SushiSpec spec = SushiSpecProvider.Provide (currentSushi);
		maxSpeedMeterText.text = new string('|', spec.MaxSpeedRank * 3);
		accelerationMeterText.text = new string('|', spec.AccelerationRank * 3);
		weightMeterText.text = new string('|', spec.WeightRank * 3);
	}

	void Start () {
		curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		maxSpeedText = GameObject.Find ("MaxSpeedText").GetComponent<Text> ();
		accelerationText = GameObject.Find ("AccelerationText").GetComponent<Text> ();
		weightText = GameObject.Find ("WeightText").GetComponent<Text> ();
		maxSpeedMeterText = GameObject.Find ("MaxSpeedMeter").GetComponent<Text> ();
		accelerationMeterText = GameObject.Find ("AccelerationMeter").GetComponent<Text> ();
		weightMeterText = GameObject.Find ("WeightMeter").GetComponent<Text> ();
		hideSpecs ();
		soundManager = GameObject.Find ("Sounds").GetComponent<SoundManager> ();
		keyConfig = KeyConfig.Current;
		switchableSushi = GameObject.Find("SwitchableSushi").GetComponent<SwitchableSushi>();
		maxSushi = Enum.GetValues (typeof(SushiType)).Length;
		currentSushi = SushiType.Amaebi;
		switchableSushi.InitializeRenderer ();
		switchableSushi.SwitchSushi (currentSushi);
		message = GameObject.Find ("Message");
		fuda = GameObject.Find ("Fuda");
		fudaSprites = new Sprite[] {
			AmaebiSprite,
			EbiSprite,
			HamachiSprite,
			HokkiSprite,
			IkaSprite,
			IkuraSprite,
			KohadaSprite,
			OotoroSprite,
			MaguroSprite,
			SulmonSprite,
			TakoSprite,
			TamagoSprite,
			UniSprite
		};
		frame = 0;
		hideSprites ();
		initFudaPosition ();
		isSelected = false;
		isReturn = false;
	}

	void hideSprites() {
		message.GetComponent<Image> ().enabled = false;
		fuda.GetComponent<Image> ().enabled = false;
	}

	void showSprites() {
		message.GetComponent<Image> ().enabled = true;
		fuda.GetComponent<Image> ().enabled = true;
	}

	void initFudaPosition() {
		fuda.GetComponent<RectTransform> ().anchoredPosition = hiddenFudaPosition;
	}

	void Update () {
		updateScreenSize ();
		if (keyConfig.IsPushed (ConfigurableKeyType.M)) {
			soundManager.IsMute = !soundManager.IsMute;
		}
		if (frame >= 60 && !isSelected) {
			if (keyConfig.IsPushed (ConfigurableKeyType.Right)) {
				if ((int)currentSushi + 1 < maxSushi) {
					currentSushi++;
				} else {
					currentSushi = (SushiType)0;
				}
				onSushiChanged ();
			}
			if (keyConfig.IsPushed (ConfigurableKeyType.Left)) {
				if (0 < (int)currentSushi) {
					currentSushi--;
				} else {
					currentSushi = (SushiType)maxSushi - 1;
				}
				onSushiChanged ();
			}
			if (keyConfig.IsPushed(ConfigurableKeyType.Enter) || keyConfig.IsPushed(ConfigurableKeyType.Space)) {
				Global.CurrentSushiType = currentSushi;
				isSelected = true;
				soundManager.ClickAudioSource.Play ();
				curtain.FadeOut ();
			}
			if (keyConfig.IsPushed(ConfigurableKeyType.Esc)) {
				isSelected = true;
				isReturn = true;
				soundManager.BackAudio.Play ();
				curtain.FadeOut ();
			}
			RectTransform fudaRect = fuda.GetComponent<RectTransform> ();
			fudaRect.anchoredPosition = Vector2.Lerp (fudaRect.anchoredPosition, preferedFudaPositon, 0.2f);
		}

		if (isSelected && curtain.IsFinished) {
			if (!isReturn) {
				Application.LoadLevel ("StageSelect");
			} else {
				Application.LoadLevel ("Title");
			}
		}

		orbitAngle = (orbitAngle + orbitSpeed) % (2f * Mathf.PI);
		Vector3 preferedPosition = new Vector3 (Mathf.Cos (orbitAngle) * orbitRadius, 3, Mathf.Sin (orbitAngle) * orbitRadius - 10f);
		transform.position = Vector3.Lerp (transform.position, preferedPosition, 0.1f);
		Vector3 forward = switchableSushi.transform.position - transform.position;
		Quaternion nextRotation = Quaternion.LookRotation (forward);
		transform.rotation = nextRotation;
		if (frame < 60) {
			frame++;
		} else if (frame == 60) {
			showSprites ();
			updateSpecs ();
			showSpecs ();
		}
	}

	void onSushiChanged() {
		soundManager.PiAudioSource.Play ();
		switchableSushi.SwitchSushi (currentSushi);
		fuda.GetComponent<Image> ().sprite = fudaSprites [(int)currentSushi];
		updateSpecs ();
		initFudaPosition ();
	}
}
