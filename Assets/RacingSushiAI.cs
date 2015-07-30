using UnityEngine;
using System.Collections;
using System;

public class RacingSushiAI : MonoBehaviour, IRankProvider {
	public int sushiType = -1;
	SwitchableSushi skin;
	Course course;
	float speed = 0.5f;
	int currentCheckPointOrder = -1;
	int goaledCount = 0;
	CheckPoint lastCheckPoint;

	void Start () {
		if (sushiType == -1) {
			int maxSushi = Enum.GetValues (typeof(SushiType)).Length;
			sushiType = UnityEngine.Random.Range (0, maxSushi);
		}
		skin = GetComponentInChildren<SwitchableSushi>().GetComponent("SwitchableSushi") as SwitchableSushi;
		skin.InitializeRenderer ();
		skin.SwitchSushi ((SushiType)sushiType);
		course = GameObject.Find ("Course").GetComponent<Course> ();
		speed = 0.5f - 0.025f * (float)UnityEngine.Random.Range (0, 10);
	}

	void Update () {
		if (!course.IsStarted) {
			return;
		}
		CheckPoint nextCheckPoint = course.GetNextCheckPoint (currentCheckPointOrder);
		Vector3 forward = nextCheckPoint.transform.position - nextCheckPoint.transform.up * 4.75f - transform.position;
		//forward.y = nextCheckPoint.transform.position.y - 4.75f;
		Quaternion nextRotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation (forward), 0.1f);
		transform.rotation = nextRotation;
		Vector3 nextPosition = transform.position + transform.forward * speed;
		//nextPosition.y = nextPosition.y * 0.9f + forward.y * 0.1f;
		transform.position = nextPosition;
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "CheckPoint") {
			CheckPoint currentCheckPoint = col.GetComponent<CheckPoint> ();
			currentCheckPointOrder = currentCheckPoint.CheckPointOrder;
			if (lastCheckPoint != null && lastCheckPoint.CheckPointOrder == course.GoalCheckPointOrder && currentCheckPoint.CheckPointOrder == course.StartPoint.CheckPointOrder) {
				goaledCount++;
			}
			lastCheckPoint = currentCheckPoint;
		}
	}

	public int Rank {
		get {
			return currentCheckPointOrder + (course.GoalPoint.CheckPointOrder + 1) * goaledCount;
		}
	}

	public bool IsUser {
		get {
			return false;
		}
	}
}
