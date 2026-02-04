using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerJoinManager : MonoBehaviour
{
    [Header("1P〜4PのUI枠オブジェクトを順番に登録")]
    [SerializeField] private CharacterSelectUI[] m_playerUIs;

    // PlayerInputManagerの通知(Broadcast Messages)で実行
    public void OnPlayerJoined(PlayerInput pi)
    {
        int index = pi.playerIndex;
        if (index < m_playerUIs.Length)
        {
            // 1. 生成されたプレハブ内のEventSystemを取得
            var es = pi.GetComponent<MultiplayerEventSystem>();
            
            // 2. そのプレイヤーが操作するUIのルート(1P枠など)を紐づける
            // これをしないと決定ボタンが反応しません
            es.playerRoot = m_playerUIs[index].gameObject;

            // 3. UIを表示状態にする
            m_playerUIs[index].ActivateUI(pi);

            // 4. そのUI内の「決定ボタン」を自動でフォーカスする
            if (es.firstSelectedGameObject != null)
            {
                es.SetSelectedGameObject(es.firstSelectedGameObject);
            }
        }
    }

    public void OnPlayerLeft(PlayerInput pi)
    {
        int index = pi.playerIndex;
        if (index < m_playerUIs.Length)
        {
            m_playerUIs[index].DeactivateUI(pi);
        }
    }
}