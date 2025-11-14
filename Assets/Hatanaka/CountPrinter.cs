using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountPrinter : MonoBehaviour
{
	TextMeshProUGUI m_countText;
    private int enemyCount;

	private void Start()
	{
		m_countText = GetComponent<TextMeshProUGUI>();
	}
	// Update is called once per frame
	void Update()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

		m_countText.GetComponent<TextMeshProUGUI>().text = enemyCount.ToString();
	}
}
