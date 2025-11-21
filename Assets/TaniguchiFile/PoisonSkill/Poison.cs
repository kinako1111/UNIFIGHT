using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poison : MonoBehaviour
{
	[Header("毒のダメージ"), SerializeField]
	int m_poisonDamage;

	[Header("毒の持続時間（秒）"), SerializeField]
	float m_duration = 3f;

	private Dictionary<GameObject, Coroutine> damageCoroutines = new();

	private void Start()
	{
		StartCoroutine(DestroyAfterSeconds(m_duration));
	}

	private IEnumerator DestroyAfterSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);

		// コルーチン停止
		foreach (var kvp in damageCoroutines)
		{
			StopCoroutine(kvp.Value);
		}

		damageCoroutines.Clear();

		Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Enemy"))
		{
			GameObject enemy = other.gameObject;

			if (!damageCoroutines.ContainsKey(enemy))
			{
				Coroutine coroutine = StartCoroutine(DamageOverTime(enemy));
				damageCoroutines.Add(enemy, coroutine);
			}
		}
	}

	private IEnumerator DamageOverTime(GameObject enemy)
	{
		float elapsed = 0f;
		while (elapsed < m_duration)
		{
			Status status;
			if(TryGetComponent(out status))
			{
				status.Damage(m_poisonDamage);
			}
			yield return new WaitForSeconds(1f);
			elapsed += 1f;
		}
	}
}