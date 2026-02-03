using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 勝利／敗北シーン共通で使うリザルト画面制御用スクリプト
public class ResultSceneController : MonoBehaviour
{
	[Header("設定")]
	[SerializeField] float inputEnableTime = 3f;   // シーン開始から何秒後に入力を有効にするか
	[SerializeField] GameObject resultPanel;       // ボタン2つをまとめたパネル
	[SerializeField] Button mapSelectButton;       // マップ選択に戻るボタン
	[SerializeField] Button titleButton;           // タイトルに戻るボタン

	private bool canInput = false;                 // 入力受付中かどうかのフラグ

	void Start()
	{
		// シーン開始時はリザルトUIを非表示にする
		if (resultPanel != null)
			resultPanel.SetActive(false);

		// ボタンを押せない状態にしておく
		SetButtonsInteractable(false);

		// 指定秒数後に入力を有効化する
		Invoke(nameof(EnableInput), inputEnableTime);
	}

	// 入力を有効化し、UIを表示する
	void EnableInput()
	{
		canInput = true;

		// リザルトUIを表示
		if (resultPanel != null)
			resultPanel.SetActive(true);

		// ボタンを押せるようにする
		SetButtonsInteractable(true);
	}

	// ボタンの押下可否をまとめて制御する
	void SetButtonsInteractable(bool value)
	{
		if (mapSelectButton != null)
			mapSelectButton.interactable = value;

		if (titleButton != null)
			titleButton.interactable = value;
	}

	public void GoToMapSelect()
	{
		SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
		changer.ChangeScene("StageSelectionScene");
	}

	public void GoToTitle()
	{
		SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
		changer.ChangeScene("TitleScene");
	}
}
