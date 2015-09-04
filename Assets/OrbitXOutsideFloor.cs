using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class OrbitXOutsideFloor: Floor {
	public Vector3 Center;
	public Vector3 Axis;
	public Quaternion Rotation;
	public float Radius;
	public float StartTheta;
	public float EndTheta;

	public OrbitXOutsideFloor(
		Vector3 center,
		Vector3 axis,
		Vector3 up,
		float width,
		float radius,
		int partitionCount,
		float startTheta,
		float endTheta
	) {
		Center = center;
		Axis = axis;
		Width = width;
		Radius = radius;
		StartTheta = startTheta;
		EndTheta = endTheta;

		Vector3 [] vertices = new Vector3[4 * partitionCount];
		Vector2 [] uv = new Vector2[vertices.Length];
		int[] triangles = new int[2 * 3 * partitionCount];

		int partitionCountInOneRect = (int)(Width / ((Mathf.PI * Radius * 2f) * (((endTheta - startTheta)/partitionCount) / (Mathf.PI * 2f))));

		float diffTheta = (endTheta - startTheta) / (float)partitionCount;
		Vector3 forward = Vector3.Cross (axis, up);
		Quaternion rotation = Quaternion.LookRotation (forward, up);
		Rotation = rotation;

		for (int i = 0; i < partitionCount;i++) {
			float backTheta = (float)i * diffTheta + startTheta;
			float forwardTheta = (float)(i + 1) * diffTheta + startTheta;
			Vector3 localForward = new Vector3 (0f, Mathf.Sin (forwardTheta) * radius, Mathf.Cos (forwardTheta) * radius);
			Vector3 localBack = new Vector3 (0f, Mathf.Sin (backTheta) * radius, Mathf.Cos (backTheta) * radius);
			Vector3 localRightForward = localForward + Vector3.right * (width / 2f);
			Vector3 localLeftForward = localForward - Vector3.right * (width / 2f);
			Vector3 localLeftBack = localBack - Vector3.right * (width / 2f);
			Vector3 localRightBack = localBack + Vector3.right * (width / 2f);

			Vector3 rightForward = rotation * localRightForward + center;
			Vector3 leftForward = rotation * localLeftForward + center;
			Vector3 leftBack = rotation * localLeftBack + center;
			Vector3 rightBack = rotation * localRightBack + center;

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
			triangles[triangleIndices + 0] = vertexIndices + 2;
			triangles[triangleIndices + 1] = vertexIndices + 1;
			triangles[triangleIndices + 2] = vertexIndices + 0;
			triangles[triangleIndices + 3] = vertexIndices + 0;
			triangles[triangleIndices + 4] = vertexIndices + 3;
			triangles[triangleIndices + 5] = vertexIndices + 2;

			if (i == partitionCount - 1) {
				RightForward = rightForward;
				LeftForward = leftForward;
			}
		}

		Vector3 endVec = rotation * (new Vector3 (0f, Mathf.Sin (endTheta), Mathf.Cos (endTheta))).normalized;
		Forward = -Vector3.Cross (endVec, axis.normalized).normalized;

		Mesh mesh = new Mesh ();
		mesh.vertices  = vertices;
		mesh.uv        = uv;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		Mesh = mesh;
		SetupNormal ();
	}

	public override Intersection GetIntersection(Vector3 pos) {
		Vector3 localPos = Quaternion.Inverse(Rotation) * (pos - Center);
		float shadowLengthOnAxis = Vector3.Dot (Vector3.right, localPos);
		if (Mathf.Abs (shadowLengthOnAxis) <= Width / 2f) {
			Vector3 localVec = localPos - Vector3.right * shadowLengthOnAxis;
			Vector3 localVecNomal = localVec.normalized;
			float localVecTheta = Mathf.Atan2 (localVecNomal.y, localVecNomal.z);
			/*
			if (localVecTheta < StartTheta || localVecTheta > EndTheta) {
				return Intersection.NotIntersected ();
			}*/
			if (StartTheta - EndTheta > 0) {
				if (localVecTheta > StartTheta || localVecTheta < EndTheta) {
					return Intersection.NotIntersected ();
				}
			} else {
				if (localVecTheta < StartTheta || localVecTheta > EndTheta) {
					return Intersection.NotIntersected ();
				}
			}
			Vector3 localIntersectPosition = localVecNomal * Radius + Vector3.right * shadowLengthOnAxis;
			Vector3 nomal = Rotation * localVecNomal;
			Vector3 intersectPosition = Rotation * localIntersectPosition + Center;
			return Intersection.Intersected (intersectPosition, nomal);
		}
		return Intersection.NotIntersected ();
	}

	public override Vector3 GetTangentDir(Vector3 pos) {
		Vector3 localPos = Quaternion.Inverse(Rotation) * (pos - Center);
		float shadowLengthOnAxis = Vector3.Dot (Vector3.right, localPos);
		Vector3 localVec = localPos - Vector3.right * shadowLengthOnAxis;
		Vector3 localVecNomal = localVec.normalized;
		return (Rotation * (Vector3.Cross(Vector3.right, localVecNomal))).normalized;
	}

	public override Vector3 GetMiddle (Vector3 pos) {
		Vector3 localPos = Quaternion.Inverse(Rotation) * (pos - Center);
		localPos.x = 0f;
		localPos.Normalize ();
		return Rotation * (localPos * Radius) + Center;
	}

	public override float GetPercentage (Vector3 pos) {
		Vector3 localPos = Quaternion.Inverse(Rotation) * (pos - Center);
		float shadowLengthOnAxis = Vector3.Dot (Vector3.right, localPos);
		Vector3 localVec = localPos - Vector3.right * shadowLengthOnAxis;
		Vector3 localVecNomal = localVec.normalized;
		float localVecTheta = Mathf.Atan2 (localVecNomal.y, localVecNomal.z);
		return (localVecTheta - StartTheta) / (EndTheta - StartTheta);
	}
}
