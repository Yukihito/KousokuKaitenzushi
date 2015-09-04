using UnityEngine;
using System.Collections;

public class SushiBar : MonoBehaviour {
	SushiFactory sushiFactory;
	WrapedSushi[] sushis;
	WrapedSushi[] rsushis;
	float[] emitTimes;
	int sushiCount;
	GameObject cameraObject;
	float lastEmitTime;
	float emitInterval;
	int lastEmitIndex;
	float speed;
	bool isVisible = false;

	public bool IsVisible {
		get { 
			return isVisible;
		}

		set {
			if (!isVisible && value) {
				float t = Time.time;
				for (int i = 0; i < sushis.Length; i++) {
					emitTimes [i] = t - (float)(sushiCount - i - 2) * emitInterval;
				}
				lastEmitIndex = sushiCount - 2;
				lastEmitTime = t;
			}
			isVisible = value;
			foreach (WrapedSushi sushi in sushis) {
				sushi.IsEnabled = isVisible;
			}
			foreach (WrapedSushi sushi in rsushis) {
				sushi.IsEnabled = isVisible;
			}
		}
	}

	// Use this for initialization
	void Start () {
		sushiCount = 13;
		emitInterval = 0.18f;
		speed = 1f;
		sushiFactory = GameObject.Find ("SushiFactory").GetComponent<SushiFactory>();
		cameraObject = GameObject.Find ("Main Camera");
		sushis = new WrapedSushi[sushiCount];
		rsushis = new WrapedSushi[sushiCount];
		emitTimes = new float[sushiCount];
		for (int i = 0; i < sushis.Length; i++) {
			sushis [i] = sushiFactory.Create (i % 13, Vector3.zero, Quaternion.LookRotation (Vector3.forward), false);
			rsushis [i] = sushiFactory.Create (i % 13, Vector3.zero, Quaternion.LookRotation (Vector3.forward), false);
		}
		IsVisible = false;
	}

	void emit() {
		int index = (lastEmitIndex + 1) % sushiCount;
		lastEmitTime = Time.time;
		emitTimes [index] = Time.time;
		lastEmitIndex = index;
	}

	Vector3 calcPosition(float elapsedTime, bool isReverse) {
		if (!isReverse) {
			return
				cameraObject.transform.position +
				cameraObject.transform.forward * 1f +
				cameraObject.transform.right * (elapsedTime * speed - 1f) +
				cameraObject.transform.up * 0.5f;
		} else {
			return
				cameraObject.transform.position +
				cameraObject.transform.forward * 1f -
				cameraObject.transform.right * (elapsedTime * speed - 1f) -
				cameraObject.transform.up * 0.5f;
		}
	}

	Quaternion calcRotation() {
		return cameraObject.transform.rotation * Quaternion.Euler(new Vector3(-90f, 0f, 0f));
	}
	
	// Update is called once per frame
	void Update () {
		if (!IsVisible) {
			IsVisible = true;
		}

		if (isVisible) {
			for (int i = 0; i < sushis.Length; i++) {
				WrapedSushi sushi = sushis [i];
				WrapedSushi rsushi = rsushis [i];
				float elapsedTimeFromEmitted = Time.time - emitTimes [i];
				sushi.Underlying.transform.position = calcPosition (elapsedTimeFromEmitted, false);
				sushi.Underlying.transform.rotation = calcRotation ();
				rsushi.Underlying.transform.position = calcPosition (elapsedTimeFromEmitted, true);
				rsushi.Underlying.transform.rotation = calcRotation ();
			}
			if (Time.time - lastEmitTime >= emitInterval) {
				emit ();
			}
		}
	}
}
