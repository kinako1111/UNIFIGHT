using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnClick : MonoBehaviour
{

    private enum ButtonType
    {
		AssaultRifle,   // アサルトライフル
		ShotGun,        // ショットガン
        Sniper,         // スナイパー
		Potion,         // ポーション
	}

    [SerializeField] GameObject[] m_charactors;
    [SerializeField] Button[] m_buttons;

    [SerializeField] Button b;


    // Start is called before the first frame update
    void Start()
    {
        // ボタンコンポーネントの取得
        for (int i = 0; i < m_buttons.Length; i++) 
        {
            m_buttons[i] = GetComponent<Button>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
