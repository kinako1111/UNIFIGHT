using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharaStatus : MonoBehaviour
{
    private enum StatusType
    {
        Hp,         // HP
        Attack,     // 攻撃力
		Range,      // 射程
        Speed,      // 移動速度
	}

	[SerializeField] Status m_status; // キャラクターの速度などを保持するStatusスクリプト
    [SerializeField] TextMeshProUGUI[] m_statusText;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		m_statusText[(int)StatusType.Hp].text = "HP / " + m_status.GetMaxHp().ToString();
		m_statusText[(int)StatusType.Attack].text = "攻撃力 / " +  m_status.GetAttackPower().ToString();
		m_statusText[(int)StatusType.Range].text =  "射程距離 / " + m_status.GetRange().ToString();
		m_statusText[(int)StatusType.Speed].text = "移動速度 / " + m_status.GetSpeed().ToString();
	}
}
