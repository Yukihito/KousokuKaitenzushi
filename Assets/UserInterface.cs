using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {
	public Car Player;
	public Car[] AIs;
	State state;
	public Text CenterText;
	public Text TimeText;
	public Text LapTimeText;
	public Text RankText;
	public Text GoalTimeText;
	public Text LastRankNumText;
	public Text LastRankDenomText;
	public Text LapText;
	public Text ReturnSelectorText;
	public Text TweetSelectorText;
	public Text HighScoreNoticeText;

	public Sprite TokujoSprite;
	public Sprite JoSprite;
	public Sprite NamiSprite;

	public Image ResultImage;
	public Image LastRankImage;
	public RectTransform LastRankRect;

	public Button TweetButton;
	public Button ReturnButton;

	public float StartedTime = 0f;
	public float FinishedTime = 0f;
	public int LastUserRankNum = -1;
	public int NextSceneSelectorIndex = 0;
	public int CurrentStageId;
	public SushiPopper SushiPopper;
	public static bool IsAvailableStageInclemented;

	public SoundManager SoundManager;
	KeyConfig keyConfig;
	public Curtain Curtain;
	bool isReturn = false;

	void updateState() {
		if (state != null) {
			state.Update ();
			if (state.IsEnded ()) {
				state.OnEnd ();
				state = state.CreateNextState ();
			}
		}
	}

	void initDrivingStateUI() {
		RankText = GameObject.Find ("Rank").GetComponent<Text> ();
		TimeText = GameObject.Find ("Time").GetComponent<Text> ();
		CenterText = GameObject.Find ("CenterText").GetComponent<Text> ();
		LapText = GameObject.Find ("Lap").GetComponent<Text> ();
		LapTimeText = GameObject.Find ("LapTime").GetComponent<Text> ();
	}

	void hideDrivingStateUI() {
		RankText.enabled = false;
		TimeText.enabled = false;
		CenterText.enabled = false;
		LapText.enabled = false;
		LapTimeText.enabled = false;
	}

	void initResultUI() {
		SushiPopper = GameObject.Find ("SushiPopper").GetComponent<SushiPopper> ();
		ResultImage = GameObject.Find ("Result").GetComponent<Image> ();
		GoalTimeText = GameObject.Find ("GoalTime").GetComponent<Text> ();
		LastRankNumText = GameObject.Find ("LastRankNum").GetComponent<Text> ();
		LastRankDenomText = GameObject.Find ("LastRankNumDenom").GetComponent<Text> ();
		LastRankImage = GameObject.Find ("LastRank").GetComponent<Image> ();
		LastRankRect = GameObject.Find ("LastRank").GetComponent<RectTransform> ();
		TweetButton = GameObject.Find ("TweetButton").GetComponent<Button> ();
		ReturnButton = GameObject.Find ("ReturnButton").GetComponent<Button> ();
		ReturnButton.onClick.AddListener (() => {
			SaveData.Save();
			Application.LoadLevel("StageSelect");
		});
		TweetButton.onClick.AddListener (() => {
			OpenTwitter();
		});

		ReturnSelectorText = GameObject.Find ("ReturnSelector").GetComponent<Text> ();
		TweetSelectorText = GameObject.Find ("TweetSelector").GetComponent<Text> ();
		HighScoreNoticeText = GameObject.Find ("HighScoreNotice").GetComponent<Text> ();
	}

	public void OpenTwitter() {
		string stageName = Stage.GetStageById (CurrentStageId).Name;
		string message = "『" + stageName + "』を " + FormatFloatTime(GoalTime) + " でクリア! #高速廻転寿司 http://yukihito.github.io/KousokuKaitenzushi/";
		Application.OpenURL("http://twitter.com/intent/tweet?text=" + WWW.EscapeURL(message));
	}

	void hideResultUI() {
		ResultImage.enabled = false;
		GoalTimeText.enabled = false;
		LastRankNumText.enabled = false;
		LastRankDenomText.enabled = false;
		LastRankImage.enabled = false;
		TweetButton.enabled = false;
		TweetButton.image.enabled = false;
		TweetButton.GetComponentInChildren<Text> ().enabled = false;
		ReturnButton.enabled = false;
		ReturnButton.image.enabled = false;
		ReturnButton.GetComponentInChildren<Text> ().enabled = false;
		ReturnSelectorText.enabled = false;
		TweetSelectorText.enabled = false;
		HighScoreNoticeText.enabled = false;
	}

	public static string FormatFloatTime(float time) {
		int min = (int)(time / 60);
		int sec = (int)(time % 60);
		int mil = (int)((time * 100) % 100);
		return zeroFill (min) + ":" + zeroFill (sec) + ":" + zeroFill (mil);
	}

	static string zeroFill(int time) {
		if (time < 10) {
			return "0" + time;
		} else {
			return "" + time;
		}
	}

	public float GoalTime {
		get { return FinishedTime - StartedTime;}
	}

	// Use this for initialization
	void Start () {
		Curtain = GameObject.Find ("Curtain").GetComponent<Curtain> ();
		UserInterface.IsAvailableStageInclemented = false;
		SoundManager = GetComponent<SoundManager>();
		Player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Car> ();
		GameObject[] aiGameObjects = GameObject.FindGameObjectsWithTag ("AI");
		AIs = new Car[aiGameObjects.Length];
		for (int i = 0; i < AIs.Length; i++) {
			AIs [i] = aiGameObjects [i].GetComponent<Car> ();
		}
		state = new ReadyState (this);

		initDrivingStateUI ();
		initResultUI ();
		hideDrivingStateUI ();
		hideResultUI ();
		keyConfig = KeyConfig.Current;
	}

	// Update is called once per frame
	void Update () {
		if (keyConfig.IsPushed (ConfigurableKeyType.M)) {
			SoundManager.IsMute = !SoundManager.IsMute;
		}
		if (keyConfig.IsPushed (ConfigurableKeyType.Esc)) {
			isReturn = true;
			SoundManager.BackAudio.Play ();
			Curtain.FadeOut ();
		}
		if (isReturn && Curtain.IsFinished) {
			Application.LoadLevel ("StageSelect");
		}
		if (!isReturn) {
			updateState ();
		}
	}
}

