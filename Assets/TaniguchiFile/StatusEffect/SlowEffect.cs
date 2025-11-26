
using UnityEngine;

/// <summary>
/// 移動速度低下：倍率で速度を下げる
/// ※ 攻撃力には影響しない。速度合算は Manager 側で行う想定。
/// </summary>
public class SlowEffect : StatusEffectBase
{
	/// <summary>
	/// 速度倍率（例：0.8f → 20%低下）
	/// </summary>
	public float SpeedMultiplier { get; private set; }

	/// <param name="speedMultiplier">速度倍率（0～1推奨。0で停止、1で等速）</param>
	/// <param name="duration">効果時間（秒）</param>
	/// <param name="icon">UI表示用アイコン（任意）</param>
	public SlowEffect(float speedMultiplier, float duration, Sprite icon = null)
		: base("Slow", duration, icon)
	{
		// 速度は 0～1 を推奨（上限は状況によって>1も可）
		SpeedMultiplier = Mathf.Max(0f, speedMultiplier);
	}

	// 攻撃力には影響しない
	public override float ModifyAttackPower(float currentPower)
	{
		return currentPower;
	}

	// DoT等の処理なし、時間進行のみ（基底クラスに委譲）
	//public override void Tick(float deltaTime) { base.Tick(deltaTime); }
}
