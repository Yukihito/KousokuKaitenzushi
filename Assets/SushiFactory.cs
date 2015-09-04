using UnityEngine;
using System.Collections;

public class SushiFactory : MonoBehaviour {
	public GameObject[] Sushis;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public WrapedSushi Create(int id, Vector3 position, Quaternion rotation, bool isUseAlpha = true) {
		return new WrapedSushi(Instantiate(Sushis[id], position, rotation) as GameObject, isUseAlpha);
	}
}

public class WrapedSushi {
	GameObject underlying;
	MeshRenderer[] renderers;
	Material material;
	public WrapedSushi(GameObject underlying, bool isUseAlpha) {
		this.underlying = underlying;
		renderers = underlying.GetComponentsInChildren<MeshRenderer> ();
		if (isUseAlpha) {
			material = new Material (renderers [0].material);
			material.shader = Shader.Find ("Particles/Alpha Blended");
			material.SetTexture ("_MainTex", renderers [0].material.GetTexture ("_MainTex"));
			foreach (MeshRenderer renderer in renderers) {
				renderer.material = material;
			}
		}
	}
	public GameObject Underlying {
		get {
			return underlying;
		}
	}
	public float Alpha {
		get { 
			return material.GetColor("_TintColor").a;
		}
		set {
			Color c = material.GetColor ("_TintColor");
			material.SetColor("_TintColor", new Color (c.r, c.g, c.b, value));
			material.SetFloat ("_InvFade", 2f);
			foreach (MeshRenderer renderer in renderers)  {
				renderer.material = material;
			}
		}
	}
	public bool IsEnabled {
		get { 
			return renderers [0].enabled;
		}
		set { 
			foreach (MeshRenderer renderer in renderers)  {
				renderer.enabled = value;
			}
		}
	}
}
