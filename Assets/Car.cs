using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour {
	float speed = 0f;
	float acceleration = 0.01f;
	float speedDecrFactor = 0.98f;
	float rotationSpeed = 1f;
	bool isGrounding = true;
	int elapsedFlameFromLastGrounded = 0;
	float zLength = 0.8f;
	float xLength = 0.5f;
	CourseFactory courseFactory;
	//Floor lastIntersectedFloor;
	Floor rfLastIntersectedFloor;
	Floor lfLastIntersectedFloor;
	Floor rbLastIntersectedFloor;
	Floor lbLastIntersectedFloor;

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
		courseFactory = GameObject.Find ("Course").GetComponent<CourseFactory> ();
		courseFactory.CreateCourse ();
		//lastIntersectedFloor = courseFactory.Floor;
		rfLastIntersectedFloor = courseFactory.Floor;
		lfLastIntersectedFloor = courseFactory.Floor;
		rbLastIntersectedFloor = courseFactory.Floor;
		lbLastIntersectedFloor = courseFactory.Floor;
	}

	/*
	Intersection updateLastIntersectedFloor(Floor floor) {
		if (floor == null) {
			return Intersection.NotIntersected ();
		}

		Intersection intersection = floor.GetIntersection (transform.position);
		if (intersection.IsIntersected) {
			lastIntersectedFloor = floor;
		}
		return intersection;
	}
	*/

	FloorWithIntersection getIntersectedFloor(Floor prevIntersectedFloor, Vector3 position) {
		if (prevIntersectedFloor == null) {
			return new FloorWithIntersection(null, Intersection.NotIntersected());
		}

		Intersection intersection = prevIntersectedFloor.GetIntersection (position);
		if (intersection.IsIntersected) {
			return new FloorWithIntersection (prevIntersectedFloor, intersection);
		}
		intersection = prevIntersectedFloor.Next.GetIntersection (position);
		if (intersection.IsIntersected) {
			return new FloorWithIntersection (prevIntersectedFloor.Next, intersection);
		}
		intersection = prevIntersectedFloor.Prev.GetIntersection (position);
		if (intersection.IsIntersected) {
			return new FloorWithIntersection (prevIntersectedFloor.Prev, intersection);
		}
		return new FloorWithIntersection (null, intersection);
	}

	/*
	Quaternion getRotationByIntersection(Intersection intersection) {
		Vector3 nextUp = intersection.Up;
		Vector3 nextRight = (intersection.Plane.GetIntersectPosition (transform.right + transform.position) - transform.position).normalized;
		Vector3 nextForward = Vector3.Cross (nextRight, nextUp).normalized;
		return Quaternion.LookRotation (nextForward, nextUp);
	}
	*/

	// Update is called once per frame
	void Update () {
		Vector3 rfPosition = transform.position + transform.forward * (zLength / 2f) + transform.right * (xLength / 2f);
		Vector3 lfPosition = transform.position + transform.forward * (zLength / 2f) - transform.right * (xLength / 2f);
		Vector3 rbPosition = transform.position - transform.forward * (zLength / 2f) + transform.right * (xLength / 2f);
		Vector3 lbPosition = transform.position - transform.forward * (zLength / 2f) - transform.right * (xLength / 2f);

		FloorWithIntersection rfIntersectedFloor = getIntersectedFloor (rfLastIntersectedFloor, rfPosition);
		FloorWithIntersection lfIntersectedFloor = getIntersectedFloor (lfLastIntersectedFloor, lfPosition);
		FloorWithIntersection rbIntersectedFloor = getIntersectedFloor (rbLastIntersectedFloor, rbPosition);
		FloorWithIntersection lbIntersectedFloor = getIntersectedFloor (lbLastIntersectedFloor, lbPosition);

		Vector3 forwardDirRight = (rfIntersectedFloor.Intersection.IntersectPosition - rbIntersectedFloor.Intersection.IntersectPosition).normalized;
		Vector3 forwardDirLeft = (lfIntersectedFloor.Intersection.IntersectPosition - lbIntersectedFloor.Intersection.IntersectPosition).normalized;
		Vector3 rightDirForward = (rfIntersectedFloor.Intersection.IntersectPosition - lfIntersectedFloor.Intersection.IntersectPosition).normalized;
		Vector3 rightDirBack = (rbIntersectedFloor.Intersection.IntersectPosition - lbIntersectedFloor.Intersection.IntersectPosition).normalized;

		Vector3 nextForward = ((forwardDirRight + forwardDirLeft) / 2f).normalized;
		Vector3 nextRight = ((rightDirForward + rightDirBack) / 2f).normalized;

		/*
		Intersection intersection = updateLastIntersectedFloor (lastIntersectedFloor);
		if (!intersection.IsIntersected) {
			intersection = updateLastIntersectedFloor (lastIntersectedFloor.Next);
		}

		if (!intersection.IsIntersected) {
			intersection = updateLastIntersectedFloor (lastIntersectedFloor.Prev);
		}
		*/

		if (rfIntersectedFloor.Intersection.IsIntersected) {
			rfLastIntersectedFloor = rfIntersectedFloor.Floor;
		}

		if (lfIntersectedFloor.Intersection.IsIntersected) {
			lfLastIntersectedFloor = lfIntersectedFloor.Floor;
		}


		if (rbIntersectedFloor.Intersection.IsIntersected) {
			rbLastIntersectedFloor = rbIntersectedFloor.Floor;
		}


		if (lbIntersectedFloor.Intersection.IsIntersected) {
			lbLastIntersectedFloor = lbIntersectedFloor.Floor;
		}


		if (rfIntersectedFloor.Intersection.IsIntersected &&
		    lfIntersectedFloor.Intersection.IsIntersected &&
		    rbIntersectedFloor.Intersection.IsIntersected &&
		    lbIntersectedFloor.Intersection.IsIntersected) {
			Vector3 nextPosition = (
				rfIntersectedFloor.Intersection.IntersectPosition +
				lfIntersectedFloor.Intersection.IntersectPosition +
				rbIntersectedFloor.Intersection.IntersectPosition +
				lbIntersectedFloor.Intersection.IntersectPosition
			) / 4f;
			transform.position = nextPosition;//Vector3.Lerp(transform.position, nextPosition, 0.1f);

			Quaternion nextRotation = Quaternion.LookRotation(nextForward, Vector3.Cross(nextForward, nextRight));
			transform.rotation = nextRotation;//Quaternion.Lerp(transform.rotation, nextRotation, 0.3f);
		}
		//if (intersection.IsIntersected) {
			//transform.position = intersection.IntersectPosition;

			//transform.position = Vector3.Lerp(transform.position, intersection.IntersectPosition, 0.1f);
	
			/*
			Vector3 nextUp = intersection.Up;
			Vector3 nextRight = (intersection.Plane.GetIntersectPosition (transform.right + transform.position) - transform.position).normalized;
			Vector3 nextForward = Vector3.Cross (nextRight, nextUp).normalized;
			*/

			/*
			Quaternion preferedRotation = getRotationByIntersection (intersection);


			if (lastIntersectedFloor.Next != null && lastIntersectedFloor.Prev != null) {
				Vector3 currentCenter = lastIntersectedFloor.Center;
				Quaternion nextFloorRotation = getRotationByIntersection (lastIntersectedFloor.Next.GetNearPlaneIntersection (transform.position));
				Vector3 nextCenter = lastIntersectedFloor.Next.Center;
				Vector3 vecToCurrent = currentCenter - transform.position;
				Vector3 vecToNext = nextCenter - transform.position;
				if (Vector3.Dot(vecToCurrent, vecToNext) < 0) {
					Vector3 currentToNext = (nextCenter - currentCenter);
					Vector3 currentToNextDir = currentToNext.normalized;
					preferedRotation = Quaternion.Lerp (preferedRotation, nextFloorRotation, -Vector3.Dot (currentToNextDir, vecToCurrent) / currentToNext.magnitude);
				}

				Quaternion prevFloorRotation = getRotationByIntersection (lastIntersectedFloor.Prev.GetNearPlaneIntersection (transform.position));
				Vector3 prevCenter = lastIntersectedFloor.Prev.Center;
				Vector3 vecToPrev = prevCenter - transform.position;
				if (Vector3.Dot(vecToCurrent, vecToPrev) < 0) {
					Vector3 currentToPrev = (prevCenter - currentCenter);
					Vector3 currentToPrevDir = currentToPrev.normalized;
					Debug.Log (-Vector3.Dot (currentToPrevDir, vecToCurrent) / currentToPrev.magnitude);
					preferedRotation = Quaternion.Lerp (preferedRotation, prevFloorRotation, -Vector3.Dot (currentToPrevDir, vecToCurrent) / currentToPrev.magnitude);
				}
			}*/
		/*
			if (intersection.NearPlanes != null) {

				Quaternion preferedRotation = intersection.NearPlanes.GetLerpRotation (transform.position, transform.right);

				//transform.rotation = Quaternion.Lerp (transform.rotation, preferedRotation, 0.2f);
				transform.rotation = preferedRotation;
			}
		}
	*/
		if (Input.GetKey(KeyCode.Space)) {
			speed += acceleration;
		} else {
			speed *= speedDecrFactor;
		}

		transform.position += transform.forward * speed;

		if (Input.GetKey(KeyCode.LeftArrow)) {
			transform.Rotate (Vector3.down * rotationSpeed);
		}

		if (Input.GetKey(KeyCode.RightArrow)) {
			transform.Rotate (Vector3.up * rotationSpeed);
		}
	}
}
