using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAction: MonoBehaviour
{
	[Header("Navmesh"), SerializeField]
	NavMeshAgent m_navmeshAgent;

	//防衛対象のリスト
	List<GameObject> m_targetList = new();

	//プレイヤーのリスト
	List<GameObject> m_playerList = new();

	[Header("攻撃のクールタイム"), SerializeField]
	float m_attackCoolTime;

	Animator m_animator;
	float m_coolTime;
	bool isMove;

	private void Start()
	{
		m_animator = GetComponent<Animator>();

		//攻撃対象を持ってなければタグ参照
		if (m_targetList == null)
		{
			m_targetList.AddRange(GameObject.FindGameObjectsWithTag("Target"));
		}
	}

	private void FixedUpdate()
	{
		//現在最も近いプレイヤーを取得
		GameObject clossPlayer = m_playerList.OrderBy(target => Vector3.Distance(target.transform.position, transform.position)).First();

		//攻撃範囲内にプレイヤーがいる場合
		if ((clossPlayer.transform.position - transform.position).magnitude <= m_navmeshAgent.stoppingDistance)
		{
			isMove = false;

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

	public void SetTarget(GameObject target)
	{
		m_targetList.Add(target);
	}
}
