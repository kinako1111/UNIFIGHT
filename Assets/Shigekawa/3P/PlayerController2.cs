using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // UI要素のために追加
using TMPro; // TextMeshProのために追加

public class PlayerController2 : MonoBehaviour
{
	[Header("Movement Settings")]
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float rotateSpeed = 10f; // PlayerController1のturnSpeedに相当
	[SerializeField] float gravity = 15f;

	[SerializeField] Status m_status;
	private Transform cameraTransform;
	public Vector3 CurrentLookDirection { get; private set; }

	[SerializeField] PlayerShooting2 playerShooting;

	[Header("Passive Aura Settings")]
	[SerializeField] GameObject auraEffectPrefab;
	[SerializeField] float auraRadius = 5f;
	[SerializeField] float passiveHealAmount = 1f;
	[SerializeField] float passivePoisonDamage = 1f;
	[SerializeField] float passiveEffectInterval = 1f;

	// UI関連のフィールド
	[Header("UI Settings")]
	[SerializeField] Slider m_playerSlider;
	[SerializeField] TextMeshProUGUI m_hpText;

	[Header("Ultimate Skill Settings")]
	[SerializeField] float ultimateCooldown = 60f; // ウルトのクールダウン時間
	[SerializeField] int reviveHpPercentage = 50; // 蘇生時のHP割合 (例: 50で50%)
	[SerializeField] float ultimateEffectRadius = 10f; // ウルトの効果範囲
	bool canUseUltimate = true; // ウルトが使用可能か
	public bool IsDead { get; private set; } = false; // 死亡状態のフラグ

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
	Vector2 lookInput;
	float verticalVelocity;

	private Vector3 moveDirection;

	bool isAttackMode = false; // 初期値を回復モード(false)に変更

	GameObject currentAuraEffect;
	private Coroutine passiveAuraCoroutine;
	private Coroutine ultimateCooldownCoroutine; // ウルトのクールダウンコルーチンを管理するためのフィールド

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		m_playerInput = GetComponent<PlayerInput>();

		if (Camera.main != null)
		{
			cameraTransform = Camera.main.transform;
		}

		if (m_status == null)
		{
			m_status = GetComponent<Status>();
		}

