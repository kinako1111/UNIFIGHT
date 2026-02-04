using UnityEngine;
using System.Collections.Generic;

public class SEManager : MonoBehaviour
{
	public static SEManager Instance;

	[System.Serializable]
	public class SEData
	{
		public SEType type;
		public AudioClip clip;
		[Range(0f, 1f)] public float volume = 1f;
	}

	[SerializeField] List<SEData> seList = new();

	Dictionary<SEType, SEData> seDict;
	AudioSource audioSource;

	void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		audioSource = gameObject.AddComponent<AudioSource>();

		seDict = new Dictionary<SEType, SEData>();
		foreach (var se in seList)
		{
			seDict[se.type] = se;
		}
	}

	public void Play(SEType type)
	{
		if (!seDict.TryGetValue(type, out var data))
		{
			Debug.LogError($"SEManager: –¢“o˜^‚Ì SEType {type}");
			return;
		}

		if (data.clip == null)
		{
			Debug.LogError($"SEManager: AudioClip ‚ª null ({type})");
			return;
		}

		audioSource.PlayOneShot(data.clip, data.volume);
	}

	public void SetVolume(SEType type, float volume)
	{
		if (seDict.TryGetValue(type, out var data))
		{
			data.volume = Mathf.Clamp01(volume);
		}
	}
}
