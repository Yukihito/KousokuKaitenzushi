using UnityEngine;
using System.Collections;

public class CheckPoint : MonoBehaviour {
	public int CheckPointOrder = -1;
	public bool IsRecoveryPoint = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	string toString() {
		return "" + CheckPointOrder;
	}
}
