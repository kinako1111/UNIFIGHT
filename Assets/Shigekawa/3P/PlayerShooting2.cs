using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShooting2 : MonoBehaviour
{
	[Header("Potion Settings")]
	[SerializeField] GameObject attackPotionPrefab;  // 攻撃モードで投擲するポーションのPrefab
	[SerializeField] GameObject healPotionPrefab;    // 回復モードで投擲するポーションのPrefab

	[SerializeField] float throwForce = 15f;
	[SerializeField] Transform throwPoint;
	[SerializeField] float shotDelay = 0.5f;

	[Header("Skill Settings")]
	[SerializeField] float skill2Duration = 5f;
	[SerializeField] float skill2Cooldown = 15f;

	bool isAttackMode = false; // 初期値を回復モード(false)に変更
	bool canUseSkill2 = true;
	bool isDoubleShotActive = false;
	bool isShooting = false;

	private Vector3 currentLookDirection;
	Animator m_animator;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
	}

	public void SetAttackMode(bool mode)
	{
		isAttackMode = mode;
	}

	public void SetLookDirection(Vector3 lookDir)
	{
		currentLookDirection = lookDir;
	}

	// --- ポーション投擲処理 ---
	public void Shot()
	{
		if (!isShooting)
		{
			StartCoroutine(HandleShot());
			m_animator.SetTrigger("Attack");
		}
	}

	void ThrowPotion()
	{
		GameObject currentPotionPrefab = isAttackMode ? attackPotionPrefab : healPotionPrefab;

		if (currentPotionPrefab == null)
		{
			return;
		}
		if (throwPoint == null)
		{
			return;
		}

		GameObject potionInstance = Instantiate(currentPotionPrefab, throwPoint.position, throwPoint.rotation);
		Rigidbody rb = potionInstance.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.AddForce(currentLookDirection * throwForce, ForceMode.VelocityChange);

			PotionProjectile potionProjectile = potionInstance.GetComponent<PotionProjectile>();
			if (potionProjectile != null)
			{
				potionProjectile.SetPotionMode(isAttackMode);
				potionProjectile.SetThrower(this.gameObject);
			}
		}
	}

	// ポーション発射と連射をハンドリングするコルーチン
	IEnumerator HandleShot()
	{
		isShooting = true;
		ThrowPotion();
		if (isDoubleShotActive)
		{
			yield return new WaitForSeconds(0.15f); // 連射時の間隔
			ThrowPotion();
		}
		yield return new WaitForSeconds(shotDelay); // 次のショットまでのディレイ
		isShooting = false;
	}

	// --- スキル2: 2連射をアクティブにするコルーチン ---
	public void ActivateSkill2Action()
	{
		if (canUseSkill2)
		{
			StartCoroutine(ActivateSkill2());
		}
	}

	IEnumerator ActivateSkill2()
	{
		canUseSkill2 = false;
		isDoubleShotActive = true;

		yield return new WaitForSeconds(skill2Duration);

		isDoubleShotActive = false;

		float remainingCooldown = skill2Cooldown;
		while (remainingCooldown > 0)
		{
			yield return new WaitForSeconds(1f);
			remainingCooldown -= 1f;
		}

		canUseSkill2 = true;
	}
}