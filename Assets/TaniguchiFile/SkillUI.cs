using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillUI : MonoBehaviour
{
	enum Type
	{
		Point,
		Target,
		Direction,
	}

	[Header("発動スキル"), SerializeField]
	SkillPoisonArea skill;

	[Header("スキルのタイプ"), SerializeField]
	Type m_skillType;

	[Header("スキルの範囲"), SerializeField]
	GameObject m_skillBasePoint;

	[Header("スキルの感度"), SerializeField]
	float m_skillSensitivity;

	[Header("スキルの最大範囲"), SerializeField]
	float m_skillRange;

	[Header("スキルのクールタイム（秒）"), SerializeField]
	float m_skillCooldownTime = 5f;

	Vector2 m_skillDirection;
	float m_strength;
	bool m_approvalSkill;

	float m_currentCooldown = 0f;
	bool m_isCooldown = false;

	PlayerInput m_playerInput;
	Animator m_animator;

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		m_skillBasePoint.SetActive(false);
	}

	private void OnEnable()
	{
		m_playerInput.actions["SkillButton"].performed += OnPreparation;
		m_playerInput.actions["SkillButton"].canceled += OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed += OnSkillCancel;
	}

	private void OnDisable()
	{
		m_playerInput.actions["SkillButton"].performed -= OnPreparation;
		m_playerInput.actions["SkillButton"].canceled -= OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed -= OnSkillCancel;
	}

	void OnPreparation(InputAction.CallbackContext context)
	{
		if (m_isCooldown) return;

		m_skillBasePoint.SetActive(true);
		m_approvalSkill = true;

		m_skillBasePoint.transform.position = new Vector3(
			transform.position.x,
			m_skillBasePoint.transform.position.y,
			transform.position.z);
	}

	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		m_skillBasePoint.SetActive(false);

		if (m_approvalSkill && !m_isCooldown)
		{
			skill.InstansPoison(m_skillBasePoint.transform.position);

			// クールタイム開始
			m_isCooldown = true;
			m_currentCooldown = m_skillCooldownTime;
		}
		m_animator.SetTrigger("Use");
		m_approvalSkill = false;
	}

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

		m_skillDirection = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		m_strength = m_skillDirection.magnitude;

		switch (m_skillType)
		{
			case Type.Target:
			case Type.Direction:
				if (m_strength > 0.2f)
				{
					Vector3 direction = new Vector3(m_skillDirection.x, 0, m_skillDirection.y);
					m_skillBasePoint.transform.rotation = Quaternion.LookRotation(direction);
				}
				break;

			case Type.Point:
				if (m_strength > 0.1f)
				{
					m_skillBasePoint.transform.position = new Vector3(
						m_skillDirection.x * m_skillRange + transform.position.x,
						m_skillBasePoint.transform.position.y,
						m_skillDirection.y * m_skillRange + transform.position.z
					);
				}
				break;
		}
	}
}