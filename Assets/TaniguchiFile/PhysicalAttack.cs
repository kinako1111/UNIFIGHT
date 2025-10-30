using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalAttack : MonoBehaviour
{
	[Header("攻撃の当たり判定"), SerializeField]
	Collider m_AttackPos;

	[Header("参照するステータス"), SerializeField]
	Status m_status;

	private void OnTriggerEnter(Collider other)
	{
		Status status;
		if (other.gameObject.TryGetComponent(out status))
		{
			Debug.Log("ステータススクリプト取得");
			//ダメージ付与
			status.Damage(m_status.GetAttackPower());
		}
	}

}
