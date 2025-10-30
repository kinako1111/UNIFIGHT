using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerStatus : MonoBehaviour
{
	[SerializeField] Status m_status;
	[SerializeField] Slider m_slider;
	[SerializeField] TextMeshProUGUI m_hptext;


	// Update is called once per frame
	void FixedUpdate()
    {
		m_slider.maxValue = m_status.GetMaxHp();
		m_slider.value = m_status.GetHp();
		m_hptext.text = m_status.GetHp().ToString() + " / " + m_status.GetMaxHp().ToString();
	}
}
