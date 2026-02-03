using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
	[SerializeField] GameSelectionData gameData;

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(10, 10, 300, 200), GUI.skin.box);

		GUILayout.Label($"Stage ID : {gameData.selectedStageId}");
		GUILayout.Label($"Player Count : {gameData.selectedCharacterIds.Count} / {gameData.maxPlayerCount}");
		GUILayout.Label($"All Ready : {gameData.IsAllPlayerReady()}");

		for (int i = 0; i < gameData.selectedCharacterIds.Count; i++)
		{
			GUILayout.Label($"Player {i + 1} ¨ Character {gameData.selectedCharacterIds[i]}");
		}

		GUILayout.EndArea();
	}
}
