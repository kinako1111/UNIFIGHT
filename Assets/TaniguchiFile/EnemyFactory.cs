using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
	private enum EnemyName
	{
	
	
	}

	[SerializeField]
	List<GameObject> m_enemyList = new();

	[Header("Enemy¶¬êŠ"), SerializeField]
	Transform m_factoryPos;


	private void Awake()
	{
		//Assets’¼‰º‚ÌPrefab‚ğæ“¾
		m_enemyList.AddRange(Resources.LoadAll<GameObject>("Enemy"));
	}

	//public void CreateEnemey(int name , Transform transform)
	//{
	//	Instantiate(GetEnemyInfo(name), transform);
	//}

	//public GameObject GetEnemyInfo(int name)
	//{
	//	return m_enemyList.FirstOrDefault(enemy => enemy.GetComponent<Status>().Get);
	//}

}
