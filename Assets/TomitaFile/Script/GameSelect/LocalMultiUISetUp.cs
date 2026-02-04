using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class LocalMultiUISetUp : MonoBehaviour
{
	[SerializeField] PlayerInputManager m_playerInputManager;

	// プレイヤー毎のUI情報
	[Serializable]
	private struct PlayerUIInfo
	{
		public GameObject playerRoot;
		public GameObject firstSelecterd;
	}

	[SerializeField] PlayerUIInfo[] m_playerUIInfo;

	// 入室イベントの登録・解除
	private void Awake() => m_playerInputManager.onPlayerJoined += OnPlayerJoined;
	private void OnDestroy() => m_playerInputManager.onPlayerJoined -= OnPlayerJoined;

	// プレイヤーが入室したときの処理
	private void OnPlayerJoined(PlayerInput playerInput)
	{
		// MultiPlayer Event Systemを取得
		if(!playerInput.TryGetComponent(out MultiplayerEventSystem eventSystem))
		{
			// MultiPlayer Event Systemがアタッチされていない場合は追加
			eventSystem = playerInput.gameObject.AddComponent<MultiplayerEventSystem>();
		}

		// プレイヤー情報を取得
		if(playerInput.playerIndex >= m_playerUIInfo.Length)
		{
			Debug.LogError("割り当て可能なプレイヤー情報がありません。");
			return;
		}

		var playerUiInfo = m_playerUIInfo[playerInput.playerIndex];

		// UI情報を設定
		eventSystem.playerRoot = playerUiInfo.playerRoot;	
		eventSystem.firstSelectedGameObject = playerUiInfo.firstSelecterd;
	}
}
