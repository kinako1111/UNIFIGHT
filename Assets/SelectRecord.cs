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

		//selectionの配列の大きさが、現在のプレイヤーの人数と同じになればシーン遷移
		if(selection.Count == m_playerCount)
		{
			var sheneChager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneChanger>();
			sheneChager.ChangeScene(m_selectMapID);
		}
	}

	//もう一度続ける場合とかに中身を変えれるように
	public void SelectionClear()
	{
		selection.Clear();
	}

	public void SetPlayerCount(int playerCount)
	{
		m_playerCount = playerCount;
	}

	public Dictionary<PlayerInput, int> GetDictionary() => selection;
	public int GetMaxPlayerCount() => MaxPlayerCount;
	public int GetPlayerCount() => m_playerCount;
	public int GetMapID() => m_selectMapID;

	// 既存の Decision や SetSelection もそのまま残す
	public void Decision(int mapID) => m_selectMapID = mapID;
}