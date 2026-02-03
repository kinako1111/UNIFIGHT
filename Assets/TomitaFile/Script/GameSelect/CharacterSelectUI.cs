using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{

    [SerializeField] GameSelectionData gameData;

	public void OnCharacterSelected(int characterId)
	{
		gameData.selectedCharacterId = characterId;
		SceneManager.LoadScene(gameData.selectedStageId);
	}
}
