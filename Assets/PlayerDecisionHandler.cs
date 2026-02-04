using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class PlayerDecisionHandler : MonoBehaviour
{
	[Header("この枠で選択されるキャラクターID")]
	[SerializeField] private int m_characterID;

	private SelectRecord m_record;

	private void Start()
	{
		// データを渡す先の Record を取得
		GameObject controller = GameObject.FindGameObjectWithTag("GameController");
		if (controller != null)
		{
			m_record = controller.GetComponent<SelectRecord>();
		}
	}

	/// <summary>
	/// UIボタンの OnClick() にこの関数を登録してください
	/// </summary>
	public void OnClickDecision()
	{
		// 1. 現在このボタンを操作している EventSystem を取得
		var currentES = EventSystem.current as MultiplayerEventSystem;

		if (currentES != null && currentES.playerRoot != null)
		{
			// 2. EventSystem が紐付いている PlayerInput を取得
			// (Playerプレハブ側に PlayerInput が付いている想定)
			PlayerInput pi = currentES.GetComponent<PlayerInput>();

			if (pi != null)
			{
				Debug.Log($"[Decision] Player:{pi.playerIndex} selected ID:{m_characterID}");

				// 3. SelectRecord に登録
				if (m_record != null)
				{
					m_record.Register(pi, m_characterID);
				}
			}
		}
		else
		{
			Debug.LogWarning("操作しているプレイヤーを特定できません。EventSystemの設定を確認してください。");
		}
	}
}