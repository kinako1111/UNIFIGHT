using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StausBar : MonoBehaviour
{
	[SerializeField]Status m_status;
	Slider m_slider;

	private void Awake()
	{
		m_slider = GetComponent<Slider>();
		if (m_status != null) return;
		m_status = transform.root.GetComponent<Status>();
	}

	private void Start()
	{
		// 初期反映…
		m_slider.maxValue = m_status.GetMaxHp();
		m_slider.value = m_status.GetHp();

		// サブスクライブ
		m_status.OnHpChanged += HandleHpChanged;
	}

	private void OnDisable()
	{
		m_status.OnHpChanged -= HandleHpChanged;
	}

	private void HandleHpChanged(int cur, int max)
	{
		Debug.Log("HP変更");
		if (!Mathf.Approximately(m_slider.maxValue, max)) m_slider.maxValue = max;
		m_slider.value = cur;
	}

}
