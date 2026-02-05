using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class SelectRecord : MonoBehaviour
{
    [Header("最大人数"), SerializeField] const int MaxPlayerCount = 4;
    [Header("プレイヤーの人数"), SerializeField] int m_playerCount = 1;
    [Header("選択したマップ"), SerializeField] int m_selectMapID = 1;

    // キーを InputDevice[] にすることで、シーン遷移後の参照切れを防ぐ
    private Dictionary<InputDevice[], int> selection = new Dictionary<InputDevice[], int>();

    public void Register(PlayerInput playerInput, int prefabID)
    {
        // キーボード・マウスでの参加をブロックするガード
        if (playerInput.devices.Any(d => d is Keyboard || d is Mouse))
        {
            Debug.Log("Keyboard/Mouse is not allowed.");
            return;
        }

        var devices = playerInput.devices.ToArray();

        // デバイスの組み合わせを確認して登録
        var existingKey = selection.Keys.FirstOrDefault(d => d.SequenceEqual(devices));
        if (existingKey != null)
        {
            selection[existingKey] = prefabID;
        }
        else
        {
            selection[devices] = prefabID;
        }

        // 全員の選択が完了したらシーン遷移
        if (selection.Count == m_playerCount)
        {
            var sceneChanger = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneChanger>();
            sceneChanger.ChangeScene(m_selectMapID);
        }
    }

    public void SelectionClear() => selection.Clear();
    public void SetPlayerCount(int playerCount) => m_playerCount = playerCount;

    // 型を InputDevice[] に変更して取得
    public Dictionary<InputDevice[], int> GetDictionary() => selection;
    public void Decision(int mapID) => m_selectMapID = mapID;
}