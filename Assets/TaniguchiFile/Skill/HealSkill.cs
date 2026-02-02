using UnityEngine;

public class HealSkill : MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_prefab = null;
	[SerializeField] GameObject m_skillEffect = null;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_skillName = "回復ポーション";
	[SerializeField] SkillType m_skillType = SkillType.Self;
	[SerializeField] float m_healMagnification = 0.25f;
	[SerializeField] GameObject m_skillUI;
	[SerializeField] float m_skillRangeX = 0;
	[SerializeField] float m_skillRangeZ = 0;
	[SerializeField] float m_skillDistance = 0;

	Status m_status;
	PlayerController m_playerController;
	AutoAttack m_autoAttack;

	public string SkillName => m_skillName;     //スキルの名前
	public float CoolDownTime => m_cooldownTime;//スキルのクールダウン
	public SkillType SkillType => m_skillType;	//スキルのタイプ（範囲、対象の指定方法）
	public GameObject SkillUI => m_skillUI;		//スキルのUI 
	public float SkillRangeX => m_skillRangeX;	//スキル範囲の幅
	public float SkillRangeZ => m_skillRangeZ;	//スキル範囲の長さ
	public float SkillDistance => m_skillDistance;　//スキル発動場所までの距離

	private void Start()
	{
		m_status = GetComponent<Status>();
		m_playerController = GetComponent<PlayerController>();
		m_autoAttack = GetComponent<AutoAttack>();
	}

	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		//攻撃モーションに割り込むのは禁止
		if (m_autoAttack.IsAttack) return;

		//PlayerControllerの移動許可をとり下げる
		m_playerController.MoveApproval(false);

		//ポーションを表示
		m_prefab.SetActive(true);

		//回復アニメーションを表示
		Animator animator;
		if(target.TryGetComponent(out animator))
		{
			animator.SetTrigger("Heal");
			Debug.Log("回復アニメーション始動");
		}
	}

	public void ExecutionHeal()
	{
		//回復の実行
		m_status.Heal(Mathf.RoundToInt(m_status.GetAttackPower() * m_healMagnification));

		//移動の許可
		m_playerController.MoveApproval(true);

		//ポーションを隠す
		m_prefab.SetActive(false);
	}
}
