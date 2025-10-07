using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    public enum Name
    {
        A,
        B,
        C,
        D,
		Golem,
        Tower,
        Length,
    }

    [SerializeField]
    Unit unit;

    [SerializeField]
    Name m_name;

    [SerializeField]
    Animator m_animator;

    UnitData unitData;

    // キャラ情報
    string character; 

    // キャラのHp
    int hp;

    // 攻撃力
    int attackPower;

    // ダメージ倍率
    float magnitication;

    // 移動速度
    float speed;

    // パッシブの時間
    float passive;

    // スキル1クールタイム
    float skill1CoolTime;

    // スキル2クールタイム
    float skill2CoolTime;

    // ウルトクールタイム
    float urthCoolTime;

    //スキル１持続時間
    float skill1Duration;

    // スキル2持続時間
    float skill2Duration;

    // ウルトの持続時間
    float urthDuration;

    // 最大Hp
    public int maxHp;

	void Start()
    {
		//名前を元にunitDataを取得
        unitData = unit.dataArray[(int)m_name];

		//unitDataをマスターデータとしてステータスの値を取得
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
        maxHp = unitData.Hp;
	}

    void FixedUpdate()
    {
		
	}

    public string GetCharacter()
    {
        return character;
    }

    public int GetHp()
    {
        return hp;
	}

    public int GetAttackPower()
    {
        return attackPower;
    }

    public float GetMagnitication()
    {
        return magnitication;
    }


	public float GetSpeed()
    {
        return speed;
    }

    public float GetPassive()
    {
        return passive;
    }

    public float GetSkill1CoolTime()
    {
        return skill1CoolTime;
    }

    public float GetSkill2CoolTime()
    {
        return skill2CoolTime;
    }

    public float GetUrthCoolTime()
    {
        return urthCoolTime;
    }

    public float GetSkill1Duration()
    {
        return skill1Duration;
    }

    public float GetSkill2Duration()
    {
        return skill2Duration;
    }

    public float GetUrthDuration()
    {
        return urthDuration;
    }


	public void Damage(int damage)
    {
        hp -= damage;
        m_animator.SetTrigger("Damage");
    }

    public void Heal(int heal)
    {
        hp += heal;
        m_animator.SetTrigger("Heal");
    }

    public int GetMaxHp()
    {
        return maxHp;
    }
}
