using UnityEngine;

public class PotionProjectile : MonoBehaviour
{
	public float effectAmount = 10f; // ポーションの回復量またはダメージ量
	public GameObject hitEffectPrefab; // ヒット時のエフェクト (オプション)
	public bool isAttackPotion; // このポーションが攻撃モードで発射されたか (true:攻撃, false:回復)

	[SerializeField] private float lifeTime = 5f; // ポーションが自動的に消えるまでの時間

	private GameObject thrower; // ポーションを投げたオブジェクト (PlayerController2など)

	void Start()
	{
		Destroy(gameObject, lifeTime); // 一定時間後に自身を消滅させる
	}

	// どのモードのポーションか設定するメソッド
	public void SetPotionMode(bool attackMode)
	{
		isAttackPotion = attackMode;
	}

	// ポーションを投げたオブジェクトを設定するメソッド
	public void SetThrower(GameObject sender)
	{
		thrower = sender;
	}

	void OnCollisionEnter(Collision collision)
	{
		// ポーションを投げたオブジェクト自身には効果を適用しない
		if (collision.gameObject == thrower)
		{
			// Debug.Log("Potion hit its thrower, no effect applied.");
			// 例えば、衝突エフェクトだけ出して消滅させる
			if (hitEffectPrefab != null)
			{
				GameObject effectInstance = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
			}
			Destroy(gameObject);
			return;
		}

		ApplyPotionEffect(collision.gameObject);

		if (hitEffectPrefab != null)
		{
			GameObject effectInstance = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
		}
		Destroy(gameObject); // 衝突したらポーションを消す
	}

	void ApplyPotionEffect(GameObject target)
	{
		// ターゲットのStatusコンポーネントを取得
		Status targetStatus = target.GetComponent<Status>();
		if (targetStatus == null)
		{
			// Statusコンポーネントがない場合は効果なし
			// Debug.LogWarning($"Target {target.name} does not have a Status component. Potion effect not applied.");
			return;
		}

		if (isAttackPotion)
		{
			// 攻撃ポーション: 敵にダメージ
			if (target.CompareTag("Enemy"))
			{
				targetStatus.Damage((int)effectAmount);
				Debug.Log($"Attack Potion hit {target.name}. Dealt {effectAmount} damage. Current HP: {targetStatus.GetHp()}");
				// プレイヤーのポーションが敵に当たった場合、敵の死亡判定も考慮
				PlayerController2 enemyController = target.GetComponent<PlayerController2>(); // 敵がPlayerController2を持つとは限らない
				if (enemyController != null) enemyController.CheckDeath(); // 敵がこのメソッドを持つ場合
			}
		}
		else
		{
			// 回復ポーション: 味方に回復 (プレイヤー自身も含む)
			if (target.CompareTag("Ally") || target.CompareTag("Player"))
			{
				// 最大HPを超えないように回復
				if (targetStatus.GetMaxHp() > targetStatus.GetHp())
				{
					targetStatus.Heal((int)effectAmount);
					Debug.Log($"Heal Potion hit {target.name}. Healed for {effectAmount}. Current HP: {targetStatus.GetHp()}");
				}
			}
		}
	}
}