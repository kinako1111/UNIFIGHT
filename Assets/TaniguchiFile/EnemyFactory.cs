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
	private int m_currentWave = 0;
	private bool m_isSpawning = false;

	[SerializeField]
	List<GameObject> m_enemyList = new();

	[Header("Enemyê∂ê¨èÍèä"), SerializeField]
	Transform m_factoryPos;


	private void Awake()
	{
		//Assetsíºâ∫ÇÃPrefabÇéÊìæ
		m_enemyList.AddRange(Resources.LoadAll<GameObject>("Enemy"));
	}
	public void Start()
	{
		StartNextWave();
		//CreateEnemy((int)EnemyName.Golem, m_factoryPos);
		//CreateEnemy((int)EnemyName.Mushroom, m_factoryPos);
		//CreateEnemy((int)EnemyName.Cactus, m_factoryPos);
	}

	public void StartNextWave()
	{
		if (!m_isSpawning && m_currentWave < waves.Length)
		{
			StartCoroutine(SpawnWave(waves[m_currentWave]));
			m_currentWave++;
		}
	}
	IEnumerator SpawnWave(Wave wave)
	{
		m_isSpawning = true;

		if (wave.isBossWave)
		{
			CreateEnemy((int)EnemyName.Golem, m_factoryPos);
		}
		else
		{
			for (int i = 0; i <= wave.enemyCount; i++)
			{
				GameObject enemy = CreateEnemy(Random.Range(1, 3), m_factoryPos);
				enemy.GetComponent<EnemyStatus>().ScaleSatus(m_currentWave + 1);
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
