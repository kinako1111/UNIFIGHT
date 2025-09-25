using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] float moveSpeed = 3f;
	[SerializeField] float gravity = 15f;

	CharacterController controller;
	PlayerInput m_playerInput;
	Vector2 moveInput;
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
	}

	private void OnDisable()
	{
		// Move の入力イベント解除
		m_playerInput.actions["Move"].performed -= OnMove;
		m_playerInput.actions["Move"].canceled -= OnMoveCancel;
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	private void OnMoveCancel(InputAction.CallbackContext context)
	{
		moveInput = Vector2.zero;
	}

	void Update()
	{
		// 重力を加える
		if (controller.isGrounded)
			verticalVelocity = -1f; // 地面に押し付ける
		else
			verticalVelocity -= gravity * Time.deltaTime;

		// 入力からワールドXZ方向に変換
		Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

		// 移動量
		Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;

		controller.Move(velocity * Time.deltaTime);
	}
}
