using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AsaltTurret : MonoBehaviour
{
	enum Type
	{
		Point,
		Target,
		Direction,
	}

	[Header("スキルのタイプ（Point推奨）")]
	[SerializeField] Type m_skillType = Type.Point;

	[Header("スキルの範囲表示オブジェクト")]
	[SerializeField] GameObject m_skillBasePoint;

	// ★修正：感度は削除しました。これ一つで「最大距離」を決めます。
	[Header("スキルの最大距離（スティック最大＝この距離）")]
	[SerializeField] float m_skillRange = 5.0f;

	[Header("設置するタレットのPrefab")]
	[SerializeField] GameObject m_turretPrefab;

	[Header("スキルクールダウン（秒）")]
	[SerializeField] float m_cooldownTime = 5.0f;

	Vector2 m_skillDirection;
	// strength変数は使わなくなりましたが、判定用にmagnitude計算は残します
	bool m_approvalSkill;

	PlayerInput m_playerInput;
	Status m_playerStatus;

	private bool m_isCoolingDown = false;
	private float m_nextSkillReadyTime = 0f;

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_playerStatus = GetComponent<Status>();
	}

	private void Start()
	{
		if (m_skillBasePoint != null)
			m_skillBasePoint.SetActive(false);
	}

	private void OnEnable()
	{
		m_playerInput.actions["Skill1"].started += OnPreparation;
		m_playerInput.actions["Skill1"].canceled += OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed += OnSkillCancel;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Skill1"].started -= OnPreparation;
		m_playerInput.actions["Skill1"].canceled -= OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed -= OnSkillCancel;
	}

	void OnPreparation(InputAction.CallbackContext context)
	{
		if (m_isCoolingDown) return;

		if (m_skillBasePoint != null)
		{
			m_skillBasePoint.SetActive(true);
			// プレイヤーの足元からスタート
			m_skillBasePoint.transform.position = transform.position;
			m_skillBasePoint.transform.rotation = transform.rotation;
		}

		m_approvalSkill = true;
	}

	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		if (m_skillBasePoint != null)
			m_skillBasePoint.SetActive(false);

		if (m_approvalSkill && !m_isCoolingDown)
		{
			if (m_turretPrefab != null)
			{
				Vector3 spawnPosition = transform.position;
				Quaternion baseRotation = Quaternion.identity;

				switch (m_skillType)
				{
					case Type.Point:
						if (m_skillBasePoint != null)
						{
							spawnPosition = m_skillBasePoint.transform.position;
							baseRotation = transform.rotation;
						}
						break;
					case Type.Target:
					case Type.Direction:
						spawnPosition = transform.position;
						if (m_skillBasePoint != null)
							baseRotation = m_skillBasePoint.transform.rotation;
						break;
				}

				GameObject newTurret = Instantiate(m_turretPrefab, spawnPosition, baseRotation);

				if (m_playerStatus != null)
				{
					var turretScript = newTurret.GetComponent<Turret>();
					if (turretScript != null)
					{
						turretScript.SetAttackPower(m_playerStatus.GetAttackPower());
					}
				}

				StartCooldown();
			}
		}
		m_approvalSkill = false;
	}

	void OnSkillCancel(InputAction.CallbackContext context)
	{
		if (m_skillBasePoint != null)
			m_skillBasePoint.SetActive(false);
		m_approvalSkill = false;
	}

	private void StartCooldown()
	{
		m_isCoolingDown = true;
		m_nextSkillReadyTime = Time.time + m_cooldownTime;
	}

	private void FixedUpdate()
	{
		if (m_isCoolingDown)
		{
			if (Time.time >= m_nextSkillReadyTime)
			{
				m_isCoolingDown = false;
				Debug.Log("スキル準備完了");
			}
			return;
		}

		if (!m_approvalSkill) return;
		if (m_skillBasePoint == null) return;

		// 入力取得
		m_skillDirection = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();

		switch (m_skillType)
		{
			case Type.Target:
			case Type.Direction:
				if (m_skillDirection.magnitude > 0.2f)
				{
					Vector3 direction = new Vector3(m_skillDirection.x, 0, m_skillDirection.y);
					if (direction != Vector3.zero)
					{
						m_skillBasePoint.transform.rotation = Quaternion.LookRotation(direction);
					}
				}
				break;

			// ★ここを修正：一番シンプルにしました
			case Type.Point:
				// スティックの入力(0.0~1.0) に Range をそのまま掛け算します。
				// スティック半分(0.5)なら、2.5m先に移動。
				// スティック最大(1.0)なら、5.0m先に移動。
				Vector3 offset = new Vector3(m_skillDirection.x, 0, m_skillDirection.y) * m_skillRange;

				Vector3 targetPos = transform.position + offset;

				m_skillBasePoint.transform.position = new Vector3(
					targetPos.x,
					transform.position.y,
					targetPos.z
				);
				break;
		}
	}
}