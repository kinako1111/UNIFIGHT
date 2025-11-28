
using UnityEngine;

/// <summary>
/// 汎用DOT（1層ごとに dmg/tick を加算）。属性別キーを使いたい場合はKeyを "Poison" 等に。
/// </summary>
public class DamageOverTimeStackEffect : StatusEffectBaseStackableModel
{
	public DamageType Type { get; private set; }

	private int baseDamagePerTick; // 1層あたりのダメージ
	private float tickInterval;
	private float tickAccum;
	private readonly bool applyImmediateFirstTick;

	public DamageOverTimeStackEffect(Status owner,
		string key, string displayName, DamageType type,
		int initialStacks, int maxStacks,
		int baseDamagePerTick, float tickInterval,
		bool perStackDecay, float durationSeconds,
		bool applyImmediateFirstTick = false)
		: base(owner, key, displayName, initialStacks, maxStacks, perStackDecay, durationSeconds)
	{
		this.Type = type;
		this.baseDamagePerTick = Mathf.Max(1, baseDamagePerTick);
		this.tickInterval = Mathf.Max(0.05f, tickInterval);
		this.applyImmediateFirstTick = applyImmediateFirstTick;
		OnStacksChanged();
	}

	protected override void OnStacksChanged() { /* ここでは特に不要 */ }

	public override void Tick(float deltaTime)
	{
		base.Tick(deltaTime);
		if (IsExpired) return;

		// 初回即時適用（共有タイマーの最初のフレームのみ）
		if (applyImmediateFirstTick && Remaining > 0f && baseDamagePerTick > 0 && Stacks > 0 && elapsed == deltaTime)
		{
			int dmg = baseDamagePerTick * Stacks;
			owner?.Damage(dmg);
		}

		tickAccum += deltaTime;
		while (tickAccum >= tickInterval && !IsExpired)
		{
			tickAccum -= tickInterval;
			int dmg = baseDamagePerTick * Stacks; // 層数に比例
			owner?.Damage(dmg);
		}
	}

	public override float ModifyAttackPower(float currentPower) => currentPower; // DOTはATKに影響なし
}