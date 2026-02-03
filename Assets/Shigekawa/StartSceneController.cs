using System.Xml.Serialization; // TextMeshProを使うために必要
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartSceneController : MonoBehaviour
{
	[Header("設定")]
	[SerializeField] float inputEnableTime = 5f;  // 何秒後に入力を受け付けるか
	[SerializeField] string nextSceneName;       // 遷移先のシーン名
	[SerializeField] GameObject guideText;       // 「Press Any Button」のテキストオブジェクト
	PlayerInput anyKeyAction;

	private bool canInput = false;
	private bool isTransitioning = false; // 多重遷移防止

	// ガイドテキスト（TextMeshPro）の参照
	TextMeshProUGUI guideTMP;

	// 点滅処理を制御するためのCoroutine参照
	Coroutine blinkCoroutine;

	private void Awake()
	{
		anyKeyAction = GetComponent<PlayerInput>();

		// ガイドテキストから TextMeshPro コンポーネントを取得
		if (guideText != null)
			guideTMP = guideText.GetComponent<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		anyKeyAction.actions["PressAny"].performed += OnAnyKey;
	}

	private void OnDisable()
	{
		anyKeyAction.actions["PressAny"].performed -= OnAnyKey;
	}

	void Start()
	{
		// シーン開始時はガイドテキストを非表示にしておく
		if (guideText != null) guideText.SetActive(false);

		// 指定秒数経過後に入力受付を有効化
		Invoke(nameof(EnableInput), inputEnableTime);
	}

	void EnableInput()
	{
		Debug.Log("4秒経過：入力許可されました！");
		canInput = true;

		// 入力可能になったタイミングでガイドテキストを表示
		if (guideText != null) guideText.SetActive(true);

		// ガイドテキストを点滅させる処理を開始
		if (guideTMP != null)
			blinkCoroutine = StartCoroutine(BlinkText());
	}

	// ガイドテキストの透明度を周期的に変化させ、点滅表現を行う
	IEnumerator BlinkText()
	{
		Color textColor = guideTMP.color;
		float blinkTimer = 0f;

		// シーン遷移が始まるまで点滅を継続
		while (!isTransitioning)
		{
			blinkTimer += Time.deltaTime * 1.5f;
			textColor.a = Mathf.Abs(Mathf.Sin(blinkTimer));
			guideTMP.color = textColor;
			yield return null;
		}
	}

	// PlayerInput の Events から呼ばれる入力コールバック
	public void OnAnyKey(InputAction.CallbackContext context)
	{
		// 入力受付前、または既に遷移中の場合は処理しない
		if (!canInput || isTransitioning) return;

		// ボタンが押された瞬間のみ処理を行う
		if (context.performed)
		{
			isTransitioning = true;

			// 点滅処理を停止してからシーン遷移へ進む
			if (blinkCoroutine != null)
				StopCoroutine(blinkCoroutine);

			Debug.Log("次のシーンへ遷移します");

			SceneChanger changer = GameObject.FindWithTag("SceneManager").GetComponent<SceneChanger>();
			changer.ChangeScene("StageSelectionScene");
		}
	}
}
