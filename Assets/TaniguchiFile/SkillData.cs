using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Game/SkillData")]
public class SkillData : ScriptableObject
{
	[Header("スキル名"),SerializeField] private string skillName;
	[Header("使用する物（無ければnullでよし）"),SerializeField] private GameObject skillPrefab;
	[Header("スキルの基準(位置基準だとか方向指定とか)"),SerializeField] private SkillType skillType;
	[Header("スキルの種類"),SerializeField] private SkillCategory skillCategory;
	[Header("スキルの範囲"),SerializeField] private float skillRange;
	[Header("スキルUIの感度（今後実装予定）"),SerializeField] private float skillSensitivity;
	[Header("スキルのクールダウン"),SerializeField] private float cooldownTime;

	public string SkillName => skillName;
	public GameObject SkillPrefab => skillPrefab;
	public SkillType SkillType => skillType;
	public SkillCategory SkillCategory => skillCategory;
	public float SkillRange => skillRange;
	public float SkillSensitivity => skillSensitivity;
	public float CooldownTime => cooldownTime;
}

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

