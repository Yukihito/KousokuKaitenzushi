using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class OrbitYFloor: Floor {
	public Vector3 Center;
	public float StartToEndHeight;
	public float Radius;
	public float StartTheta;
	public float EndTheta;
	public float Inclination;
	public bool IsLeftCurve;

	public OrbitYFloor(
		Vector3 center,
		float width,
		float height,
		float radius,
		float inclination,
		int partitionCount,
		float startTheta,
		float endTheta,
		bool isLeftCurve) {
		Center = center;
		Width = width;
		StartToEndHeight = height;
		Radius = radius;
		StartTheta = startTheta;
		EndTheta = endTheta;
		Inclination = inclination;
		IsLeftCurve = isLeftCurve;

		Vector3 [] vertices = new Vector3[4 * partitionCount];
		Vector2 [] uv = new Vector2[vertices.Length];
		int[] triangles = new int[2 * 3 * partitionCount];

		float diffTheta = (endTheta - startTheta) / (float)partitionCount;
		float diffHeight = height / (float)partitionCount;

		float innerLength = Radius - (Width / 2f) * Mathf.Abs(Mathf.Cos (inclination));
		float outerLength = Radius + (Width / 2f) * Mathf.Abs(Mathf.Cos (inclination));

		int partitionCountInOneRect = (int)(Width / ((Mathf.PI * Radius * 2f) * (((endTheta - startTheta)/partitionCount) / (Mathf.PI * 2f))));

		for (int i = 0; i < partitionCount; i++) {
			float backTheta = (float)i * diffTheta + startTheta;
			float forwardTheta = (float)(i + 1) * diffTheta + startTheta;

			Vector3 backVec = new Vector3 (Mathf.Cos (backTheta), 0f, Mathf.Sin (backTheta));
			Vector3 forwardVec = new Vector3 (Mathf.Cos (forwardTheta), 0f, Mathf.Sin (forwardTheta));

			if (!IsLeftCurve) {
				forwardVec.x = -forwardVec.x;
				backVec.x = -backVec.x;
			}

			Vector3 rightForward = forwardVec * outerLength + center + Vector3.up * diffHeight * (float)(i + 1) + Vector3.up * Mathf.Sin(inclination) * (Width / 2f);
			Vector3 leftForward = forwardVec * innerLength + center + Vector3.up * diffHeight * (float)(i + 1) + Vector3.down * Mathf.Sin(inclination) * (Width / 2f);
			Vector3 leftBack = backVec * innerLength + center + Vector3.up * diffHeight * (float)i + Vector3.down * Mathf.Sin(inclination) * (Width / 2f);
			Vector3 rightBack = backVec * outerLength + center + Vector3.up * diffHeight * (float)i + Vector3.up * Mathf.Sin(inclination) * (Width / 2f);

			if (!isLeftCurve) {
				Vector3 tmp = rightForward;
				rightForward = leftForward;
				leftForward = tmp;
				tmp = rightBack;
				rightBack = leftBack;
				leftBack = tmp;
			}

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

			RightForward = rightForward;
			LeftForward = leftForward;
		}

		Vector3 endVec = new Vector3 (Mathf.Cos (endTheta), 0f, Mathf.Sin (endTheta));
		if (!IsLeftCurve) {
			endVec.x = -endVec.x;
		}
		Forward = Vector3.Cross (endVec, Vector3.up).normalized;
		if (!IsLeftCurve) {
			Forward = -Forward;
		}


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
		float innerLength = Radius - (Width / 2f) * Mathf.Abs(Mathf.Cos (Inclination));
		float outerLength = Radius + (Width / 2f) * Mathf.Abs(Mathf.Cos (Inclination));
		Vector3 fixedCenter = Center;
		fixedCenter.y = pos.y;
		float distance = Vector3.Distance (pos, fixedCenter);
		if (innerLength <= distance && distance <= outerLength) {
			Vector3 dir = (pos - Center).normalized;
			float theta;
			if (IsLeftCurve) {
				theta = Mathf.Atan2 (dir.z, dir.x);
			} else {
				theta = Mathf.Atan2 (dir.z, -dir.x);
			}
			if (theta < 0f) {
				theta += Mathf.PI * 2f;
			}
			float heightFactor = (theta - StartTheta) / (EndTheta - StartTheta);
			if (heightFactor < 0f || heightFactor > 1f) {
				return Intersection.NotIntersected ();
			}
			float y = StartToEndHeight * heightFactor;
			float yInclination = Mathf.Sin (Inclination) * (((distance - innerLength) / (outerLength - innerLength)) - 0.5f) * Width;
			Vector3 intersectPosition = pos + Vector3.up * yInclination;
			intersectPosition.y = Center.y + y + yInclination;

			float thetaPlus = theta + Mathf.PI / 180f;
			float yPlus = StartToEndHeight * ((thetaPlus - StartTheta) / (EndTheta - StartTheta));

			float localNormalX = Mathf.Cos (theta);
			if (!IsLeftCurve) {
				localNormalX = -localNormalX;
			}

			float localPosPlusX = Mathf.Cos (thetaPlus);
			if (!IsLeftCurve) {
				localPosPlusX = -localPosPlusX;
			}

			Vector3 rightPos;
			if (IsLeftCurve) {
				rightPos = new Vector3 (localNormalX, 0f, Mathf.Sin (theta)) * outerLength + Center
				+ Vector3.up * y + Vector3.up * Mathf.Sin (Inclination) * (Width / 2f);
			} else {
				rightPos = new Vector3 (localNormalX, 0f, Mathf.Sin (theta)) * innerLength + Center
					+ Vector3.up * y + Vector3.down * Mathf.Sin (Inclination) * (Width / 2f);
			}

			Vector3 posPlus = new Vector3 (localPosPlusX, 0f, Mathf.Sin (thetaPlus)) * distance + Vector3.up * (yPlus + yInclination) + Center;
			Vector3 normal = Vector3.Cross((posPlus - intersectPosition).normalized, (rightPos - intersectPosition).normalized);
			return Intersection.Intersected (intersectPosition, normal);
		}
		return Intersection.NotIntersected ();
	}

	public override Vector3 GetTangentDir(Vector3 pos) {
		Vector3 dir = (pos - Center).normalized;
		dir.y = 0;
		dir.Normalize ();
		if (IsLeftCurve) {
			dir = -dir;
		}
		return Vector3.Cross (Vector3.up, dir).normalized;
	}

	public override Vector3 GetMiddle (Vector3 pos) {
		float percentage = GetPercentage (pos);
		if (percentage < 0) {
			Debug.LogError ("aaaaaaaaaaa");
		}
		float y = StartToEndHeight * percentage;
		Vector3 dir = (pos - Center).normalized;
		float theta = Mathf.Atan2 (dir.z, dir.x);
		return new Vector3 (Mathf.Cos (theta) * Radius, y, Mathf.Sin (theta) * Radius) + Center;
	}

	public override float GetPercentage (Vector3 pos) {
		Vector3 dir = (pos - Center).normalized;
		float theta;
		if (IsLeftCurve) {
			theta = Mathf.Atan2 (dir.z, dir.x);
		} else {
			theta = Mathf.Atan2 (dir.z, -dir.x);
		}

		if (theta < 0f) {
			theta += Mathf.PI * 2f;//yabasou
		}
		float percentage = (theta - StartTheta) / (EndTheta - StartTheta);
		return percentage;
	}
}
