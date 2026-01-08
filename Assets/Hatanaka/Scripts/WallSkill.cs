using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class WallSkill : MonoBehaviour,ISkill
{
	[SerializeField] GameObject m_prefab = null;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_skillName = "   ";
	[SerializeField] SkillType m_skillType;
	[SerializeField] GameObject m_skillUI;
	[Header("‰~Œ`‚Ì‚½‚ßA”’l‚Íˆê‚Â"), SerializeField] float m_skillRange;
	[SerializeField] float m_skillDistance;

	public string SkillName => m_skillName;
	public float CoolDownTime => m_cooldownTime;
	public SkillType SkillType => m_skillType;
	public GameObject SkillUI => m_skillUI;
	public float SkillRangeX => m_skillRange;
	public float SkillRangeZ => m_skillRange;
	public float SkillDistance => m_skillDistance;

	public void Execute(Vector3 position, Quaternion rotate, GameObject target)
	{
		if (m_prefab != null)
		{
			GameObject obj = Instantiate(m_prefab, position, this.gameObject.transform.rotation);
			Destroy(obj, 10f);
		}
	}
}	

