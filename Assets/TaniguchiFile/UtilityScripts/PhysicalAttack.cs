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
		//※注意※EnemyとPlayerでレイヤーの当たり判定を切っているため
		//攻撃の当たり判定は必ずEnemy以外にすること　
		if (other.gameObject.layer == gameObject.layer) return;

		Status status;
		if (other.gameObject.TryGetComponent(out status))
		{
			Debug.Log("ステータススクリプト取得");
			//ダメージ付与
			status.Damage(m_status.GetAttackPower());
		}
	}
}