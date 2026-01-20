
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BGMManager : MonoBehaviour
{
	public static BGMManager Instance;

	[SerializeField] private AudioClip[] m_bgmClips;

	private enum BgmType { StageSelect, GameStart }

	private AudioSource m_stageSelectAudio;
	private AudioSource m_gameStartAudio;
	private AudioSource m_active;   // 現在鳴っている方
	private Coroutine m_fade;

	[Range(0f, 5f)]
	[SerializeField] private float _crossfadeSec = 1.5f;

	private void Awake()
	{
		if (Instance != this && Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// 2本用意
		m_stageSelectAudio = gameObject.AddComponent<AudioSource>();
		m_gameStartAudio = gameObject.AddComponent<AudioSource>();
		foreach (var s in new[] { m_stageSelectAudio, m_gameStartAudio })
		{
			s.playOnAwake = false;
			s.loop = true;
			s.spatialBlend = 0f;
			s.volume = 0f;
		}
		m_active = m_stageSelectAudio;

		SceneManager.sceneLoaded += OnSceneLoaded;

		var active = SceneManager.GetActiveScene();
		OnSceneLoaded(active, LoadSceneMode.Single);
	}

	private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

	private void PlayBGM(BgmType type)
	{
		var nextClip = m_bgmClips[(int)type];
		if (nextClip == null) return;

		// 同じクリップなら何もしない（継続）
		if (m_active.isPlaying && m_active.clip == nextClip) return;

		// 受け皿（非アクティブ側）
		var inactive = m_active == m_stageSelectAudio ? m_gameStartAudio : m_stageSelectAudio;

		inactive.clip = nextClip;
		inactive.volume = 0f;
		inactive.Play();

		// 既存のクロスフェード停止
		if (m_fade != null) StopCoroutine(m_fade);
		m_fade = StartCoroutine(Crossfade(m_active, inactive, _crossfadeSec));

		m_active = inactive;
	}

	private IEnumerator Crossfade(AudioSource from, AudioSource to, float sec)
	{
		if (sec <= 0f)
		{
			from.Stop();
			to.volume = 1f;
			yield break;
		}

		float t = 0f;
		float fromStart = from.volume;
		while (t < sec)
		{
			t += Time.unscaledDeltaTime; // ポーズ中も進めたいなら unscaled
			float a = Mathf.Clamp01(t / sec);
			from.volume = Mathf.Lerp(fromStart, 0f, a);
			to.volume = Mathf.Lerp(0f, 1f, a);
			yield return null;
		}
		from.Stop();
		to.volume = 1f;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MapScene" || scene.name == "MapScene2")
			PlayBGM(BgmType.GameStart);
		else
			PlayBGM(BgmType.StageSelect);
	}
}
