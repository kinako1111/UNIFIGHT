
using UnityEngine;

/// <summary>
/// 攻撃力デバフ（1層ごとに -x%）。例：perStackReduce=0.10f, MaxStacks=5 → 最大 -50%
/// </summary>
public class AttackDebuffStackEffect : StatusEffectBaseStackableModel
{
	private readonly float perStackReduce; // -10%なら 0.10f

	public AttackDebuffStackEffect(Status owner, int initialStacks, int maxStacks, float perStackReduce,
		bool perStackDecay, float durationSeconds)
		: base(owner, key: "ATK_DEBUFF", displayName: "攻撃力デバフ",
			   initialStacks, maxStacks, perStackDecay, durationSeconds)
	{
		this.perStackReduce = Mathf.Clamp(perStackReduce, 0f, 1f);
		OnStacksChanged();
	}

	protected override void OnStacksChanged() { }

	public override float ModifyAttackPower(float currentPower)
	{
		float multiplier = Mathf.Max(0f, 1f - perStackReduce * Stacks);
		return currentPower * multiplier;
	}
}
