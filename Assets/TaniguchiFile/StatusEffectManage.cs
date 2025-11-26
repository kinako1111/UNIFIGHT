
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態異常を管理するクラス
/// 効果の追加・削除・合算処理を担当
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
	private List<IStatusEffect> activeEffects = new List<IStatusEffect>();
	private Status target;

	private void Awake()
	{
		target = GetComponent<Status>();
	}

	/// <summary>
	/// 新しい状態異常を追加
	/// </summary>
	public void AddEffect(IStatusEffect effect)
	{
		activeEffects.Add(effect);
		Debug.Log("バフします");
	}

	/// <summary>
	/// 毎フレーム状態異常を更新
	/// </summary>
	public void UpdateEffects(float deltaTime)
	{
		for (int i = activeEffects.Count - 1; i >= 0; i--)
		{
			var effect = activeEffects[i];
			effect.Tick(target, deltaTime);

			// 効果時間が切れたら削除
			if ((effect as StatusEffectBase)?.IsExpired == true)
			{
				activeEffects.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// 攻撃力を全状態異常で修正
	/// </summary>
	public float CalculateAttackPower(float basePower)
	{
		float modifiedPower = basePower;
		foreach (var effect in activeEffects)
		{
			modifiedPower = effect.ModifyAttackPower(modifiedPower);
		}
		return modifiedPower;
	}

	/// <summary>
	/// ダメージを全状態異常で修正
	/// </summary>
	public int CalculateDamage(int baseDamage)
	{
		int modifiedDamage = baseDamage;
		foreach (var effect in activeEffects)
		{
			modifiedDamage = effect.ModifyDamage(modifiedDamage);
		}
		return modifiedDamage;
	}

	/// <summary>
	/// DOTの合計値を取得
	/// </summary>
	public int GetTotalDOT()
	{
		int totalDOT = 0;
		foreach (var effect in activeEffects)
		{
			totalDOT += effect.GetAdditionalDamagePerSecond();
		}
		return totalDOT;
	}

	public List<IStatusEffect> GetActiveEffects() => activeEffects;
}
