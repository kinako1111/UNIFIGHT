using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class CharacterSelectUI : MonoBehaviour
{
	[SerializeField] private int m_playerIndex;
	[SerializeField] private GameObject m_visualRoot;

	private SelectRecord m_record;
	private bool m_isJoined = false;

	private void Awake()
	{
		// 1P(Index 0)à»äOÇÕèâä˙îÒï\é¶
		if (m_visualRoot != null) m_visualRoot.SetActive(m_playerIndex == 0);
	}

	private void Start()
	{
		m_record = GameObject.FindGameObjectWithTag("GameController").GetComponent<SelectRecord>();
	}

	// ManagerÇ©ÇÁåƒÇŒÇÍÇÈ
	public void ActivateUI(PlayerInput pi)
	{
		m_isJoined = true;
		if (m_visualRoot != null) m_visualRoot.SetActive(true);

		var es = GetComponentInChildren<MultiplayerEventSystem>();
		if (es != null)
		{
			es.playerRoot = pi.gameObject;
			if (es.firstSelectedGameObject != null) es.SetSelectedGameObject(es.firstSelectedGameObject);
		}
	}

	// ManagerÇ©ÇÁåƒÇŒÇÍÇÈ
	public void DeactivateUI(PlayerInput pi)
	{
		m_isJoined = false;
		if (m_visualRoot != null) m_visualRoot.SetActive(false);
	}

	public void OnClickDecision()
	{
		if (!m_isJoined) return;

		var currentEventSystem = EventSystem.current as MultiplayerEventSystem;
		if (currentEventSystem != null && currentEventSystem.playerRoot != null)
		{
			PlayerInput pi = currentEventSystem.playerRoot.GetComponent<PlayerInput>();
			if (pi != null) HandleDecision(pi);
		}
	}

	private void HandleDecision(PlayerInput player)
	{
		if (m_record.GetDictionary().ContainsKey(player)) return;

		m_record.Register(player, 0);
		Debug.Log($"Player {player.playerIndex} Ready!");
	}
}