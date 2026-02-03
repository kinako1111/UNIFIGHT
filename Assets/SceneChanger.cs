using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
	private const string ManagerSceneName = "ManagerScene";

	// --- 文字列（シーン名）で指定する場合 ---
	public void ChangeScene(string nextSceneName)
	{
		StartCoroutine(TransitionRoutine(nextSceneName));
	}

	// --- 数値（インデックス）で指定する場合 ---
	public void ChangeScene(int nextSceneBuildIndex)
	{
		// インデックスからシーン名を取得して実行
		// (名前を取得せずに直接インデックスでロードすることも可能ですが、
		//  一貫性のために共通のルーチンへ渡します)
		StartCoroutine(TransitionRoutine(nextSceneBuildIndex));
	}

	// 内部的な共通ロード処理（引数を object 型にして柔軟に受けるか、多重定義する）
	private IEnumerator TransitionRoutine(object sceneTarget)
	{
		// 1. マネージャー以外のシーンをアンロード
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene.name != ManagerSceneName)
			{
				yield return SceneManager.UnloadSceneAsync(scene);
			}
		}

		// 2. 次のシーンをロード
		AsyncOperation op;
		if (sceneTarget is string name)
			op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
		else
			op = SceneManager.LoadSceneAsync((int)sceneTarget, LoadSceneMode.Additive);

		while (!op.isDone) yield return null;

		// 3. アクティブ化（最後にロードされたシーンを取得）
		Scene nextScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
		if (nextScene.IsValid())
		{
			SceneManager.SetActiveScene(nextScene);
			Debug.Log($"<color=lime>[SceneChanger]</color> Scene {nextScene.buildIndex} ({nextScene.name}) へ遷移しました。");
		}
	}
}