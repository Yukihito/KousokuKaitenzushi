using UnityEngine;
using System.Collections;

public class SushiSpec {
	public float MaxSpeed;
	public float Acceleration;
	public float Weight;

	public int MaxSpeedRank;
	public int AccelerationRank;
	public int WeightRank;

	int maxRank = 5;
	int rankCenter = 3;

	public float MaxSpeedRange = 0.2f;
	public float AccelerationRange = 0.006f;
	public float WeightRange = 0.018f;

	float maxSpeedBase = 1.9f;
	float accelerationBase = 0.01f;
	float weightBase = 0.98f;

	float calcFactor(float range, int rank) {
		return range * ((float)(rank - rankCenter) / (float)(maxRank - rankCenter));
	}

	public SushiSpec(int maxSpeedRank, int accelerationRank, int weightRank) {
		MaxSpeedRank = maxSpeedRank;
		AccelerationRank = accelerationRank;
		WeightRank = weightRank;
		MaxSpeed = maxSpeedBase + calcFactor(MaxSpeedRange, MaxSpeedRank);
		Acceleration = accelerationBase + calcFactor(AccelerationRange, AccelerationRank);
		Weight = weightBase + calcFactor(WeightRange, WeightRank);
	}
}

public class SushiSpecProvider {
	public static SushiSpec Provide(SushiType sushiType) {
		switch (sushiType) {
		case SushiType.Amaebi:
			return new SushiSpec(3, 3, 3);
		case SushiType.Ebi:
			return new SushiSpec(3, 3, 4);
		case SushiType.Hamachi:
			return new SushiSpec(1, 5, 1);
		case SushiType.Hokki:
			return new SushiSpec(2, 4, 3);
		case SushiType.Ika:
			return new SushiSpec(3, 3, 1);
		case SushiType.Ikura:
			return new SushiSpec(1, 5, 5);
		case SushiType.Kohada:
			return new SushiSpec(3, 3, 2);
		case SushiType.Maguro:
			return new SushiSpec(4, 2, 3);
		case SushiType.Sulmon:
			return new SushiSpec(3, 5, 1);
		case SushiType.Tako:
			return new SushiSpec(3, 3, 5);
		case SushiType.Tamago:
			return new SushiSpec(4, 3, 5);
		case SushiType.Toro:
			return new SushiSpec(5, 1, 3);
		case SushiType.Uni:
			return new SushiSpec(2, 4, 4);
		default:
			return null;
		}
	}
}
