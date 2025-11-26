
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態異常の管理クラス：
/// - 状態異常の追加/削除/合算
/// - 効果時間の管理（UpdateでTick）
/// - 攻撃力などの計算メソッドを提供
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
	// アタッチ対象のStatus（攻撃力基本値などを参照したい時に使う）
	[SerializeField] private Status status;

	// 現在付与されている状態異常の一覧
	private readonly List<IStatusEffect> effects = new List<IStatusEffect>();

	// --- ライフサイクル ---
	private void Awake()
	{
		// 可能なら同一GameObject上のStatusを自動取得
		if (status == null)
		{
			status = GetComponent<Status>();
		}
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;

		// 全エフェクトにTickを回す（時間進行）
		for (int i = 0; i < effects.Count; i++)
		{
			effects[i].Tick(deltaTime);
		}

		// 期限切れのエフェクトを削除
		RemoveExpiredEffects();
	}

	// --- 追加・削除・参照 ---

	/// <summary>
	/// 状態異常を追加します。
	/// ルール：同種のスタック可/不可は各Effect側で定義すると拡張しやすいです。
	/// </summary>
	public void AddEffect(IStatusEffect effect)
	{
		if (effect == null) return;
		effects.Add(effect);
		// 将来的に「同種統合」「優先度比較」などをここで実施
	}

	/// <summary>
	/// 状態異常を削除します。
	/// </summary>
	public void RemoveEffect(IStatusEffect effect)
	{
		if (effect == null) return;
		effects.Remove(effect);
	}

	/// <summary>
	/// 現在アクティブな状態異常一覧を返します（UI表示向け）。
	/// 返却はコピー（外部で安全に列挙できるように）。
	/// </summary>
	public List<IStatusEffect> GetActiveEffects()
	{
		return new List<IStatusEffect>(effects);
	}

	/// <summary>
	/// 期限切れのエフェクトを一括削除
	/// </summary>
	private void RemoveExpiredEffects()
	{
		// 後方から削除するのが安全
		for (int i = effects.Count - 1; i >= 0; i--)
		{
			if (effects[i].IsExpired)
			{
				effects.RemoveAt(i);
			}
		}
	}

	// --- 計算メソッド（合算処理） ---

	/// <summary>
	/// 攻撃力の合算計算。
	/// basePower に対して、すべてのEffectの ModifyAttackPower を順次適用。
	/// </summary>
	public float CalculateAttackPower(float basePower)
	{
		float modified = basePower;
		for (int i = 0; i < effects.Count; i++)
		{
			modified = effects[i].ModifyAttackPower(modified);
		}
		return modified;
	}

	/// <summary>
	/// 速度など他ステータスにも同様の合算メソッドを拡張可能。
	/// 例：CalculateMoveSpeed(float baseSpeed)
	/// ※ SlowEffect 実装時に利用予定
	/// </summary>
	public float CalculateMoveSpeed(float baseSpeed)
	{
		float modified = baseSpeed;
		for (int i = 0; i < effects.Count; i++)
		{
			if (effects[i] is SlowEffect slow)
			{
				modified *= slow.SpeedMultiplier;
			}
		}
		return modified;
	}

}
