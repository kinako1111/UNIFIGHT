using System.Collections;
using System.Collections.Generic;
using TMPro; 
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
	[SerializeField] int KnockBackDamage;
	[SerializeField] Collider m_collider;

	Animator m_animator;
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
	bool isDeath;
	int m_knockBackDamageCount;

	// 速度変更効果を管理するための辞書 
	private Dictionary<object, float> m_speedModifiers = new Dictionary<object, float>();


	private void Awake()
	{
		m_animator = GetComponent<Animator>();
		m_collider = GetComponent<Collider>();
	}

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
		isDeath = false;
		m_knockBackDamageCount = KnockBackDamage;

		if (m_collider == null)
		{
			m_collider = gameObject.AddComponent<Collider>(); 
		}
	}

	void FixedUpdate()
	{
		if (m_knockBackDamageCount < 0)
		{
			m_animator.SetTrigger("Damage");
			m_knockBackDamageCount = KnockBackDamage;
		}
	}

	// --- ステータス取得メソッド ---
	public string GetCharacter() => character;
	public int GetHp() => hp;
	public int GetAttackPower() => attackPower;
	public float GetMagnitication() => magnitication;
	public float GetPassive() => passive;
	public float GetSkill1CoolTime() => skill1CoolTime;
	public float GetSkill2CoolTime() => skill2CoolTime;
	public float GetUrthCoolTime() => urthCoolTime;
	public float GetSkill1Duration() => skill1Duration;
	public float GetSkill2Duration() => skill2Duration;
	public float GetUrthDuration() => urthDuration;
	public int GetMaxHp() => maxHp;
	public string GetRange() => range;
	public bool GetDeath() => isDeath;

	public void Damage(int damage)
	{
		if (isDeath)
		{
			return;
		}
		hp -= damage;
		Debug.Log(damage);
		m_knockBackDamageCount -= damage;

		var takeDamage = GetComponent<TakeDamage>(); // TakeDamageが常に存在するとは限らない
		if (takeDamage != null)
		{
			takeDamage.ShowDamageUI(damage);
		}

		if (hp <= 0)
		{
			hp = 0;
			Debug.Log($"{character} は倒された！");
			m_animator.SetTrigger("Death");
			isDeath = true;
		}
	}

	void DeathAnimation() // Animatorから呼び出される想定のメソッド
	{
		if (m_collider != null) // nullチェックを追加
		{
			m_collider.enabled = false;
		}
		Destroy(gameObject, 2f);
	}

	// 回復処理
	public void Heal(int heal)
	{
		hp = Mathf.Min(hp + heal, maxHp);
		m_animator.SetTrigger("Heal");
	}

	// 速度低下効果を適用するメソッド
	public void ApplySpeedModifier(float multiplier, object source)
	{
		if (m_speedModifiers.ContainsKey(source))
		{
			m_speedModifiers[source] = multiplier;
		}
		else
		{
			m_speedModifiers.Add(source, multiplier);
		}
	}

	// 速度低下効果を解除するメソッド
	public void RemoveSpeedModifier(object source)
	{
		if (m_speedModifiers.ContainsKey(source))
		{
			m_speedModifiers.Remove(source);
		}
	}

	// 現在の実際の移動速度（基本速度にすべての速度変更効果を適用したもの）を返すメソッド
	public float GetActualSpeed()
	{
		float currentEffectiveMultiplier = 1.0f;

		if (m_speedModifiers.Count > 0)
		{
			foreach (float multiplier in m_speedModifiers.Values)
			{
				if (multiplier < currentEffectiveMultiplier)
				{
					currentEffectiveMultiplier = multiplier;
				}
			}
		}
		return speed * currentEffectiveMultiplier; // 元の speed に計算された乗数をかけて返す
	}
}