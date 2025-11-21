using System.Collections;
using System.Collections.Generic;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public class Status : MonoBehaviour
{
	public enum Name
	{
		//オブジェクト追加ごとに一つ増やすこと
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

	Animator m_animator;
	UnitData unitData;

	// ステータス情報
	string character;
	int hp;
	int attackPower;
	float magnitication;
	float passive;
	string range;
	int maxHp;
	bool isDeath;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
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
		passive = unitData.Passive;
		range = unitData.Range;
		maxHp = unitData.Hp;
		isDeath = false;
	}

	// --- ステータス取得メソッド ---
	public string GetCharacter() => character;
	public int GetHp() => hp;
	public int GetAttackPower() => attackPower;
	public float GetMagnitication() => magnitication;
	public float GetPassive() => passive;
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

		var takeDamage = GetComponent<TakeDamage>(); // TakeDamageが常に存在するとは限らない
		if (takeDamage != null)
		{
			takeDamage.ShowDamageUI(damage);
		}

		if (hp <= 0)
		{
			hp = 0;
			m_animator.SetTrigger("Death");
			Destroy(gameObject,2f);	//仮置きで２ｆ
			isDeath = true;
		}
	}

	// 回復処理
	public void Heal(int heal)
	{
		hp = Mathf.Min(hp + heal, maxHp);
		m_animator.SetTrigger("Heal");
	}
}