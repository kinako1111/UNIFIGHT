using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI; // Coroutineのために必要
using TMPro;
using UnityEditor;

public class PlayerController1 : MonoBehaviour
{
	// 依存するShootingコンポーネント
	[SerializeField] PlayerShooting playerShooting;

	[Header("Status & Movement")]
	[SerializeField] Status m_status; // キャラクターの速度などを保持するStatusスクリプト
	[SerializeField] float gravity = 15f;
	[SerializeField] float turnSpeed = 10f; // Slerpの補間速度 (値が大きいほど速く追従)

	// スキル2関連のフィールド
	[Header("Skill 2 Settings (Bullet Speed Up)")]
	[SerializeField] float skill2BulletSpeedMultiplier = 15f; // スキル発動時の弾速倍率
	//[SerializeField] float skill2Duration; // スキル2の持続時間

	// アルティメットスキル関連のフィールド
	[Header("Ultimate Skill Settings (Infinite Ammo)")]
	//[SerializeField] float ultDuration = 20f; // Ultの持続時間
	//[SerializeField] float ultCooldown = 60f; // Ultのクールダウン時間

	// キャラクターのHpを表示
	[SerializeField] Slider m_playerSlider;
	[SerializeField] TextMeshProUGUI m_hpText;

	private CharacterController controller;
	private PlayerInput m_playerInput;
	private Transform cameraTransform;
	Animator m_animator;

	private Vector2 moveInput;
	private Vector2 lookInput;

	// ★修正: moveDirection をクラスのメンバー変数にする
	private Vector3 moveDirection;

	private float verticalVelocity;

	// 現在の向きと移動方向を外部から参照できるようにする
	public Vector3 CurrentLookDirection { get; private set; }
	public Vector2 CurrentLookInput { get { return lookInput; } } // ShootingコンポーネントのためにLookInputも公開

	// スキル関連のフラグ
	private bool isSkill2Active = false;
	private bool isUltActive = false;
	private bool canUseUlt = true;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();

		if (playerShooting == null)
		{
			playerShooting = GetComponent<PlayerShooting>();
			if (playerShooting == null)
			{
				Debug.LogError("PlayerShooting component not found. Please assign it in the Inspector or ensure it's on the same GameObject.");
			}
		}

		if (Camera.main != null)
		{
			cameraTransform = Camera.main.transform;
		}
		else
		{
			Debug.LogWarning("Main Camera not found! Please tag your camera as 'MainCamera'.");
		}
	}

	private void Start()
	{
		
	}

	private void OnEnable()
	{
		m_playerInput.actions["Move"].performed += OnMovePerformed;
		m_playerInput.actions["Move"].canceled += OnMoveCanceled;

		m_playerInput.actions["Look"].performed += OnLookPerformed;
		m_playerInput.actions["Look"].canceled += OnLookCanceled;

		m_playerInput.actions["Skill2"].performed += OnSkill2;

		m_playerInput.actions["Ult"].performed += OnUlt;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Move"].performed -= OnMovePerformed;
		m_playerInput.actions["Move"].canceled -= OnMoveCanceled;

		m_playerInput.actions["Look"].performed -= OnLookPerformed;
		m_playerInput.actions["Look"].canceled -= OnLookCanceled;

		m_playerInput.actions["Skill2"].performed -= OnSkill2;

		m_playerInput.actions["Ult"].performed -= OnUlt;
	}

	private void OnMovePerformed(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
		m_animator.SetBool("Move", true);
	}

	private void OnMoveCanceled(InputAction.CallbackContext context)
	{
		moveInput = Vector2.zero;
		m_animator.SetBool("Move", false);
	}

	private void OnLookPerformed(InputAction.CallbackContext context)
	{
		lookInput = context.ReadValue<Vector2>();
		//m_animator.SetBool("Move", false);
		m_animator.SetBool("SetUp", true);
	}

	private void OnLookCanceled(InputAction.CallbackContext context)
	{
		lookInput = Vector2.zero;
		m_animator.SetBool("SetUp", false);
	}

	private void OnSkill2(InputAction.CallbackContext context)
	{
		if (!isSkill2Active)
		{
			StartCoroutine(ActivateSkill2Coroutine());
		}
	}

	private void OnUlt(InputAction.CallbackContext context)
	{
		if (canUseUlt && !isUltActive)
		{
			StartCoroutine(ActivateUltCoroutine());
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

	void FixedUpdate()
	{
		m_playerSlider.maxValue = m_status.GetMaxHp();
		m_playerSlider.value = m_status.GetHp();
		m_hpText.text = m_status.GetHp().ToString() + " / " + m_status.GetMaxHp().ToString();

		ApplyGravity();
		CalculateMovementAndRotation();
		MoveCharacter();

		if (playerShooting != null)
		{
			playerShooting.SetLookParameters(CurrentLookDirection, lookInput);
			playerShooting.SetUltActiveState(isUltActive);
		}
	}

	private void ApplyGravity()
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

	private void CalculateMovementAndRotation()
	{
		Vector3 forward = cameraTransform.forward;
		Vector3 right = cameraTransform.right;

		forward.y = 0f;
		right.y = 0f;
		forward.Normalize();
		right.Normalize();

		// ★修正: moveDirection の宣言を削除し、クラスメンバー変数に値を代入
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
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
			CurrentLookDirection = targetLookDirection;
		}
		else
		{
			CurrentLookDirection = transform.forward;
		}
	}

	private void MoveCharacter()
	{
		// moveDirection がクラスメンバー変数になったので、直接使用できる
		Vector3 velocity = moveDirection * m_status.GetSpeed() + Vector3.up * verticalVelocity;
		controller.Move(velocity * Time.deltaTime);
	}

	IEnumerator ActivateSkill2Coroutine()
	{
		isSkill2Active = true;
		Debug.Log("Skill 2 activated! Bullet speed UP!");
		if (playerShooting != null)
		{
			playerShooting.CurrentBulletSpeed = playerShooting.GetBaseBulletSpeed() * skill2BulletSpeedMultiplier;
		}
		yield return new WaitForSeconds(m_status.GetSkill2Duration());
		if (playerShooting != null)
		{
			playerShooting.CurrentBulletSpeed = playerShooting.GetBaseBulletSpeed();
		}
		isSkill2Active = false;
		Debug.Log("Skill 2 deactivated. Bullet speed returned to normal.");
	}

	IEnumerator ActivateUltCoroutine()
	{
		isUltActive = true;
		canUseUlt = false;
		Debug.Log("ULTIMATE ACTIVATED! Infinite Ammo for " + m_status.GetUrthDuration() + " seconds!");
		if (playerShooting != null)
		{
			playerShooting.ForceReload();
		}

		yield return new WaitForSeconds(m_status.GetUrthDuration());

		isUltActive = false;
		Debug.Log("ULTIMATE DEACTIVATED. Ammo limitations back on.");
		if (playerShooting != null)
		{
			if (playerShooting.GetCurrentAmmo() <= 0)
			{
				playerShooting.ForceReload();
			}
		}

		Debug.Log("ULTIMATE COOLDOWN initiated. " + m_status.GetUrthCoolTime() + " seconds remaining.");
		yield return new WaitForSeconds(m_status.GetUrthCoolTime());
		canUseUlt = true;
		Debug.Log("ULTIMATE COOLDOWN FINISHED. Ready to use again!");
	}
}