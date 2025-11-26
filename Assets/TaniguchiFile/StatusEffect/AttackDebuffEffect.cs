
using UnityEngine;

/// <summary>
/// 攻撃力デバフ：倍率で現在攻撃力を低下させる
/// 例：multiplier=0.7f → -30%
/// </summary>
public class AttackDebuffEffect : StatusEffectBase
{
	private readonly float multiplier;

	/// <param name="multiplier">攻撃力の倍率（0～1の範囲にクランプ）</param>
	/// <param name="duration">効果時間（秒）</param>
	/// <param name="icon">UI表示用アイコン（任意）</param>
	public AttackDebuffEffect(float multiplier, float duration, Sprite icon = null)
		: base("Attack Debuff", duration, icon)
	{
		// デバフなので [0,1] に制限
		this.multiplier = Mathf.Clamp(multiplier, 0f, 1f);
	}

	public override float ModifyAttackPower(float currentPower)
	{
		return currentPower * multiplier;
	}
}
