using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class PhotoChange : MonoBehaviour
{
	[SerializeField] private GameObject[] m_characterSelect;
	private int m_index = 0;

	SelectRecord m_record;

	void Start()
	{
		m_characterSelect[0].SetActive(true);

		// データを渡す先の Record を取得
		GameObject controller = GameObject.FindGameObjectWithTag("GameController");
		if (controller != null)
		{
			m_record = controller.GetComponent<SelectRecord>();
		}
	}

	public void Right()
	{
		m_characterSelect[m_index].SetActive(false);
		m_index++;
		if (m_index >= m_characterSelect.Length)
			m_index = 0;
		m_characterSelect[m_index].SetActive(true);
	}
	public void Left()
	{
		m_characterSelect[m_index].SetActive(false);
		m_index--;
		if (m_index < 0)
			m_index = m_characterSelect.Length - 1;
		m_characterSelect[m_index].SetActive(true);
	}

    public void OnClickDecision()
    {
        var currentES = EventSystem.current as MultiplayerEventSystem;

        if (currentES != null)
        {
            // 操作している PlayerInput を取得
            PlayerInput pi = currentES.GetComponentInParent<PlayerInput>();

            if (pi != null)
            {
                // --- 追加：キーボードデバイスが含まれているかチェック ---
                bool isKeyboard = false;
                foreach (var device in pi.devices)
                {
                    if (device is Keyboard || device is Mouse)
                    {
                        isKeyboard = true;
                        break;
                    }
                }

                if (isKeyboard)
                {
                    Debug.Log("キーボードでの参加は許可されていません。");
                    return; // ここで処理を中断
                }

                Debug.Log($"[Decision] Player:{pi.playerIndex} selected ID:{m_index}");

                if (m_record != null)
                {
                    m_record.Register(pi, m_index);
                }
            }
        }
    }
}
