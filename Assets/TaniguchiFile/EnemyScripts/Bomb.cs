using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Bomb : MonoBehaviour
{
	[Header("爆発範囲"), SerializeField]
	float m_bangRange;

	[Header("エフェクト"), SerializeField]
	GameObject m_effect;

	[Header("死ぬまでの時間"), SerializeField]
	float m_destroyedTime;

	Status m_status;

	private void Start()
	{
		m_status = GetComponent<Status>();
	}

	//EnemyActionのAttackStart,AttackEndのみと併用すること
	public void Bang()
	{
		Instantiate(m_effect, transform.position, Quaternion.identity);

		//範囲内のコライダーを取得
		Collider[] colliders = Physics.OverlapSphere(transform.position, m_bangRange);
		foreach (Collider col in colliders)
		{
			if (!col.gameObject.CompareTag("Player") && !col.gameObject.CompareTag("Target"))continue;

			Status status;
			if(col.gameObject.TryGetComponent(out status))
			{
				status.Damage(m_status.GetAttackPower());
			}
		}
		Destroy(gameObject , m_destroyedTime);
	}
}