		if (playerShooting == null)
		{
			playerShooting = GetComponent<PlayerShooting2>();
		}
	}

	private void OnEnable()
	{
		m_playerInput.actions["Move"].performed += OnMovePerformed;
		m_playerInput.actions["Move"].canceled += OnMoveCanceled;

		m_playerInput.actions["Look"].performed += OnLookPerformed;
		m_playerInput.actions["Look"].canceled += OnLookCanceled;

		m_playerInput.actions["Shot"].performed += OnShotPerformed;

		// 引数を追加
		m_playerInput.actions["Skill1"].performed += OnSkill1;
		m_playerInput.actions["Skill2"].performed += OnSkill2;
		m_playerInput.actions["Ult"].performed += OnUlt;

		StartPassiveAura();
		if (playerShooting != null)
		{
			playerShooting.SetAttackMode(isAttackMode); // 初期モードをPlayerShooting2に伝える
		}
	}

	private void OnDisable()
	{
		m_playerInput.actions["Move"].performed -= OnMovePerformed;
		m_playerInput.actions["Move"].canceled -= OnMoveCanceled;

		m_playerInput.actions["Look"].performed -= OnLookPerformed;
		m_playerInput.actions["Look"].canceled -= OnLookCanceled;

		m_playerInput.actions["Shot"].performed -= OnShotPerformed;

		// 引数を追加
		m_playerInput.actions["Skill1"].performed -= OnSkill1;
		m_playerInput.actions["Skill2"].performed -= OnSkill2;
		m_playerInput.actions["Ult"].performed -= OnUlt;

		StopPassiveAura();
		if (ultimateCooldownCoroutine != null)
		{
			StopCoroutine(ultimateCooldownCoroutine);
		}
	}

	// --- Input Callbacks ---
	private void OnMovePerformed(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	private void OnMoveCanceled(InputAction.CallbackContext context)
	{
		moveInput = Vector2.zero;
	}

	private void OnLookPerformed(InputAction.CallbackContext context)
	{
		lookInput = context.ReadValue<Vector2>();
	}

	private void OnLookCanceled(InputAction.CallbackContext context)
	{
		lookInput = Vector2.zero;
	}

	public void OnShotPerformed(InputAction.CallbackContext context)
	{
		if (IsDead) return;
		if (playerShooting != null)
		{
			playerShooting.Shot();
		}
	}

	private void OnSkill1(InputAction.CallbackContext context) // 引数を追加
	{
		if (IsDead) return;
		isAttackMode = !isAttackMode;
		UpdateAuraEffectVisuals();

		if (playerShooting != null)
		{
			playerShooting.SetAttackMode(isAttackMode);
		}
	}

	private void OnSkill2(InputAction.CallbackContext context) // 引数を追加
	{
		if (IsDead) return;
		if (playerShooting != null)
		{
			playerShooting.ActivateSkill2Action();
		}
	}

	// --- ウルトの入力イベントハンドラ ---
	private void OnUlt(InputAction.CallbackContext context) // 引数を追加
	{
		if (m_status == null || IsDead) return; // 自分が死んでいる場合はウルト発動不可

		if (canUseUltimate)
		{
			if (ultimateCooldownCoroutine != null)
			{
				StopCoroutine(ultimateCooldownCoroutine);
			}
			ultimateCooldownCoroutine = StartCoroutine(ActivateUlt());
		}
	}

	void Update()
	{
		if (IsDead)
		{
			controller.Move(Vector3.zero);
			moveDirection = Vector3.zero;
			verticalVelocity = 0;
			UpdateHpUI();
			return;
		}

		UpdateHpUI();
		ApplyGravity();
		HandleMovementAndRotation();
		MoveCharacter();

		if (currentAuraEffect != null)
		{
			currentAuraEffect.transform.position = transform.position;
		}

		if (playerShooting != null)
		{
			playerShooting.SetLookDirection(CurrentLookDirection);
		}
	}

	private void UpdateHpUI()
	{
		if (m_status != null && m_playerSlider != null && m_hpText != null)
		{
			m_playerSlider.maxValue = m_status.maxHp;
			m_playerSlider.value = m_status.GetHp();
			m_hpText.text = m_status.GetHp().ToString() + " / " + m_status.maxHp.ToString();
		}
	}

	void ApplyGravity()
	{
		if (controller.isGrounded)
		{
			verticalVelocity = -1f;
		}
		else
		{
			verticalVelocity -= gravity * Time.deltaTime;
		}
	}

	void HandleMovementAndRotation()
	{
		if (cameraTransform == null)
		{
			CurrentLookDirection = transform.forward;
			moveDirection = Vector3.zero;
			return;
		}

		Vector3 forward = cameraTransform.forward;
		Vector3 right = cameraTransform.right;

		forward.y = 0f;
		right.y = 0f;
		forward.Normalize();
		right.Normalize();

		moveDirection = Vector3.zero;
		if (moveInput.magnitude > 0.1f)
		{
			moveDirection = forward * moveInput.y + right * moveInput.x;
			moveDirection.Normalize();
		}

		Vector3 targetLookDirection = Vector3.zero;

		if (lookInput.magnitude > 0.1f)
		{
			targetLookDirection = forward * lookInput.y + right * lookInput.x;
			targetLookDirection.y = 0f;
			targetLookDirection.Normalize();
		}
		else if (moveDirection.magnitude > 0.1f)
		{
			targetLookDirection = moveDirection;
		}

		if (targetLookDirection != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetLookDirection, Vector3.up);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
			CurrentLookDirection = targetLookDirection;
		}
		else
		{
			CurrentLookDirection = transform.forward;
		}
	}

	private void MoveCharacter()
	{
		float currentMoveSpeed = (m_status != null) ? m_status.GetSpeed() : moveSpeed;
		Vector3 velocity = moveDirection * currentMoveSpeed + Vector3.up * verticalVelocity;
		controller.Move(velocity * Time.deltaTime);
	}

	// --- パッシブオーラの管理 ---
	void StartPassiveAura()
	{
		if (auraEffectPrefab != null && currentAuraEffect == null)
		{
			currentAuraEffect = Instantiate(auraEffectPrefab, transform.position, Quaternion.identity, transform);
			UpdateAuraEffectVisuals();
		}

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

	IEnumerator ApplyPassiveAuraEffect()
	{
		while (true)
		{
			yield return new WaitForSeconds(passiveEffectInterval);

			if (IsDead) continue;
			if (m_status == null) continue;

			Collider[] hitColliders = Physics.OverlapSphere(transform.position, auraRadius);
			foreach (var hitCollider in hitColliders)
			{
				if (hitCollider.gameObject == gameObject) continue;

				Status targetStatus = hitCollider.GetComponent<Status>();
				if (targetStatus == null) continue;

				bool isAlly = hitCollider.CompareTag("Ally") || hitCollider.CompareTag("Player");
				bool isEnemy = hitCollider.CompareTag("Enemy");


				if (isAttackMode) // 攻撃モード
				{
					if (isEnemy)
					{
						targetStatus.Damage((int)passivePoisonDamage); // 毒ダメージ
					}
				}
				else // 回復モード
				{
					if (isAlly)
					{
						if (targetStatus.GetMaxHp() > targetStatus.GetHp())
						{
							targetStatus.Heal((int)passiveHealAmount);
						}
					}
				}
			}
		}
	}

	void UpdateAuraEffectVisuals()
	{
		if (currentAuraEffect != null)
		{
			Renderer renderer = currentAuraEffect.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material.color = isAttackMode ? Color.red : Color.green;
			}

			ParticleSystem ps = currentAuraEffect.GetComponent<ParticleSystem>();
			if (ps != null)
			{
				var main = ps.main;
				main.startColor = isAttackMode ? new ParticleSystem.MinMaxGradient(Color.red) : new ParticleSystem.MinMaxGradient(Color.green);
			}
		}
	}

	public bool IsAttackMode()
	{
		return isAttackMode;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (IsDead) return;

		int hitDamage = 10;
		int healAmount = 1;

		if (hit.gameObject.CompareTag("Enemy"))
		{
			if (m_status == null || m_status.GetHp() <= 0) return;
			m_status.Damage(hitDamage);
			CheckDeath();
		}

		if (hit.gameObject.CompareTag("Heal"))
		{
			if (m_status == null || m_status.GetMaxHp() <= m_status.GetHp()) return;
			m_status.Heal(healAmount);
			// Destroy(hit.gameObject);
		}
	}

	// --- 死亡判定とウルト処理 ---
	public void CheckDeath()
	{
		if (m_status != null && m_status.GetHp() <= 0 && !IsDead)
		{
			Die();
		}
	}

	private void Die()
	{
		IsDead = true;
		// 死亡時の視覚的な変化や操作不能にする処理などをここに追加
	}

	// ウルト発動コルーチン (味方蘇生)
	IEnumerator ActivateUlt()
	{
		canUseUltimate = false;

		// 周囲の死亡している味方を探して蘇生
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateEffectRadius);
		foreach (var hitCollider in hitColliders)
		{
			// 自分自身は対象外
			if (hitCollider.gameObject == gameObject) continue;

			PlayerController2 allyController = hitCollider.GetComponent<PlayerController2>();
			if (allyController != null && allyController.IsDead)
			{
				// 味方を蘇生
				allyController.ReviveAlly(reviveHpPercentage);
			}
		}

		float remainingCooldown = ultimateCooldown;
		while (remainingCooldown > 0)
		{
			yield return new WaitForSeconds(1f);
			remainingCooldown -= 1f;
		}

		canUseUltimate = true;
		ultimateCooldownCoroutine = null;
	}

	// 味方を蘇生させるメソッド (PlayerController2のインスタンスが直接呼び出す)
	public void ReviveAlly(int hpPercentage)
	{
		if (!IsDead) return; // 死亡していない味方は蘇生しない

		IsDead = false;
		if (m_status != null)
		{
			int reviveHp = m_status.maxHp * hpPercentage / 100;
			m_status.Heal(reviveHp - m_status.GetHp()); // 指定割合まで回復
		}
		// 蘇生時の視覚的な変化などをここに追加することも可能
	}

	// デバッグ表示用 (ギズモ)
	void OnDrawGizmos()
	{
		// パッシブオーラ範囲
		Gizmos.color = isAttackMode ? Color.red : Color.green;
		Gizmos.DrawWireSphere(transform.position, auraRadius);

		// ウルト範囲
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, ultimateEffectRadius);


		if (Application.isPlaying)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, CurrentLookDirection * 2f);
		}
	}
}