using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	public AudioSource[] BgmAudioSources;
	public AudioSource PiAudioSource;
	public AudioSource PonAudioSource;
	public AudioSource ClickAudioSource;
	public AudioSource LapAudioSource;
	public AudioSource AcceleAudio;
	public AudioSource ColAudio;
	public AudioSource BackAudio;

	public float BgmTime = 37f;
	public float BgmOverwrapTime = 1.5f;
	float bgmStartTime;
	int activeIndex = 0;
	static bool isMute = false;

	public bool IsMute {
		get { return isMute; }
		set {
			isMute = value;
			for (int i = 0; i < BgmAudioSources.Length; i++) {
				BgmAudioSources [i].mute = isMute;
			}
			PiAudioSource.mute = isMute;
			PonAudioSource.mute = isMute;
			ClickAudioSource.mute = isMute;
			LapAudioSource.mute = isMute;
			AcceleAudio.mute = isMute;
			ColAudio.mute = isMute;
			BackAudio.mute = isMute;
		}
	}

	// Use this for initialization
	void Start () {
		BgmAudioSources[activeIndex].PlayDelayed (0f);
		bgmStartTime = Time.time + 0.05f;
		IsMute = IsMute;
	}
	
	// Update is called once per frame
	void Update () {
		float time = Time.time;
		if (time - bgmStartTime >= BgmTime - BgmOverwrapTime - 1f) {
			float elapsedTimeFromBgmStart = time - bgmStartTime;
			float delay = BgmTime - BgmOverwrapTime - elapsedTimeFromBgmStart;
			activeIndex++;
			if (activeIndex > 1) {
				activeIndex = 0;
			}
			BgmAudioSources[activeIndex].PlayDelayed (delay);
			bgmStartTime = time + delay + 0.05f;
		}
	}
}
