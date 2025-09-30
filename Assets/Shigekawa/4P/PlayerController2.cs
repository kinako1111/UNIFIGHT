using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic; // Listを使用するため追加

public class PlayerController2 : MonoBehaviour
{
	[Header("Movement Settings")]
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float rotateSpeed = 500f;
	[SerializeField] float gravity = 15f;

	[Header("Passive Aura Settings")]
	[SerializeField] GameObject auraEffectPrefab; // 円形のオーラエフェクトのPrefab (見た目用)
	[SerializeField] float auraRadius = 5f; // オーラの範囲
	[SerializeField] float passiveHealAmount = 1f; // 味方への自動回復量
	[SerializeField] float passivePoisonDamage = 1f; // 敵への毒ダメージ
	[SerializeField] float passiveEffectInterval = 1f; // オーラ効果の間隔

	[Header("Potion Settings")]
	[SerializeField] GameObject attackPotionPrefab; // 攻撃モードで投擲するポーション
	[SerializeField] GameObject healPotionPrefab;   // 回復モードで投擲するポーション
	[SerializeField] float throwForce = 15f; // ポーションを投げる力
	[SerializeField] Transform throwPoint; // ポーションを投げる開始位置

	[Header("Skill Settings")]
	[SerializeField] float skill2Duration = 5f; // スキル2の効果時間 (2連射)
	[SerializeField] float skill2Cooldown = 15f; // スキル2のクールダウン

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
	Vector2 lookInput;
	float verticalVelocity;

	bool isAttackMode = true; // 現在のモード (true: 攻撃, false: 回復)
	bool canUseSkill2 = true; // スキル2が使用可能か
	bool isDoubleShotActive = false; // スキル2 (2連射) がアクティブか

	GameObject currentAuraEffect; // 生成されたオーラエフェクトのインスタンス

	// ポーションが飛翔中に次のポーションを投げるまでのディレイ
	[SerializeField] float shotDelay = 0.2f;
	private bool isShooting = false; // ポーション発射中フラグ

