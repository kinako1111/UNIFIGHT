using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
//using UnityEngine.UIElements;
//using static UnityEngine.EventSystems.StandaloneInputModule;

public class PlayerController : MonoBehaviour
{
	[Header("移動速度"),SerializeField]
	float m_speed = 3;

	[Header("デフォルトのローテーションが変な奴がいるためその補正"),SerializeField]
	Vector3 revisionRotation;

	Animator m_animator;
	Status m_status;
	Vector2 m_inputMove;

	PlayerInput m_playerInput;
	Rigidbody m_rigidbody;
	Camera m_targetCamera;
	AutoAttack m_autoAttack;

	[SerializeField]bool m_moveApproval = true;

	public bool ActionApproval()
	{
		return m_moveApproval;
	}

	public void MoveApproval(bool approval)
	{
		m_moveApproval = approval;
	}

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();
		m_rigidbody = GetComponent<Rigidbody>();
		m_status = GetComponent<Status>();
		m_autoAttack = GetComponent<AutoAttack>();
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
		if (m_status.GetDeath()) return;
		m_inputMove = callback.ReadValue<Vector2>();
		m_animator.SetBool("Move", true);
	}

	public void OnMoveCancel(InputAction.CallbackContext callback)
	{
		m_inputMove = Vector2.zero;
		m_animator.SetBool("Move", false);
	}

    private void FixedUpdate()
    {
        // 1. 死亡チェック
        if (m_status.GetDeath()) return;

        // 2. 【重要】カメラがセットされるまで処理を中断する（これで78行目のエラーを防ぐ）
        if (m_targetCamera == null) return;

        // 3. カメラの向き（角度[deg]）取得（ここが元の78行目）
        var cameraAngleY = m_targetCamera.transform.eulerAngles.y;

        // 操作入力と鉛直方向速度から、現在速度を計算
        var moveVelocity = new Vector3(
            m_inputMove.x * m_status.GetSpeed(),
            0,
            m_inputMove.y * m_speed
        );

        // カメラの角度分だけ移動量を回転
        moveVelocity = Quaternion.Euler(0, cameraAngleY, 0) * moveVelocity;

        // 以降、攻撃中などの移動制限
        if (m_autoAttack.IsAttack || !m_moveApproval)
        {
            // 移動不可の時でも、重力などの鉛直方向の速度を維持したい場合は調整が必要ですが、
            // ひとまず移動停止にするなら以下：
            m_rigidbody.velocity = new Vector3(0, m_rigidbody.velocity.y, 0);
            return;
        }

        m_rigidbody.velocity = moveVelocity;

        if (moveVelocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveVelocity);
            targetRotation *= Quaternion.Euler(revisionRotation);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
        }
    }

    public void SetCamera(Camera playerCamera)
{
    m_targetCamera = playerCamera;
    
    // 子要素にあるCameraRotateUiを探してカメラを渡す
    var rotateUi = GetComponentInChildren<CameraRotateUi>();
    if (rotateUi != null)
    {
        rotateUi.SetTargetCamera(playerCamera);
    }
}
}
