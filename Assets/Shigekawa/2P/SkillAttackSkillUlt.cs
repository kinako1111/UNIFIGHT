using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillAttackSkillUlt : MonoBehaviour
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
	float m_skillSensitivity;

	[Header("スキルの最大範囲"), SerializeField]
	float m_skillRange;

	Vector2 m_skillDirection;
	float m_strength;
	bool m_approvalSkill;

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
		m_playerInput.actions["SkillCancel"].performed += OnSkillCancel;
	}

	void OnPreparation(InputAction.CallbackContext context)
	{
		m_skillBasePoint.SetActive(true);
		m_approvalSkill = true;

		//スキルの位置を初期値に戻す
		m_skillBasePoint.transform.position = new Vector3(
			transform.position.x,
			m_skillBasePoint.transform.position.y,
			transform.position.z);
	}

	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		m_skillBasePoint.SetActive(false);

		//スキル発動
		if(m_approvalSkill)
		{

		}
	}

	void OnSkillCancel(InputAction.CallbackContext context)
	{
		//スキル発動キャンセル
		m_skillBasePoint.SetActive(false);
		m_approvalSkill = false;
	}

	private void FixedUpdate()
	{
		if (!m_approvalSkill) return;
		//入力検知　
		//スキル入力をVector2で取得
		m_skillDirection = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		Debug.Log(m_skillDirection);

		//スキルの方向決定の押し込みの強さを取得
		m_strength = m_skillDirection.magnitude;
		Debug.Log(m_strength);

		//スキルのボタン使用でスキルの発動準備

		switch (m_skillType)
		{
			case Type.Target:
				if (m_strength > 0.2f) // 少しでも倒れていたら方向を更新
				{
					Vector3 direction = new Vector3(m_skillDirection.x, 0, m_skillDirection.y);
					m_skillBasePoint.transform.rotation = Quaternion.LookRotation(direction);
				}
				break;

			case Type.Point:
				if(m_strength > 0.1f)
				{
					m_skillBasePoint.transform.position = new Vector3(
						m_skillDirection.x * m_skillRange + transform.position.x,
						m_skillBasePoint.transform.position.y,
						m_skillDirection.y * m_skillRange + transform.position.z
						);
				}
				break;

			case Type.Direction:
				if (m_strength > 0.2f) // 少しでも倒れていたら方向を更新
				{
					Vector3 direction = new Vector3(m_skillDirection.x, 0, m_skillDirection.y);
					m_skillBasePoint.transform.rotation = Quaternion.LookRotation(direction);
				}
				break;
		}
		//方向スキル　例バナのソラビ
		//受け取るのは右スティックで方向、スキルのボタン入力
		//スキルの方向は変数として保持

		//範囲スキル　例ポケモンユナイトサーナイトの未来予知　
		//傾きの強さ、方向を参照
		//自身の位置、最長の位置を割合で当てる

		//対象指定スキル
		//指定した方向ににいる敵に向けて必中攻撃する


		//ボタン入力の間、スキルの攻撃範囲の表示をす

		//ボタンを離したら入力を決定、参照してスキルの発動
	}
}
