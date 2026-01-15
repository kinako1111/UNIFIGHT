using UnityEngine;

public class Pierceshot : MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_prefab = null;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_skillName = "□□□";
	[SerializeField] SkillType m_skillType;
	[SerializeField] GameObject m_skillUI;
	[SerializeField] float m_skillRangeX;
	[SerializeField] float m_skillRangeZ;
	[SerializeField] float m_skillDistance;


	public string SkillName => m_skillName;     //スキルの名前
	public float CoolDownTime => m_cooldownTime;//スキルのクールダウン
	public SkillType SkillType => m_skillType;  //スキルのタイプ（範囲、対象の指定方法）
	public GameObject SkillUI => m_skillUI;     //スキルのUI 
	public float SkillRangeX => m_skillRangeX;  //スキル範囲の幅
	public float SkillRangeZ => m_skillRangeZ;  //スキル範囲の長さ
	public float SkillDistance => m_skillDistance; //スキル発動場所までの距離

	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		//実装したいスキルの処理を記述
		//高火力の範囲貫通攻撃
		
	}
}
