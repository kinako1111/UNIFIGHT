using UnityEngine;

public class RocketLauncher: MonoBehaviour, ISkill
{
	[SerializeField] GameObject m_shotGunPrefab = null;
	[SerializeField] GameObject m_LauncherPrefab = null;
	[SerializeField] GameObject m_bulletPrefab = null;
	[SerializeField] Transform m_generateBullet = null;
	[SerializeField] float m_cooldownTime = 5f;
	[SerializeField] string m_skillName = "□□□";
	[SerializeField] SkillType m_skillType;
	[SerializeField] GameObject m_skillUI;
	[SerializeField] float m_skillRangeX;
	[SerializeField] float m_skillRangeZ;
	[SerializeField] float m_skillDistance;
	[SerializeField] AudioClip m_seClip;

	Status m_status;
	Animator m_animator;
	AutoAttack m_autoAttack;
	PlayerController m_playerController;
	AudioSource m_audioSource;

	Vector3 m_impactPoint;

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

	private void Start()
	{
		m_animator = GetComponent<Animator>();
		m_playerController = GetComponent<PlayerController>();
		m_autoAttack = GetComponent<AutoAttack>();
		m_status = GetComponent<Status>();
	}
	
	public void Execute(Vector3 position, Quaternion rotation, GameObject target)
	{
		//実装したいスキルの処理を記述

		//攻撃モーションに割り込むのは禁止
		if (m_autoAttack.IsAttack || m_status.GetDeath()) return;

		//PlayerControllerの移動許可をとり下げる
		m_playerController.MoveApproval(false);

		//ショットガンのプレファブをオフ
		m_shotGunPrefab.SetActive(false);	

		//ランチャーをオン
		m_LauncherPrefab.SetActive(true);

		//アニメーションの再生
		m_animator.SetTrigger("Skill");

		//目標地点を記録
		m_impactPoint = position;

		PlaySE();
	}

	public void Fire()
	{
		//弾を生成
		GameObject m_bullet = Instantiate(m_bulletPrefab,m_generateBullet.position,Quaternion.identity);

		//弾の初期値を設定
		m_bullet.GetComponent<BulletHoming>().SetDestination(m_impactPoint);

	}

	public void Completion()
	{
		//ショットガンのプレファブをオン
		m_shotGunPrefab.SetActive(true);

		//ランチャーをオフ
		m_LauncherPrefab.SetActive(false);

		//移動不可の取り下げ
		m_playerController.MoveApproval(true);
	}
	void PlaySE()
	{
		m_audioSource.PlayOneShot(m_seClip);
	}
}
