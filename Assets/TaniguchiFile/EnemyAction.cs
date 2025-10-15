using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAction: MonoBehaviour
{
	[SerializeField]
	Status status;

	[Header("Navmesh"), SerializeField]
	NavMeshAgent m_navmeshAgent;

	[Header("エフェクトのリスト"),SerializeField]
	List<GameObject> m_effectList = new();

	[Header("攻撃地点のTransform"), SerializeField]
	List<Transform> m_attackPos = new();

	[Header("サウンドのリスト"),SerializeField]
	List<GameObject> m_soundList = new();

	[Header("敵の速度"), SerializeField]
	float m_speed;

	[Header("攻撃範囲"), SerializeField]
	float AttackRange;

	[Header("ノックバックするダメージ"), SerializeField]
	int KnockBackDamage;

	//防衛対象のリスト
	List<GameObject> m_targetList = new();

	//プレイヤーのリスト
	[SerializeField]
	List<GameObject> m_playerList = new();

	[Header("攻撃のクールタイム"), SerializeField]
	float m_attackCoolTime;

	Animator m_animator;
	float m_coolTime;
	int m_knockBackCount;
	bool isMove;
	bool isAttack = false;
	bool m_damageAction = false;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
		m_playerList.AddRange(GameObject.FindGameObjectsWithTag("Player"));
		m_targetList.AddRange(GameObject.FindGameObjectsWithTag("Target"));
		m_targetList.AddRange(GameObject.FindGameObjectsWithTag("Player"));
	}

	private void Start()
	{
		m_knockBackCount = KnockBackDamage;
	}

	private void FixedUpdate()
	{
		m_navmeshAgent.speed = status.GetSpeed();
		m_attackCoolTime = status.GetSkill1CoolTime();

		//現在最も近いターゲットを取得
		GameObject clossTarget = m_targetList.OrderBy(target => Vector3.Distance(target.transform.position, transform.position)).First();

		//攻撃範囲内にプレイヤーがいる場合
		if ((clossTarget.transform.position - transform.position).magnitude <= AttackRange)
		{
			isMove = false;
			m_navmeshAgent.isStopped = true;

			//攻撃中は向きも変わらない
			if(!isAttack)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(clossTarget.transform.position - transform.position),0.2f);
			}

			////対象物を最もヘイト値の高いプレイヤーに変更
			////ヘイト値　：　攻撃範囲-プレイヤーの距離＋個々の値
			////攻撃範囲内のプレイヤー取得　ー＞　ヘイト値計算

			//List<GameObject> playerList = m_playerList.FindAll(target => Vector3.Distance(target.transform.position ,transform.position) > m_navmeshAgent.stoppingDistance);

			//クールタイム終了と同時に攻撃
			m_coolTime -= Time.deltaTime;
			if (m_coolTime < 0)
			{
				OnAttack();
			}
		}
		else
		{
			isMove = true;

			//攻撃中またはダメージアクション中は移動禁止
			if(!isAttack || !m_damageAction)
			{
				m_navmeshAgent.isStopped = false;
				m_navmeshAgent.SetDestination(m_targetList.Find(target => target.tag == "Target").transform.position);
			}
		}
		m_animator.SetBool("Walk", isMove);
	}

	void OnAttack()
	{
		isAttack = true;
		m_animator.SetTrigger("Attack");
		m_coolTime = m_attackCoolTime;
	}

	public void AttackHit()
	{
		Debug.Log("Golem attack started!");
		// エフェクト生成やSE再生などの処理
		GameObject obj = Instantiate(m_effectList.First(), m_attackPos.First());
	}

	public void AttackEnd()
	{
		Debug.Log("Golem attack end!");
		isAttack = false;
	}

	public void SetTarget(GameObject target)
	{
		m_targetList.Add(target);
	}

	public void Damagefirst()
	{
		m_damageAction = true;
	}

	public void DamageEnd()
	{
		m_damageAction = false;
		m_knockBackCount = KnockBackDamage;
	}

}
