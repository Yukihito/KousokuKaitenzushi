using UnityEngine;
using System.Collections;
using System;

public class Car : MonoBehaviour {
	SwitchableSushi switchableSushi;
	KeyConfig keyConfig;
	float maxSpeed = 1.9f;
	float speed = 0f;
	public float AutoDriveSpeed = 2f;
	float acceleration = 0.01f;
	float speedDecrFactor = 0.98f;
	float rotationSpeed = 1.5f;
	bool isGrounding = true;
	bool isFloating = false;
	public bool IsJumping = false;
	float maxJumpTime = 3.5f;
	float jumpStartTime = 0f;
	Vector3 jumpStartPosition;
	int elapsedFlameFromLastGrounded = 0;
	CourseFactory courseFactory;
	ParticleSystem floatingEffect;
	Floor lastIntersectedFloor;
	float gravitySpeed = 9.8f * 0.005f;
	float currentGravityAcc = 0f;
	float progress = 0f;
	int lap = 0;
	int elapsedFlameFromReverseRunStarted = 0;
	public bool IsAI = false;
	bool isStarted = false;
	public bool IsAuto = false;
	public bool IsReverse = false;
	public bool IsSushi = true;
	public bool IsObj = false;
	public bool IsStarted {
		get {
			return isStarted;
		}
		set {
			if (value) {
				LastGoaledTime = Time.time;
			}
			isStarted = value;
		}
	}
	public float LastLapTime = -1f;
	public float LastGoaledTime = -1f;
	public int Lap {
		get {
			return lap;
		}
	}

	public bool IsReverseRunning {
		get {
			return elapsedFlameFromReverseRunStarted > 60;
		}
	}

	public float Progress {
		get {
			return progress;
		}
	}

	public Floor LastIntersectedFloor {
		set {
			lastIntersectedFloor = value;
		}
	}

	public Vector3 JumpDir;
	Vector3 recoveryPosition;
	Quaternion recoveryRotation;

	float floatingSpeed = 3f;
	float floatingDuration = 1f;
	float floatingStartTime = 0f;
	float floatingRotation = 0f;
	float floatingRotationSpeed = 0.1f;
	GameObject landingSiteGameObject;
	SoundManager soundManager;

	class FloorWithIntersection {
		public Floor Floor;
		public Intersection Intersection;
		public FloorWithIntersection(Floor floor, Intersection intersection) {
			Floor = floor;
			Intersection = intersection;
		}
	}

	// Use this for initialization
	void Start () {
		keyConfig = KeyConfig.Current;
		soundManager = GameObject.Find ("UserInterface").GetComponent<SoundManager> ();
		courseFactory = GameObject.Find ("Course").GetComponent<CourseFactory> ();
		courseFactory.CreateCourse ();
		if (!IsObj) {
			lastIntersectedFloor = courseFactory.Floor;
		}
		if (IsSushi) {
			switchableSushi = GetComponentInChildren<SwitchableSushi> ();
			switchableSushi.InitializeRenderer ();
			if (IsAI) {
				int maxSushi = Enum.GetValues (typeof(SushiType)).Length;
				int sushiType = UnityEngine.Random.Range (0, maxSushi);
				switchableSushi.SwitchSushi ((SushiType)sushiType);
			} else {
				switchableSushi.SwitchSushi (Global.CurrentSushiType);
				SushiSpec spec = SushiSpecProvider.Provide (Global.CurrentSushiType);
				maxSpeed = spec.MaxSpeed;
				acceleration = spec.Acceleration;
				speedDecrFactor = spec.Weight;
			}
		}
		floatingEffect = GameObject.Find("FloatingEffect").GetComponent<ParticleSystem> ();
		floatingEffect.enableEmission = false;
	}

	Intersection updateLastIntersectedFloor(Floor floor) {
		if (floor == null) {
			return Intersection.NotIntersected ();
		}

		Intersection intersection = floor.GetIntersection (transform.position);
		if (intersection.IsIntersected) {
			if (Vector3.Distance (intersection.IntersectPosition, transform.position) < 2f) {
				lastIntersectedFloor = floor;
			} else {
				return Intersection.NotIntersected ();
			}
		}
		return intersection;
	}

	void fixPosition(Intersection intersection) {
		transform.position = intersection.IntersectPosition;
		Vector3 rightIntersectPosition = FlatFloor.GetIntersectPosition (
			transform.right + transform.position,
			intersection.Normal,
			intersection.IntersectPosition);
		Vector3 nextUp = intersection.Normal;
		Vector3 nextRight = (rightIntersectPosition - transform.position).normalized;
		Vector3 nextForward = Vector3.Cross (nextRight, nextUp).normalized;
		Quaternion preferedRotation = Quaternion.LookRotation (nextForward, nextUp);
		transform.rotation = preferedRotation;
	}

