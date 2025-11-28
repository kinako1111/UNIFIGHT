using UnityEngine;

public class PinUse : MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_prefab = null;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_pinName = "□□□";
	[SerializeField] SkillType m_pinType;
	[SerializeField] GameObject m_pinUI;
	[Header("円形のため、数値は一つ"), SerializeField] float m_pinRange;
	[SerializeField] float m_pinDistance;

	public string SkillName => m_pinName;
	public float CoolDownTime => m_cooldownTime;
	public SkillType SkillType => m_pinType;
	public GameObject SkillUI => m_pinUI;
	public float SkillRangeX => m_pinRange;
	public float SkillRangeZ => m_pinRange;
	public float SkillDistance => m_pinDistance;

	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		Debug.Log("エリアスキル");
		if (m_prefab != null)
		{
			GameObject obj = Instantiate(m_prefab, position, rotation);
			Destroy(obj, 10f);
			Debug.Log(transform.position);
			Debug.Log(position);
		}
	}
}
