public enum SkillType
{
	Point,		//位置指定スキル
	Target,		//対象指定スキル
	Direction	//方向指定スキル
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