	void updateRecoveryPosition(Intersection intersection) {
		recoveryPosition = lastIntersectedFloor.GetMiddle (transform.position);
		Vector3 forward = lastIntersectedFloor.GetTangentDir (recoveryPosition);
		recoveryRotation = Quaternion.LookRotation (forward, intersection.Normal);
	}

	void updateProgress() {
		float nextProgress = (float)lastIntersectedFloor.Order + lastIntersectedFloor.GetPercentage (transform.position);

		if (Mathf.Abs (nextProgress - progress) < 1f) {
			if (nextProgress >= progress) {
				elapsedFlameFromReverseRunStarted = 0;
			} else {
				elapsedFlameFromReverseRunStarted++;
			}
			progress = nextProgress;
		} else if (progress > 10 && nextProgress < 1) {
			LastLapTime = Time.time - LastGoaledTime;
			LastGoaledTime = Time.time;
			lap++;
			progress = nextProgress;
		}
	}

	void initializePhysicsVariables() {
		elapsedFlameFromLastGrounded = 0;
		currentGravityAcc = 0f;
		isGrounding = true;
	}

	void recovery() {
		if (!IsAI) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		initializePhysicsVariables ();
		speed = 0f;
		transform.position = recoveryPosition;
		transform.rotation = recoveryRotation;
		if (IsSushi) {
			switchableSushi.transform.localRotation = Quaternion.Euler (0f, 0f, 0f) * Quaternion.Euler (0f, -90f, 0f);
			switchableSushi.transform.localPosition = new Vector3 (0f, 0f, 0f);
		}
	}

	void onIntersect(Intersection intersection) {
		initializePhysicsVariables ();
		fixPosition (intersection);
		updateRecoveryPosition (intersection);
		updateProgress ();
	}

	void onNotIntersect() {
		if (!isFloating) {
			elapsedFlameFromLastGrounded++;
		}
		if (elapsedFlameFromLastGrounded > 3) {
			isGrounding = false;
		}

		if (elapsedFlameFromLastGrounded > 40) {
			recovery ();
		}
	}

	void onFlying() {
		speed *= speedDecrFactor;
		currentGravityAcc += gravitySpeed;
		transform.position += Vector3.down * currentGravityAcc + transform.forward * speed;
	}

	void onGrounding(Intersection intersection) {
		if (IsStarted || IsObj) {
			if (IsAI) {
				autoDrive (intersection);
			} else if (IsAuto) {
				autoDrive (intersection);
			} else if (isFloating) {
				driveOnFloating ();
			}
		}
	}

	void onGroundingBeforeIntersect() {
		if (IsStarted || IsObj) {
			if (!(IsAI || IsAuto || isFloating)) {
				drive ();
			}
		}
	}

	void startJump() {
		jumpStartTime = Time.time;
		IsJumping = true;
		landingSiteGameObject = GameObject.FindWithTag ("LandingSite");
		jumpStartPosition = transform.position;
		JumpDir = lastIntersectedFloor.GetTangentDir (jumpStartPosition).normalized;
		floatingEffect.enableEmission = true;
	}

	void onJumping() {
		float jumpProgress = (Time.time - jumpStartTime) / maxJumpTime;
		if (jumpProgress > 1f) {
			lastIntersectedFloor = landingSiteGameObject.GetComponent<LandingSite> ().Floor;
			IsJumping = false;
			floatingEffect.enableEmission = false;
			speed = maxSpeed;
			float nextProgress = (float)lastIntersectedFloor.Order + lastIntersectedFloor.GetPercentage (transform.position);
			progress = nextProgress;
		} else {
			float localX = Mathf.Cos (Mathf.PI * jumpProgress);
			float localY = Mathf.Sin (Mathf.PI * jumpProgress);
			Vector3 yAxis = lastIntersectedFloor.GetTangentDir (jumpStartPosition).normalized;
			Vector3 xAxis = -(landingSiteGameObject.transform.position - jumpStartPosition).normalized;
			Vector3 zAxis = Vector3.Cross (yAxis, xAxis);
			float radius = (landingSiteGameObject.transform.position - jumpStartPosition).magnitude / 2f;
			Vector3 center = (landingSiteGameObject.transform.position + jumpStartPosition) / 2f;
			Vector3 nextPosition = center + xAxis * localX * radius + yAxis * localY * radius;
			Vector3 up = (nextPosition - center).normalized;
			Vector3 forward = Vector3.Cross (up, zAxis);
			Quaternion nextRotation = Quaternion.LookRotation (forward, up);
			transform.rotation = nextRotation;
			transform.position = nextPosition;
		}
	}

