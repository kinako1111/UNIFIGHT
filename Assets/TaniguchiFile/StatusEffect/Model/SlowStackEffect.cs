
using UnityEngine;

public interface ISlowEffect
{
	float ModifyMoveSpeed(float currentSpeed);
}

/// <summary>
/// スロー（1層ごとに -x% 速度）。例：perStackSlow=0.15, MaxStacks=3 → 最大 -45%
/// </summary>
public class SlowStackEffect : StatusEffectBaseStackableModel, ISlowEffect
{
	private readonly float perStackSlow;

	public SlowStackEffect(Status owner, int initialStacks, int maxStacks, float perStackSlow,
		bool perStackDecay, float durationSeconds)
		: base(owner, key: "Slow", displayName: "スロー",
			   initialStacks, maxStacks, perStackDecay, durationSeconds)
	{
		this.perStackSlow = Mathf.Clamp(perStackSlow, 0f, 1f);
		OnStacksChanged();
	}

	protected override void OnStacksChanged() { }

	public float ModifyMoveSpeed(float currentSpeed)
	{
		float multiplier = Mathf.Max(0f, 1f - perStackSlow * Stacks);
		return currentSpeed * multiplier;
	}
}