	// Passive Auraのコルーチンを停止・再開するために保持
	private Coroutine passiveAuraCoroutine;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		m_playerInput = GetComponent<PlayerInput>();
	}

	private void OnEnable()
	{
		m_playerInput.actions["Move"].performed += OnMove;
		m_playerInput.actions["Move"].canceled += OnMoveCancel;

		m_playerInput.actions["Look"].performed += OnLook;
		m_playerInput.actions["Look"].canceled += OnLookCancel;

		m_playerInput.actions["Shot"].performed += OnShotPerformed;

		m_playerInput.actions["Skill1"].performed += OnSkill1; // Skill1 (モード切り替え)
		m_playerInput.actions["Skill2"].performed += OnSkill2; // Skill2 (2連射)

		StartPassiveAura(); // ゲーム開始時にオーラを開始
	}

	private void OnDisable()
	{
		m_playerInput.actions["Move"].performed -= OnMove;
		m_playerInput.actions["Move"].canceled -= OnMoveCancel;

		m_playerInput.actions["Look"].performed -= OnLook;
		m_playerInput.actions["Look"].canceled -= OnLookCancel;

		m_playerInput.actions["Shot"].performed -= OnShotPerformed;

		m_playerInput.actions["Skill1"].performed -= OnSkill1;
		m_playerInput.actions["Skill2"].performed -= OnSkill2;

		StopPassiveAura(); // ゲーム終了時にオーラを停止
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	private void OnMoveCancel(InputAction.CallbackContext context)
	{
		moveInput = Vector2.zero;
	}

	private void OnLook(InputAction.CallbackContext context)
	{
		lookInput = context.ReadValue<Vector2>();
	}

	private void OnLookCancel(InputAction.CallbackContext context)
	{
		lookInput = Vector2.zero;
	}

	// 通常攻撃 (ポーション投擲) の入力イベントハンドラ
	private void OnShotPerformed(InputAction.CallbackContext context)
	{
		if (!isShooting) // ポーション発射中でなければ
		{
			StartCoroutine(HandleShot());
		}
	}

	// スキル1: 攻撃/回復モード切り替え
	private void OnSkill1(InputAction.CallbackContext context)
	{
		isAttackMode = !isAttackMode;
		Debug.Log("Mode switched to: " + (isAttackMode ? "Attack" : "Heal"));
		UpdateAuraEffectVisuals(); // オーラの見た目を更新 (もしあれば)
	}

	// スキル2: 一定時間2連射
	private void OnSkill2(InputAction.CallbackContext context)
	{
		if (canUseSkill2)
		{
			StartCoroutine(ActivateSkill2());
		}
		else
		{
			Debug.Log("Skill2 is on cooldown.");
		}
	}

	void Update()
	{
		// 移動と重力の処理
		if (controller.isGrounded)
		{
			verticalVelocity = -1f;
		}
		else
		{
			verticalVelocity -= gravity * Time.deltaTime;
		}

		if (lookInput != Vector2.zero)
		{
			transform.Rotate(Vector3.up, lookInput.x * rotateSpeed * Time.deltaTime);
		}

		Vector3 move = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
		move.y = 0;

		Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;

		controller.Move(velocity * Time.deltaTime);

		// オーラエフェクトをプレイヤーの位置に追従させる
		if (currentAuraEffect != null)
		{
			currentAuraEffect.transform.position = transform.position;
		}
	}

	// ポーション投擲処理
	void ThrowPotion()
	{
		GameObject potionPrefab = isAttackMode ? attackPotionPrefab : healPotionPrefab;

		if (potionPrefab == null)
		{
			Debug.LogWarning($"No {(isAttackMode ? "attack" : "heal")} potion prefab assigned!");
			return;
		}
		if (throwPoint == null)
		{
			Debug.LogWarning("ThrowPoint is not assigned! Assign a Transform to throwPoint in the inspector.");
			return;
		}

		GameObject potion = Instantiate(potionPrefab, throwPoint.position, throwPoint.rotation);
		Rigidbody rb = potion.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.AddForce(throwPoint.forward * throwForce, ForceMode.VelocityChange);
			// ポーションにモード情報を持たせるためのスクリプトがある場合
			PotionProjectile potionProjectile = potion.GetComponent<PotionProjectile>();
			if (potionProjectile != null)
			{
				potionProjectile.SetPotionMode(isAttackMode);
			}
		}
		else
		{
			Debug.LogWarning("Potion prefab does not have a Rigidbody component!");
		}
	}

	// ポーション発射と連射をハンドリングするコルーチン
	IEnumerator HandleShot()
	{
		isShooting = true;
		ThrowPotion();
		if (isDoubleShotActive)
		{
			yield return new WaitForSeconds(0.1f); // 2連射時の少しのディレイ
			ThrowPotion();
		}
		yield return new WaitForSeconds(shotDelay); // 次のショットまでのディレイ
		isShooting = false;
	}

	// パッシブオーラを開始する
	void StartPassiveAura()
	{
		// オーラエフェクトの生成 (見た目用)
		if (auraEffectPrefab != null)
		{
			currentAuraEffect = Instantiate(auraEffectPrefab, transform.position, Quaternion.identity, transform);
			// オーラの見た目を設定 (例えば、色やパーティクルシステム)
			// currentAuraEffect.transform.localScale = Vector3.one * auraRadius * 2; // 円の直径に合わせる
		}

		if (passiveAuraCoroutine != null)
		{
			StopCoroutine(passiveAuraCoroutine);
		}
		passiveAuraCoroutine = StartCoroutine(ApplyPassiveAuraEffect());
	}

	// パッシブオーラを停止する
	void StopPassiveAura()
	{
		if (passiveAuraCoroutine != null)
		{
			StopCoroutine(passiveAuraCoroutine);
		}
		if (currentAuraEffect != null)
		{
			Destroy(currentAuraEffect);
		}
	}

	// パッシブオーラの効果を適用するコルーチン
	IEnumerator ApplyPassiveAuraEffect()
	{
		while (true)
		{
			yield return new WaitForSeconds(passiveEffectInterval);

			Collider[] hitColliders = Physics.OverlapSphere(transform.position, auraRadius);
			foreach (var hitCollider in hitColliders)
			{
				// 自分自身は対象外
				if (hitCollider.gameObject == gameObject) continue;

				// HealthManager healthManager = hitCollider.GetComponent<HealthManager>(); // 体力管理コンポーネント
				// if (healthManager == null) continue;

				if (isAttackMode)
				{
					// 攻撃モード: 敵に毒ダメージ
					// if (hitCollider.CompareTag("Enemy")) // 敵のタグを設定
					// {
					//     healthManager.TakeDamage(passivePoisonDamage);
					//     Debug.Log($"Poisoned {hitCollider.name} for {passivePoisonDamage} damage.");
					// }
					// ここではHealthManagerがないためDebug.Logで代替
					if (hitCollider.CompareTag("Enemy"))
					{
						Debug.Log($"Attacking: Poisoned {hitCollider.name} for {passivePoisonDamage} damage.");
					}
				}
				else
				{
					// 回復モード: 味方に少量の自動回復
					// if (hitCollider.CompareTag("Ally") || hitCollider.CompareTag("Player")) // 味方のタグを設定
					// {
					//     healthManager.Heal(passiveHealAmount);
					//     Debug.Log($"Healed {hitCollider.name} for {passiveHealAmount} health.");
					// }
					// ここではHealthManagerがないためDebug.Logで代替
					if (hitCollider.CompareTag("Ally") || hitCollider.CompareTag("Player"))
					{
						Debug.Log($"Healing: Healed {hitCollider.name} for {passiveHealAmount} health.");
					}
				}
			}
		}
	}

	// オーラエフェクトの見た目を更新する（例：色を変える、パーティクルを切り替えるなど）
	void UpdateAuraEffectVisuals()
	{
		if (currentAuraEffect != null)
		{
			// ここでオーラエフェクトの見た目をモードに応じて変更するロジックを実装
			// 例: ParticleSystemの色を変える、Materialの色を変える
			Renderer renderer = currentAuraEffect.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.color = isAttackMode ? Color.red : Color.green; // 例として色を変える
			}
			// ParticleSystemを制御する例
			// ParticleSystem ps = currentAuraEffect.GetComponent<ParticleSystem>();
			// if (ps != null)
			// {
			//     var main = ps.main;
			//     main.startColor = isAttackMode ? Color.red : Color.green;
			// }
		}
	}

	// スキル2: 2連射をアクティブにするコルーチン
	IEnumerator ActivateSkill2()
	{
		canUseSkill2 = false;
		isDoubleShotActive = true;
		Debug.Log("Skill2 Activated: Double shot for " + skill2Duration + " seconds!");

		yield return new WaitForSeconds(skill2Duration);

		isDoubleShotActive = false;
		Debug.Log("Skill2 Deactivated: Normal shot.");

		// クールダウン
		yield return new WaitForSeconds(skill2Cooldown);

		canUseSkill2 = true;
		Debug.Log("Skill2 is ready again.");
	}

	// 他のコンポーネントからこのキャラクターのモードを取得したい場合
	public bool IsAttackMode()
	{
		return isAttackMode;
	}

	// デバッグ表示用 (ギズモ)
	void OnDrawGizmos()
	{
		Gizmos.color = isAttackMode ? Color.red : Color.green;
		Gizmos.DrawWireSphere(transform.position, auraRadius);
	}
}