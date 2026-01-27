using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionSet : MonoBehaviour
{
	[SerializeField] GameObject Potion;
	public void PotionSpawn()
	{
		Potion.SetActive(true);
	}
	public void PotionDelete()
	{
		Potion.SetActive(false);
	}
}
