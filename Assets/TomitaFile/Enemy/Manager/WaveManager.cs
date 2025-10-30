using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_waveText;
    [SerializeField] GameObject m_textActive;

    private int m_waveCount = 0;

    public void WaveCount()
    {
        m_waveCount++;
		m_textActive.SetActive(true);
		m_waveText.text = "現在のウェーブは" + m_waveCount.ToString() + "です";
		StartCoroutine(WaveCountRead());

	}

    IEnumerator WaveCountRead()
    {
        yield return new WaitForSeconds(3);
        m_textActive.SetActive(false);
	}
}
