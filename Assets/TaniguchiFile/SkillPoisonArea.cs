using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPoisonArea : MonoBehaviour
{
	[Header("“Å"), SerializeField]
	GameObject m_poisonArea;

	public void InstansPoison(Vector3 pos)
	{
		Instantiate(m_poisonArea,pos,Quaternion.identity);
	}

}
