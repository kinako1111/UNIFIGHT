using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillActivation : MonoBehaviour
{
	[Header("スキル一覧 ※プレイヤーにアタッチする等の実体化必須"), SerializeField]
	List<MonoBehaviour> skillComponents ; // InspectorでISkillを持つコンポーネントを登録

	[Header("スキル一覧"), SerializeField]
	List<ISkill> skills = new List<ISkill>();

	int currentSkillIndex = 0;
	bool m_approvalSkill;
	bool m_isCooldown = false;
	float m_currentCooldown = 0f;

	PlayerInput m_playerInput;
	Animator m_animator;

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();

		// MonoBehaviourからISkillにキャストしてリスト化
		foreach (MonoBehaviour comp in skillComponents)
		{
			if (comp is ISkill skill)
			{
				skills.Add(skill);
			}
		}
	}

	private void Start()
	{
		foreach (ISkill skill in skills)
		{
			skill.SkillUI.SetActive(false);
			if(skill.SkillType == SkillType.Point)
			{
				skill.SkillUI.transform.localScale =
					new Vector3(skill.SkillRange,0.01f,skill.SkillRange);
			}
		}
	}

	private void OnEnable()
	{
		m_playerInput.actions["Skill1"].performed += ctx => OnPreparation(0);
		m_playerInput.actions["Skill2"].performed += ctx => OnPreparation(1);
		m_playerInput.actions["Skill1"].canceled += OnReleasedSkill;
		m_playerInput.actions["Skill2"].canceled += OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed += OnSkillCancel;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Skill1"].performed -= ctx => OnPreparation(0);
		m_playerInput.actions["Skill2"].performed -= ctx => OnPreparation(1);
		m_playerInput.actions["Skill1"].canceled -= OnReleasedSkill;
		m_playerInput.actions["Skill2"].canceled -= OnReleasedSkill;
		m_playerInput.actions["SkillCancel"].performed -= OnSkillCancel;
	}

	void OnPreparation(int skillIndex)
	{
		if (skillIndex >= skills.Count) return;
		if (m_isCooldown) return;

		currentSkillIndex = skillIndex; // ボタンに応じてスキル選択
		skills[currentSkillIndex].SkillUI.SetActive(true);
		m_approvalSkill = true;
	}


	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		skills[currentSkillIndex].SkillUI.SetActive(false);

		if (m_approvalSkill && !m_isCooldown)
		{
			ReleasedSkill();
			// クールタイム開始
			m_isCooldown = true;
			m_currentCooldown = skills[currentSkillIndex].CoolDownTime;
		}

		m_animator.SetTrigger("Use");
		m_approvalSkill = false;

		//スキルUIの位置をデフォルトに戻す
		skills[currentSkillIndex].SkillUI.transform.position = new Vector3(
			transform.position.x,
			skills[currentSkillIndex].SkillUI.transform.position.y,
			transform.position.z);
	}

	void OnSkillCancel(InputAction.CallbackContext context)
	{
		skills[currentSkillIndex].SkillUI.SetActive(false);
		m_approvalSkill = false;

		//スキルUIの位置をデフォルトに戻す
		skills[currentSkillIndex].SkillUI.transform.position = new Vector3(
			transform.position.x,
			skills[currentSkillIndex].SkillUI.transform.position.y,
			transform.position.z);
	}

	void ReleasedSkill()
	{
		// 現在選択中のスキルを発動
		switch (skills[currentSkillIndex].SkillType)
		{
			case SkillType.Point:
				skills[currentSkillIndex].Execute(skills[currentSkillIndex].SkillUI.transform.position);
				break;

			case SkillType.Direction:
				skills[currentSkillIndex].Execute(transform.position, skills[currentSkillIndex].SkillUI.transform.rotation);
				break;

			case SkillType.Target:

				break;

			case SkillType.Self:
				skills[currentSkillIndex].Execute(transform.position, default, gameObject);
				break;
		}
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

		// スキルタイプに応じて位置や方向を更新
		Vector2 skillDirection = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		float strength = skillDirection.magnitude;

		switch (skills[currentSkillIndex].SkillType)
		{
			case SkillType.Direction:
				if (strength > 0.2f)
				{
					Vector3 direction = new Vector3(skillDirection.x, 0, skillDirection.y);
					skills[currentSkillIndex].SkillUI.transform.rotation = Quaternion.LookRotation(direction);
				}
				break;

			case SkillType.Point:
				if (strength > 0.1f)
				{
					skills[currentSkillIndex].SkillUI.transform.position = new Vector3(
						skillDirection.x * skills[currentSkillIndex].SkillDistance + transform.position.x,
						skills[currentSkillIndex].SkillUI.transform.position.y,
						skillDirection.y * skills[currentSkillIndex].SkillDistance + transform.position.z
					);
				}
				break;

			case SkillType.Target:
				// TODO: Raycastでターゲット指定処理を追加
				//対象オブジェクトを強調表示
				break;

			case SkillType.Self:
				// TODO: 自身の強調表示
				break;
		}
	}
}