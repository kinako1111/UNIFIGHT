using UnityEngine;
using System.Collections;

namespace DamagePointUI
{
	public class Attack : MonoBehaviour
	{

		void OnTriggerEnter(Collider col)
		{
			if (col.tag == "Enemy")
			{
				col.transform.root.GetComponent<TakeDamage>().Damage(col);
			}
		}
	}
}