abstract class State {
	protected UserInterface UserInterface;
	protected float CreatedTime;
	protected float ElapsedTimeFromCreated {
		get {
			return Time.time - CreatedTime;
		}
	}

	public State(UserInterface userInterface) {
		UserInterface = userInterface;
		CreatedTime = Time.time;
	}

	public abstract void Update ();

	public abstract void OnEnd ();

	public abstract State CreateNextState ();

	public abstract bool IsEnded ();
}


class ReadyState: State {
	public ReadyState(UserInterface userInterface):base(userInterface) {}

	public override void Update() { }

	public override State CreateNextState () {
		return new ThreeState(UserInterface);
	}

	public override bool IsEnded () {
		return ElapsedTimeFromCreated >= 1f;
	}

	public override void OnEnd () { }
}


class ThreeState: State {
	public ThreeState(UserInterface userInterface):base(userInterface) {
		userInterface.CenterText.text = "3";
		userInterface.CenterText.color = new Color (1f, 0f, 0f);
		userInterface.CenterText.enabled = true;
		userInterface.SoundManager.PiAudioSource.Play ();
	}

	public override void Update() { }

	public override State CreateNextState () {
		return new TwoState(UserInterface);
	}

	public override bool IsEnded () {
		return ElapsedTimeFromCreated >= 1f;
	}

	public override void OnEnd () { }
}

class TwoState: State {
	public TwoState(UserInterface userInterface):base(userInterface) {
		userInterface.CenterText.text = "2";
		userInterface.CenterText.color = new Color (1f, 0f, 0f);
		userInterface.CenterText.enabled = true;
		userInterface.SoundManager.PiAudioSource.Play ();
	}

	public override void Update() { }

	public override State CreateNextState () {
		return new OneState(UserInterface);
	}

	public override bool IsEnded () {
		return ElapsedTimeFromCreated >= 1f;
	}

	public override void OnEnd () { }
}

class OneState: State {
	public OneState(UserInterface userInterface):base(userInterface) {
		userInterface.CenterText.text = "1";
		userInterface.CenterText.color = new Color (1f, 0f, 0f);
		userInterface.CenterText.enabled = true;
		userInterface.SoundManager.PiAudioSource.Play ();
	}

	public override void Update() { }

	public override State CreateNextState () {
		return new GoState(UserInterface);
	}

	public override bool IsEnded () {
		return ElapsedTimeFromCreated >= 1f;
	}

	public override void OnEnd () { }
}


class GoState: State {
	public GoState(UserInterface userInterface):base(userInterface) {
		userInterface.StartedTime = Time.time;
		userInterface.CenterText.text = "GO!!";
		userInterface.CenterText.color = new Color (0f, 1f, 0f);
		userInterface.CenterText.enabled = true;
		userInterface.Player.IsStarted = true;
		foreach (Car ai in userInterface.AIs) {
			ai.IsStarted = true;
		}
		userInterface.SoundManager.PonAudioSource.Play ();
	}

	public override void Update() { }

	public override State CreateNextState () {
		return new DrivingState(UserInterface);
	}

	public override bool IsEnded () {
		return ElapsedTimeFromCreated >= 1f;
	}

	public override void OnEnd () { 
		UserInterface.CenterText.enabled = false;
	}
}

