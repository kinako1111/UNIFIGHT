using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AssaultSkill1: MonoBehaviour
{
	enum Type
	{
		Point,
		Target,
		Direction,
	}

	[Header("スキルのタイプ"), SerializeField]
	Type m_skillType;

	[Header("スキルの範囲"), SerializeField]
	GameObject m_skillBasePoint;

	[Header("スキルの感度"), SerializeField]
	[Range(0.1f, 3.0f)]
	float m_skillSensitivity = 1.0f;

	[Header("スキルの最大範囲"), SerializeField]
	float m_skillRange = 5.0f;

	// 設置するタレットのPrefab
	[Header("設置するタレットのPrefab"), SerializeField]
	GameObject m_turretPrefab;

	// スキルクールダウン時間 (秒)
	[Header("スキルのクールダウン時間 (秒)"), SerializeField]
	float m_skillCooldownTime = 10.0f; // ★10秒に設定

	Vector2 m_skillDirection;
	float m_strength;
	bool m_approvalSkill;

	PlayerInput m_playerInput;
	Animator m_animator;

	private bool m_isCoolingDown = false; // クールダウン中かどうかのフラグ
	private float m_nextSkillReadyTime = 0f; // 次のスキルが使用可能になる時間

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
		// クールダウン中はスキル準備不可
		if (m_isCoolingDown)
		{
			Debug.Log("スキルはクールダウン中です。残り: " + (m_nextSkillReadyTime - Time.time).ToString("F1") + "秒");
			return;
		}

		m_skillBasePoint.SetActive(true);
		m_approvalSkill = true;

		//スキルの位置を初期値に戻す (プレイヤーの足元)
		m_skillBasePoint.transform.position = new Vector3(
			transform.position.x,
			m_skillBasePoint.transform.position.y,
			transform.position.z);
	}

	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		m_skillBasePoint.SetActive(false);

		// クールダウン中でない、かつスキルが承認されている場合のみ発動
		if (m_approvalSkill && !m_isCoolingDown)
		{
			if (m_turretPrefab != null)
			{
				Vector3 spawnPosition;
				Quaternion baseRotation;

				switch (m_skillType)
				{
					case Type.Point:
						spawnPosition = m_skillBasePoint.transform.position;
						baseRotation = transform.rotation;
						break;
					case Type.Target:
					case Type.Direction:
						spawnPosition = transform.position;
						baseRotation = m_skillBasePoint.transform.rotation;
						break;
					default:
						spawnPosition = transform.position;
						baseRotation = transform.rotation;
						break;
				}

				GameObject newTurret = Instantiate(m_turretPrefab, spawnPosition, baseRotation); // 修正: Quaternion.identity -> baseRotation
				Debug.Log("タレットを生成しました: " + newTurret.name + " at " + spawnPosition);

				// スキル発動後、クールダウンを開始
				StartCooldown();
			}
			else
			{
				Debug.LogWarning("タレットのPrefabが設定されていません。");
			}
		}
		m_approvalSkill = false; // スキルを離した時点で承認状態をリセット
	}

	void OnSkillCancel(InputAction.CallbackContext context)
	{
		m_skillBasePoint.SetActive(false);
		m_approvalSkill = false;
	}

	private void FixedUpdate()
	{
		// クールダウン中かどうかの状態を更新
		if (m_isCoolingDown)
		{
			if (Time.time >= m_nextSkillReadyTime)
			{
				m_isCoolingDown = false;
				Debug.Log("スキルが使用可能になりました！");
			}
		}

		// スキル承認中でない、またはクールダウン中の場合は処理をスキップ
		if (!m_approvalSkill || m_isCoolingDown) return;

		m_skillDirection = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		m_strength = m_skillDirection.magnitude; // スティックの傾き度合い (0.0～1.0)

		switch (m_skillType)
		{
			case Type.Target:
				if (m_strength > 0.2f)
				{
					Vector3 direction = new Vector3(m_skillDirection.x, 0, m_skillDirection.y);
					if (direction != Vector3.zero)
					{
						m_skillBasePoint.transform.rotation = Quaternion.LookRotation(direction);
					}
				}
				break;

			case Type.Point:
				// スティックが少しでも傾いていたら処理を行う
				if (m_strength > 0.1f)
				{
					float effectiveStrength = m_strength * m_skillSensitivity;
					float finalOffsetDistance = Mathf.Min(effectiveStrength, 1.0f) * m_skillRange;

					Vector3 directionVector = (m_skillDirection.magnitude > 0) ?
											  new Vector3(m_skillDirection.x, 0, m_skillDirection.y).normalized :
											  Vector3.forward;

					Vector3 desiredOffset = directionVector * finalOffsetDistance;
					m_skillBasePoint.transform.position = transform.position + desiredOffset;

					// Y座標の調整を維持 (元のスキルBasePointのY座標を使う)
					Vector3 currentPos = m_skillBasePoint.transform.position;
					m_skillBasePoint.transform.position = new Vector3(currentPos.x, m_skillBasePoint.transform.position.y, currentPos.z);
				}
				else
				{
					// スティックが傾いていない場合はプレイヤーの足元に位置を戻す
					m_skillBasePoint.transform.position = new Vector3(
						transform.position.x,
						m_skillBasePoint.transform.position.y,
						transform.position.z);
				}
				break;

			case Type.Direction:
				if (m_strength > 0.2f)
				{
					Vector3 direction = new Vector3(m_skillDirection.x, 0, m_skillDirection.y);
					if (direction != Vector3.zero)
					{
						m_skillBasePoint.transform.rotation = Quaternion.LookRotation(direction);
					}
				}
				break;
		}
	}
	private void StartCooldown()
	{
		m_isCoolingDown = true;
		m_nextSkillReadyTime = Time.time + m_skillCooldownTime;
		Debug.Log("クールダウン開始: " + m_skillCooldownTime + "秒");
	}
}