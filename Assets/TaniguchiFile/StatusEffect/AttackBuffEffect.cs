
using UnityEngine;

/// <summary>
/// 攻撃力バフ：倍率で現在攻撃力を増加させる
/// 例：multiplier=1.2f → +20%
/// </summary>
public class AttackBuffEffect : StatusEffectBase
{
	private readonly float multiplier;

	/// <param name="multiplier">攻撃力の倍率（1.0fで等倍、1.2fで+20%など）</param>
	/// <param name="duration">効果時間（秒）</param>
	/// <param name="icon">UI表示用アイコン（任意）</param>
	public AttackBuffEffect(float multiplier, float duration, Sprite icon = null)
		: base("Attack Buff", duration, icon)
	{
		// 0未満は無意味なので下限0
		this.multiplier = Mathf.Max(0f, multiplier);
	}

	public override float ModifyAttackPower(float currentPower)
	{
		return currentPower * multiplier;
	}
}
