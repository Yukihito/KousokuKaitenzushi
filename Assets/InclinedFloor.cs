using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class InclinedFloor: Floor {
	public Vector3 StartPosition;
	public Vector3 EndPosition;
	public float Depth;
	public Vector3 Right;
	public float StartInclination;
	public float EndInclination;

	public InclinedFloor(
		Vector3 position,
		Vector3 forward,
		Vector3 right,
		float depth,
		float width,
		int partitionCount,
		float startInclination,
		float endInclination) {
		StartInclination = startInclination;
		EndInclination = endInclination;
		StartPosition = position;
		EndPosition = position + forward * depth;
		Forward = forward;
		Depth = depth;
		Right = right;
		Width = width;
		float partitionDepth = Depth / (float)partitionCount;
		int partitionCountInOneRect = (int)(Width / partitionDepth);
		Vector3 [] vertices = new Vector3[4 * partitionCount];
		Vector2 [] uv = new Vector2[vertices.Length];
		int[] triangles = new int[2 * 3 * partitionCount];
		for (int i = 0; i < partitionCount; i++) {
			float backFactor = (float)i / (float)partitionCount;
			float forwardFactor = (float)(i + 1) / (float)partitionCount;

			Quaternion backRotation = Quaternion.AngleAxis ((startInclination * (1f - backFactor) + endInclination * backFactor) * 180f / Mathf.PI, forward);
			Quaternion forwardRotation = Quaternion.AngleAxis ((startInclination * (1f - forwardFactor) + endInclination * forwardFactor) * 180f / Mathf.PI, forward);

			Vector3 rightForward = position + forward * forwardFactor * depth + forwardRotation * (right * (width / 2f));
			Vector3 leftForward = position + forward * forwardFactor * depth + forwardRotation * (-right * (width / 2f));
			Vector3 leftBack = position + forward * backFactor * depth + backRotation * (-right * (width / 2f));
			Vector3 rightBack = position + forward * backFactor * depth + backRotation * (right * (width / 2f));

			int vertexIndices = 4 * i;
			vertices [vertexIndices + 0] = rightForward;
			vertices [vertexIndices + 1] = leftForward;
			vertices [vertexIndices + 2] = leftBack;
			vertices [vertexIndices + 3] = rightBack;

			float partitionStartV = (float)(i % partitionCountInOneRect) / (float)partitionCountInOneRect;
			float partitionEndV = (float)((i + 1) % partitionCountInOneRect) / (float)partitionCountInOneRect;

		
			uv [vertexIndices + 0] = new Vector2 (1.0f, partitionEndV);
			uv [vertexIndices + 1] = new Vector2 (0.0f, partitionEndV);
			uv [vertexIndices + 2] = new Vector2 (0.0f, partitionStartV);
			uv [vertexIndices + 3] = new Vector2 (1.0f, partitionStartV);

			int triangleIndices = 6 * i;
			triangles [triangleIndices + 0] = vertexIndices + 2;
			triangles [triangleIndices + 1] = vertexIndices + 1;
			triangles [triangleIndices + 2] = vertexIndices + 0;
			triangles [triangleIndices + 3] = vertexIndices + 0;
			triangles [triangleIndices + 4] = vertexIndices + 3;
			triangles [triangleIndices + 5] = vertexIndices + 2;

			if (i == partitionCount - 1) {
				RightForward = rightForward;
				LeftForward = leftForward;
			}
		}
		Mesh mesh = new Mesh ();
		mesh.vertices  = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		Mesh = mesh;
		SetupNormal ();
	}

	public override Intersection GetIntersection (Vector3 pos) {
		Vector3 localPos = pos - StartPosition;
		float depthFactor = Vector3.Dot (localPos, Forward) / Depth;
		if (depthFactor < 0f || depthFactor > 1f) {
			return Intersection.NotIntersected();
		}
		Quaternion rotation = Quaternion.AngleAxis ((StartInclination * (1f - depthFactor) + EndInclination * depthFactor) * 180f / Mathf.PI, Forward);
		Vector3 posOnLine = StartPosition + Forward * depthFactor * Depth;
		Vector3 lineToPos = pos - posOnLine;
		Vector3 rotatedRight = rotation * Right;
		float rightShadowFactor = Vector3.Dot (rotatedRight, lineToPos);
		if (Mathf.Abs(rightShadowFactor) <= (Width / 2f)) {
			Vector3 intersectPosition = posOnLine + rotatedRight * rightShadowFactor;
			Vector3 normal = Vector3.Cross (Forward, rotatedRight);
			return Intersection.Intersected (intersectPosition, normal);
		}
		return Intersection.NotIntersected();
	}

	public override Vector3 GetTangentDir(Vector3 pos) {
		return Forward;
	}

	public override Vector3 GetMiddle (Vector3 pos) {
		float percentage = GetPercentage (pos);
		return StartPosition + Forward * percentage * Depth;
	}

	public override float GetPercentage (Vector3 pos) {
		Vector3 localPos = pos - StartPosition;
		float percentage = Vector3.Dot (localPos, Forward) / Depth;
		return percentage;
	}
}
