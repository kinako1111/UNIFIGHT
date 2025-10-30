using ExitGames.Client.Photon.StructWrapping;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatus : MonoBehaviour
{
	[SerializeField] Status m_status;

	//[SerializeField] float m_waveEnemyHp;
	//[SerializeField] float m_waveEnemyAttack;

	[SerializeField] Slider m_slider;
	[SerializeField] TextMeshProUGUI m_text;

	private void FixedUpdate()
	{
		m_slider.maxValue = m_status.GetMaxHp();
		m_slider.value = m_status.GetHp();
		m_text.text = m_status.GetHp().ToString() + " / " + m_status.GetMaxHp().ToString();
	}		
}