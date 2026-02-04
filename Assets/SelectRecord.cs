using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectRecord : MonoBehaviour
{
	[Header("最大人数"),SerializeField]
	const int MaxPlayerCount = 4;

	[Header("プレイヤーの人数"),SerializeField]
	int m_playerCount = 1;

	[Header("選択したマップ"), SerializeField]
	int m_selectMapID = 1;

	[Header("選んだキャラ"),SerializeField]
	private Dictionary<PlayerInput,int> selection = new Dictionary<PlayerInput,int>();

	public void Register(PlayerInput playerInput,int prefabID)
	{
		selection.Add(playerInput, prefabID);	
	}

	public void Decision(int mapID)
	{
		m_selectMapID = mapID;
	}

	public void SetSelection(Dictionary<PlayerInput,int> selected)
	{
		//仮登録されている紐づけを登録
		selection = selected;
	}

	public int GetMaxPlayerCount()
	{
		return MaxPlayerCount;
	}

	public int GetMapID()
	{
		return m_selectMapID;
	}
	public Dictionary<PlayerInput,int> GetDictionary()
	{
		return selection;
	}

	public void OnPlayerJoined(PlayerInput playerInput)
	{
		m_playerCount = Mathf.Min(m_playerCount + 1,MaxPlayerCount); //最大人数を超えないように調整
		print($"プレイヤー#{playerInput.user.index}が入室！");
	}

	public void OnPlayerLeft(PlayerInput playerInput)
	{
		m_playerCount = Mathf.Max(0, m_playerCount - 1); // 0以下にならないように1減らす
		print($"プレイヤー#{playerInput.user.index}が退室！");
	}

	private void Start()
	{
		
	}

	//// PlayerInputManagerの "Player Joined" イベントにインスペクターから登録
	//public void OnPlayerJoined(PlayerInput playerInput)
	//{
	//	// プレイヤーが参加した時にDictionaryに登録（最初は選択なし）
	//	if (!selection.ContainsKey(playerInput))
	//	{
	//		selection.Add(playerInput, null);
	//		Debug.Log($"Player {playerInput.playerIndex} が参加しました。");
	//	}
	//}

	//public void OnPlayerLeft(PlayerInput playerInput)
	//{
	//	// プレイヤーが退出した時のクリーンアップ
	//	_selections.Remove(playerInput);
	//}

	//// 選択オブジェクトを更新する関数
	//public void UpdateSelection(PlayerInput player, GameObject target)
	//{
	//	if (_selections.ContainsKey(player))
	//	{
	//		_selections[player] = target;
	//	}
	//}
}
