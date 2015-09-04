using UnityEngine;
using System.Collections;


public abstract class Stage {
	public int StageId;
	public string Name;
	public abstract FloorListFactory CreateFloorsFactory ();
	public static Stage GetStageById(int stageId) {
		switch (stageId) {
		case 1:
			return new Stage1 ();
		case 2:
			return new Stage2 ();
		case 3:
			return new Stage3 ();
		case 4:
			return new Stage4 ();
		case 5:
			return new Stage5 ();
		}
		return null;
	}
}

public class Stage1 : Stage {
	public Stage1() {
		StageId = 1;
		Name = "大廻転";
	}
	public override FloorListFactory CreateFloorsFactory () {
		return new Stage1FloorsFactory ();
	}
}

public class Stage2 : Stage {
	public Stage2() {
		StageId = 2;
		Name = "ダイニング";
	}
	public override FloorListFactory CreateFloorsFactory () {
		return new Stage2FloorsFactory ();
	}
}

public class Stage3 : Stage {
	public Stage3() {
		StageId = 3;
		Name = "超廻転";
	}
	public override FloorListFactory CreateFloorsFactory () {
		return new Stage3FloorsFactory ();
	}
}

public class Stage4 : Stage {
	public Stage4() {
		StageId = 4;
		Name = "寿司パラダイス";
	}
	public override FloorListFactory CreateFloorsFactory () {
		return new Stage4FloorsFactory ();
	}
}

public class Stage5 : Stage {
	public Stage5() {
		StageId = 5;
		Name = "天";
	}
	public override FloorListFactory CreateFloorsFactory () {
		return new Stage5FloorsFactory ();
	}
}
