using UnityEngine;
using System.Collections;

public class SwitchableSushi : MonoBehaviour {
	SushiRenderer[] sushiRenderers;
	bool isInitialized = false;

	public void SwitchSushi(SushiType type) {
		foreach (SushiRenderer sushiRenderer in sushiRenderers) {
			sushiRenderer.enabled = sushiRenderer.type == type;
		}
	}

	public MeshRenderer findRendererByName(string name) {
		MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer> (true);
		foreach (MeshRenderer renderer in renderers) {
			if (renderer.name == name) {
				return renderer;
			}
		}
		return null;
		// return GameObject.Find (name).GetComponent<MeshRenderer> ();
	}

	public void InitializeRenderer() {
		if (!isInitialized) {
			sushiRenderers = new SushiRenderer[] {
				new SushiRenderer (SushiType.Amaebi, new string[]{ "amaebi_02", "amaebi_03" }, this),
				new SushiRenderer (SushiType.Ebi, new string[]{ "ebi_02", "ebi_03" }, this),
				new SushiRenderer (SushiType.Hamachi, new string[]{ "hamachi_02", "hamachi_03" }, this),
				new SushiRenderer (SushiType.Hokki, new string[]{ "hokki_02", "hokki_03" }, this),
				new SushiRenderer (SushiType.Ika, new string[]{ "ika_02", "ika_03" }, this),
				new SushiRenderer (SushiType.Ikura, new string[]{ "ikura_02" }, this),
				new SushiRenderer (SushiType.Kohada, new string[]{ "kohada_02", "kohada_03" }, this),
				new SushiRenderer (SushiType.Toro, new string[]{ "maguro_toro_02", "maguro_toro_03" }, this),
				new SushiRenderer (SushiType.Maguro, new string[]{ "maguro_tuna_02", "maguro_tuna_03" }, this),
				new SushiRenderer (SushiType.Sulmon, new string[]{ "sulmon_02", "sulmon_03" }, this),
				new SushiRenderer (SushiType.Tako, new string[]{ "tako_02", "tako_03", "tako_04" }, this),
				new SushiRenderer (SushiType.Tamago, new string[]{ "tamago_02", "tamago_03", "tamago_04" }, this),
				new SushiRenderer (SushiType.Uni, new string[]{ "uni" }, this)
			};
			isInitialized = true;
		}
	}

	void Start () {
		InitializeRenderer ();
	}

	void Update () {
	
	}
}

public enum SushiType {
	Amaebi = 0,
	Ebi,
	Hamachi,
	Hokki,
	Ika,
	Ikura,
	Kohada,
	Toro,
	Maguro,
	Sulmon,
	Tako,
	Tamago,
	Uni
}

class SushiRenderer {
	MeshRenderer[] renderers;
	public SushiType type;
	public SushiRenderer(SushiType type, string[] names, SwitchableSushi parent) {
		this.type = type;
		this.renderers = new MeshRenderer[names.Length];
		for (int i = 0; i < names.Length; i++) {
			this.renderers[i] = parent.findRendererByName (names[i]);
		}
	}

	public bool enabled {
		set {
			foreach (MeshRenderer renderer in renderers) {
				renderer.enabled = value;
			}
		}

		get {
			return renderers [0].enabled;
		}
	}
}