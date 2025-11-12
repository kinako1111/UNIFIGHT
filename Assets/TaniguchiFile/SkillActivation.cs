using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// スキル発動の入力処理とUI制御を行うクラス
/// </summary>
public class SkillActivation : MonoBehaviour
{
	[Header("発動スキル"), SerializeField]
	SkillData m_skillData;

	[Header("スキルの範囲表示用オブジェクト"), SerializeField]
	GameObject m_skillBasePoint;

	[Header("スキルの感度"), SerializeField]
	float m_skillSensitivity;

	// 入力関連
	Vector2 m_skillDirection;
	float m_strength;
	bool m_approvalSkill;

	// クールタイム管理
	float m_currentCooldown = 0f;
	bool m_isCooldown = false;

	// コンポーネント参照
	PlayerInput m_playerInput;
	Animator m_animator;

	private void Awake()
	{
		// PlayerInputとAnimatorを取得
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		// 初期状態ではスキルUIを非表示
		m_skillBasePoint.SetActive(false);
	}

	private void OnEnable()
	{
		// 入力イベント登録
		m_playerInput.actions["SkillButton"].performed += OnPreparation;
		m_playerInput.actions["SkillButton"].canceled += OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed += OnSkillCancel;
	}

	private void OnDisable()
	{
		// イベント解除（メモリリーク防止）
		m_playerInput.actions["SkillButton"].performed -= OnPreparation;
		m_playerInput.actions["SkillButton"].canceled -= OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed -= OnSkillCancel;
	}

	/// <summary>
	/// スキル準備開始（ボタン押下時）
	/// </summary>
	void OnPreparation(InputAction.CallbackContext context)
	{
		if (m_isCooldown) return;

		m_skillBasePoint.SetActive(true);
		m_approvalSkill = true;

		// UIの位置をプレイヤー位置に合わせる（Yは固定）
		m_skillBasePoint.transform.position = new Vector3(
			transform.position.x,
			m_skillBasePoint.transform.position.y,
			transform.position.z);
	}

	/// <summary>
	/// スキル発動（ボタン離した時）
	/// </summary>
	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		m_skillBasePoint.SetActive(false);

		if (m_approvalSkill && !m_isCooldown)
		{
			// スキル発動処理

			// クールタイム開始
			m_isCooldown = true;
		}

		// アニメーション再生
		m_animator.SetTrigger("Use");
		m_approvalSkill = false;
	}

	/// <summary>
	/// スキルキャンセル（キャンセルボタン押下時）
	/// </summary>
	void OnSkillCancel(InputAction.CallbackContext context)
	{
		m_skillBasePoint.SetActive(false);
		m_approvalSkill = false;
	}

	private void FixedUpdate()
	{
		// クールタイム処理
		if (m_isCooldown)
		{
			m_currentCooldown -= Time.fixedDeltaTime;
			if (m_currentCooldown <= 0f)
			{
				m_isCooldown = false;
			}
			return;
		}

		if (!m_approvalSkill) return;

		// 入力方向を取得
		m_skillDirection = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		m_strength = m_skillDirection.magnitude;

		//// スキルタイプに応じてUIを更新
		//switch ()
		//{
		//	case Type.Target:


		//	case Type.Direction:
		//		if (m_strength > 0.2f)
		//		{
		//			Vector3 direction = new Vector3(m_skillDirection.x, 0, m_skillDirection.y);
		//			m_skillBasePoint.transform.rotation = Quaternion.LookRotation(direction);
		//		}
		//		break;

		//	case Type.Point:
		//		if (m_strength > 0.1f)
		//		{
		//			m_skillBasePoint.transform.position = new Vector3(
		//				m_skillDirection.x/* * m_skillRange */+ transform.position.x,
		//				m_skillBasePoint.transform.position.y,
		//				m_skillDirection.y /* * m_skillRange */+ transform.position.z
		//			);
		//		}
		//		break;
		//}
	}
}