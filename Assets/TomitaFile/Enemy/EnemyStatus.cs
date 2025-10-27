using ExitGames.Client.Photon.StructWrapping;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
	[SerializeField] Status m_status;

	[SerializeField] float m_waveEnemyHp;
	[SerializeField] float m_waveEnemyAttack;

	public void ScaleSatus(int waveNumber)
	{
		// ウェーブが上がるごとにステータスアップ
		float hp = m_status.GetHp() + waveNumber * m_waveEnemyHp;
		float attack = m_status.GetAttackPower() + waveNumber * m_waveEnemyAttack;
	}
}
