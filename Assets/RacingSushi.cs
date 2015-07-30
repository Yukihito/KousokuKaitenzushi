using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RacingSushi : MonoBehaviour, IRankProvider {
	float maxSpeed = 0.7f;
	float speed = 0f;
	float acceleration = 0.01f;
	float speedDecrFactor = 0.98f;
	float rotationSpeed = 1f;
	KeyConfig keyConfig;
	bool isFlying = false;
	int elapsedTimeFromLastGrounded = 0;
	Vector3 gravity;
	float gravitySpeed = 9.8f * 3f;
	CheckPoint lastCheckPoint;
	public int GoaledCount = 0;
	public int MaxGoalCount = 3;
	Course course;
	bool isFloating = false;
	int elapsedTimeFromFloatingStarted = 0;
	int floatingDuration = 60;
	float floatingRotation = 0f;
	float floatingRotationSpeed = 0.1f;
	SwitchableSushi switchableSushi;
	ParticleSystem floatingEffect;
	Text lapText;
	public bool IsGoaled = false;
	public bool IsShowLapTime = false;
	int elapsedTimeFromShowLapTime = 0;
	AudioSource acceleAudio;
	AudioSource lapAudio;
	AudioSource colAudio;

	void Start () {
		keyConfig = KeyConfig.Current;
		course = GameObject.Find ("Course").GetComponent<Course> ();
		switchableSushi = GetComponentInChildren<SwitchableSushi> ();
		switchableSushi.InitializeRenderer ();
		switchableSushi.SwitchSushi(SushiSelectCamera.currentSushi);
		floatingEffect = switchableSushi.GetComponentInChildren<ParticleSystem> ();
		floatingEffect.enableEmission = false;
		lapText = GameObject.Find ("Lap").GetComponent<Text> ();
		acceleAudio = GameObject.Find ("AcceleAudio").GetComponent<AudioSource> ();
		lapAudio = GameObject.Find ("LapAudio").GetComponent<AudioSource> ();
		colAudio = GameObject.Find ("ColAudio").GetComponent<AudioSource> ();
	}

	void Update () {
		if (!course.IsStarted) {
			return;
		}
		if (!isFloating) {
			if (!IsGoaled) {
				userControl ();
			} else {
				transform.position += transform.forward * speed;
				speed *= speedDecrFactor;
			}
		} else {
			floatingUpdate ();
		}
		gravityAccelerate ();
		updateFlyingStatus ();
		recovery ();
		lapTimeVisibilityUpdate ();
	}

	void lapTimeVisibilityUpdate() {
		if (IsShowLapTime) {
			elapsedTimeFromShowLapTime++;
			if (elapsedTimeFromShowLapTime > 60) {
				IsShowLapTime = false;
			}
		}
	}

	void floatingUpdate() {
		elapsedTimeFromFloatingStarted++;
		if (elapsedTimeFromFloatingStarted < floatingDuration) {
			floatingRotation = floatingRotation * (1f - floatingRotationSpeed) + 720f * floatingRotationSpeed;
			float yUp = Mathf.Sin (((float)elapsedTimeFromFloatingStarted / (float)floatingDuration) * Mathf.PI);
			float yFix = - Mathf.Cos((floatingRotation / 180f) * Mathf.PI) * 0.14f;
			Debug.Log (yFix);
			switchableSushi.transform.localRotation = Quaternion.Euler(0f, 0f, floatingRotation) * Quaternion.Euler (0f, -90f, 0f);
			switchableSushi.transform.localPosition = new Vector3(0f, yFix - 0.06f + yUp, 0f);
		} else {
			elapsedTimeFromFloatingStarted = 0;
			floatingRotation = 0f;
			isFloating = false;
			switchableSushi.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
			switchableSushi.transform.localPosition = new Vector3 (0f, -0.2f, 0f);
			floatingEffect.enableEmission = false;
		}
		speed = 1f;
		transform.position += transform.forward * speed;
		if (keyConfig.IsPressed(ConfigurableKeyType.Left)) {
			transform.Rotate (Vector3.down * rotationSpeed);
		}

		if (keyConfig.IsPressed(ConfigurableKeyType.Right)) {
			transform.Rotate (Vector3.up * rotationSpeed);
		}
	}

	void userControl() {
		if (keyConfig.IsPressed (ConfigurableKeyType.Space) && !isFlying) {
			if (speed < maxSpeed) {
				speed += acceleration;
			}
		} else {
			speed *= speedDecrFactor;
		}

		transform.position += transform.forward * speed;

		if (keyConfig.IsPressed(ConfigurableKeyType.Left)) {
			transform.Rotate (Vector3.down * rotationSpeed);
		}

		if (keyConfig.IsPressed(ConfigurableKeyType.Right)) {
			transform.Rotate (Vector3.up * rotationSpeed);
		}
	}

	void gravityAccelerate() {
		if (isFlying) {
			GetComponent<Rigidbody> ().AddForce (Vector3.down * gravitySpeed, ForceMode.Acceleration);
		} else {
			GetComponent<Rigidbody> ().AddForce (gravity * gravitySpeed, ForceMode.Acceleration);
		}
	}

	void updateFlyingStatus() {
		elapsedTimeFromLastGrounded++;

		if (elapsedTimeFromLastGrounded < 10) {
			isFlying = false;
		} else {
			isFlying = true;
		}
	}

	void recovery() {
		if (transform.position.y < -10f) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			if (lastCheckPoint != null) {
				transform.position = lastCheckPoint.transform.position - lastCheckPoint.transform.up * 3f;
				transform.rotation = Quaternion.LookRotation(lastCheckPoint.transform.right);
				speed = 0;
			} else {
				// TODO:
			}
		}
	}

	void OnCollisionStay(Collision col) {
		if (col.gameObject.tag == "Road") {
			changeGravityAngle (col);
		} else if (col.gameObject.tag == "AI" || col.gameObject.tag == "Wall") {
			colAudio.Play ();
		}
	}

	void changeGravityAngle(Collision road) {
		gravity = -road.transform.up;
		gravity.Normalize ();
		elapsedTimeFromLastGrounded = 0;
		isFlying = false;
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "CheckPoint") {
			CheckPoint currentCheckPoint = col.GetComponent<CheckPoint> ();
			if (lastCheckPoint == null && currentCheckPoint.CheckPointOrder == 0) {
				lastCheckPoint = currentCheckPoint;
			} else if (lastCheckPoint.CheckPointOrder + 1 == currentCheckPoint.CheckPointOrder) {
				lastCheckPoint = currentCheckPoint;
			} else if (lastCheckPoint.CheckPointOrder == course.GoalCheckPointOrder && currentCheckPoint.CheckPointOrder == 0) {
				lastCheckPoint = currentCheckPoint;
				GoaledCount++;
				lapText.text = "Lap    " + (GoaledCount + 1) + "/3";
				IsShowLapTime = true;
				elapsedTimeFromShowLapTime = 0;
				lapAudio.Play ();
				if (GoaledCount >= 3) {
					IsGoaled = true;
				}
			}
		} else if (col.gameObject.tag == "OkeAccelerator") {
			acceleAudio.Play ();
			isFloating = true;
			floatingEffect.enableEmission = true;
		}
	}

	public int Rank {
		get {
			if (lastCheckPoint == null) {
				return 0;
			}
			return lastCheckPoint.CheckPointOrder + (course.GoalPoint.CheckPointOrder + 1) * GoaledCount;
		}
	}

	public bool IsUser {
		get {
			return true;
		}
	}
}
