using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class CharacterSelectUI : MonoBehaviour
{
	SelectRecord m_record;
	// 決定済みのプレイヤーを管理
	[SerializeField]HashSet<PlayerInput> confirmedPlayers = new HashSet<PlayerInput>();

	private void Start()
	{
		m_record = GameObject.FindGameObjectWithTag("GameController").GetComponent<SelectRecord>();
	}

	// CharacterSelectUI.cs にこのメソッドを追加/修正
	public void OnClickDecision()
	{
		// 現在このボタンをクリックした(Focusしている)EventSystemを取得
		var currentEventSystem = EventSystem.current as MultiplayerEventSystem;

		if (currentEventSystem != null && currentEventSystem.playerRoot != null)
		{
			// MultiuserEventSystemのPlayerRootに設定されているPlayerInputを取得
			PlayerInput pi = currentEventSystem.playerRoot.GetComponent<PlayerInput>();

			if (pi != null)
			{
				// 既存の決定ロジックへ飛ばす
				HandleDecision(pi);
			}
		}
	}

	// 共通の決定ロジック
	public void HandleDecision(PlayerInput player)
	{
		if (confirmedPlayers.Contains(player))
		{
			Debug.Log($"プレイヤー {player.user.index} は既に決定済みです。");
			return;
		}

		// 選択中のID（ここでは仮に0とする。実際はカーソル位置などから取得）
		int selectedID = 0;

		// 記録用Dictionaryに追加
		m_record.Register(player, selectedID);
		confirmedPlayers.Add(player);

		Debug.Log($"プレイヤー {player.user.index} が キャラ {selectedID} を選択！");
	}
}