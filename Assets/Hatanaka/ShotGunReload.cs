using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunReload : MonoBehaviour
{
   public void Reloading()
	{
		GetComponent<Animator>().SetTrigger("Reload");
	}
}
