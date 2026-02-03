using UnityEngine;
using UnityEngine.SceneManagement;

public static class ManagerSceneAutoLoader
{
	private const string ManagerSceneName = "ManagerScene";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void LoadManagerScene()
	{
		// 1. すでに読み込まれていないかチェック
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).name == ManagerSceneName)
			{
				return; // すでに存在すれば何もしない
			}
		}

		// 2. ビルド設定に含まれているかチェック（エラー防止）
		if (Application.CanStreamedLevelBeLoaded(ManagerSceneName))
		{
			// 同期的にロード（BeforeSceneLoad内なので、最初のシーンの開始前に完了する）
			SceneManager.LoadScene(ManagerSceneName, LoadSceneMode.Additive);
			Debug.Log($"<color=cyan>[AutoLoader]</color> {ManagerSceneName} を読み込みました。");
		}
		else
		{
			Debug.LogError($"[AutoLoader] シーン '{ManagerSceneName}' がBuild Settingsに追加されていません！");
		}
	}
}