using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class PlayerController : MonoBehaviour
{
	[SerializeField]
	float m_speed = 3;

	Animator m_animator;
	Status m_status;
	Vector2 m_inputMove;

	PlayerInput m_playerInput;
	CharacterController m_characterController;
	Camera m_targetCamera;
	AutoAttack m_autoAttack;

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();
		m_characterController = GetComponent<CharacterController>();
		m_status = GetComponent<Status>();
		m_autoAttack = GetComponent<AutoAttack>();
		m_targetCamera = Camera.main;
	}

	private void OnEnable()
	{
		m_playerInput.actions["Move"].performed += OnMove;
		m_playerInput.actions["Move"].canceled += OnMoveCancel;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Move"].performed -= OnMove;
		m_playerInput.actions["Move"].canceled -= OnMoveCancel;
	}

	public void OnMove(InputAction.CallbackContext callback)
	{
		m_inputMove = callback.ReadValue<Vector2>();
		m_animator.SetBool("Move", true);
	}

	public void OnMoveCancel(InputAction.CallbackContext callback)
	{
		m_inputMove = Vector3.zero;
		m_animator.SetBool("Move", false);
	}

	private void FixedUpdate()
	{
		// カメラの向き（角度[deg]）取得
		var cameraAngleY = m_targetCamera.transform.eulerAngles.y;

		// 操作入力と鉛直方向速度から、現在速度を計算
		var moveVelocity = new Vector3(
			m_inputMove.x * m_speed,
			0,
			m_inputMove.y * m_speed
		);
		// カメラの角度分だけ移動量を回転
		moveVelocity = Quaternion.Euler(0, cameraAngleY, 0) * moveVelocity;

		// 現在フレームの移動量を移動速度から計算
		var moveDelta = moveVelocity * Time.deltaTime;

		// CharacterControllerに移動量を指定し、オブジェクトを動かす
		//攻撃中は移動、振り向き不可
		if (m_autoAttack.IsAttack) return;
		m_characterController.Move(moveDelta);

		if (moveVelocity != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(moveVelocity);
			targetRotation *= Quaternion.Euler(0, 90f, 0); // ← ここで補正
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
		}
	}

}
