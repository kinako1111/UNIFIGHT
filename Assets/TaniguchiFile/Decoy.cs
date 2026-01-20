using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Decoy : MonoBehaviour
{
	[Header("注目範囲"), SerializeField]
	Collider m_attentionCollider;

	[Header("消えるまでの時間"), SerializeField]
	float m_deathTimer;

	[Header("破壊時のエフェクト"), SerializeField]
	GameObject m_effect;

	[Header("破壊時の効果音"), SerializeField]
	AudioClip m_se;

	Collider[] m_targetCollider;

	Status m_status;

	private void Awake()
	{
		m_status = GetComponent<Status>();
	}

	private void Start()
	{
		//m_targetCollider = Physics.OverlapSphere(transform.positio);
		//foreach(Collider targetCollider in m_targetCollider)
		//{
		//	GameObject target = targetCollider.gameObject;
		//	EnemyAction enemyaction;
		//	if (target.TryGetComponent(out enemyaction))
		//	{
		//		enemyaction.ChangeHate(0, gameObject);
		//		Debug.Log("ヘイト変更");
		//	}
		//}
		Destroy(gameObject,m_deathTimer);
	}

	private void OnTriggerEnter(Collider other)
	{
		//Enemeyタグがなければ対象外
		if (!other.CompareTag("Enemy")) return;
		
	}

	private void OnTriggerExit(Collider other)
	{
		
	}

	private void OnDestroy()
	{

		if(m_effect != null)
		{
			Instantiate(m_effect,gameObject.transform.position,Quaternion.identity);
		}

		if(m_se != null)
		{
			SoundEffect.Play3D(m_se, gameObject.transform.position);
		}
	}
}
