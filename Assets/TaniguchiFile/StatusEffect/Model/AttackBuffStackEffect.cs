
using UnityEngine;

/// <summary>
/// 攻撃力バフ（1層ごとに +x% を加算）。例：perStackAdd=0.10f, MaxStacks=5 → 最大 +50%
/// </summary>
public class AttackBuffStackEffect : StatusEffectBaseStackableModel
{
	private readonly float perStackAdd; // +10%なら 0.10f

	public AttackBuffStackEffect(Status owner, int initialStacks, int maxStacks, float perStackAdd,
		bool perStackDecay, float durationSeconds)
		: base(owner, key: "ATK_BUFF", displayName: "攻撃力バフ",
			   initialStacks, maxStacks, perStackDecay, durationSeconds)
	{
		this.perStackAdd = Mathf.Max(0f, perStackAdd);
		OnStacksChanged();
	}

	protected override void OnStacksChanged() { /* 計算は Modify 内で行うので空でもOK */ }

	public override float ModifyAttackPower(float currentPower)
	{
		float multiplier = 1f + perStackAdd * Stacks;
		return currentPower * multiplier;
	}
}
