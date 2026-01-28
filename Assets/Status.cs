
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Status : MonoBehaviour
{
	public enum Name
	{
		Asult,
		ShotGun,
		Healer,
		Sniper,
		Golem,
		Necro,
		Skelton,
		Mushroom,
		Cactus,
		Tower,
		Turret,
		Bomb,
		Length,
	}

	[SerializeField] Unit unit;
	[SerializeField] Name m_name;

	Animator m_animator;
	UnitData unitData;
	StatusEffectManagerModel effectManager; // ★ Manager参照追加
	TakeDamage takeDamage;

	// イベント宣言（現在HP, 最大HP）
	public event System.Action<int, int> OnHpChanged;

	// ステータス情報
	string character;
	[SerializeField]int hp;
	int baseAttackPower; // 基本値
	int attackPower;     // 現在値（フォールバック用）
	float baseSpeed;	 //基本値
	float speed;		 //現在値
	string range;
	int maxHp;
	bool isDeath;
	const float DeathTimer = 2f;

	//速度上限と下限
	const float MaxSpeed = 10;
	const float MinSpeed = 0.5f;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
		effectManager = GetComponent<StatusEffectManagerModel>(); // ★ 自動取得
		takeDamage = GetComponent<TakeDamage>();
	}

	void Start()
	{
		//ステータスの取得
		unitData = unit.dataArray[(int)m_name];

		character = unitData.Character;
		hp = unitData.Hp;
		baseAttackPower = unitData.Attackpower;
		baseSpeed = unitData.Speed;
		maxHp = unitData.Hp;
		isDeath = false;

		//ステータスの初期化
		attackPower = baseAttackPower;
		speed = baseSpeed;
	}

	// --- ステータス取得メソッド ---
	public string GetCharacter() => character;
	public int GetHp() => hp;

	/// <summary>
	/// 攻撃力取得（Manager経由で状態異常を反映）
	/// </summary>
	public int GetAttackPower()
	{
		if (effectManager != null)
		{
			float modified = effectManager.CalculateAttackPower(baseAttackPower);
			return Mathf.RoundToInt(modified);
		}
		return attackPower; // Manager未設定時は従来値
	}

	public float GetSpeed()
	{
		if(effectManager != null)
		{
			//状態異常マネージャー設定時、変更後の速度を返す
			float modified = effectManager.CalculateMoveSpeed(baseSpeed);
			return modified;
		}
		return speed;
	}

	public int GetMaxHp() => maxHp;
	public string GetRange() => range;
	public bool GetDeath() => isDeath;

	// 攻撃力の外部変更（フォールバック用）
	public void SetAttackPower(int newPower)
	{
		if (isDeath) return;
		attackPower = Mathf.Max(0, newPower);
	}

	public void ResetAttackPower()
	{
		if (isDeath) return;
		attackPower = baseAttackPower;
	}

	public void SetSpeed(int newSpeed)
	{
		if (isDeath) return;
		speed = Mathf.Clamp(newSpeed,MinSpeed,MaxSpeed);
	}

	public void ResetSpeed()
	{
		if (isDeath) return;
		speed = baseSpeed;
	}

	public void Damage(int damage)
	{
		if (isDeath) return;

		hp -= damage;
		Debug.Log(damage);

		// Damage の末尾や hp が変わった直後に：
		OnHpChanged?.Invoke(hp, maxHp);

		if (takeDamage!= null) takeDamage.ShowDamageUI(damage);

		if (hp <= 0)
		{
			Debug.Log("死にました");
			hp = 0;
			m_animator.SetTrigger("Death");
			isDeath = true;

			// ★ 追加：状態異常を全消去
			if (effectManager != null)
			{
				effectManager.ClearAll();
			}

			Destroy(gameObject, DeathTimer);
		}
	}

	public void Heal(int heal)
	{
		int originalHP = hp;
		if (isDeath) return;
		hp = Mathf.Min(hp + heal, maxHp);
		OnHpChanged?.Invoke(hp, maxHp);
		if (takeDamage != null) takeDamage.ShowDamageUI(hp - originalHP,DamageKinds.Heal);
		Debug.Log(hp - originalHP + "回復しました");
	}
}
