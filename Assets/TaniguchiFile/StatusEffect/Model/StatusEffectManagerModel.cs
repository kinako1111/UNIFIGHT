
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態異常の辞書管理。Key単位でスタック増減。
/// </summary>
public class StatusEffectManagerModel : MonoBehaviour
{
	private readonly Dictionary<string, IStatusEffectModel> _effectsByKey = new();
	private Status _status;

	private void Awake()
	{
		if (_status == null) _status = GetComponent<Status>();
	}

	private void Update()
	{
		float dt = Time.deltaTime;
		// Tick & 期限切れ削除
		var toRemove = new List<string>();
		foreach (var kv in _effectsByKey)
		{
			kv.Value.Tick(dt);
			if (kv.Value.IsExpired)
				toRemove.Add(kv.Key);
		}
		foreach (var key in toRemove) _effectsByKey.Remove(key);
	}

	/// <summary>
	/// 新規効果を「同Keyならスタック増加」で扱う。新規Keyなら追加。
	/// </summary>
	public void AddOrStack(IStatusEffectModel effect, int stacksToAdd = 1, float? perStackDuration = null)
	{
		if (effect == null || string.IsNullOrEmpty(effect.Key)) return;

		if (_effectsByKey.TryGetValue(effect.Key, out var existing))
		{
			existing.AddStacks(stacksToAdd, perStackDuration);
		}
		else
		{
			_effectsByKey[effect.Key] = effect;
		}
	}

	/// <summary>同Keyの層数を減らす（浄化/解除）</summary>
	public void ReduceStacks(string key, int amount)
	{
		if (!_effectsByKey.TryGetValue(key, out var ef)) return;
		ef.RemoveStacks(amount);
		if (ef.IsExpired) _effectsByKey.Remove(key);
	}

	/// <summary>同Keyの効果を完全解除</summary>
	public void RemoveEffect(string key)
	{
		_effectsByKey.Remove(key);
	}

	/// <summary>全解除（死亡時など）</summary>
	public void ClearAll() => _effectsByKey.Clear();

	/// <summary>攻撃力合算（バフ/デバフのスタックを反映）</summary>
	public float CalculateAttackPower(float basePower)
	{
		float current = basePower;
		foreach (var kv in _effectsByKey)
			current = kv.Value.ModifyAttackPower(current);
		return Mathf.Max(0f, current);
	}

	/// <summary>移動速度合算（SlowStackEffectに対応）</summary>
	public float CalculateMoveSpeed(float baseSpeed)
	{
		float current = baseSpeed;
		foreach (var kv in _effectsByKey)
		{
			if (kv.Value is ISlowEffect slow)
				current = slow.ModifyMoveSpeed(current);
		}
		return Mathf.Max(0f, current);
	}

	// UIやデバッグ用
	public IReadOnlyDictionary<string, IStatusEffectModel> GetActiveEffects() => _effectsByKey;
}
