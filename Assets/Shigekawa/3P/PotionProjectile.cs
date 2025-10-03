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
		if (collision.gameObject == thrower)
		{
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
		Status targetStatus = target.GetComponent<Status>();
		if (targetStatus == null)
		{
			return;
		}

		if (isAttackPotion)
		{
			if (target.CompareTag("Enemy"))
			{
				targetStatus.Damage((int)effectAmount);
				PlayerController2 enemyController = target.GetComponent<PlayerController2>();
				if (enemyController != null) enemyController.CheckDeath();
			}
		}
		else
		{
			if (target.CompareTag("Ally") || target.CompareTag("Player"))
			{
				if (targetStatus.GetMaxHp() > targetStatus.GetHp())
				{
					targetStatus.Heal((int)effectAmount);
				}
			}
		}
	}
}