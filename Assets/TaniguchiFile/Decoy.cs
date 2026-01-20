using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Decoy : MonoBehaviour
{
	[Header("注目範囲"), SerializeField]
	float m_attentionRange;

	[Header("消えるまでの時間"), SerializeField]
	float m_deathTimer;

	[Header("破壊時のエフェクト"), SerializeField]
	GameObject m_effect;

	[Header("破壊時の効果音"), SerializeField]
	AudioClip m_se;

	Status m_status;

	private void Awake()
	{
		m_status = GetComponent<Status>();
	}

	private void Start()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, m_attentionRange);
		foreach(Collider targetCollider in colliders)
		{
			GameObject target = targetCollider.gameObject;
			EnemyAction enemyaction;
			if (target.TryGetComponent(out enemyaction))
			{
				enemyaction.ChangeHate(0, target);
			}
		}
		Destroy(gameObject,m_deathTimer);
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
