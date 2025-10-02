using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

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
	[SerializeField] GameObject potionPrefab; // 投擲するポーションのPrefab (PotionProjectileコンポーネントを持つもの)
	[SerializeField] float throwForce = 15f; // ポーションを投げる力
	[SerializeField] Transform throwPoint; // ポーションを投げる開始位置
	[SerializeField] float shotDelay = 0.5f; // ポーションが飛翔中に次のポーションを投げるまでのディレイ

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
	bool isShooting = false; // ポーション発射中フラグ

	GameObject currentAuraEffect; // 生成されたオーラエフェクトのインスタンス
	private Coroutine passiveAuraCoroutine; // Passive Auraのコルーチンを停止・再開するために保持

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

	// --- Input Callbacks ---
	private void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
	private void OnMoveCancel(InputAction.CallbackContext context) => moveInput = Vector2.zero;
	private void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
	private void OnLookCancel(InputAction.CallbackContext context) => lookInput = Vector2.zero;

	// 通常攻撃 (ポーション投擲) の入力イベントハンドラ
	private void OnShotPerformed(InputAction.CallbackContext context)
	{
		if (!isShooting)
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
		// --- 移動と重力の処理 ---
		ApplyGravity();
		HandleMovementAndRotation();

		// オーラエフェクトをプレイヤーの位置に追従させる
		if (currentAuraEffect != null)
		{
			currentAuraEffect.transform.position = transform.position;
		}
	}

	void ApplyGravity()
	{
		if (controller.isGrounded)
		{
			verticalVelocity = -1f; // 接地している場合は少しだけ下に押し付ける
		}
		else
		{
			verticalVelocity -= gravity * Time.deltaTime;
		}
	}

	void HandleMovementAndRotation()
	{
		// 向きの変更
		if (lookInput != Vector2.zero)
		{
			transform.Rotate(Vector3.up, lookInput.x * rotateSpeed * Time.deltaTime);
		}

		// 移動量の計算
		Vector3 moveDirection = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
		moveDirection.y = 0; // Y軸方向の移動は重力に任せる

		Vector3 velocity = moveDirection * moveSpeed + Vector3.up * verticalVelocity;

		controller.Move(velocity * Time.deltaTime);
	}

	// --- ポーション投擲処理 ---
	void ThrowPotion()
	{
		if (potionPrefab == null)
		{
			Debug.LogWarning("Potion prefab is not assigned!");
			return;
		}
		if (throwPoint == null)
		{
			Debug.LogWarning("ThrowPoint is not assigned! Assign a Transform to throwPoint in the inspector.");
			return;
		}

		GameObject potionInstance = Instantiate(potionPrefab, throwPoint.position, throwPoint.rotation);
		Rigidbody rb = potionInstance.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.AddForce(throwPoint.forward * throwForce, ForceMode.VelocityChange);
			// ポーションにモード情報を持たせるためのスクリプト
			PotionProjectile potionProjectile = potionInstance.GetComponent<PotionProjectile>();
			if (potionProjectile != null)
			{
				potionProjectile.SetPotionMode(isAttackMode); // 現在のモードをポーションに渡す
			}
			else
			{
				Debug.LogWarning("PotionPrefab does not have a PotionProjectile component!");
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
			yield return new WaitForSeconds(0.15f); // 2連射時の少しのディレイ (調整可能)
			ThrowPotion();
		}
		yield return new WaitForSeconds(shotDelay); // 次のショットまでのディレイ
		isShooting = false;
	}

	// --- パッシブオーラの管理 ---
	void StartPassiveAura()
	{
		// オーラエフェクトの生成 (見た目用)
		if (auraEffectPrefab != null && currentAuraEffect == null) // 既に存在しない場合のみ生成
		{
			currentAuraEffect = Instantiate(auraEffectPrefab, transform.position, Quaternion.identity, transform);
			// オーラの見た目を初期化
			UpdateAuraEffectVisuals();
		}

		// 既存のコルーチンがあれば停止し、新しく開始
		if (passiveAuraCoroutine != null)
		{
			StopCoroutine(passiveAuraCoroutine);
		}
		passiveAuraCoroutine = StartCoroutine(ApplyPassiveAuraEffect());
	}

	void StopPassiveAura()
	{
		if (passiveAuraCoroutine != null)
		{
			StopCoroutine(passiveAuraCoroutine);
			passiveAuraCoroutine = null;
		}
		if (currentAuraEffect != null)
		{
			Destroy(currentAuraEffect);
			currentAuraEffect = null;
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

				// HealthManager healthManager = hitCollider.GetComponent<HealthManager>(); 
				// if (healthManager == null) continue; // 体力管理コンポーネントがない場合はスキップ

				if (isAttackMode)
				{
					// 攻撃モード: 敵に毒ダメージ
					if (hitCollider.CompareTag("Enemy"))
					{
						// healthManager.TakeDamage(passivePoisonDamage);
						Debug.Log($"Passive Aura (Attack Mode): Poisoned {hitCollider.name} for {passivePoisonDamage} damage.");
					}
				}
				else
				{
					// 回復モード: 味方に少量の自動回復
					if (hitCollider.CompareTag("Ally") || hitCollider.CompareTag("Player"))
					{
						// healthManager.Heal(passiveHealAmount);
						Debug.Log($"Passive Aura (Heal Mode): Healed {hitCollider.name} for {passiveHealAmount} health.");
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
			// ここでオーラエフェクトの見た目をモードに応じて変更するロジックを実装します。
			// 例: マテリアルの色を変更
			Renderer renderer = currentAuraEffect.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.color = isAttackMode ? Color.red : Color.green;
			}

			// ParticleSystemを制御する例
			ParticleSystem ps = currentAuraEffect.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				var main = ps.main;
				main.startColor = isAttackMode ? new ParticleSystem.MinMaxGradient(Color.red) : new ParticleSystem.MinMaxGradient(Color.green);
			}
		}
	}

	// --- スキル2: 2連射をアクティブにするコルーチン ---
	IEnumerator ActivateSkill2()
	{
		canUseSkill2 = false;
		isDoubleShotActive = true;
		Debug.Log("Skill2 Activated: Double shot for " + skill2Duration + " seconds!");

		yield return new WaitForSeconds(skill2Duration);

		isDoubleShotActive = false;
		Debug.Log("Skill2 Deactivated: Normal shot.");

		// クールダウン
		float remainingCooldown = skill2Cooldown;
		while (remainingCooldown > 0)
		{
			Debug.Log($"Skill2 Cooldown: {Mathf.CeilToInt(remainingCooldown)}s remaining.");
			yield return new WaitForSeconds(1f);
			remainingCooldown -= 1f;
		}

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
		Gizmos.color = isAttackMode ? Color.red : Color.green; // 現在のモードに応じて色を変える
		Gizmos.DrawWireSphere(transform.position, auraRadius);
	}
}