using UnityEngine;
using UnityEngine.InputSystem; // Input Systemを直接リッスンするため
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
	[Header("Shooting Settings")]
	[SerializeField] GameObject bulletPrefab;
	[SerializeField] Transform firePoint; // 弾の発射位置 
	[SerializeField] float shootRate = 0.5f; // 発射レート（秒間に何発撃てるか、小さいほど速い）
	[SerializeField] float baseBulletSpeed = 20f; // 弾の基本速度

	[Header("Ammo and Reload")]
	[SerializeField] int maxAmmo = 20; // マガジン内の最大弾数
	[SerializeField] float reloadDuration = 3f; // マガジンリロードにかかる時間

	// 射撃方向線用のフィールド
	[Header("Aim Line Settings")]
	[SerializeField] LineRenderer aimLineRenderer; // Line Rendererコンポーネンスへの参照
	[SerializeField] float aimLineLength = 10f; // 線の長さ
	[SerializeField] Color aimLineStartColor = Color.blue;
	[SerializeField] Color aimLineEndColor = Color.cyan;
	[SerializeField] float aimLineWidth = 0.1f;

	private PlayerInput m_playerInput; // ShootingでもInput Systemをリッスンするため
	private float currentBulletSpeed; // 現在の弾速を保持する変数
	private int currentAmmo; // 現在の弾数
	private bool isReloading = false; // マガジンリロード中かどうか

	private bool canShoot = true; // 連射速度制御用フラグ
	private float shootTimer = 0f; // 連射速度制御用タイマー

	private bool isShootingInputPressed = false; // 射撃入力が押されているかどうか

	// PlayerController1から渡される値
	private Vector3 currentLookDirection;
	private Vector2 currentLookInput;
	private bool isUltActive = false; // Ultがアクティブかどうか

	Animator m_animator;

	// 外部から設定するためのプロパティ（主にPlayerController1から設定される）
	public float CurrentBulletSpeed
	{
		get { return currentBulletSpeed; }
		set { currentBulletSpeed = value; }
	}

	void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>(); // Input Systemを取得
		m_animator = GetComponent<Animator>();
		currentBulletSpeed = baseBulletSpeed;
		currentAmmo = maxAmmo;

		// Line Rendererの初期設定
		if (aimLineRenderer == null)
		{
			aimLineRenderer = GetComponent<LineRenderer>();
			if (aimLineRenderer == null)
			{
				Debug.LogWarning("Line Renderer not found on Player object. Adding one automatically.");
				aimLineRenderer = gameObject.AddComponent<LineRenderer>();
				aimLineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 仮のマテリアル
				aimLineRenderer.startWidth = aimLineWidth;
				aimLineRenderer.endWidth = aimLineWidth * 0.5f;
				aimLineRenderer.startColor = aimLineStartColor;
				aimLineRenderer.endColor = aimLineEndColor;
				aimLineRenderer.positionCount = 2;
				aimLineRenderer.useWorldSpace = true;
			}
		}
		aimLineRenderer.enabled = false; // 最初は非表示
	}

	private void OnEnable()
	{
		// 射撃入力のみここでリッスン
		m_playerInput.actions["Shot"].performed += OnShotInputPerformed;
		m_playerInput.actions["Shot"].canceled += OnShotInputCanceled;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Shot"].performed -= OnShotInputPerformed;
		m_playerInput.actions["Shot"].canceled -= OnShotInputCanceled;
	}

	private void OnShotInputPerformed(InputAction.CallbackContext context)
	{
		isShootingInputPressed = true;
		m_animator.SetBool("SetUp", true);
	}

	private void OnShotInputCanceled(InputAction.CallbackContext context)
	{
		isShootingInputPressed = false;
		m_animator.SetBool("SetUp", false);
	}

	// PlayerController1からルック方向とルック入力を受け取るメソッド
	public void SetLookParameters(Vector3 lookDirection, Vector2 lookInput)
	{
		currentLookDirection = lookDirection;
		currentLookInput = lookInput;
	}

	// PlayerController1からUltの状態を受け取るメソッド
	public void SetUltActiveState(bool active)
	{
		isUltActive = active;
	}

	void Update() // Shootingコンポーネント自身がUpdateを持つ
	{
		UpdateShootTimer();
		HandleShooting();
		UpdateAimLine();

		// UI表示のためのデバッグログ（後で実際のUIに置き換える）
		if (isReloading)
		{
			Debug.Log("Reloading... (UI Placeholder)");
		}
		if (isUltActive) // この情報はPlayerController1から受け取る
		{
			Debug.Log("ULTIMATE ACTIVE! (UI Placeholder)");
		}
		// PlayerController1でUltのクールダウンを管理するので、ここでは表示しない
	}

	private void UpdateShootTimer()
	{
		if (!canShoot)
		{
			shootTimer -= Time.deltaTime;
			if (shootTimer <= 0)
			{
				canShoot = true;
			}
		}
	}

	private void HandleShooting()
	{
		if (isShootingInputPressed && canShoot && !isReloading)
		{
			if (isUltActive || currentAmmo > 0)
			{
				Shoot(currentLookDirection);
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
	}

	private void Shoot(Vector3 shootDirection)
	{
		if (bulletPrefab != null && firePoint != null)
		{
			GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

			Bullet bullet = bulletInstance.GetComponent<Bullet>();
			if (bullet != null)
			{
				bullet.Initialize(shootDirection, currentBulletSpeed);
			}
		}
		else
		{
			Debug.LogWarning("Bullet Prefab or Fire Point is not set in PlayerShooting.");
		}
	}

	public void ForceReload()
	{
		if (!isReloading)
		{
			StartCoroutine(Reload());
		}
	}

	IEnumerator Reload()
	{
		isReloading = true;
		Debug.Log("Start Reloading...");
		yield return new WaitForSeconds(reloadDuration);
		currentAmmo = maxAmmo;
		isReloading = false;
		Debug.Log("Reload Complete! Ammo: " + currentAmmo);
	}

	private void UpdateAimLine()
	{
		// 射撃入力があるか、Look入力がある場合に照準線を表示
		if (isShootingInputPressed || currentLookInput.magnitude > 0.1f)
		{
			if (!aimLineRenderer.enabled)
			{
				aimLineRenderer.enabled = true;
			}

			if (firePoint != null)
			{
				Vector3 startPoint = firePoint.position;
				Vector3 endPoint = startPoint + currentLookDirection * aimLineLength;
				aimLineRenderer.SetPosition(0, startPoint);
				aimLineRenderer.SetPosition(1, endPoint);
			}
		}
		else
		{
			if (aimLineRenderer.enabled)
			{
				aimLineRenderer.enabled = false;
			}
		}
	}

	public int GetCurrentAmmo()
	{
		return currentAmmo;
	}

	public int GetMaxAmmo()
	{
		return maxAmmo;
	}

	public bool IsReloading()
	{
		return isReloading;
	}

	public float GetBaseBulletSpeed()
	{
		return baseBulletSpeed;
	}
}