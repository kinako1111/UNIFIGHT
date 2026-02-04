using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinManager : MonoBehaviour
{
	[Header("UI˜g‚ÌQÆi0=1P, 1=2P...j")]
	[SerializeField] private CharacterSelectUI[] m_playerUIs;

	// PlayerInputManager ‚Ì Broadcast Messages ‚ÅŒÄ‚Î‚ê‚é
	public void OnPlayerJoined(PlayerInput pi)
	{
		int index = pi.playerIndex;
		if (index < m_playerUIs.Length)
		{
			// ŠY“–‚·‚éUI˜g‚ÉQ‰Á‚ğ’Ê’m
			m_playerUIs[index].ActivateUI(pi);
		}
	}

	// PlayerInputManager ‚Ì Broadcast Messages ‚ÅŒÄ‚Î‚ê‚é
	public void OnPlayerLeft(PlayerInput pi)
	{
		int index = pi.playerIndex;
		if (index < m_playerUIs.Length)
		{
			// ŠY“–‚·‚éUI˜g‚É‘Şo‚ğ’Ê’m
			m_playerUIs[index].DeactivateUI(pi);
		}
	}
}