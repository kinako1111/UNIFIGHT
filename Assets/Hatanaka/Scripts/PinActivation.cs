
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class PinActivation : MonoBehaviour
{
	[Header("ピン一覧 ※プレイヤーにアタッチする等の実体化必須"), SerializeField]
	List<MonoBehaviour> skillComponents; // InspectorでISkillを持つコンポーネントを登録

	[Header("ピン一覧"), SerializeField]
	List<ISkill> skills = new List<ISkill>();

	int currentSkillIndex = 0;
	bool m_approvalSkill;

	PlayerInput m_playerInput;

	private void Awake()
	{
		m_playerInput = GetComponent<PlayerInput>();

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

	}

	private void OnEnable()
	{
		m_playerInput.actions["Pin"].performed += ctx => OnPreparation(0);
		m_playerInput.actions["Pin"].canceled += OnReleasedSkill;
		m_playerInput.actions["PinCancel"].performed += OnSkillCancel;
	}

	private void OnDisable()
	{
		m_playerInput.actions["Pin"].performed -= ctx => OnPreparation(0);
		m_playerInput.actions["Pin"].canceled -= OnReleasedSkill;
		m_playerInput.actions["PinCancel"].performed -= OnSkillCancel;
	}
	void OnPreparation(int skillIndex)
	{
		if (skillIndex >= skills.Count) return;
		Debug.Log("こいつ、動くぞ？！");
		ISkill skill = skills[skillIndex];
		currentSkillIndex = skillIndex;

		if (skill.SkillType == SkillType.Self) return;

		// スケールを設定
		skill.SkillUI.transform.localScale = new Vector3(skill.SkillRangeX, 1f, skill.SkillRangeZ);

		// プレイヤーの足元に配置
		skill.SkillUI.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

		// 表示
		skill.SkillUI.SetActive(true);
		m_approvalSkill = true;

		Debug.Log($"SkillUI activated at {skill.SkillUI.transform.position}");
	}

	void OnReleasedSkill(InputAction.CallbackContext context)
	{
		ISkill skill = skills[currentSkillIndex];
		if (skill.SkillType == SkillType.Point || skill.SkillType == SkillType.Direction)
		{
			skill.SkillUI.SetActive(false);

			// Directionのみ位置を戻す
			if (skill.SkillType == SkillType.Direction)
			{
				skill.SkillUI.transform.position = new Vector3(
					transform.position.x,
					skill.SkillUI.transform.position.y,
					transform.position.z);
			}
		}

		m_approvalSkill = false;
		ReleasedSkill();
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
