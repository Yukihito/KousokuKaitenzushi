using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class StageSelector : MonoBehaviour {
	public Material GalaxySkybox;
	public Material NightSkybox;
	public Material BlueSkybox;
	public Material PinkSkybox;

	KeyConfig keyConfig;
	GameObject mainCamera;
	Material[] skyboxes;
	Stage[] stages;
	Mesh[] courses;

	Text stageNameText;
	Text stageNumberText;
	Text bestTimeText;
	Text rightText;
	Text leftText;
	Text newStageNoticeText;
	SoundManager soundManager;

	int stageIndex = 0;
	int maxStageIndex = 0;

	bool isSelected;
	bool isReturn;
	Curtain curtain;

	// Use this for initialization
	void Start () {
		curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		isSelected = false;
		maxStageIndex = SaveData.GetAvailableStage () - 1;
		stageNameText = GameObject.Find ("StageName").GetComponent<Text> ();
		stageNumberText = GameObject.Find ("StageNumber").GetComponent<Text> ();
		bestTimeText = GameObject.Find ("BestTime").GetComponent<Text> ();
		rightText = GameObject.Find ("Right").GetComponent<Text> ();
		leftText = GameObject.Find ("Left").GetComponent<Text> ();
		newStageNoticeText = GameObject.Find ("NewStageNotice").GetComponent<Text> ();
		soundManager = GameObject.Find ("Sounds").GetComponent<SoundManager> ();
		keyConfig = KeyConfig.Current;
		mainCamera = GameObject.Find("Main Camera");
		skyboxes = new Material[] { GalaxySkybox, NightSkybox, GalaxySkybox, PinkSkybox, BlueSkybox };
		stages = new Stage[] { new Stage1(), new Stage2(), new Stage3(), new Stage4(), new Stage5() };
		courses = new Mesh[] {
			stages [0].CreateFloorsFactory ().CreateFloorList (null).Mesh,
			stages [1].CreateFloorsFactory ().CreateFloorList (null).Mesh,
			stages [2].CreateFloorsFactory ().CreateFloorList (null).Mesh,
			stages [3].CreateFloorsFactory ().CreateFloorList (null).Mesh,
			stages [4].CreateFloorsFactory ().CreateFloorList (null).Mesh
		};
		if (UserInterface.IsAvailableStageInclemented) {
			stageIndex = SaveData.GetAvailableStage () - 1;
			newStageNoticeText.enabled = true;
		} else {
			stageIndex = 0;
			newStageNoticeText.enabled = false;
		}
		isSelected = false;
		isReturn = false;
		changeStage();
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if (Input.GetKeyDown (KeyCode.D)) {
			SaveData.DeleteAll ();
			Debug.Log ("d");
		}
		*/
		if (keyConfig.IsPushed (ConfigurableKeyType.M)) {
			soundManager.IsMute = !soundManager.IsMute;
		}
		rightText.rectTransform.anchoredPosition = new Vector2 (350f + ((Time.time * 2f) % 1f) * 10f, 0f);
		leftText.rectTransform.anchoredPosition = new Vector2 (-350f - ((Time.time * 2f) % 1f) * 10f, 0f);
		bool changed = false;
		if (!isSelected && keyConfig.IsPushed (ConfigurableKeyType.Right)) {
			stageIndex++;
			if (stageIndex > maxStageIndex) {
				stageIndex = maxStageIndex;
			} else {
				changed = true;
			}
		}

		if (!isSelected && keyConfig.IsPushed (ConfigurableKeyType.Left)) {
			stageIndex--;
			if (stageIndex < 0) {
				stageIndex = 0;
			} else {
				changed = true;
			}
		}

		if (changed) {
			soundManager.PiAudioSource.Play ();
			newStageNoticeText.enabled = false;
			changeStage ();
		}

		if (!isSelected && keyConfig.IsPushed(ConfigurableKeyType.Enter) || keyConfig.IsPushed(ConfigurableKeyType.Space)) {
			Global.CurrentStage = stages [stageIndex];
			isSelected = true;
			soundManager.ClickAudioSource.Play ();
			curtain.FadeOut ();
		}
		if (!isSelected && keyConfig.IsPushed (ConfigurableKeyType.Esc)) {
			isSelected = true;
			isReturn = true;
			soundManager.BackAudio.Play ();
			curtain.FadeOut ();
		}

		if (isSelected && curtain.IsFinished) {
			if (!isReturn) {
				Application.LoadLevel ("Stage" + (stageIndex + 1));
			} else {
				Application.LoadLevel ("SushiSelect");
			}
		}
	}

	void changeStage() {
		mainCamera.GetComponent<Skybox> ().material = skyboxes [stageIndex];
		GetComponent<MeshFilter> ().sharedMesh = courses[stageIndex];
		GetComponent<MeshFilter> ().sharedMesh.name = "courseMesh";
		stageNumberText.text = "Stage" + (stageIndex + 1);
		stageNameText.text = stages [stageIndex].Name;
		float time = SaveData.GetStageBestTime (stageIndex + 1);
		if (time != -1) {
			bestTimeText.text = "ベストタイム: " + UserInterface.FormatFloatTime (time);
		} else {
			bestTimeText.text = "ベストタイム: -";
		}

		if (stageIndex == 0 && stageIndex == maxStageIndex) {
			leftText.enabled = false;
			rightText.enabled = false;
		} else if (stageIndex == 0 && stageIndex != maxStageIndex) {
			leftText.enabled = false;
			rightText.enabled = true;
		} else if (stageIndex != 0 && stageIndex == maxStageIndex) {
			leftText.enabled = true;
			rightText.enabled = false;
		} else if (stageIndex != 0 && stageIndex != maxStageIndex) {
			leftText.enabled = true;
			rightText.enabled = true;
		}
	}
}
