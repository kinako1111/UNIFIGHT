using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
	//全ユニットのリスト
	List<GameObject> m_unitList = new();

	//ユニットの追加
	public void AddUnit(GameObject unit ,Vector3 pos, Quaternion rotation)
	{
		GameObject addUnit = Instantiate(unit,pos,rotation);
		m_unitList.Add(addUnit);
	}

	//受け渡し用のユニットのリスト
	public List<GameObject> GetUnitList()
	{
		return m_unitList;
	}
}
