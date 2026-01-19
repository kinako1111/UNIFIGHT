using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StausBar : MonoBehaviour
{
	[SerializeField] Status m_status;

	Slider m_slider;

	private void Awake()
	{
		m_slider = GetComponent<Slider>();
	}

	private void OnEnable()
	{
		// 初期反映…
		m_slider.maxValue = m_status.GetMaxHp();
		m_slider.value = m_status.GetHp();

		// サブスクライブ
		m_status.OnHpChanged += HandleHpChanged;
	}

	private void OnDisable()
	{
		if (m_status != null) m_status.OnHpChanged -= HandleHpChanged;
	}
	private void HandleHpChanged(int cur, int max)
	{
		if (!Mathf.Approximately(m_slider.maxValue, max)) m_slider.maxValue = max;
		m_slider.value = cur;
	}

}
