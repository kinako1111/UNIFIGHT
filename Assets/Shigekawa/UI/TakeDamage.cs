using UnityEngine;

public class TakeDamage : MonoBehaviour
{
	[SerializeField] GameObject damageUIPrefab;

	public void ShowDamageUI(int damage)
	{
		var position = transform.position + Vector3.up * 3f;
		var obj = Instantiate(damageUIPrefab, position, Quaternion.identity);
		obj.transform.LookAt(Camera.main.transform);

		var damageUI = obj.GetComponent<DamageUI>();
		if (damageUI != null)
		{
			damageUI.SetDamage(damage);
		}
	}
}