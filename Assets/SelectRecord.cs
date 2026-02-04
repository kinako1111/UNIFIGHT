using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectRecord : MonoBehaviour
{
	[Header("最大人数"), SerializeField] const int MaxPlayerCount = 4;
	[Header("プレイヤーの人数"), SerializeField] int m_playerCount = 1;
	[Header("選択したマップ"), SerializeField] int m_selectMapID = 1;

	[Header("選んだキャラ")]
	private Dictionary<PlayerInput, int> selection = new Dictionary<PlayerInput, int>();

	// 他の箇所で使うロジックを維持
	public void Register(PlayerInput playerInput, int prefabID)
	{
		// Addではなくインデクサを使うことで、上書きを許容しエラーを防ぐ
		selection[playerInput] = prefabID;
	}

	public void OnPlayerJoined(PlayerInput playerInput)
	{
		m_playerCount = Mathf.Min(m_playerCount + 1, MaxPlayerCount);
		print($"プレイヤー#{playerInput.user.index}が入室！");
	}

	public void OnPlayerLeft(PlayerInput playerInput)
	{
		m_playerCount = Mathf.Max(0, m_playerCount - 1);
		if (selection.ContainsKey(playerInput)) selection.Remove(playerInput);
		print($"プレイヤー#{playerInput.user.index}が退室！");
	}

	public Dictionary<PlayerInput, int> GetDictionary() => selection;
	public int GetMaxPlayerCount() => MaxPlayerCount;
	public int GetMapID() => m_selectMapID;

	// 既存の Decision や SetSelection もそのまま残す
	public void Decision(int mapID) => m_selectMapID = mapID;
}