class DrivingState: State {
	int prevRankNum = 1;
	public DrivingState(UserInterface userInterface):base(userInterface) {
		updateRankText ();
		userInterface.RankText.enabled = true;
		userInterface.TimeText.enabled = true;
		userInterface.LapText.enabled = true;
	}

	public override void Update() {
		updateRankText ();
		playRankChangeSound ();
	}

	public override State CreateNextState () {
		return new GoalState(UserInterface);
	}

	public override bool IsEnded () {
		return UserInterface.Player.Lap > 2;
	}

	public override void OnEnd () {
		UserInterface.Player.IsAuto = true;
		UserInterface.RankText.enabled = false;
		UserInterface.TimeText.enabled = false;
		UserInterface.LapText.enabled = false;
		UserInterface.FinishedTime = Time.time;
		UserInterface.LastUserRankNum = getUserRankNum ();
	}

	void playRankChangeSound() {
		int currentRankNum = getUserRankNum ();
		if (currentRankNum < prevRankNum) {
			UserInterface.SoundManager.LapAudioSource.Play ();
		}
		prevRankNum = currentRankNum;
	}

	void updateRankText() {
		UserInterface.RankText.text = "順位   " + getUserRankNum() + "/" + (UserInterface.AIs.Length + 1);
		UserInterface.TimeText.text = UserInterface.FormatFloatTime (ElapsedTimeFromCreated);
		UserInterface.LapText.text = "Lap    " + (UserInterface.Player.Lap + 1) + "/3";

		if (UserInterface.Player.Lap > 0 && Time.time - UserInterface.Player.LastGoaledTime < 1f) {
			if (!UserInterface.LapTimeText.enabled) {
				UserInterface.SoundManager.LapAudioSource.Play ();
				UserInterface.SushiPopper.IsVisible = true;
			}
			float alphaFactor = Mathf.Sin((Time.time - UserInterface.Player.LastGoaledTime) * Mathf.PI);
			UserInterface.LapTimeText.text = "Lap " + UserInterface.FormatFloatTime (UserInterface.Player.LastLapTime);
			UserInterface.LapTimeText.color = new Color (1f, 0.2f, 1f, alphaFactor);
			UserInterface.LapTimeText.enabled = true;
			UserInterface.CenterText.color = new Color (1f, 1f, 1f, alphaFactor);
			UserInterface.CenterText.enabled = true;
			UserInterface.CenterText.text = "　あと" + (3 - UserInterface.Player.Lap) + "周！";
			UserInterface.CenterText.fontSize = 50;
		} else {
			UserInterface.CenterText.fontSize = 100;
			UserInterface.CenterText.enabled = false;
			UserInterface.LapTimeText.enabled = false;
		}

		if (UserInterface.Player.IsReverseRunning) {
			UserInterface.CenterText.color = new Color (1f, 1f, 0f, (Mathf.Sin (((((Time.time * 3f) % 1) - 0.5f)) * 2f * Mathf.PI) + 1f) / 2f);
			UserInterface.CenterText.text = "逆走中！！";
			UserInterface.CenterText.enabled = true;
		}
	}

	Car[] getSortedCars() {
		int length = UserInterface.AIs.Length + 1;
		Car[] cars = new Car[length];
		cars [0] = UserInterface.Player;
		for (int i = 1; i < cars.Length; i++) {
			cars [i] = UserInterface.AIs [i - 1];
		}
		Array.Sort (cars, (x, y) => getProgressForCmp(y) - getProgressForCmp(x));
		return cars;
	}

	int getProgressForCmp(Car car) {
		return (int)(((float)car.Lap * 1000f + car.Progress) * 100f);
	}

	int getUserRankNum() {
		Car[] cars = getSortedCars ();
		for (int i = 0; i < cars.Length; i++) {
			if (cars [i].tag == "Player") {
				return i + 1;
			}
		}
		return 0;
	}
}

class GoalState: State {
	public GoalState(UserInterface userInterface):base(userInterface) {
		UserInterface.CenterText.fontSize = 100;
		userInterface.CenterText.text = "GOAL!!";
		userInterface.CenterText.fontSize = 60;
		userInterface.CenterText.color = new Color (0f, 1f, 0f, 0f);
		userInterface.CenterText.enabled = true;
	}

	public override void Update() {
		float alphaFactor = Mathf.Sin((Time.time - UserInterface.Player.LastGoaledTime) * Mathf.PI);
		UserInterface.LapTimeText.color = new Color (1f, 0.2f, 1f, alphaFactor);
		UserInterface.CenterText.color = new Color (0f, 1f, 0f, alphaFactor);
	}

	public override State CreateNextState () {
		return new ResultState(UserInterface);
	}

	public override bool IsEnded () {
		return ElapsedTimeFromCreated >= 1f;
	}

	public override void OnEnd () {
		UserInterface.CenterText.enabled = false;
		UserInterface.LapTimeText.enabled = false;
	}
}

