using UnityEngine;
using System.Collections;

public class SushiMatrix : MonoBehaviour {
	public GameObject[] Sushis;
	GameObject[] elements;
	public int Size;
	float dist = 150f;
	float xSpeed;
	float ySpeed;
	float zSpeed;
	float xRotation;
	float yRotation;
	float zRotation;


	// Use this for initialization
	void Start () {
		xSpeed = Random.Range (-0.1f, 0.1f);
		ySpeed = Random.Range (-0.1f, 0.1f);
		zSpeed = Random.Range (-0.1f, 0.1f);
		for (int i = 0; i < Size; i++) {
			for (int j = 0; j < Size; j++) {
				for (int k = 0; k < Size; k++) {
					int sushiKind = Random.Range (0, 12);
					GameObject go = Instantiate (
						Sushis [sushiKind],
						new Vector3 ((i - (Size / 2)) * dist, (j - (Size / 2)) * dist, (k - (Size / 2)) * dist),
						Quaternion.identity) as GameObject;
					go.transform.localScale = new Vector3(100f, 100f, 100f);
					go.transform.parent = transform;
					go.transform.position += transform.position;
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		xRotation += xSpeed;
		yRotation += ySpeed;
		zRotation += zSpeed;
		xRotation %= 360f;
		yRotation %= 360f;
		zRotation %= 360f;
		transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
	}
}
