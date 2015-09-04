using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CourseFactory : MonoBehaviour {
	public Floor Floor;
	public bool IsCourseCreated = false;
	public GameObject AcceleratorGameObject;
	public GameObject RunningHorseGameObject;
	public GameObject RollingAgariGameObject;
	public GameObject SuperAcceleratorGameObject;
	public GameObject LandingSiteGameObject;
	public int FloorId;

	public void CreateCourse () {
		if (!IsCourseCreated) {
			FloorList floors = CreateFloorList();
			Floor = floors.Head;
			GetComponent<MeshFilter> ().sharedMesh = floors.Mesh;
			GetComponent<MeshFilter> ().sharedMesh.name = "courseMesh";
			IsCourseCreated = true;
		}
	}

	protected FloorList CreateFloorList () {
		switch (FloorId) {
		case 1:
			return new Stage1FloorsFactory().CreateFloorList(this);
		case 2:
			return new Stage2FloorsFactory ().CreateFloorList (this);
		case 3:
			return new Stage3FloorsFactory ().CreateFloorList (this);
		case 4:
			return new Stage4FloorsFactory ().CreateFloorList (this);
		case 5:
			return new Stage5FloorsFactory ().CreateFloorList (this);
		default:
			return null;
		}
	}

	public void PutAccelerator(Vector3 position, Quaternion rotation) {
		Instantiate(AcceleratorGameObject, position, rotation);
	}

	public void PutAgari(Vector3 position, Quaternion rotation) {
		Instantiate(RollingAgariGameObject, position, rotation);
	}

	public void PutHorse(Vector3 position, Quaternion rotation, Floor initFloor) {
		Car horse = (Instantiate(RunningHorseGameObject, position, rotation) as GameObject).GetComponent<Car>();
		horse.LastIntersectedFloor = initFloor;
	}

	public void PutSuperAccelerator(Vector3 position, Quaternion rotation) {
		Instantiate(SuperAcceleratorGameObject, position, rotation);
	}

	public void PutLandingSite(Vector3 position, Quaternion rotation, Floor initFloor) {
		LandingSite site = (Instantiate(LandingSiteGameObject, position, rotation) as GameObject).GetComponent<LandingSite>();
		site.Floor = initFloor;
	}

	// Use this for initialization
	void Start () {
		CreateCourse ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public abstract class FloorListFactory {
	public abstract FloorList CreateFloorList (CourseFactory cf);
}

public class Intersection {
	public bool IsIntersected;
	public Vector3 IntersectPosition;
	public Vector3 Normal;

	public Intersection(
		bool isIntersected,
		Vector3 intersectPosition,
		Vector3 normal) {
		IsIntersected = isIntersected;
		IntersectPosition = intersectPosition;
		Normal = normal;
	}

	public static Intersection Intersected(Vector3 intersectPosition, Vector3 normal) {
		return new Intersection (true, intersectPosition, normal);
	}

	public static Intersection NotIntersected() {
		return new Intersection (false, Vector3.zero, Vector3.zero);
	}
}

public abstract class Floor {
	public Floor Next;
	public Floor Prev;
	public Vector3 RightForward;
	public Vector3 LeftForward;
	public Vector3 RightBack;
	public Vector3 LeftBack;
	public float Height = 2f;
	public float Width;
	public Vector3 Forward;
	public Mesh Mesh;
	public int Order = 0;

	public abstract Intersection GetIntersection (Vector3 pos);

	public abstract Vector3 GetTangentDir (Vector3 pos);

	public abstract Vector3 GetMiddle (Vector3 pos);

	public abstract float GetPercentage (Vector3 pos);

	protected void SetupNormal(bool isFlip = false) {
		Vector3[] vertices = Mesh.vertices;
		Vector3[] normals = new Vector3[vertices.Length];
		/**
		 * 6 5
		 * 7 8
		 * 2 1
		 * 3 4
		 * ---
		 * 4 4
		 * 4 4
		 * ---
		 * 
		 * vi + 5, vi + 4
		 * vi + 6, vi + 7
		 * vi + 1, vi + 0
		 * vi + 2, vi + 3
		 * 
		 * merge (vi + 7, vi + 1) and (vi + 8, vi + 0)
		 */
		float rectCount = vertices.Length / 4;

		for (int i = 0; i < (rectCount - 1); i++) {
			int vi = i * 4;
			Vector3 rfn = - Vector3.Cross (vertices [vi + 4] - vertices [vi + 7], vertices [vi + 6] - vertices [vi + 7]).normalized;
			Vector3 rbn = Vector3.Cross (vertices [vi + 3] - vertices [vi + 0], vertices [vi + 1] - vertices [vi + 0]).normalized;
			Vector3 lfn = Vector3.Cross (vertices [vi + 5] - vertices [vi + 6], vertices [vi + 7] - vertices [vi + 6]).normalized;
			Vector3 lbn = - Vector3.Cross (vertices [vi + 2] - vertices [vi + 1], vertices [vi + 0] - vertices [vi + 1]).normalized;
			Vector3 rn = ((rfn + rbn) / 2f).normalized;
			Vector3 ln = ((lfn + lbn) / 2f).normalized;
			normals[vi + 7] = rn;
			normals[vi + 0] = rn;
			normals[vi + 6] = ln;
			normals[vi + 1] = ln;
		}


		if (isFlip) {
			for (int i = 0; i < normals.Length; i++) {
				normals [i] = -normals [i];
			}
		}

		normals [2] = normals [1];
		normals [3] = normals [0];
		normals [normals.Length - 3] = normals [normals.Length - 2];
		normals [normals.Length - 4] = normals [normals.Length - 1];
		Mesh.normals = normals;
	}
}

public class FloorList {
	public Floor Head;
	public Floor Last;
	CourseFactory factory;
	public Mesh Mesh {
		get {
			Mesh mesh = new Mesh ();
			Floor it = Head;
			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uv = new List<Vector2>();
			List<int> triangles = new List<int>();

			do {
				int vertCount = vertices.Count;
				vertices.AddRange (it.Mesh.vertices);
				normals.AddRange (it.Mesh.normals);
				uv.AddRange (it.Mesh.uv);
				int[] nextTriangles = new int[it.Mesh.triangles.Length];
				for (int i = 0; i < nextTriangles.Length; i++) {
					nextTriangles [i] = vertCount + it.Mesh.triangles [i];
				}
				triangles.AddRange (nextTriangles);
				it = it.Next;
			} while (it != Head);

			int floorCount = vertices.Count / 4;

			// back

			int vc = vertices.Count;

			for (int i = 0; i < vc; i++) {
				vertices.Add (vertices [i] - normals [i] * 2f);
			}

			Vector3[] reversedNormals = new Vector3[normals.Count];
			for (int i = 0; i < normals.Count; i++) {
				reversedNormals [i] = -normals [i];
			}

			normals.AddRange(normals);

			for (int i = 0; i < floorCount; i++) {
				int vi = (floorCount + i) * 4;
				int[] nextTriangles = new int[6];
				nextTriangles [0] = vi + 0;
				nextTriangles [1] = vi + 1;
				nextTriangles [2] = vi + 2;
				nextTriangles [3] = vi + 2;
				nextTriangles [4] = vi + 3;
				nextTriangles [5] = vi + 0;
				triangles.AddRange (nextTriangles);
			}

			uv.AddRange (uv);
	
			// side

			for (int i = 0; i < floorCount; i++) {
				int vi = i * 4;
				int vi0 = vi + 1;
				int vi1 = floorCount * 4 + vi + 1;
				int vi2 = floorCount * 4 + vi + 2;
				int vi3 = vi + 2;

				int vi4 = vi + 0;
				int vi5 = floorCount * 4 + vi + 0;
				int vi6 = floorCount * 4 + vi + 3;
				int vi7 = vi + 3;

				Vector3 f = vertices [vi0] - vertices [vi3];
				Vector3 f2 = vertices [vi1] - vertices [vi2];
				Vector3[] nextVertices = new Vector3[] {
					vertices[vi0], vertices[vi1], vertices[vi2], vertices[vi3],
					vertices[vi4], vertices[vi5], vertices[vi6], vertices[vi7]
				};
				Vector3[] nextNormals = new Vector3[] {
					Vector3.Cross(f, normals[vi0]),
					Vector3.Cross(f, normals[vi1]),
					Vector3.Cross(f, normals[vi2]),
					Vector3.Cross(f, normals[vi3]),
					-Vector3.Cross(f2, normals[vi0]),
					-Vector3.Cross(f2, normals[vi1]),
					-Vector3.Cross(f2, normals[vi2]),
					-Vector3.Cross(f2, normals[vi3])
				};
				Vector2[] nextUVs = new Vector2[] {
					uv [vi0], uv [vi4], uv [vi7], uv [vi3],
					uv [vi0], uv [vi4], uv [vi7], uv [vi3]  };


				int vertCount = vertices.Count;
				int[] nextTriangles = new int[] {
					vertCount + 2,
					vertCount + 1,
					vertCount + 0,
					vertCount + 0,
					vertCount + 3,
					vertCount + 2,
					vertCount + 0 + 4,
					vertCount + 1 + 4,
					vertCount + 2 + 4,
					vertCount + 2 + 4,
					vertCount + 3 + 4,
					vertCount + 0 + 4
				};

				vertices.AddRange (nextVertices);
				normals.AddRange (nextNormals);
				uv.AddRange (nextUVs);
				triangles.AddRange (nextTriangles);
			}

			mesh.vertices  = vertices.ToArray();
			mesh.uv        = uv.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.normals = normals.ToArray ();

			mesh.RecalculateBounds();

			return mesh;
		}
	}

	public FloorList(Floor head, CourseFactory factory) {
		Head = head;
		Last = head;
		this.factory = factory;
	}

	public FloorList Flat(float depth) {
		Vector3 forward = Last.Forward;
		Vector3 startPosition = (Last.RightForward + Last.LeftForward) / 2f;
		Vector3 position = startPosition + forward * (depth / 2f);
		Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
		Vector3 normal = Vector3.Cross (forward, right).normalized;
		float width = Last.Width;
		Floor floor = new FlatFloor (normal, position, forward, depth, width);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList Incline(float depth, float startInclination, float endInclination, bool isFlip = false) {
		Vector3 forward = Last.Forward;
		Vector3 startPosition = (Last.RightForward + Last.LeftForward) / 2f;
		Vector3 position = startPosition;
		Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
		if (isFlip) {
			right = -right;
		}
		float width = Last.Width;
		Floor floor = new InclinedFloor (position, forward, right, depth, width, (int)(depth * 2f), startInclination, endInclination);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList OrbitY(float radius, float height, float startTheta, float endTheta, float inclination, Vector3 right, bool isClockwise = true) {
		Vector3 startPosition = (Last.RightForward + Last.LeftForward) / 2f;
		Vector3 center = startPosition - right * radius;

		if (!isClockwise) {
			center = startPosition + right * radius;
		}

		float width = Last.Width;
		Floor floor = new OrbitYFloor (center, width, height, radius, inclination, 50, startTheta, endTheta, isClockwise);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList Upslope(float radius, float startTheta, float endTheta) {
		Vector3 startPosition = (Last.RightForward + Last.LeftForward) / 2f;
		Vector3 center = startPosition + Vector3.up * radius;
		Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
		Floor floor = new OrbitXInsideFloor (center, right, Vector3.up, Last.Width, radius, 50, startTheta, endTheta);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList Cup(float radius) {
		Vector3 startPosition = (Last.RightForward + Last.LeftForward) / 2f;

		Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
		Vector3 up = -Vector3.Cross (right, Vector3.up).normalized;
		Vector3 center = startPosition + up * radius;
		Floor floor = new OrbitXInsideFloor (center, right, Vector3.up, Last.Width, radius, 50, 0, -Mathf.PI);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList Downslope(float radius, float startTheta, float endTheta) {
		Vector3 startPosition = (Last.RightForward + Last.LeftForward) / 2f;
		Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
		Vector3 forward = Last.Forward;
		Vector3 up = Vector3.Cross (forward, right);
		Vector3 center = startPosition + up * radius;
		Floor floor = new OrbitXInsideFloor (center, right, Vector3.up, Last.Width, radius, 50, startTheta, endTheta);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList SlopeTop(float radius, float startTheta, float endTheta) {
		Vector3 startPosition = (Last.RightForward + Last.LeftForward) / 2f;
		Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
		Vector3 up = Vector3.Cross (Last.Forward, right);
		Vector3 center = startPosition - up * radius;
		Floor floor = new OrbitXOutsideFloor (center, right, Vector3.up, Last.Width, radius, 50, startTheta, endTheta);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList Close() {
		Floor floor = new FlatFloor (Head.RightBack, Head.LeftBack, Last.LeftForward, Last.RightForward);
		Last.Next = floor;
		floor.Prev = Last;
		Last = floor;
		floor.Next = Head;
		Head.Prev = floor;
		floor.Order = floor.Prev.Order + 1;
		return this;
	}

	public FloorList Accelerator() {
		if (factory != null) {
			Vector3 pos = (Last.LeftForward + Last.RightForward) / 2f;
			Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
			Vector3 up = Vector3.Cross (Last.Forward, right);
			Quaternion rotation = Quaternion.LookRotation (-Last.Forward, up);
			factory.PutAccelerator (pos, rotation);
		}
		return this;
	}

	public FloorList Agari() {
		if (factory != null) {
			Vector3 pos = (Last.LeftForward + Last.RightForward) / 2f;
			Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
			Vector3 up = Vector3.Cross (Last.Forward, right);
			Quaternion rotation = Quaternion.LookRotation (-Last.Forward, up);
			float width = Last.Width;
			factory.PutAgari (pos + ((width / 2f) + 5f) * right, rotation);
			factory.PutAgari (pos - ((width / 2f) + 5f) * right, rotation);
		}
		return this;
	}

	public FloorList Horse() {
		if (factory != null) {
			Vector3 pos = (Last.LeftForward + Last.RightForward) / 2f;
			Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
			Vector3 up = Vector3.Cross (Last.Forward, right);
			Quaternion rotation = Quaternion.LookRotation (-Last.Forward, up);
			factory.PutHorse (pos, rotation, Last);
		}
		return this;
	}

	public FloorList SuperAccelerator() {
		if (factory != null) {
			Vector3 pos = (Last.LeftForward + Last.RightForward) / 2f;
			Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
			Vector3 up = Vector3.Cross (Last.Forward, right);
			Quaternion rotation = Quaternion.LookRotation (-Last.Forward, up);
			factory.PutSuperAccelerator (pos, rotation);
		}
		return this;
	}

	public FloorList LandingSite() {
		if (factory != null) {
			Vector3 pos = (Last.LeftForward + Last.RightForward) / 2f;
			Vector3 right = (Last.RightForward - Last.LeftForward).normalized;
			Vector3 up = Vector3.Cross (Last.Forward, right);
			Quaternion rotation = Quaternion.LookRotation (-Last.Forward, up);
			factory.PutLandingSite (pos, rotation, Last);
		}
		return this;
	}
}


// Daikaiten
class Stage1FloorsFactory: FloorListFactory {
	public override FloorList CreateFloorList (CourseFactory cf) {
		FloorList floors = new FloorList (new FlatFloor (Vector3.up, Vector3.zero, Vector3.forward, 60f, 20f), cf)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Flat (40f)
			.Accelerator ()
			.Flat (100f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Flat(40f)
			.Accelerator()
			.Flat (200f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.SlopeTop (20f, Mathf.PI / 2f, 0f)
			.Downslope (50f, Mathf.PI, Mathf.PI / 2f)
			.Upslope (50f, Mathf.PI / 2f, 0f)
			.SlopeTop (20f, Mathf.PI, Mathf.PI / 2f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, -Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Flat (40F)
			.Accelerator()
			.Close ()
			.Agari();
		return floors;
	}
}

// DarkRoom
class Stage2FloorsFactory: FloorListFactory {
	public override FloorList CreateFloorList (CourseFactory cf) {
		FloorList floors = new FloorList (new FlatFloor (Vector3.up, Vector3.zero, Vector3.forward, 40f, 20f), cf)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right)
			.Horse()
			.Flat (50f)
			.OrbitY (80f, 21f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, Vector3.forward)
			.OrbitY (80f, 21f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.OrbitY (80f, 21f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, -Vector3.forward)
			.OrbitY (80f, 21f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right)
			.OrbitY (80f, 21f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, Vector3.forward)
			.OrbitY (80f, 21f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 8f)

			.Upslope (50f, Mathf.PI / 2f, 0f)
			.Flat (20f)
			.SlopeTop (20f, Mathf.PI, 0f)
			.Flat (20f)
			.Downslope (50f, Mathf.PI, Mathf.PI / 2f)
			.Flat (40f)
			.Horse()

			.Incline (40f, 0f, -Mathf.PI / 8f)
			.OrbitY (80f, -21f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, -Vector3.forward, false)
			.OrbitY (80f, -21f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right, false)
			.OrbitY (80f, -21f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, Vector3.forward, false)
			.OrbitY (80f, -21f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right, false)
			.OrbitY (80f, -21f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, -Vector3.forward, false)
			.OrbitY (80f, -21f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right, false)
			.Flat (50f)
			.OrbitY (80f, 0f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, Vector3.forward, false)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.Flat (10F)
			.Close ()
			.Agari();
		return floors;
	}
}

// ChoKaiten
class Stage3FloorsFactory: FloorListFactory {
	public override FloorList CreateFloorList (CourseFactory cf) {
		FloorList floors = new FloorList (new FlatFloor (Vector3.up, Vector3.zero, Vector3.forward, 60f, 20f), cf)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (120f, 0f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Accelerator ()
			.SlopeTop (20f, Mathf.PI / 2f, 0f)
			.Downslope (100f, Mathf.PI, Mathf.PI / 2f)
			.Accelerator ()
			.Upslope (100f, Mathf.PI / 2f, 0f)
			.SlopeTop (20f, Mathf.PI, Mathf.PI / 2f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 8f)

			.Upslope (50f, Mathf.PI / 2f, 0f)
			.Flat (20f)
			.SuperAccelerator ()
			.SlopeTop (25f, Mathf.PI, 0f)
			.Flat (20f)
			.Downslope (50f, Mathf.PI, Mathf.PI / 2f)

			.Incline (20f, 0f, Mathf.PI / 8f)
			.Horse ()
			.OrbitY (80f, 0f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.Horse ()
			.Incline (40f, 0f, -Mathf.PI / 4f)
			.Horse ()
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.forward, false)
			.Horse ()
			.Incline (40f, 0f, Mathf.PI / 4f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI * 3f / 2f, Mathf.PI / 8f, Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 4f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, -Vector3.forward, false)
			.Incline (20f, 0f, Mathf.PI / 8f)

			.Upslope (50f, Mathf.PI / 2f, 0f)
			.Flat (20f)
			.SlopeTop (25f, Mathf.PI, 0f)
			.LandingSite ()
			.Flat (20f)
			.Downslope (50f, Mathf.PI, Mathf.PI / 2f)

			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (120f, 0f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Flat (40f)
			.Accelerator ()
			.Flat (200f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (120f, 0f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, -Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Flat (10F)
			.Accelerator ()
			.Incline (160f, 0f, Mathf.PI)
			.Incline (160f, 0f, Mathf.PI)
			.Flat (40f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (100f, 0f, 0f, Mathf.PI / 2f, Mathf.PI / 8, Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 4f)
			.OrbitY (100f, 0f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8, Vector3.forward, false)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.Close ()
			.Agari();
		return floors;
	}
}

// Matrix
class Stage4FloorsFactory: FloorListFactory {
	public override FloorList CreateFloorList (CourseFactory cf) {
		FloorList floors = new FloorList (new FlatFloor (Vector3.up, Vector3.zero, Vector3.forward, 40f, 20f), cf)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.Flat (10f)
			.OrbitY (80f, 0f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right)
			.Flat(10f)
			.Accelerator ()
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Flat (165f)
			.Upslope (50f, Mathf.PI / 2f, 0f)
			.Flat (20f)
			.SlopeTop (25f, Mathf.PI, 0f)
			.Flat (20f)
			.Horse()
			.Downslope (50f, Mathf.PI, Mathf.PI / 2f)
			.Flat (10f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.Flat (10f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, Vector3.forward)
			.OrbitY (80f, 0f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.Incline (20f, 0f, -Mathf.PI / 4f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, -Vector3.forward, false)
			.Incline (20f, 0f, Mathf.PI / 8f)
			.Flat (10f)
			.SlopeTop (20f, Mathf.PI / 2f, 0f)
			.Downslope (100f, Mathf.PI, Mathf.PI / 2f)
			.Flat (20f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.Flat (10f)
			.OrbitY (80f, 0f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Flat (20f)
			.Accelerator ()
			.Upslope (100f, Mathf.PI / 2f, 0f)
			.SlopeTop (20f, Mathf.PI, Mathf.PI / 2f)
			.Flat(20f)
			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, -Vector3.forward)
			.Incline (40f, 0f, - Mathf.PI / 8f)
			.Accelerator()
			.Close()
			.Agari();
		return floors;
	}
}

// SushiParadice
class Stage5FloorsFactory: FloorListFactory {
	public override FloorList CreateFloorList (CourseFactory cf) {
		FloorList floors = new FloorList (new FlatFloor (Vector3.up, Vector3.zero, Vector3.forward, 10f, 20f), cf)
			.Flat(110f)

			.Upslope (50f, Mathf.PI / 2f, 0f)
			.Flat (20f)

			.Incline (80f, 0f, Mathf.PI)
			.Incline (80f, 0f, Mathf.PI)
			.Flat (100f)

			.SuperAccelerator()
			.SlopeTop (25f, Mathf.PI, 0f)
			.Flat (120f)
			.Downslope (50f, Mathf.PI, Mathf.PI / 2f)

			.Incline (50f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 21f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right)
			.OrbitY (80f, 21f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, Vector3.forward)
			.OrbitY (80f, 21f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.OrbitY (80f, 21f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, -Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 8f)

			.Upslope (50f, Mathf.PI / 2f, 0)
			.Flat(30f)

			.SlopeTop (25f, Mathf.PI, Mathf.PI / 2f)
			.SlopeTop (25f, Mathf.PI / 2f, 0f)
			.LandingSite()
			.Flat(30f)

			.Downslope (50f, Mathf.PI, Mathf.PI / 2f)

			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, 0f, Mathf.PI / 2f, Mathf.PI / 8f, Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 8f)

			.SlopeTop (100f, Mathf.PI / 2f, 0f)
			.Flat(115f)
			.Downslope (30f, Mathf.PI, Mathf.PI / 2f)

			.Incline (40f, 0f, Mathf.PI / 8f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, Vector3.forward)

			.Flat(60f)
			.OrbitY (80f, 0f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.Incline (40f, 0f, -Mathf.PI / 4f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.forward, false)
			.Incline (40f, 0f, Mathf.PI / 4f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI * 3f / 2f, Mathf.PI / 8f, Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 4f)
			.OrbitY (80f, 0f, Mathf.PI / 2f, Mathf.PI, Mathf.PI / 8f, -Vector3.forward, false)
			.Incline (20f, 0f, Mathf.PI / 8f)

			.Incline (60f, 0f, Mathf.PI / 8f)
			.OrbitY (85f, 0f, Mathf.PI, Mathf.PI * 3f / 2f, Mathf.PI / 8f, -Vector3.right)
			.OrbitY (85f, 0f, Mathf.PI * 3f / 2f, Mathf.PI * 2f, Mathf.PI / 8f, -Vector3.forward)
			.Incline (40f, 0f, -Mathf.PI / 8f)
			.Close ()
			.Agari();
		return floors;
	}
}