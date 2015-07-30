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
	Sprite[] fudaSprites;
	int frame;
	Vector3 hiddenFudaPosition = new Vector2(500f, 100f);
	Vector3 preferedFudaPositon = new Vector2(300f, 100f);

	void Start () {
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
			MaguroSprite,
			OotoroSprite,
			SulmonSprite,
			TakoSprite,
			TamagoSprite,
			UniSprite
		};
		frame = 0;
		hideSprites ();
		initFudaPosition ();
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
		if (frame >= 60) {
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
			RectTransform fudaRect = fuda.GetComponent<RectTransform> ();
			fudaRect.anchoredPosition = Vector2.Lerp (fudaRect.anchoredPosition, preferedFudaPositon, 0.2f);
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
		}
	}

	void onSushiChanged() {
		switchableSushi.SwitchSushi (currentSushi);
		fuda.GetComponent<Image> ().sprite = fudaSprites [(int)currentSushi];
		initFudaPosition ();
	}
}
