using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CourseFactory : MonoBehaviour {
	Mesh mesh;
	public Floor Floor;
	public bool IsCourseCreated = false;

	public void CreateCourse() {
		if (!IsCourseCreated) {
			mesh = new Mesh(); 

			Floor floor = new Floor (new Vector3 (0f, 0f, 8f), new Vector3 (0f, 0f, -8f), new Vector3(24f, 0f, -8f), new Vector3(24f, 0f, 8f));
			floor
				.Extend (24f)
				.Extend(24f)
				.Extend (24f)
				.Extend(24f)
				.ExtendOrbitY(100f, -Mathf.PI / 48, 48, 2f)
				.ExtendHorizontally(24f, true)
				.Extend(24f)
				.ExtendOrbitX(200f, -Mathf.PI / 48f, 24);

			Vector3 [] vertices = floor.Vertices.ToArray();
			Vector2 [] uv = new Vector2[vertices.Length];
			int[] triangles = new int[floor.Count() * 2 * 3];

			for (int i = 0; i < uv.Length; i += 4) {
				uv[i] = new Vector2(0.0f, 0.0f);
				uv[i + 1] = new Vector2(0.0f, 1.0f);
				uv[i + 2] = new Vector2(1.0f, 1.0f);
				uv[i + 3] = new Vector2(1.0f, 0.0f);
			}

			for (int i = 0; i < triangles.Length; i += 6) {
				int vi = (i * 4) / 6;
				triangles[i + 0] = vi + 2;
				triangles[i + 1] = vi + 1;
				triangles[i + 2] = vi + 0;
				triangles[i + 3] = vi + 0;
				triangles[i + 4] = vi + 3;
				triangles[i + 5] = vi + 2;
			}

			mesh.vertices  = vertices;
			mesh.uv        = uv;
			mesh.triangles = triangles;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			GetComponent<MeshFilter>().sharedMesh = mesh;
			GetComponent<MeshFilter>().sharedMesh.name = "courseMesh";
			Floor = floor;
			IsCourseCreated = true;
		}
	}

	// Use this for initialization
	void Start () {
		CreateCourse ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}



public class NearPlanes {
	Plane Nearest;
	Plane Next;
	Plane Prev;

	public NearPlanes(Plane nearest, Plane next, Plane prev) {
		Nearest = nearest;
		Next = next;
		Prev = prev;
	}

	public Quaternion GetLerpRotation(Vector3 currentPosition, Vector3 currentRight) {
		/*
		Quaternion nearestRotation = getRotationByPlane(Nearest, currentPosition, currentRight);
		Quaternion nextRotation = getRotationByPlane (Next, currentPosition, currentRight);
		Quaternion prevRotation = getRotationByPlane (Prev, currentPosition, currentRight);
		*/
		Quaternion nearestRotation = getRotationByPlane(Nearest, currentPosition, currentRight);
		Quaternion q = lerpRotationByPlaneDistance (currentPosition, currentRight, Nearest, Next);
		if (nearestRotation == q) {
			return lerpRotationByPlaneDistance (currentPosition, currentRight, Nearest, Prev);
		}
		return q;
	}

	Quaternion lerpRotationByPlaneDistance(Vector3 currentPosition, Vector3 currentRight, Plane p1, Plane p2) {
		Vector3 p1Center = p1.GetCenter();
		Vector3 p2Center = p2.GetCenter();
		Quaternion p1Rotation = getRotationByPlane(p1, currentPosition, currentRight);
		Quaternion p2Rotation = getRotationByPlane(p2, currentPosition, currentRight);

		Vector3 vecToP1 = p1Center - currentPosition;
		Vector3 vecToP2 = p2Center - currentPosition;
		if (Vector3.Dot(vecToP1, vecToP2) < 0) {
			Vector3 p1ToP2 = (p2Center - p1Center);
			Vector3 p1ToP2Dir = p1ToP2.normalized;
			//Debug.Log (p1ToP2.magnitude);
			Debug.Log ((-Vector3.Dot (p1ToP2Dir, vecToP1)) / p1ToP2.magnitude);
			return Quaternion.Lerp (p1Rotation, p2Rotation, -Vector3.Dot (p1ToP2Dir, vecToP1) / p1ToP2.magnitude);
		}
		return p1Rotation;
	}

	Quaternion getRotationByPlane(Plane plane, Vector3 currentPosition, Vector3 currentRight) {
		Vector3 nextUp = plane.GetUp();
		//Debug.Log (nextUp);
		Vector3 nextRight = (plane.GetIntersectPosition (currentRight + currentPosition) - currentPosition).normalized;
		Vector3 nextForward = Vector3.Cross (nextRight, nextUp).normalized;
		return Quaternion.LookRotation (nextForward, nextUp);
	}
}

public class Intersection {
	public bool IsIntersected;
	public Vector3 IntersectPosition;
	public Vector3[] TriangleVertices;
	public Plane Plane;
	public NearPlanes NearPlanes;

	public Intersection(bool isIntersected, Vector3 intersectPosition, Vector3[] triangleVertices, Plane plane) {
		IsIntersected = isIntersected;
		IntersectPosition = intersectPosition;
		TriangleVertices = triangleVertices;
		Plane = plane;
	}

	public static Intersection Intersected(Vector3 intersectPosition, Vector3 v0, Vector3 v1, Vector3 v2) {
		Vector3[] triangleVertices = new Vector3[3];
		triangleVertices [0] = v0;
		triangleVertices [1] = v1;
		triangleVertices [2] = v2;
		return new Intersection (true, intersectPosition, triangleVertices, new Plane(v0, v1, v2));
	}

	public Vector3 Up {
		get {
			if (TriangleVertices == null) {
				return Vector3.up;
			} else {
				return - Vector3.Cross (TriangleVertices [1] - TriangleVertices [0], TriangleVertices [2] - TriangleVertices [0]).normalized;
			}
		}
	}

	public static Intersection NotIntersected() {
		return new Intersection (false, Vector3.zero, null, null);
	}
}

public class Plane {
	public Vector3 V0;
	public Vector3 V1;
	public Vector3 V2;

	public Plane(Vector3 v0, Vector3 v1, Vector3 v2) {
		V0 = v0;
		V1 = v1;
		V2 = v2;
	}

	public Vector3 GetIntersectPosition(Vector3 pos) {
		Vector3 normal = Vector3.Cross (V1 - V0, V2 - V0).normalized;
		float d = -(V0.x * normal.x + V0.y * normal.y + V0.z * normal.z);
		float t = d - Vector3.Dot (pos, normal);
		return pos + normal * t;
	}

	public Vector3 GetCenter() {
		return (V0 + V1 + V2) / 3f;
	}

	public Vector3 GetUp() {
		return - Vector3.Cross (V1 - V0, V2 - V0).normalized;
	}
}

public class Floor {
	public List<Vector3> Vertices;
	public Floor Next;
	public Floor Prev;
	public Vector3 Center;
	int startIndex;

	public Floor(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3) {
		Vertices = new List<Vector3> ();
		Vertices.Add (v0);
		Vertices.Add (v1);
		Vertices.Add (v2);
		Vertices.Add (v3);
		Center = (v0 + v1 + v2 + v3) / 4f;
		startIndex = 0;
	}

	Vector3 getForward() {
		return (Vector3.Lerp (
			Vertices [startIndex], Vertices [startIndex + 1], 0.5f)
			- Vector3.Lerp (Vertices [startIndex + 3], Vertices [startIndex + 2], 0.5f)).normalized;
	}

	public Floor(Vector3 v0, Vector3 v1, Floor prev) {
		this.Prev = prev;
		this.Vertices = prev.Vertices;
		this.startIndex = this.Vertices.Count;
		Vector3 v2 = this.Vertices [prev.startIndex + 1];
		Vector3 v3 = this.Vertices [prev.startIndex];
		this.Vertices.Add (v0);
		this.Vertices.Add (v1);
		this.Vertices.Add (v2);
		this.Vertices.Add (v3);
		Center = (v0 + v1 + v2 + v3) / 4f;
	}

	public Floor Extend(Vector3 v0, Vector3 v1) {
		Floor floor = new Floor (v0, v1, this);
		this.Next = floor;
		return floor;
	}

	public Floor Extend(float length) {
		Vector3 forward = getForward ();
		Vector3 v0 = Vertices [startIndex] + forward * length;
		Vector3 v1 = Vertices [startIndex + 1] + forward * length;
		return Extend (v0, v1);
	}

	public Floor ExtendHorizontally(float length, bool isUseLeftY) {
		Vector3 forward = getForward ();
		Vector3 v0 = Vertices [startIndex] + forward * length;
		Vector3 v1 = Vertices [startIndex + 1] + forward * length;
		if (isUseLeftY) {
			v1.y = v0.y;
		} else {
			v0.y = v1.y;
		}
		return Extend (v0, v1);
	}

	public Floor ExtendOrbitY(float radius, float theta, int extendCount, float up) {
		Vector3 prevV0 = Vertices [startIndex];
		Vector3 prevV1 = Vertices [startIndex + 1];
		Vector3 dirStartToCenter = (prevV1 - prevV0).normalized;
		if (theta < 0f) {
			dirStartToCenter = -dirStartToCenter;
		}
		Vector3 dirCenterToStart = -dirStartToCenter;
		Vector3 floorEnd = Vector3.Lerp(prevV0, prevV1, 0.5f);
		Vector3 center = floorEnd + dirStartToCenter * radius;
		float centerToV0Length = (prevV0 - center).magnitude;
		float centerToV1Length = (prevV1 - center).magnitude;
		float startTheta = Mathf.Atan2 (dirCenterToStart.z, dirCenterToStart.x);
		Floor floor = this;
		for (int i = 0; i < extendCount; i++) {
			center.y += up;
			float nextTheta = startTheta + theta * (float)(i + 1);
			Vector3 nextDir = new Vector3 (Mathf.Cos (nextTheta), 0f, Mathf.Sin (nextTheta));
			Vector3 v0 = center + nextDir * centerToV0Length;
			Vector3 v1 = center + nextDir * centerToV1Length;
			/*
			if (theta < 0f) {
				v1.y += up;
			} else {
				v0.y += up;
			}*/
			floor = floor.Extend (v0, v1);
		}
		return floor;
	}

	public Floor ExtendOrbitX(float radius, float theta, int extendCount) {
		Vector3 prevV0 = Vertices [startIndex];
		Vector3 prevV1 = Vertices [startIndex + 1];
		Vector3 prevV2 = Vertices [startIndex + 2];
		Vector3 v1ToV0 = prevV0 - prevV1;
		Vector3 floorUp = Vector3.Cross(prevV1 - prevV0, prevV2 - prevV0).normalized;
		Vector3 floorForward = (prevV1 - prevV2).normalized;
		Quaternion startRotation = Quaternion.LookRotation (floorForward, floorUp);
		Vector3 dirCenterToStart = floorUp;
		if (theta > 0f) {
			dirCenterToStart = -dirCenterToStart;
		}
		Vector3 floorEnd = Vector3.Lerp(prevV0, prevV1, 0.5f);
		Vector3 center = floorEnd - dirCenterToStart * radius;
		Floor floor = this;
		float startDir = Mathf.PI * 3f / 2f;
		if (theta < 0f) {
			startDir = Mathf.PI / 2f;
		}
		for (int i = 0; i < extendCount; i++) {
			float nextTheta = theta * (float)(i + 1) + startDir;
			Vector3 nextDir = new Vector3 (0f, Mathf.Sin (nextTheta), Mathf.Cos (nextTheta));
			Vector3 v0 = center + startRotation * (nextDir * radius) + v1ToV0 / 2f;
			Vector3 v1 = center + startRotation * (nextDir * radius) - v1ToV0 / 2f;
			floor = floor.Extend (v0, v1);
		}
		return floor;
	}

	public Intersection GetIntersection(Vector3 pos) {
		Intersection triangleIntersection = getIntersection (pos, Vertices [startIndex], Vertices [startIndex + 1], Vertices [startIndex + 2]);
		if (triangleIntersection.IsIntersected) {
			if (Next != null) {
				Plane nearest = new Plane (Vertices [startIndex], Vertices [startIndex + 1], Vertices [startIndex + 2]);
				Plane next = new Plane (Vertices [startIndex + 6], Vertices [startIndex + 7], Vertices [startIndex + 4]);
				Plane prev = new Plane (Vertices [startIndex + 2], Vertices [startIndex + 3], Vertices [startIndex]);
				NearPlanes planes = new NearPlanes (nearest, next, prev);
				triangleIntersection.NearPlanes = planes;
			}
			return triangleIntersection;
		}
		triangleIntersection = getIntersection (pos, Vertices [startIndex + 2], Vertices [startIndex + 3], Vertices [startIndex]);
		if (triangleIntersection.IsIntersected) {
			if (Next != null) {
				Plane nearest = new Plane (Vertices [startIndex + 2], Vertices [startIndex + 3], Vertices [startIndex]);
				Plane next = new Plane (Vertices [startIndex], Vertices [startIndex + 1], Vertices [startIndex + 2]);
				Plane prev = new Plane (Vertices [startIndex - 4], Vertices [startIndex - 3], Vertices [startIndex - 2]);
				NearPlanes planes = new NearPlanes (nearest, next, prev);
				triangleIntersection.NearPlanes = planes;
			}
			return triangleIntersection;
		}
		return triangleIntersection;
	}

	public Intersection GetNearPlaneIntersection(Vector3 pos) {
		Intersection intersection1 = getPlaneIntersection (pos, Vertices [startIndex], Vertices [startIndex + 1], Vertices [startIndex + 2]);
		Intersection intersection2 = getPlaneIntersection (pos, Vertices [startIndex + 2], Vertices [startIndex + 3], Vertices [startIndex]);
		float dist1 = Vector3.Distance (intersection1.IntersectPosition, pos);
		float dist2 = Vector3.Distance (intersection1.IntersectPosition, pos);
		if (dist1 < dist2) {
			return intersection1;
		} else {
			return intersection2;
		}
	}

	Intersection getIntersection(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2) {
		Vector3 intersectPosition = getIntersectPosition (pos, v0, v1, v2);
		Vector3 v01 = v1 - v0;
		Vector3 v1i = intersectPosition - v1;
		Vector3 v12 = v2 - v1;
		Vector3 v2i = intersectPosition - v2;
		Vector3 v20 = v0 - v2;
		Vector3 v0i = intersectPosition - v0;
		Vector3 c1 = Vector3.Cross (v01, v1i);
		Vector3 c2 = Vector3.Cross (v12, v2i);
		Vector3 c3 = Vector3.Cross (v20, v0i);
		bool isIntersect = Vector3.Dot (c1, c2) >= 0f && Vector3.Dot (c1, c3) >= 0f;
		if (isIntersect) {
			return Intersection.Intersected (intersectPosition, v0, v1, v2);
		}
		return Intersection.NotIntersected();
	}

	Intersection getPlaneIntersection(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2) {
		Vector3 intersectPosition = getIntersectPosition (pos, v0, v1, v2);
		Vector3 v01 = v1 - v0;
		Vector3 v1i = intersectPosition - v1;
		Vector3 v12 = v2 - v1;
		Vector3 v2i = intersectPosition - v2;
		Vector3 v20 = v0 - v2;
		Vector3 v0i = intersectPosition - v0;
		Vector3 c1 = Vector3.Cross (v01, v1i);
		Vector3 c2 = Vector3.Cross (v12, v2i);
		Vector3 c3 = Vector3.Cross (v20, v0i);
		return Intersection.Intersected (intersectPosition, v0, v1, v2);
	}

	public Vector3 getIntersectPosition(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2) {
		Vector3 normal = Vector3.Cross (v1 - v0, v2 - v0).normalized;
		Vector3 rayDir = -normal;
		float d = (v0.x * normal.x + v0.y * normal.y + v0.z * normal.z);
		float t = (d - Vector3.Dot (pos, normal)) / Vector3.Dot(rayDir, normal);
		return pos + rayDir * t;
	}

	public int Count() {
		if (Next == null) {
			return 1;
		} else {
			return 1 + Next.Count ();
		}
	}
}
