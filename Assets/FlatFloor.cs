using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class FlatFloor: Floor {
	public Vector3 Normal;
	public Vector3 Position;
	public Vector3 Right;
	public float Depth;

	public FlatFloor(
		Vector3 normal,
		Vector3 position,
		Vector3 forward,
		float depth,
		float width) {
		Depth = depth;
		Normal = normal;
		Position = position;
		Forward = forward;
		Width = width;
		Right = Vector3.Cross (normal, forward).normalized;
		RightForward = position + forward * (depth / 2f) + Right * (width / 2f);
		LeftForward = position + forward * (depth / 2f) - Right * (width / 2f);
		LeftBack = position - forward * (depth / 2f) - Right * (width / 2f);
		RightBack = position - forward * (depth / 2f) + Right * (width / 2f);
		Mesh = createMesh ();
	}

	public FlatFloor(
		Vector3 rightForward,
		Vector3 leftForward,
		Vector3 leftBack,
		Vector3 rightBack) {
		Depth = ((rightForward + leftForward) / 2f - (rightBack + leftBack) / 2f).magnitude;
		Normal = Vector3.Cross(leftForward - rightForward, rightBack - rightForward).normalized;
		Position = (rightForward + leftForward + leftBack + rightBack) / 4f;
		Forward = Vector3.Lerp(rightForward, leftForward, 0.5f) - Vector3.Lerp(rightBack, leftBack, 0.5f);
		Width = (leftForward - rightForward).magnitude;
		Right = Vector3.Cross (Normal, Forward).normalized;
		RightForward = rightForward;
		LeftForward = leftForward;
		LeftBack = leftBack;
		RightBack = rightBack;
		Mesh = createMesh ();
		SetupNormal ();
	}

	Mesh createMesh() {
		int partitionCount = (int)(Depth / Width);
		Mesh mesh = new Mesh ();
		Vector3[] vertices = new Vector3[4 * (partitionCount + 1)];// {RightForward, LeftForward, LeftBack, RightBack};
		Vector2 [] uv = new Vector2[vertices.Length];
		int[] triangles = new int[2 * 3 * (partitionCount + 1)];

		Vector3 LeftBack2 = LeftForward + (LeftBack - LeftForward).normalized * Width * partitionCount;
		Vector3 RightBack2 = RightForward + (RightBack - RightForward).normalized * Width * partitionCount;

		for (int i = 0; i < partitionCount; i++) {
			float backLerpFactor = (float)(i + 1) / (float)partitionCount;
			float forwardLerpFactor = (float)i / (float)partitionCount;
			Vector3 rf = Vector3.Lerp (RightForward, RightBack2, forwardLerpFactor);
			Vector3 lf = Vector3.Lerp (LeftForward, LeftBack2, forwardLerpFactor);
			Vector3 lb = Vector3.Lerp (LeftForward, LeftBack2, backLerpFactor);
			Vector3 rb = Vector3.Lerp (RightForward, RightBack2, backLerpFactor);

			int vi = i * 4;

			vertices [vi + 0] = rf;
			vertices [vi + 1] = lf;
			vertices [vi + 2] = lb;
			vertices [vi + 3] = rb;

			uv [vi + 0] = new Vector2 (1.0f, 1.0f);
			uv [vi + 1] = new Vector2 (0.0f, 1.0f);
			uv [vi + 2] = new Vector2 (0.0f, 0.0f);
			uv [vi + 3] = new Vector2 (1.0f, 0.0f);

			int ti = i * 6;

			triangles[ti + 0] = vi + 2;
			triangles[ti + 1] = vi + 1;
			triangles[ti + 2] = vi + 0;
			triangles[ti + 3] = vi + 0;
			triangles[ti + 4] = vi + 3;
			triangles[ti + 5] = vi + 2;
		}

		int pc4times = partitionCount * 4;

		float modLength = (Depth % Width) / Width;

		vertices [pc4times + 0] = RightBack2;
		vertices [pc4times + 1] = LeftBack2;
		vertices [pc4times + 2] = LeftBack;
		vertices [pc4times + 3] = RightBack;

		uv [pc4times + 0] = new Vector2 (1.0f, 1.0f * modLength);
		uv [pc4times + 1] = new Vector2 (0.0f, 1.0f * modLength);
		uv [pc4times + 2] = new Vector2 (0.0f, 0.0f);
		uv [pc4times + 3] = new Vector2 (1.0f, 0.0f);

		int pc6times = partitionCount * 6;

		triangles[pc6times + 0] = pc4times + 2;
		triangles[pc6times + 1] = pc4times + 1;
		triangles[pc6times + 2] = pc4times + 0;
		triangles[pc6times + 3] = pc4times + 0;
		triangles[pc6times + 4] = pc4times + 3;
		triangles[pc6times + 5] = pc4times + 2;

		mesh.vertices  = vertices;
		mesh.uv        = uv;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		return mesh;
	}

	static Intersection getIntersection(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2) {
		Vector3 intersectPosition = GetIntersectPosition (pos, v0, v1, v2);
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
			Vector3 normal = Vector3.Cross (v2 - v0, v1 - v0).normalized;
			return Intersection.Intersected (intersectPosition, normal);
		}
		return Intersection.NotIntersected();
	}

	public static Vector3 GetIntersectPosition(Vector3 pos, Vector3 v0, Vector3 v1, Vector3 v2) {
		Vector3 normal = Vector3.Cross (v1 - v0, v2 - v0).normalized;
		Vector3 rayDir = -normal;
		float d = (v0.x * normal.x + v0.y * normal.y + v0.z * normal.z);
		float t = (d - Vector3.Dot (pos, normal)) / Vector3.Dot(rayDir, normal);
		return pos + rayDir * t;
	}

	public static Vector3 GetIntersectPosition(Vector3 pos, Vector3 normal, Vector3 p) {
		Vector3 rayDir = -normal;
		float d = (p.x * normal.x + p.y * normal.y + p.z * normal.z);
		float t = (d - Vector3.Dot (pos, normal)) / Vector3.Dot(rayDir, normal);
		return pos + rayDir * t;
	}

	public override Intersection GetIntersection(Vector3 pos) {
		Intersection triangleIntersection = getIntersection (pos, RightForward, LeftForward, LeftBack);
		if (triangleIntersection.IsIntersected) {
			return triangleIntersection;
		}
		return getIntersection (pos, LeftBack, RightBack, RightForward);
	}

	public override Vector3 GetTangentDir(Vector3 pos) {
		Vector3 forwardPos = (RightForward + LeftForward) / 2f;
		Vector3 backPos = (RightBack + LeftBack) / 2f;
		return (forwardPos - backPos).normalized;
	}

	public override Vector3 GetMiddle (Vector3 pos) {
		Vector3 forwardPos = (RightForward + LeftForward) / 2f;
		Vector3 backPos = (RightBack + LeftBack) / 2f;
		Vector3 tangentDir = (forwardPos - backPos).normalized;
		Vector3 backToPos = pos - backPos;
		float shadowLength = Vector3.Dot (tangentDir, backToPos);
		return backPos + tangentDir * shadowLength;
	}

	public override float GetPercentage (Vector3 pos) {
		Vector3 forwardPos = (RightForward + LeftForward) / 2f;
		Vector3 backPos = (RightBack + LeftBack) / 2f;
		Vector3 tangentDir = (forwardPos - backPos).normalized;
		Vector3 backToPos = pos - backPos;
		float shadowLength = Vector3.Dot (tangentDir, backToPos);
		return shadowLength / (forwardPos - backPos).magnitude;
	}
}
