using System.Xml.Serialization; // TextMeshProを使うために必要
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
	[Header("設定")]
	[SerializeField] float inputEnableTime = 4f;  // 何秒後に入力を受け付けるか
	[SerializeField] string nextSceneName;       // 遷移先のシーン名
	[SerializeField] GameObject guideText;       // 「Press Any Button」のテキストオブジェクト
	PlayerInput anyKeyAction;


	private bool canInput = false;
	private bool isTransitioning = false; // 多重遷移防止

	private void Awake()
	{
		anyKeyAction = GetComponent<PlayerInput>();
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
		// 最初はテキストを隠しておく（念のためコードでも制御）
		if (guideText != null) guideText.SetActive(false);

		// 指定秒数後に EnableInput を実行
		Invoke(nameof(EnableInput), inputEnableTime);
	}

	void EnableInput()
	{
		Debug.Log("4秒経過：入力許可されました！"); // これが出るか確認
		canInput = true;
		if (guideText != null) guideText.SetActive(true);
	}

	// PlayerInputのEventsから呼ばれる関数
	public void OnAnyKey(InputAction.CallbackContext context)
	{
		// 4秒経っていない、または既に遷移中なら何もしない
		if (!canInput || isTransitioning) return;

		// ボタンが「押された瞬間」だけ判定
		if (context.performed)
		{
			isTransitioning = true;
			Debug.Log("次のシーンへ遷移します");
			SceneManager.LoadScene(nextSceneName);
		}

	}
}