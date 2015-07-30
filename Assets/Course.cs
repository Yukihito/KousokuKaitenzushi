using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Course : MonoBehaviour {
	CheckPoint[] checkPoints;
	Dictionary<int, CheckPoint> checkPointTable;
	public int GoalCheckPointOrder;
	GameObject user;
	GameObject[] ais;
	Text rankText;
	Text timeText;
	Text centerText;
	Text lapTimeText;
	int prevUserRankOrder = -1;
	float sceneStartTime;
	float startTime;
	public bool IsStarted = false;
	bool isPrevLapTimeShowing = false;
	float lastLapTime;
	float lastGoaledTime;
	public Sprite TokujoSprite;
	public Sprite JoSprite;
	public Sprite NamiSprite;
	int lastRankOrder;

	Image resultImage;
	Text goalTimeText;
	Text lastRankNumText;
	Text lastRankDenomText;
	Text lapText;
	Image lastRankImage;
	RectTransform lastRankRect;
	Button tweetButton;
	Button returnButton;

	private AudioSource piAudioSource;
	private AudioSource ponAudioSource;
	private AudioSource bgmAudioSource;
	private AudioSource clickAudioSource;

	int elapsedFrameFromGoaled = 0;

	int piCount = 0;
	bool isPonPlayed = false;
	bool isBgmPlayed = false;

	void Start () {
		GameObject[] checkPointObjs = GameObject.FindGameObjectsWithTag ("RoadRoot");
		checkPoints = new CheckPoint[checkPointObjs.Length];
		for (int i = 0; i < checkPointObjs.Length; i++) {
			checkPoints [i] = checkPointObjs [i].GetComponentInChildren<CheckPoint> ();
		}
		Array.Sort (checkPoints, (a, b) => a.CheckPointOrder - b.CheckPointOrder);
		checkPointTable = new Dictionary<int, CheckPoint> ();
		for (int i = 0; i + 1 < checkPoints.Length; i++) {
			checkPointTable.Add (checkPoints [i].CheckPointOrder, checkPoints [i + 1]);
		}
		checkPointTable.Add (checkPoints [checkPoints.Length - 1].CheckPointOrder, checkPoints [0]);
		checkPointTable.Add (-1, checkPoints [0]);
		user = GameObject.Find ("RacingSushi");
		ais = GameObject.FindGameObjectsWithTag ("AI");
		rankText = GameObject.Find ("Rank").GetComponent<Text> ();
		timeText = GameObject.Find ("Time").GetComponent<Text> ();
		centerText = GameObject.Find ("CenterMessage").GetComponent<Text> ();
		lapText = GameObject.Find ("Lap").GetComponent<Text> ();
		lapTimeText = GameObject.Find ("LapTime").GetComponent<Text> ();
		sceneStartTime = Time.time;
		startTime = Time.time;
		lapTimeText.text = "";
		lastRankImage = GameObject.Find ("LastRank").GetComponent<Image> ();
		initResult ();
		hideResult ();
		piAudioSource = GameObject.Find ("Pi").GetComponent<AudioSource> ();
		ponAudioSource = GameObject.Find ("Pon").GetComponent<AudioSource> ();
		bgmAudioSource = GameObject.Find ("BGM").GetComponent<AudioSource> ();
		clickAudioSource = GameObject.Find ("Select").GetComponent<AudioSource> ();
	}

	void initResult() {
		resultImage = GameObject.Find ("Result").GetComponent<Image> ();
		goalTimeText = GameObject.Find ("GoalTime").GetComponent<Text> ();
		lastRankNumText = GameObject.Find ("LastRankNum").GetComponent<Text> ();
		lastRankDenomText = GameObject.Find ("LastRankNumDenom").GetComponent<Text> ();
		lastRankImage = GameObject.Find ("LastRank").GetComponent<Image> ();
		lastRankRect = GameObject.Find ("LastRank").GetComponent<RectTransform> ();
		tweetButton = GameObject.Find ("TweetButton").GetComponent<Button> ();
		returnButton = GameObject.Find ("ReturnButton").GetComponent<Button> ();

		tweetButton.onClick.AddListener (() => {
			Application.OpenURL("http://twitter.com/intent/tweet?text=" + WWW.EscapeURL("テスト"));
		});
	}

	void hideResult() {
		resultImage.enabled = false;
		goalTimeText.enabled = false;
		lastRankNumText.enabled = false;
		lastRankDenomText.enabled = false;
		lastRankImage.enabled = false;
		tweetButton.enabled = false;
		tweetButton.image.enabled = false;
		tweetButton.GetComponentInChildren<Text> ().enabled = false;
		returnButton.enabled = false;
		returnButton.image.enabled = false;
		returnButton.GetComponentInChildren<Text> ().enabled = false;
	}

	public void ShowResult() {
		lastRankNumText.text = "" + lastRankOrder;
		lastRankDenomText.text = "/ " + (ais.Length + 1);
		if (lastRankOrder == 1) {
			lastRankImage.sprite = TokujoSprite;
		} else if (lastRankOrder == 2) {
			lastRankImage.sprite = JoSprite;
			lastRankRect.sizeDelta = new Vector2(75f, 75f);
		} else {
			lastRankImage.sprite = NamiSprite;
			lastRankRect.sizeDelta = new Vector2(75f, 75f);
		}
		goalTimeText.text = formatFloatTime (lastGoaledTime);
		resultImage.enabled = true;
		goalTimeText.enabled = true;
		lastRankNumText.enabled = true;
		lastRankDenomText.enabled = true;
		lastRankImage.enabled = true;
		tweetButton.enabled = true;
		tweetButton.image.enabled = true;
		tweetButton.GetComponentInChildren<Text> ().enabled = true;
		returnButton.enabled = true;
		returnButton.image.enabled = true;
		returnButton.GetComponentInChildren<Text> ().enabled = true;
	}

	public CheckPoint GetNextCheckPoint(int currentCheckPointOrder) {
		return checkPointTable[currentCheckPointOrder];
	}

	public CheckPoint GoalPoint {
		get {
			return checkPoints [checkPoints.Length - 1];
		}
	}

	public CheckPoint StartPoint {
		get {
			return checkPoints [0];
		}
	}

	bool inRange(float v, float min, float max) {
		return min <= v && v < max;
	}
	
	// Update is called once per frame
	void Update () {
		float elapsedTimeFromSceneStart = Time.time - sceneStartTime;
		if (inRange(elapsedTimeFromSceneStart, 0f, 1f)) {
			if (!isBgmPlayed) {
				bgmAudioSource.loop = true;
				bgmAudioSource.Play ();
				isBgmPlayed = true;
			}
			centerText.text = "";
		} else if (inRange(elapsedTimeFromSceneStart, 1f, 2f)) {
			if (piCount < 1) {
				piAudioSource.Play ();
				piCount++;
			}
			centerText.text = "3";
			centerText.color = new Color (1f, 0f, 0f);
		} else if (inRange(elapsedTimeFromSceneStart, 2f, 3f)) {
			if (piCount < 2) {
				piAudioSource.Play ();
				piCount++;
			}
			centerText.text = "2";
			centerText.color = new Color (1f, 0f, 0f);
		} else if (inRange(elapsedTimeFromSceneStart, 3f, 4f)) {
			if (piCount < 3) {
				piAudioSource.Play ();
				piCount++;
			}
			centerText.text = "1";
			centerText.color = new Color (1f, 0f, 0f);
		} else if (inRange(elapsedTimeFromSceneStart, 4f, 5f)) {
			if (!isPonPlayed) {
				ponAudioSource.Play ();
				isPonPlayed = true;
			}
			if (!IsStarted) {
				IsStarted = true;
				startTime = Time.time;
				lastLapTime = startTime;
				lastGoaledTime = startTime;
			}
			centerText.text = "GO!!";
			centerText.color = new Color (0f, 1f, 0f);
		} else {
			centerText.text = "";
		}

		if (user.GetComponent<RacingSushi> ().IsGoaled) {
			if (elapsedFrameFromGoaled == 0) {
				rankText.enabled = false;
				timeText.enabled = false;
				lapText.enabled = false;
			} else if (elapsedFrameFromGoaled < 60) {
				centerText.text = "GOAL!!";
			} else if (elapsedFrameFromGoaled == 60) {
				centerText.enabled = false;
				ShowResult ();
			}
			elapsedFrameFromGoaled++;
		}

		IRankProvider[] sushis = sortedSushis ();
		int userRankOrder = -1;
		for (int i = 0; i < sushis.Length; i++) {
			if (sushis [i].IsUser) {
				userRankOrder = i + 1;
			}
		}
		if (!user.GetComponent<RacingSushi>().IsGoaled && prevUserRankOrder != userRankOrder) {
			lastRankOrder = userRankOrder;
			rankText.text = "順位   " + userRankOrder + "/" + sushis.Length;
		}
		float currentTime = Time.time - startTime;
		if (user.GetComponent<RacingSushi> ().IsGoaled) {
			timeText.text = formatFloatTime (lastGoaledTime);
		} else {
			timeText.text = formatFloatTime (currentTime);
		}
		prevUserRankOrder = userRankOrder;
		bool isLapTimeShowing = user.GetComponent<RacingSushi> ().IsShowLapTime;
		if (isLapTimeShowing != isPrevLapTimeShowing && isLapTimeShowing) {
			lastLapTime = currentTime - lastGoaledTime;
			lastGoaledTime = currentTime;
		}
		if (isLapTimeShowing) {
			lapTimeText.text = "Lap " + formatFloatTime (lastLapTime);
		} else {
			lapTimeText.text = "";
		}
		isPrevLapTimeShowing = isLapTimeShowing;
	}

	string formatFloatTime(float time) {
		if (!IsStarted) {
			return "00:00:00";
		}
		int min = (int)(time / 60);
		int sec = (int)(time % 60);
		int mil = (int)((time * 100) % 100);
		return zeroFill (min) + ":" + zeroFill (sec) + ":" + zeroFill (mil);
	}

	string zeroFill(int time) {
		if (time < 10) {
			return "0" + time;
		} else {
			return "" + time;
		}
	}

	IRankProvider[] sortedSushis() {
		int length = 1 + ais.Length;
		IRankProvider[] result = new IRankProvider[length];
		result [0] = user.GetComponent<RacingSushi>();
		for (int i = 1; i < result.Length; i++) {
			result [i] = ais [i - 1].GetComponent<RacingSushiAI>();
		}
		Array.Sort (result, (x, y) => y.Rank - x.Rank);
		return result;
	}
}

interface IRankProvider {
	int Rank {
		get;
	}

	bool IsUser {
		get;
	}
}
