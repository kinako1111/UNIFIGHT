using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	//防衛対象のリスト
	List<GameObject> m_targetList = new();

	//プレイヤーのリスト
	[SerializeField]
	List<GameObject> m_playerList = new();

	[Header("攻撃のクールタイム"), SerializeField]
	float m_attackCoolTime;

	Animator m_animator;
	float m_coolTime;
	bool isMove;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();

		m_playerList.AddRange(GameObject.FindGameObjectsWithTag("Player"));
		m_targetList.AddRange(GameObject.FindGameObjectsWithTag("Target"));
	}

	private void FixedUpdate()
	{
		m_navmeshAgent.speed = status.GetSpeed();
		m_attackCoolTime = status.GetSkill1CoolTime();

		//現在最も近いプレイヤーを取得
		GameObject clossPlayer = m_playerList.OrderBy(target => Vector3.Distance(target.transform.position, transform.position)).First();

		//攻撃範囲内にプレイヤーがいる場合
		if ((clossPlayer.transform.position - transform.position).magnitude <= m_navmeshAgent.stoppingDistance
			||(m_targetList.First().transform.position - transform.position).magnitude <= m_navmeshAgent.stoppingDistance)
		{
			isMove = false;

			//対象物を最もヘイト値の高いプレイヤーに変更
			//ヘイト値　：　攻撃範囲-プレイヤーの距離＋個々の値
			//攻撃範囲内のプレイヤー取得　ー＞　ヘイト値計算

			List<GameObject> playerList = m_playerList.FindAll(target => Vector3.Distance(target.transform.position ,transform.position) > m_navmeshAgent.stoppingDistance);
				


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
			m_navmeshAgent.SetDestination(m_targetList.First().transform.position);
		}
		m_animator.SetBool("Walk", isMove);
	}

	void OnAttack()
	{
		m_animator.SetTrigger("Attack");
		m_coolTime = m_attackCoolTime;
	}



	public void AttackStart()
	{
		Debug.Log("Golem attack started!");
		// エフェクト生成やSE再生などの処理

		GameObject obj = Instantiate(m_effectList.First(), m_attackPos.First());
		Debug.Log(obj);
	}


	public void AttackEnd()
	{

	}



	public void SetTarget(GameObject target)
	{
		m_targetList.Add(target);
	}
}
