using UnityEngine;
using System.Collections;

public class SaveData {
	static string createStageBestTimeKey(int stageId) {
		return "stage_" + stageId + "_best_time";
	}

	static string availableStageKey = "available_stage";

	public static void SetStageBestTime(int stageId, float time) {
		PlayerPrefs.SetFloat (createStageBestTimeKey(stageId), time);
	}

	public static float GetStageBestTime(int stageId) {
		return PlayerPrefs.GetFloat (createStageBestTimeKey (stageId), -1f);
	}

	public static void SetAvailableStage(int stageId) {
		PlayerPrefs.SetInt (availableStageKey, stageId);
	}

	public static int GetAvailableStage() {
		return PlayerPrefs.GetInt (availableStageKey, 1);
	}

	public static void Save() {
		PlayerPrefs.Save ();
	}

	public static void DeleteAll() {
		PlayerPrefs.DeleteAll ();
	}
}
