using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController1 : MonoBehaviour
{
	[SerializeField] Status m_status;
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

	// ★追加: 射撃方向線用のフィールド
	[Header("Aim Line Settings")]
	[SerializeField] LineRenderer aimLineRenderer; // Line Rendererコンポーネンスへの参照
	[SerializeField] float aimLineLength = 10f; // 線の長さ

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
	Vector2 lookInput; // 左スティックの入力として使用
	float verticalVelocity;

	private bool canShoot = true; // 連射速度制御用フラグ
	private float shootTimer = 0f; // 連射速度制御用タイマー

	private float currentBulletSpeed; // 現在の弾速を保持する変数

	// スキル2関連のフラグ
	private bool isSkill2Active = false;

	// 弾薬とリロード関連の変数
	private int currentAmmo; // 現在の弾数
	private bool isReloading = false; // マガジンリロード中かどうか

	// アルティメットスキル関連の変数
	private bool isUltActive = false; // Ult発動中かどうか
	private bool canUseUlt = true; // Ultが使用可能かどうか

	// 射撃入力が押されているかどうかを追跡するフラグ
	private bool isShootingInputPressed = false;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		m_playerInput = GetComponent<PlayerInput>();
		currentBulletSpeed = baseBulletSpeed; // 初期弾速を設定
		currentAmmo = maxAmmo; // 初期弾数を設定

		// ★追加: Line Rendererの初期設定
		if (aimLineRenderer == null)
		{
			aimLineRenderer = GetComponent<LineRenderer>();
			if (aimLineRenderer == null)
			{
				Debug.LogWarning("Line Renderer not found on Player object. Adding one automatically.");
				aimLineRenderer = gameObject.AddComponent<LineRenderer>();
				// デフォルトマテリアルが設定されるので、エディタで忘れずに設定し直す
				aimLineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 仮のマテリアル
				aimLineRenderer.startWidth = 0.1f;
				aimLineRenderer.endWidth = 0.05f;
				aimLineRenderer.startColor = Color.blue;
				aimLineRenderer.endColor = Color.cyan;
				aimLineRenderer.positionCount = 2;
				aimLineRenderer.useWorldSpace = true;
			}
		}
		aimLineRenderer.enabled = false; // 最初は非表示
	}

	private void OnEnable()
	{
		m_playerInput.actions["Move"].performed += OnMove;
		m_playerInput.actions["Move"].canceled += OnMoveCancel;

		m_playerInput.actions["Look"].performed += OnLook;
		m_playerInput.actions["Look"].canceled += OnLookCancel;

		m_playerInput.actions["Skill2"].performed += OnSkill2;

		m_playerInput.actions["Ult"].performed += OnUlt;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Move"].performed -= OnMove;
		m_playerInput.actions["Move"].canceled -= OnMoveCancel;

		m_playerInput.actions["Look"].performed -= OnLook;
		m_playerInput.actions["Look"].canceled -= OnLookCancel;

		m_playerInput.actions["Skill2"].performed -= OnSkill2;

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
		if (lookInput.magnitude > 0.1f)
		{
			isShootingInputPressed = true;
			// ★追加: 左スティックが倒されたら線を表示
			aimLineRenderer.enabled = true;
		}
		else
		{
			isShootingInputPressed = false;
			// ★追加: 左スティックが離されたら線を非表示
			aimLineRenderer.enabled = false;
		}
	}

	private void OnLookCancel(InputAction.CallbackContext context)
	{
		lookInput = Vector2.zero;
		isShootingInputPressed = false;
		// ★追加: 左スティックが離されたら線を非表示
		aimLineRenderer.enabled = false;
	}

	private void OnSkill2(InputAction.CallbackContext context)
	{
		if (!isSkill2Active)
		{
			StartCoroutine(ActivateSkill2());
		}
	}

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

		// 移動方向への向きの変更（右スティックのMove入力に基づく）
		if (moveInput != Vector2.zero)
		{
			Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
			Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_status.GetSpeed() * 10f * Time.deltaTime); // 向きの滑らかさ調整
		}

		Vector3 move = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
		move.y = 0;

		Vector3 velocity = move * m_status.GetSpeed() + Vector3.up * verticalVelocity;
		Debug.Log(m_status.GetSpeed());

		controller.Move(velocity * Time.deltaTime);

		// 連射速度制御タイマーの更新
		if (!canShoot)
		{
			shootTimer -= Time.deltaTime;
			if (shootTimer <= 0)
			{
				canShoot = true;
			}
		}

		// 連射処理
		if (isShootingInputPressed && canShoot && !isReloading)
		{
			if (isUltActive || currentAmmo > 0)
			{
				Shoot();
				canShoot = false;
				shootTimer = shootRate;

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
				StartCoroutine(Reload());
			}
			else if (isReloading)
			{
				Debug.Log("Reloading...");
			}
		}

		// ★追加: 射撃方向線の更新
		if (aimLineRenderer.enabled)
		{
			Vector3 shootDirection;
			if (lookInput.magnitude > 0.1f)
			{
				shootDirection = new Vector3(lookInput.x, 0, lookInput.y).normalized;
			}
			else
			{
				shootDirection = transform.forward; // 左スティック入力がない場合でも、線が表示されているなら体の向き
			}

			// FirePointから線が伸びるように調整
			// Y座標をFirePointの高さに合わせることで、地面と平行に線が表示されやすくなる
			Vector3 startPoint = firePoint.position;
			startPoint.y = transform.position.y + 0.5f; // キャラクターの少し上あたり

			Vector3 endPoint = startPoint + shootDirection * aimLineLength;
			aimLineRenderer.SetPosition(0, startPoint);
			aimLineRenderer.SetPosition(1, endPoint);
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
			Vector3 shootDirection;

			if (lookInput.magnitude > 0.1f)
			{
				shootDirection = new Vector3(lookInput.x, 0, lookInput.y).normalized;
			}
			else
			{
				shootDirection = transform.forward;
			}

			GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

			Bullet bullet = bulletInstance.GetComponent<Bullet>();
			if (bullet != null)
			{
				bullet.Initialize(shootDirection, currentBulletSpeed);
			}
		}
		else
		{
			Debug.LogWarning("Bullet Prefab or Fire Point is not set in PlayerController1.");
		}
	}

	// 以下、Reload, ActivateSkill2, ActivateUlt メソッドは変更なし
	IEnumerator Reload()
	{
		isReloading = true;
		Debug.Log("Start Reloading...");
		yield return new WaitForSeconds(reloadDuration);
		currentAmmo = maxAmmo;
		isReloading = false;
		Debug.Log("Reload Complete! Ammo: " + currentAmmo);
	}

	IEnumerator ActivateSkill2()
	{
		isSkill2Active = true;
		Debug.Log("Skill 2 activated! Bullet speed UP!");
		currentBulletSpeed = baseBulletSpeed * skill2BulletSpeedMultiplier;
		yield return new WaitForSeconds(skill2Duration);
		currentBulletSpeed = baseBulletSpeed;
		isSkill2Active = false;
		Debug.Log("Skill 2 deactivated. Bullet speed returned to normal.");
	}

	IEnumerator ActivateUlt()
	{
		isUltActive = true;
		canUseUlt = false;
		Debug.Log("ULTIMATE ACTIVATED! Infinite Ammo for " + ultDuration + " seconds!");
		isReloading = false;
		currentAmmo = maxAmmo;
		yield return new WaitForSeconds(ultDuration);
		isUltActive = false;
		Debug.Log("ULTIMATE DEACTIVATED. Ammo limitations back on.");
		if (currentAmmo <= 0)
		{
			StartCoroutine(Reload());
		}
		Debug.Log("ULTIMATE COOLDOWN initiated. " + ultCooldown + " seconds remaining.");
		yield return new WaitForSeconds(ultCooldown);
		canUseUlt = true;
		Debug.Log("ULTIMATE COOLDOWN FINISHED. Ready to use again!");
	}
}