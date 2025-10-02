using UnityEngine;

public class PotionProjectile : MonoBehaviour
{
	public float effectAmount = 10f; // ポーションの回復量またはダメージ量
	public GameObject hitEffectPrefab; // ヒット時のエフェクト (オプション)
	public bool isAttackPotion; // このポーションが攻撃モードで発射されたか (true:攻撃, false:回復)

	[SerializeField] private float lifeTime = 5f; // ポーションが自動的に消えるまでの時間

	void Start()
	{
		Destroy(gameObject, lifeTime); // 一定時間後に自身を消滅させる
	}

	// どのモードのポーションか設定するメソッド
	public void SetPotionMode(bool attackMode)
	{
		isAttackPotion = attackMode;
	}

	void OnCollisionEnter(Collision collision)
	{
		ApplyPotionEffect(collision.gameObject);

		if (hitEffectPrefab != null)
		{
			// ヒットエフェクトを生成し、ポーションのTransformを継承しないようにする
			GameObject effectInstance = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
			// 必要であれば、エフェクトを一定時間後に破棄する処理も追加
			// Destroy(effectInstance, effectDuration); 
		}
		Destroy(gameObject); // 衝突したらポーションを消す
	}

	void ApplyPotionEffect(GameObject target)
	{
		// ここでターゲットのHealthManagerなどを取得し、効果を適用する
		// 実際のゲームでは、HealthManagerなどの体力管理コンポーネントが対象にアタッチされていることを想定
		// 今回はDebug.Logで効果をシミュレートします。

		if (isAttackPotion)
		{
			// 攻撃ポーション: 敵にダメージ
			if (target.CompareTag("Enemy"))
			{
				// HealthManager targetHealth = target.GetComponent<HealthManager>();
				// if (targetHealth != null) targetHealth.TakeDamage(effectAmount);
				Debug.Log($"Attack Potion hit {target.name}. Dealt {effectAmount} damage.");
			}
		}
		else
		{
			// 回復ポーション: 味方に回復
			if (target.CompareTag("Ally") || target.CompareTag("Player"))
			{
				// HealthManager targetHealth = target.GetComponent<HealthManager>();
				// if (targetHealth != null) targetHealth.Heal(effectAmount);
				Debug.Log($"Heal Potion hit {target.name}. Healed for {effectAmount}.");
			}
		}
	}
}