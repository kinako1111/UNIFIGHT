using UnityEngine;
public interface ISkill
{
	string SkillName { get; }
	float CoolDownTime { get; }
	SkillType SkillType { get; }
	GameObject SkillUI { get; }
	float SkillRangeX { get; }
	float SkillRangeZ { get; }
	float SkillDistance { get; }

	void Execute(Vector3 position, Quaternion rotation = default, GameObject target = null);
}
