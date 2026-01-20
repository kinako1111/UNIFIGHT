
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態異常の辞書管理。Key単位でスタック増減。
/// </summary>
public class StatusEffectManagerModel : MonoBehaviour
{
	private readonly Dictionary<string, IStatusEffectModel> m_effectsByKey = new();
	private readonly List<string> pendingRemove = new();
	private Status m_status;

	private void Awake()
	{
		m_status = GetComponent<Status>();
	}



	private void Update()
	{
		float dt = Time.deltaTime;

		// ★ コピーを作って回す（元の Dictionary を直接回さない）
		foreach (var kv in new List<KeyValuePair<string, IStatusEffectModel>>(m_effectsByKey))
		{
			kv.Value.Tick(dt);

			if (kv.Value.IsExpired)
				pendingRemove.Add(kv.Key);
		}

		// ★ Tick が全部終わってからまとめて Remove
		foreach (var key in pendingRemove)
			m_effectsByKey.Remove(key);

		pendingRemove.Clear();
	}


	/// <summary>
	/// 新規効果を「同Keyならスタック増加」で扱う。新規Keyなら追加。
	/// </summary>
	public void AddOrStack(IStatusEffectModel effect, int stacksToAdd = 1, float? perStackDuration = null)
	{
		if (string.IsNullOrEmpty(effect.Key)) return;

		if (m_effectsByKey.TryGetValue(effect.Key, out var existing))
		{
			existing.AddStacks(stacksToAdd, perStackDuration);
		}
		else
		{
			m_effectsByKey[effect.Key] = effect;
		}
	}

	/// <summary>同Keyの層数を減らす（浄化/解除）</summary>
	public void ReduceStacks(string key, int amount)
	{
		if (!m_effectsByKey.TryGetValue(key, out var ef)) return;
		ef.RemoveStacks(amount);
		if (ef.IsExpired) m_effectsByKey.Remove(key);
	}

	/// <summary>同Keyの効果を完全解除</summary>

	public void RemoveEffect(string key)
	{
		if (m_effectsByKey.ContainsKey(key))
			pendingRemove.Add(key);
	}


	/// <summary>全解除（死亡時など）</summary>
	public void ClearAll() => m_effectsByKey.Clear();

	/// <summary>攻撃力合算（バフ/デバフのスタックを反映）</summary>
	public float CalculateAttackPower(float basePower)
	{
		float current = basePower;
		foreach (var kv in m_effectsByKey)
			current = kv.Value.ModifyAttackPower(current);
		return Mathf.Max(0f, current);
	}

	/// <summary>移動速度合算（SlowStackEffectに対応）</summary>
	public float CalculateMoveSpeed(float baseSpeed)
	{
		float current = baseSpeed;
		foreach (var kv in m_effectsByKey)
		{
			if (kv.Value is ISlowEffect slow)current = slow.ModifyMoveSpeed(current);
		}
		return Mathf.Max(0f, current);
	}

	// UIやデバッグ用
	public IReadOnlyDictionary<string, IStatusEffectModel> GetActiveEffects() => m_effectsByKey;
}
