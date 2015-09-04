using UnityEngine;
using System.Collections;

public class SushiPopper : MonoBehaviour {
	SushiFactory sushiFactory;
	WrapedSushi[] sushis;
	bool isVisible = false;
	float visibleTime;
	float duration;
	int sushiCount;
	GameObject cameraObject;

	public bool IsVisible {
		get { 
			return isVisible;
		}

		set {
			isVisible = value;
			foreach (WrapedSushi sushi in sushis) {
				sushi.IsEnabled = isVisible;
			}
			if (isVisible) {
				visibleTime = Time.time;
			}
		}
	}

	// Use this for initialization
	void Start () {
		duration = 1f;
		sushiCount = 13;
		sushiFactory = GameObject.Find ("SushiFactory").GetComponent<SushiFactory>();
		cameraObject = GameObject.Find ("Main Camera");
		sushis = new WrapedSushi[sushiCount];
		for (int i = 0; i < sushis.Length; i++) {
			sushis [i] = sushiFactory.Create (i % 13, Vector3.zero, Quaternion.LookRotation (Vector3.forward));
		}
		IsVisible = false;
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if (Input.GetKeyDown (KeyCode.S)) {
			IsVisible = !IsVisible;
		}
		*/
		if (isVisible) {
			float elapsedTime = Time.time - visibleTime;
			if (elapsedTime > duration) {
				IsVisible = false;
				return;
			}
			float alpha = Mathf.Sin(Mathf.PI * (elapsedTime / duration));
			Vector3 center = cameraObject.transform.position +
			                cameraObject.transform.forward * 1f;
			for (int i = 0; i < sushis.Length; i++) {
				sushis [i].Alpha = alpha;
				sushis [i].Underlying.transform.position = calcPosition (elapsedTime, i);
				sushis [i].Underlying.transform.rotation =
				Quaternion.LookRotation (sushis [i].Underlying.transform.position - center, -cameraObject.transform.forward);
			}
		}
	}

	Vector3 calcPosition(float elapsedTime, int index) {
		float initTheta = ((float)index / (float)sushiCount) * Mathf.PI * 2f;
		float radius = 0.5f;
		float rotateDegree = elapsedTime * 3f;
		return
			cameraObject.transform.position +
			cameraObject.transform.forward * 1f +
			cameraObject.transform.right * Mathf.Cos(initTheta + rotateDegree) * radius +
			cameraObject.transform.up * Mathf.Sin(initTheta + rotateDegree) * radius;
	}
}
