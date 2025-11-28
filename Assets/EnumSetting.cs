public enum SkillType
{
	Point,		//位置指定スキル
	Target,		//対象指定スキル
	Direction,	//方向指定スキル
	Self,		//自身指定スキル
}

public enum SkillCategory
{
	Attack,        // 攻撃系（直接ダメージ）
	Buff,          // 味方強化（攻撃力、防御力アップ）
	Debuff,        // 敵弱体化（攻撃力、防御力ダウン）
	Heal,          // 回復系（HP回復、状態異常解除）
	Summon,        // 召喚系（ペット、タレット）
	Utility,       // 移動や特殊効果（テレポート、ステルス）
	CrowdControl,  // 状態異常（スタン、スロー、ノックバック）
	AreaEffect     // 範囲攻撃や範囲効果（爆発、毒エリア）
}


public enum EffectKind
{
	DamageOverTime, // 継続ダメージ（毒など）
	HealOverTime,   // 継続回復（リジェネなど）
	Buff,           // 能力強化（攻撃力アップなど）
	Debuff          // 能力低下（防御力ダウンなど）
}

public enum StackMode
{
	None,                   // スタックしない（常に1個だけ）
	PerStackDecay,          // スタックごとに個別タイマーで減衰
	SingleTimerTotalStacks  // 全スタックをまとめて1つのタイマーで管理
}
public enum NonStackReapplyPolicy
{
	Ignore,          // 再付与を無視（何も変わらない）
	RefreshDuration, // 残り時間をリフレッシュ（再付与で延命）
	ExtendByBase     // 残り時間に基本値を加算（延長）
}

public enum ReferStat
{
	AttackPower, // 攻撃力を参照
	Defense,     // 防御力を参照
	MaxHP,       // 最大HPを参照
	MoveSpeed    // 移動速度を参照
}


