using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController1 : MonoBehaviour
{
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float rotateSpeed = 500f; // 追記: 向きを変える速度
	[SerializeField] float gravity = 15f;

	// 銃撃関連のフィールド
	[Header("Shooting Settings")]
	[SerializeField] GameObject bulletPrefab; // 発射する弾のプレハブ
	[SerializeField] Transform firePoint; // 弾の発射位置
	[SerializeField] float reloadTime = 0.5f; // リロード時間 (連射間隔)

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
	Vector2 lookInput; // 追記: 右スティックの入力値
	float verticalVelocity;

	private bool canShoot = true; // 銃を撃てるかどうか
	private float reloadTimer = 0f; // リロード用のタイマー

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		m_playerInput = GetComponent<PlayerInput>();
	}

	private void OnEnable()
	{
		// Move の入力イベント登録
		m_playerInput.actions["Move"].performed += OnMove;
		m_playerInput.actions["Move"].canceled += OnMoveCancel;

		// 追記: Look の入力イベント登録
		m_playerInput.actions["Look"].performed += OnLook;
		m_playerInput.actions["Look"].canceled += OnLookCancel;

		// 追加: Shot の入力イベント登録
		m_playerInput.actions["Shot"].performed += OnShot;
	}

	private void OnDisable()
	{
		// Move の入力イベント解除
		m_playerInput.actions["Move"].performed -= OnMove;
		m_playerInput.actions["Move"].canceled -= OnMoveCancel;

		// 追記: Look の入力イベント解除
		m_playerInput.actions["Look"].performed -= OnLook;
		m_playerInput.actions["Look"].canceled -= OnLookCancel;

		// 追加: Shot の入力イベント解除
		m_playerInput.actions["Shot"].performed -= OnShot;
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	private void OnMoveCancel(InputAction.CallbackContext context)
	{
		moveInput = Vector2.zero;
	}

	// 追記: Look の入力イベントハンドラ
	private void OnLook(InputAction.CallbackContext context)
	{
		lookInput = context.ReadValue<Vector2>();
	}

	// 追記: Look の入力キャンセルハンドラ
	private void OnLookCancel(InputAction.CallbackContext context)
	{
		lookInput = Vector2.zero;
	}

	// 追加: Shot の入力イベントハンドラ
	private void OnShot(InputAction.CallbackContext context)
	{
		if (canShoot)
		{
			Shoot();
			canShoot = false; // 発射後、撃てない状態にする
			reloadTimer = reloadTime; // リロードタイマーをセット
		}
	}

	void Update()
	{
		// 重力を加える
		if (controller.isGrounded)
		{
			verticalVelocity = -1f; // 地面に押し付ける
		}
		else
		{
			verticalVelocity -= gravity * Time.deltaTime;
		}

		if (lookInput != Vector2.zero)
		{
			transform.Rotate(Vector3.up, lookInput.x * rotateSpeed * Time.deltaTime);
		}

		// 入力からワールドXZ方向に変換
		// キャラクター自身の前方方向を基準に移動するように修正
		Vector3 move = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
		move.y = 0; // 上下方向の移動成分をリセット

		// 移動量
		Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;

		controller.Move(velocity * Time.deltaTime);

		// リロードタイマーの更新
		if (!canShoot)
		{
			reloadTimer -= Time.deltaTime;
			if (reloadTimer <= 0)
			{
				canShoot = true; // リロード完了、撃てる状態にする
			}
		}
	}

	// 銃を撃つ処理
	private void Shoot()
	{
		if (bulletPrefab != null && firePoint != null)
		{
			// 弾を生成
			GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

			// Bulletスクリプトを取得して、進行方向を設定
			Bullet bullet = bulletInstance.GetComponent<Bullet>();
			if (bullet != null)
			{
				bullet.SetDirection(firePoint.forward); // firePointの前方を発射方向とする
			}
		}
	}
}