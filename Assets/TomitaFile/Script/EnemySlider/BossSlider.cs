using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EnemyFactory;

public class BossSlider : MonoBehaviour
{
	EnemyFactory enemyFactory;
    [SerializeField] Status m_status;
    [SerializeField] Slider m_slider;

	private void Start()
	{
		m_slider.GetComponent<Slider>();
	}

	// Update is called once per frame
	void FixedUpdate()
    {
		m_slider.maxValue = m_status.GetMaxHp();
		m_slider.value = m_status.GetHp();
	}

	public void SetCharaStatus(EnemyFactory.EnemyName type)
	{
		
	}
}
