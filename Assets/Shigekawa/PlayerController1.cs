using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController1 : MonoBehaviour
{
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float rotateSpeed = 500f;
	[SerializeField] float gravity = 15f;

	// 銃撃関連のフィールド
	[Header("Shooting Settings")]
	[SerializeField] GameObject bulletPrefab;
	[SerializeField] Transform firePoint;
	[SerializeField] float shootRate = 0.5f; // 発射レート（秒間に何発撃てるか、小さいほど速い）
	[SerializeField] float baseBulletSpeed = 20f; // 弾の基本速度

	[Header("Ammo and Reload")]
	[SerializeField] int maxAmmo = 20; // マガジン内の最大弾数
	[SerializeField] float reloadDuration = 3f; // マガジンリロードにかかる時間

	// スキル2関連のフィールド
	[Header("Skill 2 Settings (Bullet Speed Up)")]
	[SerializeField] float skill2BulletSpeedMultiplier = 1.5f; // スキル発動時の弾速倍率
	[SerializeField] float skill2Duration = 5f; // スキル2の持続時間

	// アルティメットスキル関連のフィールド
	[Header("Ultimate Skill Settings (Infinite Ammo)")]
	[SerializeField] float ultDuration = 20f; // Ultの持続時間
	[SerializeField] float ultCooldown = 60f; // Ultのクールダウン時間

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
	Vector2 lookInput;
	float verticalVelocity;

	private bool canShoot = true; // 連射速度制御用フラグ
	private float shootTimer = 0f; // 連射速度制御用タイマー (reloadTimerから名称変更)

	private float currentBulletSpeed; // 現在の弾速を保持する変数

	// スキル2関連のフラグ
	private bool isSkill2Active = false;

	// 弾薬とリロード関連の変数
	private int currentAmmo; // 現在の弾数
	private bool isReloading = false; // マガジンリロード中かどうか

	// アルティメットスキル関連の変数
	private bool isUltActive = false; // Ult発動中かどうか
	private bool canUseUlt = true; // Ultが使用可能かどうか

	// ★追加: 射撃入力が押されているかどうかを追跡するフラグ
	private bool isShootingInputPressed = false;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		m_playerInput = GetComponent<PlayerInput>();
		currentBulletSpeed = baseBulletSpeed; // 初期弾速を設定
		currentAmmo = maxAmmo; // 初期弾数を設定
	}

	private void OnEnable()
	{
		m_playerInput.actions["Move"].performed += OnMove;
		m_playerInput.actions["Move"].canceled += OnMoveCancel;

		m_playerInput.actions["Look"].performed += OnLook;
		m_playerInput.actions["Look"].canceled += OnLookCancel;

		// Shotアクションのperformedとcanceledイベントを両方登録
		m_playerInput.actions["Shot"].performed += OnShotPerformed;
		m_playerInput.actions["Shot"].canceled += OnShotCanceled;

		// Skill2 の入力イベント登録
		m_playerInput.actions["Skill2"].performed += OnSkill2;

		// Ult の入力イベント登録
		m_playerInput.actions["Ult"].performed += OnUlt;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Move"].performed -= OnMove;
		m_playerInput.actions["Move"].canceled -= OnMoveCancel;

		m_playerInput.actions["Look"].performed -= OnLook;
		m_playerInput.actions["Look"].canceled -= OnLookCancel;

		// Shotアクションのperformedとcanceledイベントを両方解除
		m_playerInput.actions["Shot"].performed -= OnShotPerformed;
		m_playerInput.actions["Shot"].canceled -= OnShotCanceled;

		// Skill2 の入力イベント解除
		m_playerInput.actions["Skill2"].performed -= OnSkill2;

		// Ult の入力イベント解除
		m_playerInput.actions["Ult"].performed -= OnUlt;
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

	// ★修正: Shotボタンが押されたとき
	private void OnShotPerformed(InputAction.CallbackContext context)
	{
		isShootingInputPressed = true;
	}

	// ★修正: Shotボタンが離されたとき
	private void OnShotCanceled(InputAction.CallbackContext context)
	{
		isShootingInputPressed = false;
	}

	// Skill2 の入力イベントハンドラ
	private void OnSkill2(InputAction.CallbackContext context)
	{
		if (!isSkill2Active) // 重複防止のみ
		{
			StartCoroutine(ActivateSkill2());
		}
	}

	// Ult の入力イベントハンドラ
	private void OnUlt(InputAction.CallbackContext context)
	{
		if (canUseUlt && !isUltActive)
		{
			StartCoroutine(ActivateUlt());
		}
		else if (isUltActive)
		{
			Debug.Log("Ultimate is already active!");
		}
		else if (!canUseUlt)
		{
			Debug.Log("Ultimate is on cooldown!");
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

		// ★連射速度制御タイマーの更新
		if (!canShoot)
		{
			shootTimer -= Time.deltaTime;
			if (shootTimer <= 0)
			{
				canShoot = true;
			}
		}

		// ★連射処理
		if (isShootingInputPressed && canShoot && !isReloading)
		{
			// Ult発動中、または弾がある場合にのみ射撃
			if (isUltActive || currentAmmo > 0)
			{
				Shoot();
				canShoot = false; // 次の射撃まで待機
				shootTimer = shootRate; // 連射速度タイマーをリセット

				// Ult中でなければ弾を消費し、弾切れならリロードを開始
				if (!isUltActive)
				{
					currentAmmo--;
					Debug.Log($"Ammo: {currentAmmo}/{maxAmmo}");
					if (currentAmmo <= 0 && !isReloading)
					{
						StartCoroutine(Reload());
					}
				}
			}
			else if (currentAmmo <= 0 && !isReloading)
			{
				// 弾切れだがリロード中でない場合、自動的にリロード開始
				StartCoroutine(Reload());
			}
			else if (isReloading)
			{
				Debug.Log("Reloading...");
			}
		}

		// UI表示のためのデバッグログ（後で実際のUIに置き換える）
		if (isReloading)
		{
			Debug.Log("Reloading... (UI Placeholder)");
		}
		if (isUltActive)
		{
			Debug.Log("ULTIMATE ACTIVE! (UI Placeholder)");
		}
		else if (!canUseUlt)
		{
			Debug.Log("ULTIMATE COOLDOWN! (UI Placeholder)");
		}
	}

	// 銃を撃つ処理
	private void Shoot()
	{
		if (bulletPrefab != null && firePoint != null)
		{
			GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

			Bullet bullet = bulletInstance.GetComponent<Bullet>();
			if (bullet != null)
			{
				// 弾速をBulletスクリプトに渡す
				bullet.Initialize(firePoint.forward, currentBulletSpeed);
			}
		}
		else
		{
			Debug.LogWarning("Bullet Prefab or Fire Point is not set in PlayerController1.");
		}
	}

	// マガジンリロードを行うコルーチン
	IEnumerator Reload()
	{
		isReloading = true;
		Debug.Log("Start Reloading...");
		yield return new WaitForSeconds(reloadDuration);
		currentAmmo = maxAmmo; // 弾数を満タンにする
		isReloading = false;
		Debug.Log("Reload Complete! Ammo: " + currentAmmo);
	}

	// スキル2を発動するコルーチン
	IEnumerator ActivateSkill2()
	{
		isSkill2Active = true;
		Debug.Log("Skill 2 activated! Bullet speed UP!");

		// 弾速をアップ
		currentBulletSpeed = baseBulletSpeed * skill2BulletSpeedMultiplier;

		yield return new WaitForSeconds(skill2Duration); // 指定時間待機

		// スキル効果終了
		currentBulletSpeed = baseBulletSpeed; // 弾速を元に戻す
		isSkill2Active = false;
		Debug.Log("Skill 2 deactivated. Bullet speed returned to normal.");
	}

	// Ultを発動するコルーチン
	IEnumerator ActivateUlt()
	{
		isUltActive = true;
		canUseUlt = false; // Ult使用不可にする
		Debug.Log("ULTIMATE ACTIVATED! Infinite Ammo for " + ultDuration + " seconds!");

		// Ult中はリロードフラグを強制的に解除し、弾数を考慮しない
		isReloading = false;
		currentAmmo = maxAmmo; // UI表示のため一応満タンにしておく

		yield return new WaitForSeconds(ultDuration);

		// Ult効果終了
		isUltActive = false;
		Debug.Log("ULTIMATE DEACTIVATED. Ammo limitations back on.");

		// Ult終了後、もし弾数が0ならリロードを開始する
		if (currentAmmo <= 0)
		{
			StartCoroutine(Reload());
		}


		// クールダウン開始
		Debug.Log("ULTIMATE COOLDOWN initiated. " + ultCooldown + " seconds remaining.");
		yield return new WaitForSeconds(ultCooldown);

		canUseUlt = true; // クールダウン終了、再度使用可能に
		Debug.Log("ULTIMATE COOLDOWN FINISHED. Ready to use again!");
	}
}