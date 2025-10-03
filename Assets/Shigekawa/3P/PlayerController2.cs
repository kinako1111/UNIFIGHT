using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // UI要素のために追加
using TMPro; // TextMeshProのために追加
			 // using UnityEditor; // ビルド時にエラーになるため削除

public class PlayerController2 : MonoBehaviour
{
	[Header("Movement Settings")]
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float rotateSpeed = 10f;
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

	// Pスクリプトから移行したUI関連のフィールド
	[Header("UI Settings")]
	[SerializeField] Slider m_playerSlider;
	[SerializeField] TextMeshProUGUI m_hpText;

	[Header("Ultimate Skill Settings")]
	[SerializeField] float ultimateCooldown = 60f; // ウルトのクールダウン時間
	[SerializeField] int reviveHpPercentage = 50; // 蘇生時のHP割合 (例: 50で50%)
	bool canUseUltimate = true; // ウルトが使用可能か
	public bool IsDead { get; private set; } = false; // 死亡状態のフラグ

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
	Vector2 lookInput;
	float verticalVelocity;

	private Vector3 moveDirection;

	bool isAttackMode = true;

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
		else
		{
			Debug.LogWarning("Main Camera not found! Please tag your camera as 'MainCamera'.");
		}

		if (m_status == null)
		{
			m_status = GetComponent<Status>();
			if (m_status == null)
			{
				Debug.LogError("Status component not found on this GameObject. Player speed will use default 'moveSpeed' value from PlayerController2.");
			}
		}

