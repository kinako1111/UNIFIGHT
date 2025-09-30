using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float rotateSpeed = 500f; // 追記: 向きを変える速度
	[SerializeField] float gravity = 15f;

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
	Vector2 lookInput; // 追記: 右スティックの入力値
	float verticalVelocity;

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
	}

	private void OnDisable()
	{
		// Move の入力イベント解除
		m_playerInput.actions["Move"].performed -= OnMove;
		m_playerInput.actions["Move"].canceled -= OnMoveCancel;

		// 追記: Look の入力イベント解除
		m_playerInput.actions["Look"].performed -= OnLook;
		m_playerInput.actions["Look"].canceled -= OnLookCancel;
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
	}
}