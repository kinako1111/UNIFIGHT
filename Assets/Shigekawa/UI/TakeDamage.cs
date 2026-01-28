using System.Collections.Generic;
using UnityEngine;

public enum DamageKinds
{
	Attack,   // 攻撃（赤）
	Heal,     // 回復（緑）
	Special   // その他（黄・青など）
}

public class TakeDamage : MonoBehaviour
{
	[SerializeField] GameObject damageUIPrefab;
	[SerializeField] GameObject healUIPrefab;
	[SerializeField] GameObject specialUIPrefab;
	List<GameObject> m_damageUI = new();

	private void Start()
	{
		//現状のコードや設定したものを崩さないための処理
		m_damageUI.Add(damageUIPrefab);
		m_damageUI.Add(healUIPrefab);
		m_damageUI.Add(specialUIPrefab);
	}

	public void ShowDamageUI(int damage,DamageKinds kinds = DamageKinds.Attack)
	{
		var position = transform.position + Vector3.up * 5f;
		var obj = Instantiate(m_damageUI[(int)kinds], position, Quaternion.identity);
		obj.transform.LookAt(Camera.main.transform);

		var damageUI = obj.GetComponent<DamageUI>();
		if (damageUI != null)
		{
			damageUI.SetDamage(damage);
		}
	}
}