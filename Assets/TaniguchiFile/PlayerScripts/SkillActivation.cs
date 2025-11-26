
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillActivation : MonoBehaviour
{
	[Header("スキル一覧 ※プレイヤーにアタッチする等の実体化必須"), SerializeField]
	List<MonoBehaviour> skillComponents; // InspectorでISkillを持つコンポーネントを登録

	[Header("スキル一覧"), SerializeField]
	List<ISkill> skills = new List<ISkill>();

	int currentSkillIndex = 0;
	bool m_approvalSkill;

	PlayerInput m_playerInput;
	Animator m_animator;

	// スキルごとのクールタイム管理
	Dictionary<ISkill, float> cooldownTimers = new Dictionary<ISkill, float>();

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();
		m_animator = GetComponent<Animator>();

		foreach (MonoBehaviour comp in skillComponents)
		{
			if (comp is ISkill skill)
			{
				skills.Add(skill);
				cooldownTimers[skill] = 0f; // 初期化
			}
		}
	}

	private void Start()
	{

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

		ISkill skill = skills[skillIndex];
		if (cooldownTimers[skill] > 0f) return; // クールタイム中なら発動不可

		currentSkillIndex = skillIndex;

		//UIの大きさを変更 自身対象スキルならなくてもよし
		if (skills[currentSkillIndex].SkillType == SkillType.Self) return;
		skill.SkillUI.transform.localScale = new Vector3(skill.SkillRangeX, 0.01f, skill.SkillRangeZ);
		skill.SkillUI.SetActive(true);
		m_approvalSkill = true;
	}

	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		ISkill skill = skills[currentSkillIndex];
		if(skill.SkillType == SkillType.Point || skill.SkillType == SkillType.Direction)
		{
			skill.SkillUI.SetActive(false);

			skill.SkillUI.transform.position = new Vector3(
				transform.position.x,
				skill.SkillUI.transform.position.y,
				transform.position.z);
		}

		if (m_approvalSkill && cooldownTimers[skill] <= 0f)
		{
			ReleasedSkill();
			cooldownTimers[skill] = skill.CoolDownTime; // 個別クールタイム開始
		}

		m_animator.SetTrigger("Use");
		m_approvalSkill = false;
	}

	void OnSkillCancel(InputAction.CallbackContext context)
	{
		ISkill skill = skills[currentSkillIndex];
		skill.SkillUI.SetActive(false);
		m_approvalSkill = false;

		skill.SkillUI.transform.position = new Vector3(
			transform.position.x,
			skill.SkillUI.transform.position.y,
			transform.position.z);
	}

	void ReleasedSkill()
	{
		ISkill skill = skills[currentSkillIndex];
		switch (skill.SkillType)
		{
			case SkillType.Point:
				skill.Execute(skill.SkillUI.transform.position);
				break;
			case SkillType.Direction:
				skill.Execute(transform.position, skill.SkillUI.transform.rotation);
				break;
			case SkillType.Target:
				break;
			case SkillType.Self:
				skill.Execute(transform.position, default, gameObject);
				break;
		}
	}

	private void FixedUpdate()
	{
		// クールタイム更新
		List<ISkill> keys = new List<ISkill>(cooldownTimers.Keys);
		foreach (ISkill skill in keys)
		{
			if (cooldownTimers[skill] > 0f)
			{
				cooldownTimers[skill] -= Time.fixedDeltaTime;
				if (cooldownTimers[skill] < 0f) cooldownTimers[skill] = 0f;
			}
		}

		if (!m_approvalSkill) return;

		Vector2 skillDirection = m_playerInput.actions["SkillDirection"].ReadValue<Vector2>();
		float strength = skillDirection.magnitude;

		ISkill currentSkill = skills[currentSkillIndex];
		switch (currentSkill.SkillType)
		{
			case SkillType.Direction:
				if (strength > 0.2f)
				{
					Vector3 direction = new Vector3(skillDirection.x, 0, skillDirection.y);
					currentSkill.SkillUI.transform.rotation = Quaternion.LookRotation(direction);
				}
				break;
			case SkillType.Point:
				if (strength > 0.1f)
				{
					currentSkill.SkillUI.transform.position = new Vector3(
						skillDirection.x * currentSkill.SkillDistance + transform.position.x,
						currentSkill.SkillUI.transform.position.y,
						skillDirection.y * currentSkill.SkillDistance + transform.position.z
					);
				}
				break;
		}
	}
}