	void driveOnFloating() {
		float elapsedTimeFromFloatingStarted = Time.time - floatingStartTime;
		if (elapsedTimeFromFloatingStarted <= floatingDuration) {
			Rigidbody rb = GetComponent<Rigidbody> ();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			floatingRotation = floatingRotation * (1f - floatingRotationSpeed) + 720f * floatingRotationSpeed;
			float yUp = Mathf.Sin ((elapsedTimeFromFloatingStarted / floatingDuration) * Mathf.PI);
			float yFix = - Mathf.Cos((floatingRotation / 180f) * Mathf.PI) * 0.14f;
			switchableSushi.transform.localRotation = Quaternion.Euler(0f, 0f, floatingRotation) * Quaternion.Euler (0f, -90f, 0f);
			switchableSushi.transform.localPosition = new Vector3(0f, yFix - 0.06f + yUp, 0f);
		} else {
			elapsedTimeFromFloatingStarted = 0;
			floatingRotation = 0f;
			isFloating = false;
			switchableSushi.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
			switchableSushi.transform.localPosition = new Vector3 (0f, 0f, 0f);
			speed = maxSpeed;
			floatingEffect.enableEmission = false;
		}
		transform.position += transform.forward * floatingSpeed;
		if (keyConfig.IsPressed(ConfigurableKeyType.Left)) {
			transform.Rotate (Vector3.down * rotationSpeed);
		}

		if (keyConfig.IsPressed(ConfigurableKeyType.Right)) {
			transform.Rotate (Vector3.up * rotationSpeed);
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (IsJumping) {
			onJumping ();
			return;
		}
		onGroundingBeforeIntersect ();

		Intersection intersection = updateLastIntersectedFloor (lastIntersectedFloor);
		if (!intersection.IsIntersected) {
			intersection = updateLastIntersectedFloor (lastIntersectedFloor.Next);
		}

		if (!intersection.IsIntersected) {
			intersection = updateLastIntersectedFloor (lastIntersectedFloor.Prev);
		}

		if (intersection.IsIntersected) {
			onIntersect (intersection);
		} else {
			onNotIntersect ();
		}

		if (isGrounding || isFloating) {
			onGrounding (intersection);
		} else {
			onFlying ();
		}
	}

	void drive() {
		if (keyConfig.IsPressed(ConfigurableKeyType.Space) || keyConfig.IsPressed(ConfigurableKeyType.Enter)) {
			speed += acceleration;
			if (speed > maxSpeed) {
				speed = maxSpeed;
			}
		} else {
			speed *= speedDecrFactor;
		}

		transform.position += transform.forward * speed;

		if (keyConfig.IsPressed(ConfigurableKeyType.Left)) {
			transform.RotateAround (transform.position, transform.up, -rotationSpeed);
		}

		if (keyConfig.IsPressed(ConfigurableKeyType.Right)) {
			transform.RotateAround (transform.position, transform.up, rotationSpeed);
		}
	}

	void autoDrive(Intersection intersection) {
		if (!intersection.IsIntersected) {
			transform.position = Vector3.Lerp(transform.position, lastIntersectedFloor.GetMiddle(transform.position), 0.1f);
			return;
		}
		Vector3 forward = lastIntersectedFloor.GetTangentDir (transform.position);
		if (IsReverse) {
			forward = -forward;
		}
		Vector3 up = intersection.Normal;
		transform.rotation = Quaternion.LookRotation (forward, up);
		transform.position = Vector3.Lerp(transform.position, lastIntersectedFloor.GetMiddle(transform.position), 0.01f);
		transform.position += transform.forward * AutoDriveSpeed;
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag == "Accelerator") {
			if (!IsAI && !IsAuto) {
				soundManager.AcceleAudio.Play ();
				floatingStartTime = Time.time;
				isFloating = true;
				floatingEffect.enableEmission = true;
			}
		} else if (col.gameObject.tag == "SuperAccelerator") {
			if (!IsAI && !IsAuto) {
				soundManager.AcceleAudio.Play ();
				startJump ();
			}
		}
	}

	void OnCollisionStay(Collision col) {
		if (col.gameObject.tag == "AICol") {
			soundManager.ColAudio.Play ();
		}
	}
}
