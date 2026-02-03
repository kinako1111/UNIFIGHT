using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{

    [SerializeField] GameSelectionData gameData;

	public void OnCharacterSelected(int characterId)
	{
		// 既に選択されていたら何もしない
		if(gameData.IsCharacterUsed(characterId)) return;

		// キャラIDを追加
		gameData.selectedCharacterIds.Add(characterId);

		// Debug用
		gameData.UpdateDebug();
		Debug.Log($"[CharacterSelect] PlayerCount = {gameData.selectedCharacterIds.Count}");

		// 全員選択完了したか
		if(gameData.selectedCharacterIds.Count >= gameData.selectedPlayerCount)
		{
			Debug.Log("[CharacterSelect] All Player READY");

			// シーン遷移
			SceneManager.LoadScene(gameData.selectedStageId);
		}
		
	}
}
