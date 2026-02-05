using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
	[SerializeField] int m_maxWave = 3;
	[SerializeField] SceneChanger m_sceneChanger;

	private int m_startedWave = 0;
	private bool m_waitingClear = false;
	private bool m_gameClear = false;

	public void WaveCount()
	{
		if (m_gameClear) return;

		m_startedWave++;
		Debug.Log($"Wave {m_startedWave} 開始通知");

		m_waitingClear = true;
	}

	private void Update()
	{
		if (!m_waitingClear || m_gameClear) return;

		if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0)
		{
			Debug.Log($"Wave {m_startedWave} 終了");
			m_waitingClear = false;

			if (m_startedWave >= m_maxWave)
			{
				m_gameClear = true;
				StartCoroutine(GoToWinScene());
			}
		}
	}

	IEnumerator GoToWinScene()
	{
		yield return new WaitForSeconds(3f);
		SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
		changer.ChangeScene("WinScene");
	}
}