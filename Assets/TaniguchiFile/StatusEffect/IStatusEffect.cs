
/// <summary>
/// 状態異常の共通インターフェース。
/// 攻撃力などのステータス計算に参加し、毎フレームTickで時間・挙動を進行する。
/// </summary>
public interface IStatusEffect
{
	/// <summary>
	/// 攻撃力に対する修正（合算時は直列適用）
	/// 入力：現在の攻撃力（他Effect適用後の値）／戻り：修正後の攻撃力
	/// </summary>
	float ModifyAttackPower(float currentPower);

	/// <summary>
	/// フレームの経過時間を受け取り、状態異常の時間進行・DoT処理などを行う
	/// </summary>
	void Tick(float deltaTime);

	/// <summary>
	/// 効果時間が切れているかどうか
	/// </summary>
	bool IsExpired { get; }

	/// <summary>
	/// UI表示等で使う識別用の名前
	/// </summary>
	string Name { get; }

	/// <summary>
	/// UI表示用のアイコン（必要に応じて利用）
	/// null許容：アイコン未指定
	/// </summary>
	UnityEngine.Sprite Icon { get; }
}
