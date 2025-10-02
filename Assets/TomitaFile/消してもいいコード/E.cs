using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class E : MonoBehaviour
{
	private enum TargetType
	{
		Tower,
		Player,
	}

	[Header("Navmesh"), SerializeField]
	NavMeshAgent m_navmeshAgent;

	[Header("–h‰q‘ÎÛ"), SerializeField]
	GameObject[] m_target;

	[SerializeField]
	Animator m_animator;

	[SerializeField]
	float m_stopingDistance;

	float m_attackCoolTime;
	bool isMove;

	private void Start()
	{
		//if (m_target == null)
		//{
		//	m_target = GameObject.FindGameObjectWithTag("Target");
		//}
	}

	private void FixedUpdate()
	{
		Debug.Log(m_target[((int)TargetType.Tower)].transform.position - transform.position);

		if ((m_target[((int)TargetType.Player)].transform.position - transform.position).magnitude >= m_stopingDistance || 
			(m_target[((int)TargetType.Tower)].transform.position - transform.position).magnitude >= m_stopingDistance)
		{
			isMove = true;
		}
		else
		{
			isMove = false;
		}

		// “G‚©‚çŒ©‚Ätower‚Ì•û‚ªPlayer‚æ‚è‚à‰“‚©‚Á‚½‚çPlyer‚ğ’ÇÕ‚·‚é
		if ((m_target[((int)TargetType.Tower)].transform.position - transform.position).magnitude >=
			(m_target[((int)TargetType.Player)].transform.position - transform.position).magnitude)
		{
			//Navmesh‚ÌˆÚ“®
			m_navmeshAgent.SetDestination(m_target[((int)TargetType.Player)].transform.position);
		}
		// “G‚©‚çŒ©‚Ätower‚Ì•û‚ªPlayer‚æ‚è‚à‹ß‚¢‚©‚Â“G‚ÆPlayer‚Æ‚Ì‹——£‚ª10–¢–‚¾‚Á‚½‚çtower‚ğ’ÇÕ‚·‚é
		else if ((m_target[((int)TargetType.Tower)].transform.position - transform.position).magnitude <=
				(m_target[((int)TargetType.Player)].transform.position - transform.position).magnitude ||
				(m_target[((int)TargetType.Player)].transform.position - transform.position).magnitude <= 10)
		{
			//Navmesh‚ÌˆÚ“®
			m_navmeshAgent.SetDestination(m_target[((int)TargetType.Tower)].transform.position);
		}
		//else if ((m_target[((int)TargetType.Tower)].transform.position - transform.position).magnitude <= m_stopingDistance ||
		//		(m_target[((int)TargetType.Player)].transform.position - transform.position).magnitude <= m_stopingDistance)
		//{
		//	isMove = false;
		//	Debug.Log("a");
		//}

		if (!isMove)
		{
			m_attackCoolTime += Time.deltaTime;
			if (m_attackCoolTime >= 4)
			{
				OnAttack();
			}
		}

		m_animator.SetBool("Walk", isMove);
	}

	void OnAttack()
	{
		m_animator.SetTrigger("Attack");
		m_attackCoolTime = 0;
	}
}
