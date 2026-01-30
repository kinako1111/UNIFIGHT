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

	// ★追加：BGMマスター音量
	[Header("Volume")]
	[Range(0f, 1f)]
	[SerializeField] private float m_masterVolume = 1f;

	private const string PREF_BGM_VOL = "BGM_VOLUME";

	private void Awake()
	{
		if (Instance != this && Instance != null) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);

		// ★保存値があれば復元
		if (PlayerPrefs.HasKey(PREF_BGM_VOL))
			m_masterVolume = PlayerPrefs.GetFloat(PREF_BGM_VOL, 1f);

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

	/// <summary>
	/// ★追加：BGM音量を設定（0?1）
	/// UIスライダーなどから呼ぶ想定
	/// </summary>
	public void SetMasterVolume(float volume, bool save = true)
	{
		m_masterVolume = Mathf.Clamp01(volume);

		// クロスフェード中なら「目標値」がズレるので、即時反映
		ApplyVolumeImmediate();

		if (save)
		{
			PlayerPrefs.SetFloat(PREF_BGM_VOL, m_masterVolume);
			PlayerPrefs.Save();
		}
	}

	/// <summary>
	/// ★追加：現在の状態に応じて音量を即反映
	/// </summary>
	private void ApplyVolumeImmediate()
	{
		// 今鳴っている方はマスター音量へ寄せる
		if (m_active != null && m_active.isPlaying)
			m_active.volume = m_masterVolume;

		// もう片方が鳴っていないなら0にしておく（ノイズ対策）
		var inactive = m_active == m_stageSelectAudio ? m_gameStartAudio : m_stageSelectAudio;
		if (inactive != null && !inactive.isPlaying)
			inactive.volume = 0f;
	}

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
			to.volume = m_masterVolume; // ★変更：1fではなくマスター音量
			yield break;
		}

		float t = 0f;
		float fromStart = from.volume;

		while (t < sec)
		{
			t += Time.unscaledDeltaTime;
			float a = Mathf.Clamp01(t / sec);

			// ★変更：toのゴールが1f→m_masterVolume
			from.volume = Mathf.Lerp(fromStart, 0f, a);
			to.volume = Mathf.Lerp(0f, m_masterVolume, a);

			yield return null;
		}

		from.Stop();
		to.volume = m_masterVolume; // ★変更
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MapScene" || scene.name == "MapScene2")
			PlayBGM(BgmType.GameStart);
		else
			PlayBGM(BgmType.StageSelect);
	}

	/// <summary>
	/// ★便利：現在の音量を取得（UI表示用など）
	/// </summary>
	public float GetMasterVolume() => m_masterVolume;
}
