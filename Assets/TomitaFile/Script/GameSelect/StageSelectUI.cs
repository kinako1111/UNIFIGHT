using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectUI : MonoBehaviour
{

    [SerializeField] GameSelectionData gameData;

   public void OnStageSelected(int stageId)
    {
        {
            gameData.selectedStageId = stageId;
            SceneManager.LoadScene("CharacterSelectionScene");
        }
    }

}
