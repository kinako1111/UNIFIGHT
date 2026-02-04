using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectUI : MonoBehaviour
{
    // ステージが選択された時に呼ばれる
   public void OnStageSelected(int stageId)
    {
        //ステージ保存場所の検索
		SelectRecord record = GameObject.FindGameObjectWithTag("GameController").GetComponent<SelectRecord>();

		//ステージ選択
        record.Decision(stageId);

		SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
		changer.ChangeScene("CharacterSelectionScene");
	}
}
