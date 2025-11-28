
public interface IStatusEffect
{
	// 識別用キー（同名扱いのためのキー）。例："Poison", "ATK_BUFF"
	string Key { get; }

	// 表示名（UI用）。例："毒", "攻撃力バフ"
	string DisplayName { get; }

	// スタック情報
	int Stacks { get; }
	int MaxStacks { get; }

	// 持続管理
	bool IsExpired { get; }
	float Remaining { get; }   // 残り時間（SharedDuration時）。PerStackDecayの場合は全体の最大残り時間などを返す。

	// 時間経過
	void Tick(float deltaTime);

	// スタック増減（newDurationは追加スタックの持続。Sharedなら全体リフレッシュ、PerStackなら各スタックの持続）
	void AddStacks(int amount, float? newDuration = null);
	void RemoveStacks(int amount);

	// 攻撃力への影響（バフ/デバフ）
	float ModifyAttackPower(float currentPower);
}
