using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
	[Header("Navmesh"), SerializeField]
	NavMeshAgent m_navmeshAgent;

	[Header("–h‰q‘ÎÛ"), SerializeField]
	GameObject m_target;
	
	[SerializeField]
	Animator m_animator;

	float m_attackCoolTime;
	bool isMove;

	private void Start()
	{
		if(m_target == null)
		{
			m_target = GameObject.FindGameObjectWithTag("Target");
		}
	}

	private void FixedUpdate()
	{
		//Navmesh‚ÌˆÚ“®
		m_navmeshAgent.SetDestination(m_target.transform.position);

		//ˆÚ“®’†‚È‚ç‚ÎˆÚ“®ƒAƒjƒ[ƒVƒ‡ƒ“Ä¶
		if ((m_target.transform.position - transform.position).magnitude > 2.2f)
		{
			m_animator.SetBool("Walk", true);
			isMove = true;
		}
		else
		{
			m_animator.SetBool("Walk", false);
			isMove = false;
		}

		if(!isMove)
		{
			m_attackCoolTime += Time.deltaTime;
			if(m_attackCoolTime >= 4)
			{
				OnAttack();
			}
		}

	}


	void OnAttack()
	{
		m_animator.SetTrigger("Attack");
		m_attackCoolTime = 0;
	}
}
