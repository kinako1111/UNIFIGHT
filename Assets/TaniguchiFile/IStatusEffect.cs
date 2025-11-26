
/// <summary>
/// 状態異常の共通インターフェース
/// 各効果はこの契約に従って実装される
/// </summary>
public interface IStatusEffect
{
	string Name { get; } // 状態異常の名前
	float Duration { get; } // 効果時間

	/// <summary>
	/// 毎フレーム呼ばれる処理（時間経過やDOTなど）
	/// </summary>
	void Tick(Status target, float deltaTime);

	/// <summary>
	/// 攻撃力を修正する（バフ・デバフ対応）
	/// </summary>
	float ModifyAttackPower(float currentPower);

	/// <summary>
	/// ダメージを修正する（防御バフ・デバフ対応）
	/// </summary>
	int ModifyDamage(int currentDamage);

	/// <summary>
	/// 継続ダメージ（DOT）を返す
	/// </summary>
	int GetAdditionalDamagePerSecond();
}
