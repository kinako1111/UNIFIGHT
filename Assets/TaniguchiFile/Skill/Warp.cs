using UnityEngine;

public class Warp : MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_prefab;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_skillName = "WarpShot";
	[SerializeField] SkillType m_skillType;
	[SerializeField] GameObject m_skillUI;
	[SerializeField] float m_skillRangeX;
	[SerializeField] float m_skillRangeZ;
	[SerializeField] float m_skillDistance;
	[SerializeField] AudioClip m_seClip;

	//攻撃バフに関しての値

	[Header("1層あたりの上昇率（例：0.20 → +20%/層）"), SerializeField]
	private float m_buffRate = 2f;

	[Header("持続時間（秒）"), SerializeField]
	private float m_buffTime = 1f;

	// まずは固定の最大スタック数（必要なら SerializeField にして調整）
	private const int DEFAULT_MAX_STACKS = 1;

	Status m_status;
	AudioSource m_audioSource;
	Animator m_animator;
	AutoAttack m_autoAttack;
	PlayerController m_playerController;

	Vector3 m_warpPos;

	public string SkillName => m_skillName;     //スキルの名前
	public float CoolDownTime => m_cooldownTime;//スキルのクールダウン
	public SkillType SkillType => m_skillType;	//スキルのタイプ（範囲、対象の指定方法）
	public GameObject SkillUI => m_skillUI;		//スキルのUI 
	public float SkillRangeX => m_skillRangeX;	//スキル範囲の幅
	public float SkillRangeZ => m_skillRangeZ;	//スキル範囲の長さ
	public float SkillDistance => m_skillDistance; //スキル発動場所までの距離
	void Awake()
	{
		m_audioSource = GetComponent<AudioSource>();
	}


	void Start()
	{
		m_animator = GetComponent<Animator>();
		m_playerController = GetComponent<PlayerController>();
		m_autoAttack = GetComponent<AutoAttack>();
		m_status = GetComponent<Status>();
	}

	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		//攻撃に割り込むのは禁止
		if (m_autoAttack.IsAttack) return;

		//死んでたら動かない
		if (m_status.GetDeath()) return;
		
		//ワープアニメーションスタート
		m_animator.SetTrigger("Warp");

		//PlayerControllerの移動許可をとり下げる
		m_playerController.MoveApproval(false);

		//エフェクトの出現
		Instantiate(m_prefab,position,rotation);

		//ワープ後のポジションを保持
		m_warpPos = position;

		//ダメージをロック
		m_status.SetDamageApproval(false);
	}

	public void ExecutionWarp()
	{
		//ワープの実行
		transform.position = m_warpPos;

		//移動の許可
		m_playerController.MoveApproval(true);

		//ダメージロックを解除
		m_status.SetDamageApproval(true);

		//ワープ後、ごく短い時間だけ攻撃力大アップ
		// 攻撃力 +m_buffRate を m_buffTime 秒（スタック対応）
		// 共有タイマー運用：同キー "ATK_BUFF" が既にあればスタック+1＆残り時間をリフレッシュ
		var effect = new AttackBuffStackEffect(
			owner: m_status,
			initialStacks: 1,
			maxStacks: DEFAULT_MAX_STACKS,
			perStackAdd: Mathf.Max(0f, m_buffRate),
			perStackDecay: false,                      // バフは共有タイマーが一般的
			durationSeconds: Mathf.Max(0.1f, m_buffTime)
		);

		PlaySE();
	}
	void PlaySE()
	{
		m_audioSource.PlayOneShot(m_seClip);
	}
}
