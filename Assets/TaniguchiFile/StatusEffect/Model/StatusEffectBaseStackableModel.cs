
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スタック対応の基底クラス。SharedDuration と PerStackDecay をサポート。
/// </summary>
public abstract class StatusEffectBaseStackableModel : IStatusEffectModel
{
	public string Key { get; private set; }
	public string DisplayName { get; private set; }

	protected Status owner;

	// スタック関連
	public int Stacks { get; protected set; }
	public int MaxStacks { get; protected set; }

	// 時間管理
	private readonly bool perStackDecay;   // true: 各スタックの個別タイマー, false: 共通タイマー
	protected float duration;              // SharedDuration用の総持続秒
	protected float elapsed;               // SharedDuration用の経過秒
	private readonly List<float> stackTimers = new(); // PerStackDecay: 各スタックの残り秒

	public bool IsExpired => Stacks <= 0;

	// SharedDurationの残り時間（PerStackDecayの場合は最大残り）
	public float Remaining
	{
		get
		{
			if (!perStackDecay) return Mathf.Max(0f, duration - elapsed);
			float max = 0f;
			foreach (var t in stackTimers) max = Mathf.Max(max, t);
			return max;
		}
	}

	protected StatusEffectBaseStackableModel(
		Status owner, string key, string displayName,
		int initialStacks, int maxStacks,
		bool perStackDecay, float initialDuration)
	{
		this.owner = owner;
		this.Key = key;
		this.DisplayName = displayName;
		this.perStackDecay = perStackDecay;
		this.MaxStacks = Mathf.Max(1, maxStacks);
		this.Stacks = Mathf.Clamp(initialStacks, 0, MaxStacks);

		if (perStackDecay)
		{
			// 初期スタック分のタイマーを登録
			for (int i = 0; i < Stacks; i++) stackTimers.Add(Mathf.Max(0.01f, initialDuration));
		}
		else
		{
			duration = Mathf.Max(0.01f, initialDuration);
			elapsed = 0f;
		}
	}

	public virtual void Tick(float deltaTime)
	{
		if (IsExpired) return;

		if (!perStackDecay)
		{
			elapsed += deltaTime;
			if (elapsed >= duration)
			{
				Stacks = 0; // 全消滅
			}
		}
		else
		{
			for (int i = stackTimers.Count - 1; i >= 0; i--)
			{
				stackTimers[i] -= deltaTime;
				if (stackTimers[i] <= 0f)
				{
					stackTimers.RemoveAt(i);
				}
			}
			Stacks = stackTimers.Count;
		}
	}

	// スタック追加
	public virtual void AddStacks(int amount, float? newDuration = null)
	{
		if (amount <= 0) return;

		int canAdd = Mathf.Min(amount, MaxStacks - Stacks);
		if (canAdd <= 0) return;

		if (!perStackDecay)
		{
			Stacks += canAdd;
			// SharedDuration：追加時のリフレッシュ
			if (newDuration.HasValue)
			{
				duration = Mathf.Max(duration, newDuration.Value);
				elapsed = 0f; // リフレッシュ方針：全体を更新
			}
		}
		else
		{
			float dur = Mathf.Max(0.01f, newDuration ?? Remaining);
			for (int i = 0; i < canAdd; i++)
				stackTimers.Add(dur);
			Stacks = stackTimers.Count;
		}

		OnStacksChanged();
	}

	// スタック減少（浄化・解除など）
	public virtual void RemoveStacks(int amount)
	{
		if (amount <= 0 || Stacks <= 0) return;

		if (!perStackDecay)
		{
			Stacks = Mathf.Max(0, Stacks - amount);
			if (Stacks == 0) elapsed = duration; // 失効
		}
		else
		{
			int remove = Mathf.Min(amount, stackTimers.Count);
			// 古いスタックから消す（UI的に自然）
			stackTimers.Sort(); // 小さい残り時間からでも、大きいからでも好みで。ここでは小→大
			for (int i = 0; i < remove; i++)
				stackTimers.RemoveAt(0);
			Stacks = stackTimers.Count;
		}

		OnStacksChanged();
	}

	// 派生で「スタック数が変わった時の再計算」を行う
	protected abstract void OnStacksChanged();

	// デフォルトでは攻撃力に影響しない
	public virtual float ModifyAttackPower(float currentPower) => currentPower;
}
