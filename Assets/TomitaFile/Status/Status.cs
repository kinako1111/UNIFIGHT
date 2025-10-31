using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;

public class Status : MonoBehaviour
{
	public enum Name
	{
		A,
		B,
		C,
		D,
		Golem,
		Necro,
		Skelton,
		Mushroom,
		Cactus,
		Tower,
		Turret,
		Length,
	}

	[SerializeField] Unit unit;
	[SerializeField] Name m_name;
	[SerializeField] Animator m_animator;
	[Header("ダメージモーション開始までのダメージのカウント"),SerializeField]int KnockBackDamage;

	UnitData unitData;

	// ステータス情報
	string character;
	int hp;
	int attackPower;
	float magnitication;
	float speed;
	float passive;
	float skill1CoolTime;
	float skill2CoolTime;
	float urthCoolTime;
	float skill1Duration;
	float skill2Duration;
	float urthDuration;
	string range;
	int maxHp;
	string speedText;
	bool isDeath;
	int m_knockBackDamageCount;

	void Start()
	{
		// 名前を元にunitDataを取得
		unitData = unit.dataArray[(int)m_name];

		// unitDataからステータスを初期化
		character = unitData.Character;
		hp = unitData.Hp;
		attackPower = unitData.Attackpower;
		magnitication = unitData.Magnification;
		speed = unitData.Speed;
		passive = unitData.Passive;
		skill1CoolTime = unitData.Skill1cooltime;
		skill2CoolTime = unitData.Skill2cooltime;
		urthCoolTime = unitData.Urthcooltime;
		skill1Duration = unitData.Skill1duration;
		skill2Duration = unitData.Skill2duration;
		urthDuration = unitData.Urthduration;
		range = unitData.Range;
		maxHp = unitData.Hp;
		speedText = unitData.Speedtext;
		isDeath = false;
		m_knockBackDamageCount = KnockBackDamage;

		
	}

	void FixedUpdate()
	{
		// 必要に応じて物理更新処理を追加

		//ノックバックまでのカウントが０の時
		if (m_knockBackDamageCount < 0)
		{
			m_animator.SetTrigger("Damage");
			m_knockBackDamageCount = KnockBackDamage;
		}
	}

	// ステータス取得メソッド
	public string GetCharacter() => character;
	public int GetHp() => hp;
	public int GetAttackPower() => attackPower;
	public float GetMagnitication() => magnitication;
	public float GetSpeed() => speed;
	public float GetPassive() => passive;
	public float GetSkill1CoolTime() => skill1CoolTime;
	public float GetSkill2CoolTime() => skill2CoolTime;
	public float GetUrthCoolTime() => urthCoolTime;
	public float GetSkill1Duration() => skill1Duration;
	public float GetSkill2Duration() => skill2Duration;
	public float GetUrthDuration() => urthDuration;
	public int GetMaxHp() => maxHp;
	public string GetRange() => range;
	public string GetSpeedText() => speedText;
	public bool GetDeath() => isDeath;

	// ダメージ処理
	public void Damage(int damage)
	{
		//死体蹴りはしない
		if(isDeath)
		{
			return;
		}
		hp -= damage;
		Debug.Log(damage);
		m_knockBackDamageCount -= damage;

		// DamageUI表示処理（TakeDamageと連携）
		var takeDamage = GetComponent<TakeDamage>();
		if (takeDamage != null)
		{
			takeDamage.ShowDamageUI(damage);
		}

		if (hp <= 0)
		{
			hp = 0;
			Debug.Log($"{character} は倒された！");
			m_animator.SetTrigger("Death");
			Destroy(gameObject, 2f);
			// 死亡処理などを追加可能
			isDeath = true;
			Debug.Log("aa");
		}
	}

	// 回復処理
	public void Heal(int heal)
	{
		hp = Mathf.Min(hp + heal, maxHp);
		m_animator.SetTrigger("Heal");
	}
}
