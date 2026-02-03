using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectUI : MonoBehaviour
{

    [SerializeField] GameSelectionData gameData;


    // ステージが選択された時に呼ばれる
   public void OnStageSelected(int stageId)
    {
        // 選択されたステージIDを保存
        gameData.selectedStageId = stageId;

        //前回プレイの選択が残らないようにする
        gameData.ResetData();

        // Player人数選択シーンへ移動
        SceneManager.LoadScene("PlayerNumberScene");
    }

}
