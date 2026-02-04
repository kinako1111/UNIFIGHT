
using UnityEngine;

public class AttackBuff : MonoBehaviour, ISkill
{
	[SerializeField] private GameObject m_prefab = null;
	[SerializeField] private float m_cooldownTime = 5f;
	[SerializeField] private string m_skillName = "□□□";
	[SerializeField] private SkillType m_skillType;
	[SerializeField] private GameObject m_skillUI;
	[SerializeField] AudioClip m_seClip;

	[Header("円形のため、数値は一つ"), SerializeField]
	private float m_skillRange;

	[SerializeField] private float m_skillDistance;

	[Header("1層あたりの上昇率（例：0.20 → +20%/層）"), SerializeField]
	private float m_buffRate = 0.20f;

	[Header("持続時間（秒）"), SerializeField]
	private float m_buffTime = 5f;

	// まずは固定の最大スタック数（必要なら SerializeField にして調整）
	private const int DEFAULT_MAX_STACKS = 5;

	AudioSource m_audioSource;

	public string SkillName => m_skillName;
	public float CoolDownTime => m_cooldownTime;
	public SkillType SkillType => m_skillType;
	public GameObject SkillUI => m_skillUI;
	public float SkillRangeX => m_skillRange;
	public float SkillRangeZ => m_skillRange;
	public float SkillDistance => m_skillDistance;
	void Awake()
	{
		m_audioSource = GetComponent<AudioSource>();
	}

	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		if (target == null) return;

		// 対象ユニットの Status / Manager を取得
		var status = target.GetComponent<Status>();
		if (status == null || status.GetDeath()) return;

		var manager = target.GetComponent<StatusEffectManagerModel>();
		if (manager == null)
		{
			manager = target.AddComponent<StatusEffectManagerModel>();
		}

		// 攻撃力 +m_buffRate を m_buffTime 秒（スタック対応）
		// 共有タイマー運用：同キー "ATK_BUFF" が既にあればスタック+1＆残り時間をリフレッシュ
		var effect = new AttackBuffStackEffect(
			owner: status,
			initialStacks: 1,
			maxStacks: DEFAULT_MAX_STACKS,
			perStackAdd: Mathf.Max(0f, m_buffRate),
			perStackDecay: false,                      // バフは共有タイマーが一般的
			durationSeconds: Mathf.Max(0.1f, m_buffTime)
		);

		// 同名キーならスタック+1、共有タイマーの残り時間をリフレッシュ
		manager.AddOrStack(
			effect,
			stacksToAdd: 1,
			perStackDuration: m_buffTime
		);

		// 演出（対象の位置・回転で生成、m_buffTime後に消去）
		if (m_prefab != null)
		{
			var fx = Instantiate(m_prefab, target.transform.position, rotation, target.transform);
			Destroy(fx, m_buffTime);
		}

		PlaySE();
	}
	void PlaySE()
	{
		m_audioSource.PlayOneShot(m_seClip);
	}
}
