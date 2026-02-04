using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
	//選んだステージと選んだキャラを保持すくりぷおぶじぇ
	[SerializeField] GameSelectionData gameData;

	private void Awake()
	{
		//シーン開始と同時にステージセレクトに飛ぶ
		SceneManager.LoadScene("StageSelectionScene", LoadSceneMode.Additive);
	}

	public void OnStageSelected(int stageId)
	{
		//選んだステージを元にステージシーンへ
		{
			//gameData.selectedStageId = stageId;
			SceneManager.LoadScene("CharacterSelectionScene",LoadSceneMode.Additive);
		}
	}
}