		if (playerShooting == null)
		{
			playerShooting = GetComponent<PlayerShooting2>();
			if (playerShooting == null)
			{
				Debug.LogError("PlayerShooting component not found. Please assign it in the Inspector or ensure it's on the same GameObject.");
			}
		}
	}

	private void OnEnable()
	{
		m_playerInput.actions["Move"].performed += OnMove;
		m_playerInput.actions["Move"].canceled += OnMoveCancel;

		m_playerInput.actions["Look"].performed += OnLook;
		m_playerInput.actions["Look"].canceled += OnLookCancel;

		m_playerInput.actions["Shot"].performed += OnShotPerformed;

		m_playerInput.actions["Skill1"].performed += OnSkill1;
		m_playerInput.actions["Skill2"].performed += OnSkill2;
		// ここを修正: "Ultimate" から "Ult" へ変更
		m_playerInput.actions["Ult"].performed += OnUlt;

		StartPassiveAura();
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
		// ここを修正: "Ultimate" から "Ult" へ変更
		m_playerInput.actions["Ult"].performed -= OnUlt; // OnUltimate -> OnUlt に修正

		StopPassiveAura();
		if (ultimateCooldownCoroutine != null)
		{
			StopCoroutine(ultimateCooldownCoroutine);
		}
	}

	// --- Input Callbacks ---
	private void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
	private void OnMoveCancel(InputAction.CallbackContext context) => moveInput = Vector2.zero;
	private void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
	private void OnLookCancel(InputAction.CallbackContext context) => lookInput = Vector2.zero;

	public void OnShotPerformed(InputAction.CallbackContext context)
	{
		if (IsDead) return; // 死亡中は攻撃不可
		if (playerShooting != null)
		{
			playerShooting.Shot();
		}
	}

	private void OnSkill1(InputAction.CallbackContext context)
	{
		if (IsDead) return; // 死亡中はスキル不可
		isAttackMode = !isAttackMode;
		Debug.Log("Mode switched to: " + (isAttackMode ? "Attack" : "Heal"));
		UpdateAuraEffectVisuals();

		if (playerShooting != null)
		{
			playerShooting.SetAttackMode(isAttackMode);
		}
	}

	private void OnSkill2(InputAction.CallbackContext context)
	{
		if (IsDead) return; // 死亡中はスキル不可
		if (playerShooting != null)
		{
			playerShooting.ActivateSkill2Action();
		}
	}

	// --- ウルトの入力イベントハンドラ ---
	private void OnUlt(InputAction.CallbackContext context)
	{
		if (IsDead && canUseUltimate) // 死亡中で、かつウルトが使用可能であれば発動
		{
			if (ultimateCooldownCoroutine != null)
			{
				StopCoroutine(ultimateCooldownCoroutine); // 既存のクールダウンコルーチンがあれば停止
			}
			ultimateCooldownCoroutine = StartCoroutine(ActivateUlt()); // ActivateUltimate -> ActivateUlt に修正
		}
		else if (!IsDead)
		{
			Debug.Log("Ultimate can only be used when the player is dead.");
		}
		else // IsDead && !canUseUltimate の場合
		{
			Debug.Log("Ultimate is on cooldown. Cannot revive yet.");
		}
	}

	void Update()
	{
		// 死亡中は移動やスキル発動を停止
		if (IsDead)
		{
			// 死亡中の特別な処理 (例えば、キャラクターモデルを非表示にするなど)
			controller.Move(Vector3.zero); // 移動を停止
			moveDirection = Vector3.zero; // 移動方向をリセット
			verticalVelocity = 0; // 重力の影響も停止

			UpdateHpUI(); // HP表示だけは継続
			return;
		}

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

		UpdateHpUI(); // HP表示を毎フレーム更新
	}

	// Pスクリプトから移行したHP UI更新メソッド
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
			// カメラが見つからない場合のフォールバック
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

			// 死亡中はパッシブオーラも停止
			if (IsDead) continue;
			// Statusがnullの場合は処理しない
			if (m_status == null) continue;

			Collider[] hitColliders = Physics.OverlapSphere(transform.position, auraRadius);
			foreach (var hitCollider in hitColliders) // hitCollizers -> hitColliders に修正
			{
				if (hitCollider.gameObject == gameObject) continue;

				// Statusコンポーネントを持つオブジェクトのみを対象とする (味方/敵の判定)
				Status targetStatus = hitCollider.GetComponent<Status>();
				if (targetStatus == null) continue;

				// 自身と同じプレイヤー/味方グループのタグを持つもの (例: "Player", "Ally")
				bool isAlly = hitCollider.CompareTag("Ally") || hitCollider.CompareTag("Player");
				// 自身と異なるプレイヤー/敵グループのタグを持つもの (例: "Enemy")
				bool isEnemy = hitCollider.CompareTag("Enemy");


				if (isAttackMode) // 攻撃モード
				{
					if (isEnemy)
					{
						targetStatus.Damage((int)passivePoisonDamage); // 毒ダメージ
						Debug.Log($"Passive Aura (Attack Mode): Poisoned {hitCollider.name} for {passivePoisonDamage} damage. Current HP: {targetStatus.GetHp()}");
					}
				}
				else // 回復モード
				{
					if (isAlly)
					{
						// 最大HPを超えないように回復
						if (targetStatus.GetMaxHp() > targetStatus.GetHp())
						{
							targetStatus.Heal((int)passiveHealAmount);
							Debug.Log($"Passive Aura (Heal Mode): Healed {hitCollider.name} for {passiveHealAmount} health. Current HP: {targetStatus.GetHp()}");
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

	// Pスクリプトから移行したOnControllerColliderHit
	// CharacterControllerがアタッチされていることを前提とします。
	// 敵や回復アイテムとの直接接触によるダメージ/回復処理
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (IsDead) return; // 死亡中は衝突処理も停止

		int hitDamage = 10; // 直接接触時のダメージ
		int healAmount = 1; // 直接接触時の回復量

		// 敵との接触
		if (hit.gameObject.CompareTag("Enemy"))
		{
			if (m_status == null || m_status.GetHp() <= 0) return; // Statusがないか、既にHPが0以下の場合は処理しない
			m_status.Damage(hitDamage);
			Debug.Log($"Direct hit by {hit.gameObject.name}. Took {hitDamage} damage. Current HP: {m_status.GetHp()}");
			CheckDeath(); // ダメージを受けた後に死亡判定
		}

		// 回復アイテムとの接触
		if (hit.gameObject.CompareTag("Heal"))
		{
			if (m_status == null || m_status.GetMaxHp() <= m_status.GetHp()) return; // Statusがないか、最大HPの場合は回復しない
			m_status.Heal(healAmount);
			Debug.Log($"Touched Heal item. Healed for {healAmount}. Current HP: {m_status.GetHp()}");
			// 回復アイテムは一度触れたら消えるなど、別途処理が必要な場合が多い
			// Destroy(hit.gameObject);
		}
	}

	// --- 死亡判定とウルト処理 ---
	public void CheckDeath()
	{
		if (m_status != null && m_status.GetHp() <= 0 && !IsDead) // Statusがnullでないことを確認
		{
			Die();
		}
	}

	private void Die()
	{
		IsDead = true;
		Debug.Log("Player has died!");
		// 死亡時の視覚的な変化や操作不能にする処理などをここに追加
		// 例: モデルを非表示にする、アニメーションを停止する、UIに死亡メッセージを表示するなど
		// gameObject.SetActive(false); // 例えば、プレイヤーオブジェクト自体を非アクティブにする

		// ウルトのクールダウン表示などを開始
		if (canUseUltimate)
		{
			Debug.Log("Press 'Ult' to revive!"); // メッセージもUltに変更
		}
		else
		{
			Debug.Log("Ultimate is on cooldown. Cannot revive.");
		}
	}

	// ウルト発動コルーチン
	IEnumerator ActivateUlt() // ActivateUltimate -> ActivateUlt に修正
	{
		canUseUltimate = false; // ウルト使用不可にする
		Debug.Log("Activating Ultimate: Revive!");

		// 死亡状態を解除し、HPを回復
		IsDead = false;
		if (m_status != null) // Statusがnullでないことを確認
		{
			int reviveHp = m_status.maxHp * reviveHpPercentage / 100;
			// 現在のHPが復活HPよりも低い場合のみ回復
			if (m_status.GetHp() < reviveHp)
			{
				m_status.Heal(reviveHp - m_status.GetHp()); // 現在のHPに関わらず指定割合まで回復
			}
		}


		// プレイヤーを再度アクティブにする（もし非アクティブにしていた場合）
		// gameObject.SetActive(true);

		Debug.Log($"Player revived with {(m_status != null ? (m_status.maxHp * reviveHpPercentage / 100).ToString() : "N/A")} HP! Current HP: {(m_status != null ? m_status.GetHp().ToString() : "N/A")}");

		// クールダウン開始
		float remainingCooldown = ultimateCooldown;
		while (remainingCooldown > 0)
		{
			Debug.Log($"Ultimate Cooldown: {Mathf.CeilToInt(remainingCooldown)}s remaining.");
			yield return new WaitForSeconds(1f);
			remainingCooldown -= 1f;
		}

		canUseUltimate = true;
		Debug.Log("Ultimate is ready again.");
		ultimateCooldownCoroutine = null; // クールダウン終了時にコルーチン参照をクリア
	}

	// デバッグ表示用 (ギズモ)
	void OnDrawGizmos()
	{
		Gizmos.color = isAttackMode ? Color.red : Color.green;
		Gizmos.DrawWireSphere(transform.position, auraRadius);

		if (Application.isPlaying)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, CurrentLookDirection * 2f);
		}
	}
}