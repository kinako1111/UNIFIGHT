
/// <summary>
/// 状態異常の基底クラス
/// 共通処理（時間管理など）を提供
/// </summary>
public abstract class StatusEffectBase : IStatusEffect
{
	public string Name { get; protected set; } // 効果名
	public float Duration { get; protected set; } // 効果時間
	protected float elapsedTime; // 経過時間

	public StatusEffectBase(string name, float duration)
	{
		Name = name;
		Duration = duration;
		elapsedTime = 0f;
	}

	/// <summary>
	/// 毎フレーム呼ばれる処理（経過時間を加算）
	/// </summary>
	public abstract void Tick(Status target, float deltaTime);

	/// <summary>
	/// 効果が切れたかどうか
	/// </summary>
	public bool IsExpired => elapsedTime >= Duration;

	// デフォルトでは値を変更しない
	public virtual float ModifyAttackPower(float currentPower) => currentPower;
	public virtual int ModifyDamage(int currentDamage) => currentDamage;
	public virtual int GetAdditionalDamagePerSecond() => 0;
}
