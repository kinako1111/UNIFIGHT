using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCountSelectUI : MonoBehaviour
{
    [SerializeField] GameSelectionData gameData;


    public void OnPlayerCountSelectecd(int count)
    {
        // 遊ぶ人数を保存
        gameData.selectedPlayerCount = count;

        // キャラ選択用データ初期化
        gameData.ResetData();

        Debug.Log($"Player Count Selected : {count}");

		SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
		changer.ChangeScene("CharacterSelectionScene");
	}
}
