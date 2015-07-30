using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KeyConfig {
	static KeyConfig current;

	public static KeyConfig Current {
		get { 
			if (current == null) {
				current = new KeyConfig ();
				current.Initialize ();
				return current;
			} else {
				return current;
			}
		}

		set {
			current = value;
		}
	}

	Dictionary<ConfigurableKeyType, ConfigurableKey> keyTable;

	public void Initialize() {
		keyTable = new Dictionary<ConfigurableKeyType, ConfigurableKey>();
		keyTable.Add(ConfigurableKeyType.Right, new ConfigurableKey(new KeyCode[] { KeyCode.D, KeyCode.RightArrow }));
		keyTable.Add(ConfigurableKeyType.Left, new ConfigurableKey(new KeyCode[] { KeyCode.A, KeyCode.LeftArrow }));
		keyTable.Add(ConfigurableKeyType.Up, new ConfigurableKey(new KeyCode[] { KeyCode.W, KeyCode.UpArrow }));
		keyTable.Add(ConfigurableKeyType.Down, new ConfigurableKey(new KeyCode[] { KeyCode.S, KeyCode.DownArrow }));
		keyTable.Add(ConfigurableKeyType.Enter, new ConfigurableKey(new KeyCode[] { KeyCode.KeypadEnter, KeyCode.Return }));
		keyTable.Add(ConfigurableKeyType.Space, new ConfigurableKey(new KeyCode[] { KeyCode.Space }));
	}

	public void InitializeAsVim() {
		keyTable = new Dictionary<ConfigurableKeyType, ConfigurableKey>();
		keyTable.Add(ConfigurableKeyType.Right, new ConfigurableKey(new KeyCode[] { KeyCode.D, KeyCode.RightArrow, KeyCode.L }));
		keyTable.Add(ConfigurableKeyType.Left, new ConfigurableKey(new KeyCode[] { KeyCode.A, KeyCode.LeftArrow, KeyCode.H }));
		keyTable.Add(ConfigurableKeyType.Up, new ConfigurableKey(new KeyCode[] { KeyCode.W, KeyCode.UpArrow, KeyCode.K }));
		keyTable.Add(ConfigurableKeyType.Down, new ConfigurableKey(new KeyCode[] { KeyCode.S, KeyCode.DownArrow, KeyCode.J }));
		keyTable.Add(ConfigurableKeyType.Enter, new ConfigurableKey(new KeyCode[] { KeyCode.KeypadEnter, KeyCode.Return }));
		keyTable.Add(ConfigurableKeyType.Space, new ConfigurableKey(new KeyCode[] { KeyCode.Space }));
	}

	public bool IsPressed(ConfigurableKeyType type) {
		return keyTable[type].IsPressed;
	}

	public bool IsPushed(ConfigurableKeyType type) {
		return keyTable[type].IsPushed;
	}
}

class ConfigurableKey {
	KeyCode[] keys;
	public ConfigurableKey(KeyCode[] keys) {
		this.keys = keys;
	}

	public bool IsPressed {
		get {
			foreach (KeyCode code in this.keys) {
				if (Input.GetKey (code)) {
					return true;
				}
			}
			return false;
		}
	}

	public bool IsPushed {
		get {
			foreach (KeyCode code in this.keys) {
				if (Input.GetKeyDown (code)) {
					return true;
				}
			}
			return false;
		}
	}
}

public enum ConfigurableKeyType {
	Right = 0,
	Left,
	Up,
	Down,
	Enter,
	Space
}
