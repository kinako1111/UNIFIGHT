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

    int character;
    int hp;
    int attackPower;
    float magnitication;
    float speed;

    // Start is called before the first frame update
    void Start()
    {
        unitData = unit.dataArray[(int)m_name];
        speed = unitData.Speed;
        Debug.Log(unitData.Hp);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    public int GetCharacter()
    {
        return character;
    }

    public int GetHp()
    {
        return hp;
	}


    public float GetSpeed()
    {
        return speed;
    }
}
