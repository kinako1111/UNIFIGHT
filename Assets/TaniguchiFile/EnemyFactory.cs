using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
	private enum EnemyName
	{
		Golem,
		Mushroom,
		Cactus
	}

	private enum EnemyPrefab
	{
		Mushroom,
		Cactus,
		Length
	}

	[SerializeField] Wave[] waves;
	[SerializeField] WaveManager m_waveManager;
	private int m_currentWave = 0;
	private bool m_isSpawning = false;

	[SerializeField]
	List<GameObject> m_enemyList = new();

	[Header("Enemy生成場所"), SerializeField]
	Transform[] m_factoryPos;

	[SerializeField] float m_waveDelay = 5f;

	private void Awake()
	{
		//Assets直下のPrefabを取得
		m_enemyList.AddRange(Resources.LoadAll<GameObject>("Enemy"));
	}
	public void Start()
	{
		//StartNextWave();
		//CreateEnemy((int)EnemyName.Golem, m_factoryPos);
		//CreateEnemy((int)EnemyName.Mushroom, m_factoryPos);
		//CreateEnemy((int)EnemyName.Cactus, m_factoryPos);
	}

	IEnumerator WaitAndStartNextWave()
	{
		yield return new WaitForSeconds(m_waveDelay);
		StartNextWave();
	}

	private void FixedUpdate()
	{
		if(!m_isSpawning && GameObject.FindGameObjectsWithTag("Enemy").Length <= 0)
		{
			StartCoroutine(WaitAndStartNextWave());
		}
	}

	// ウェーブ
	public void StartNextWave()
	{
		if (!m_isSpawning && m_currentWave < waves.Length)
		{
			StartCoroutine(SpawnWave(waves[m_currentWave]));
			m_currentWave++;

			Debug.Log("現在のウェーブは" + m_currentWave + "です");
			m_waveManager.WaveCount();

		}
	}

	// 敵のスポーン
	IEnumerator SpawnWave(Wave wave)
	{
		m_isSpawning = true;

		// もしボスのウェーブだったら
		if (wave.isBossWave)
		{
			CreateEnemy((int)EnemyName.Golem, m_factoryPos[Random.Range(0, m_factoryPos.Length)]);
		}
		else
		{
			// モブ敵の生成
			for (int i = 0; i < wave.enemyCount; i++)
			{
				GameObject enemy = CreateEnemy(Random.Range(1, 3), m_factoryPos[Random.Range(0, m_factoryPos.Length)]);
				//enemy.GetComponent<EnemyStatus>().ScaleSatus(m_currentWave + 1);
				yield return new WaitForSeconds(wave.spawnInterval);
			}

			m_isSpawning = false;
		}
	}
	public GameObject CreateEnemy(int name, Transform transform)
	{
		return Instantiate(GetEnemyInfo(name), transform);
	}

	public GameObject GetEnemyInfo(int name)
	{
		return m_enemyList[name];
	}
}
