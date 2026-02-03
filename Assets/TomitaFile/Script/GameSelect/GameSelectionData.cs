using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameSelectionData", menuName = "Game/GameSelectionData")]
public class GameSelectionData : ScriptableObject
{
	public int selectedCharacterId;
	public int selectedStageId;
}
