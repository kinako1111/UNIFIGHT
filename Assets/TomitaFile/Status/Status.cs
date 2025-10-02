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
        Length,
    }

    [SerializeField]
    Unit unit;

    [SerializeField]
    Name m_name;

    UnitData unitData;

    string character;
    int hp;
    int attackPower;
    float magnitication;
    float speed;

    // Start is called before the first frame update
    void Start()
    {
        unitData = unit.dataArray[(int)m_name];

        character = unitData.Character;
        hp = unitData.Hp;
        attackPower = unitData.Attackpower;
        magnitication = unitData.Magnification;
        speed = unitData.Speed;
    }

    // Update is called once per frame
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

    public void Damage(int damage)
    {
        hp -= damage;
    }
}
