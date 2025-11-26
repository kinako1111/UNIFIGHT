using UnityEngine;

public class AttackBuff : MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_prefab = null;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_skillName = "   ";
	[SerializeField] SkillType m_skillType;
	[SerializeField] GameObject m_skillUI;
	[Header("‰~Œ`‚Ì‚½‚ßA”’l‚Íˆê‚Â"), SerializeField] float m_skillRange;
	[SerializeField] float m_skillDistance;
	[SerializeField] float m_buffRate;
	[SerializeField] float m_buffTime;

	public string SkillName => m_skillName;
	public float CoolDownTime => m_cooldownTime;
	public SkillType SkillType => m_skillType;
	public GameObject SkillUI => m_skillUI;
	public float SkillRangeX => m_skillRange;
	public float SkillRangeZ => m_skillRange;
	public float SkillDistance => m_skillDistance;

	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		var status = target.GetComponent<Status>();
		var manager = target.GetComponent<StatusEffectManager>();

		// UŒ‚—Í +20% ‚ğ 5•b
		manager.AddEffect(new AttackBuffEffect(m_buffRate,m_buffTime));
		GameObject buffPrefab = Instantiate(m_prefab,transform);
		Destroy(buffPrefab, m_buffTime);
	}
}