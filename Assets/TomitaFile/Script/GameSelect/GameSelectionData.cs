using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSelectionData", menuName = "Game/GameSelectionData")]
public class GameSelectionData : ScriptableObject
{
	// 選択されたキャラID一覧(参加プレイヤー分)
	[Header("今回遊ぶ人数")]
	public int selectedPlayerCount;

	[Header("=== Character Select ===")]
	public List<GameObject> selectedCharacterIds = new List<GameObject>();

	// 選択されたステージID
	[Header("=== Stage ===")]
	//public int selectedStageId;

	// 最大人数(4人)
	[Header("== Player Select ===")]
	public int maxPlayerCount = 4;

	// 入室した人数
	public int m_joinedPlayerCount;

	[Header("=== Debug ===")]
	[SerializeField] int m_currentPlayerCount;
	[SerializeField] bool m_allPlayerReady;

	// 既に使用されているキャラか
	//public bool IsCharacterUsed(int characterId)
	//{
	//	return selectedCharacterIds.Contains(characterId);
	//}

	// 全員キャラ選択が完了したか
	public bool IsAllPlayerReady()
	{
		return selectedCharacterIds.Count >= m_joinedPlayerCount;
	}

	public void OnPlayerJoined()
	{
		m_joinedPlayerCount++;
	}

	// ゲーム開始前に初期化
	public void ResetData()
	{
		selectedCharacterIds.Clear();
		UpdateDebug();
	}

	// Debug用
	public void UpdateDebug()
	{
		m_currentPlayerCount = selectedCharacterIds.Count;
		m_allPlayerReady = IsAllPlayerReady();
	}
}
