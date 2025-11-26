
using UnityEngine;
using System;

/// <summary>
/// 毒（DoT）：毎秒ダメージを与える状態異常
/// </summary>
public class PoisonEffect : StatusEffectBase
{
	private readonly float dps;                 // 1秒あたりダメージ
	private readonly Action<int> onDamage;      // ダメージ適用先（例：status.Damage）
	private float fractionalDamageBuffer = 0f;  // 少数ダメージの蓄積（整数で適用するため）

	/// <param name="dps">1秒あたりダメージ（例：10fなら毎秒10ダメージ）</param>
	/// <param name="duration">効果時間（秒）</param>
	/// <param name="onDamage">ダメージ適用コールバック（例：d => status.Damage(d)）</param>
	/// <param name="icon">UI表示用アイコン（任意）</param>
	public PoisonEffect(float dps, float duration, Action<int> onDamage, Sprite icon = null)
		: base("Poison", duration, icon)
	{
		this.dps = Mathf.Max(0f, dps);
		this.onDamage = onDamage;
	}

	/// <summary>
	/// 攻撃力には影響しない（純粋なDoT）
	/// </summary>
	public override float ModifyAttackPower(float currentPower)
	{
		return currentPower;
	}

	/// <summary>
	/// 経過時間に応じてDoTダメージを適用。
	/// 有効時間を超える分のダメージは適用しないように、残り時間分だけ計算する。
	/// </summary>
	public override void Tick(float deltaTime)
	{
		// 残り時間分のみダメージを計算
		float remaining = Mathf.Max(0f, duration - elapsed);
		float effectiveDelta = Mathf.Min(deltaTime, remaining);

		if (effectiveDelta > 0f && dps > 0f && onDamage != null)
		{
			// 今フレーム分のダメージを加算（整数適用のためにバッファへ）
			fractionalDamageBuffer += dps * effectiveDelta;

			// 整数分のみ適用（UIやゲーム側が int を想定しているため）
			int toApply = Mathf.FloorToInt(fractionalDamageBuffer);
			if (toApply > 0)
			{
				onDamage.Invoke(toApply);
				fractionalDamageBuffer -= toApply;
			}
		}

		// 時間進行
		base.Tick(deltaTime);
	}
}
