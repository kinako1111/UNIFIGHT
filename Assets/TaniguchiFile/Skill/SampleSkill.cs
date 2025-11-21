using UnityEngine;

public class SampleSkill : MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_prefab = null;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_skillName = "□□□";
	[SerializeField] SkillType m_skillType;
	[SerializeField] GameObject m_skillUI;
	[SerializeField] float m_skillRange;
	[SerializeField] float m_skillDistance;


	public string SkillName => m_skillName;
	public float CoolDownTime => m_cooldownTime;
	public SkillType SkillType => m_skillType;
	public GameObject SkillUI => m_skillUI;
	public float SkillRange => m_skillRange;
	public float SkillDistance => m_skillDistance;

	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		//実装したいスキルの処理を記述
		if (m_prefab != null)
		{
			GameObject obj = Instantiate(m_prefab, position, rotation);
			Destroy(obj, 10f);
		}
	}
}
