using UnityEngine;

public class PotionProjectile : MonoBehaviour
{
	public float effectAmount = 10f; // ポーションの回復量またはダメージ量
	public GameObject hitEffectPrefab; // ヒット時のエフェクト (オプション)

	private bool isAttackModePotion; // このポーションが攻撃モードで発射されたか (true:攻撃, false:回復)
	[SerializeField] private float lifeTime = 5f; // ポーションが自動的に消えるまでの時間

	void Start()
	{
		Destroy(gameObject, lifeTime); // 一定時間後に自身を消滅させる
	}

	public void SetPotionMode(bool mode)
	{
		isAttackModePotion = mode;
	}

	void OnCollisionEnter(Collision collision)
	{
		ApplyPotionEffect(collision.gameObject);

		if (hitEffectPrefab != null)
		{
			Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
		}
		Destroy(gameObject); // 衝突したらポーションを消す
	}

	void ApplyPotionEffect(GameObject target)
	{
		// ここでターゲットのHealthManagerなどを取得し、効果を適用する
		// 例:
		// HealthManager targetHealth = target.GetComponent<HealthManager>();
		// if (targetHealth != null)
		// {
		//     if (isAttackModePotion)
		//     {
		//         // 攻撃モードで発射されたポーション: 敵にダメージ
		//         if (target.CompareTag("Enemy"))
		//         {
		//             targetHealth.TakeDamage(effectAmount);
		//             Debug.Log($"Attack Potion hit {target.name}. Dealt {effectAmount} damage.");
		//         }
		//     }
		//     else
		//     {
		//         // 回復モードで発射されたポーション: 味方に回復
		//         if (target.CompareTag("Ally") || target.CompareTag("Player"))
		//         {
		//             targetHealth.Heal(effectAmount);
		//             Debug.Log($"Heal Potion hit {target.name}. Healed for {effectAmount}.");
		//         }
		//     }
		// }
		// HealthManagerがないため、Debug.Logで代替
		if (isAttackModePotion)
		{
			// 攻撃モードで発射されたポーション: 敵にダメージ
			if (target.CompareTag("Enemy"))
			{
				Debug.Log($"Attack Potion hit {target.name}. Dealt {effectAmount} damage.");
			}
		}
		else
		{
			// 回復モードで発射されたポーション: 味方に回復
			if (target.CompareTag("Ally") || target.CompareTag("Player"))
			{
				Debug.Log($"Heal Potion hit {target.name}. Healed for {effectAmount}.");
			}
		}
	}
}