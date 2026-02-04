using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class LocalMultiUISetUp : MonoBehaviour
{
	[SerializeField] private PlayerInputManager m_playerInputManager;

	[Serializable]
	private struct PlayerUIInfo
	{
		public GameObject playerRoot;    // 1PCharacterSelectなどの親オブジェクト
		public GameObject firstSelected; // 最初にフォーカスするボタン（決定ボタン等）
	}

	[Header("1P〜4PのUI情報を順番に登録")]
	[SerializeField] private PlayerUIInfo[] m_playerUIInfo;

	private void Awake()
	{
		if (m_playerInputManager == null)
		{
			m_playerInputManager = GetComponent<PlayerInputManager>();
		}

		// イベント登録
		m_playerInputManager.onPlayerJoined += OnPlayerJoined;
		m_playerInputManager.onPlayerLeft += OnPlayerLeft;
	}

	private void OnDestroy()
	{
		if (m_playerInputManager != null)
		{
			m_playerInputManager.onPlayerJoined -= OnPlayerJoined;
			m_playerInputManager.onPlayerLeft -= OnPlayerLeft;
		}
	}

	// プレイヤーが入室したときの処理
	private void OnPlayerJoined(PlayerInput playerInput)
	{
		int index = playerInput.playerIndex;
		if (index >= m_playerUIInfo.Length)
		{
			Debug.LogError($"プレイヤーインデックス {index} に対応するUI設定がありません。");
			return;
		}

		SelectRecord selectRecord = GameObject.FindWithTag("GameController").GetComponent<SelectRecord>();
		selectRecord.SetPlayerCount(index + 1);

		var uiInfo = m_playerUIInfo[index];

		// 1. UIをアクティブにする
		if (uiInfo.playerRoot != null)
		{
			uiInfo.playerRoot.SetActive(true);
		}

		// 2. プレハブ側のMultiplayerEventSystemを取得・設定
		if (playerInput.TryGetComponent(out MultiplayerEventSystem es))
		{
			// このプレイヤーが操作するUIの範囲を限定
			es.playerRoot = uiInfo.playerRoot;
			es.firstSelectedGameObject = uiInfo.firstSelected;

			// 接続直後はEventSystemが不安定なため、1フレーム遅らせてフォーカス
			StartCoroutine(SetFocusRoutine(es, uiInfo.firstSelected));
		}
		Debug.Log($"[Joined] Player {index} connected. UI Activated.");
	}

	// プレイヤーが退室したときの処理（MissingReferenceException対策済み）
	private void OnPlayerLeft(PlayerInput playerInput)
	{
		// PlayerInputオブジェクト自体が破棄されていても、indexは取得可能
		int index = playerInput.playerIndex;

		if (index >= 0 && index < m_playerUIInfo.Length)
		{
			var uiInfo = m_playerUIInfo[index];

			// UIを非表示にする（nullチェックで破壊済みエラーを回避）
			if (uiInfo.playerRoot != null)
			{
				uiInfo.playerRoot.SetActive(false);
			}

			Debug.Log($"[Left] Player {index} disconnected. UI Deactivated.");
		}
	}

	private IEnumerator SetFocusRoutine(MultiplayerEventSystem es, GameObject target)
	{
		yield return null; // 1フレーム待機
		if (es != null && target != null)
		{
			es.SetSelectedGameObject(null); // 一度リセット
			es.SetSelectedGameObject(target);
		}
	}
}