class ResultState: State {
	KeyConfig keyConfig;
	bool isFinished = false;
	public ResultState(UserInterface userInterface):base(userInterface) {
		keyConfig = KeyConfig.Current;
		userInterface.LastRankNumText.text = "" + userInterface.LastUserRankNum;
		userInterface.LastRankDenomText.text = "/ " + (userInterface.AIs.Length + 1);
		if (userInterface.LastUserRankNum == 1) {
			userInterface.LastRankImage.sprite = userInterface.TokujoSprite;
		} else if (userInterface.LastUserRankNum == 2) {
			userInterface.LastRankImage.sprite = userInterface.JoSprite;
			userInterface.LastRankRect.sizeDelta = new Vector2(75f, 75f);
		} else {
			userInterface.LastRankImage.sprite = userInterface.NamiSprite;
			userInterface.LastRankRect.sizeDelta = new Vector2(75f, 75f);
		}
		float time = userInterface.GoalTime;
		userInterface.GoalTimeText.text = UserInterface.FormatFloatTime (time);
		userInterface.ResultImage.enabled = true;
		userInterface.GoalTimeText.enabled = true;
		userInterface.LastRankNumText.enabled = true;
		userInterface.LastRankDenomText.enabled = true;
		userInterface.LastRankImage.enabled = true;
		userInterface.TweetButton.enabled = true;
		userInterface.TweetButton.image.enabled = true;
		userInterface.TweetButton.GetComponentInChildren<Text> ().enabled = true;
		userInterface.ReturnButton.enabled = true;
		userInterface.ReturnButton.image.enabled = true;
		userInterface.ReturnButton.GetComponentInChildren<Text> ().enabled = true;
		float prevBestTime = SaveData.GetStageBestTime (UserInterface.CurrentStageId);
		if (prevBestTime == -1 || time < prevBestTime) {
			SaveData.SetStageBestTime (UserInterface.CurrentStageId, time);
			UserInterface.HighScoreNoticeText.enabled = true;
		}
		int prevAvailableStageId = SaveData.GetAvailableStage ();
		if (prevAvailableStageId < 5 && prevAvailableStageId == UserInterface.CurrentStageId) {
			SaveData.SetAvailableStage (UserInterface.CurrentStageId + 1);
			UserInterface.IsAvailableStageInclemented = true;
		}
		userInterface.ReturnSelectorText.enabled = true;
	}

	public override void Update() {
		if (!isFinished) {
			if (UserInterface.NextSceneSelectorIndex == 0) {
				UserInterface.ReturnSelectorText.rectTransform.anchoredPosition = new Vector2 (-220f + ((Time.time * 3f) % 1f) * 5f, -215f);

				if (keyConfig.IsPushed (ConfigurableKeyType.Enter) || keyConfig.IsPushed (ConfigurableKeyType.Space)) {
					UserInterface.Curtain.FadeOut ();
					isFinished = true;
					UserInterface.SoundManager.LapAudioSource.Play ();
				}

				if (keyConfig.IsPushed (ConfigurableKeyType.Down) ||
				   keyConfig.IsPushed (ConfigurableKeyType.Up) ||
				   keyConfig.IsPushed (ConfigurableKeyType.Right) ||
				   keyConfig.IsPushed (ConfigurableKeyType.Left)) {
					UserInterface.ReturnSelectorText.enabled = false;
					UserInterface.TweetSelectorText.enabled = true;
					UserInterface.NextSceneSelectorIndex = 1;
					UserInterface.SoundManager.PiAudioSource.Play ();
				}
			} else {
				UserInterface.TweetSelectorText.rectTransform.anchoredPosition = new Vector2 (70 + ((Time.time * 3f) % 1f) * 5f, -215f);
				if (keyConfig.IsPushed (ConfigurableKeyType.Enter) || keyConfig.IsPushed (ConfigurableKeyType.Space)) {
					UserInterface.OpenTwitter ();
				}

				if (keyConfig.IsPushed (ConfigurableKeyType.Down) ||
				   keyConfig.IsPushed (ConfigurableKeyType.Up) ||
				   keyConfig.IsPushed (ConfigurableKeyType.Right) ||
				   keyConfig.IsPushed (ConfigurableKeyType.Left)) {
					UserInterface.ReturnSelectorText.enabled = true;
					UserInterface.TweetSelectorText.enabled = false;
					UserInterface.NextSceneSelectorIndex = 0;
					UserInterface.SoundManager.PiAudioSource.Play ();
				}
			}
		}
	}

	public override State CreateNextState () {
		return null;
	}

	public override bool IsEnded () {
		return isFinished && UserInterface.Curtain.IsFinished;
	}

	public override void OnEnd () {
		SaveData.Save ();
		Application.LoadLevel("StageSelect");
	}
}