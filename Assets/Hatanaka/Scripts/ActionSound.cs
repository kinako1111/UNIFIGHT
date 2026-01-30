using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadSound : MonoBehaviour
{
	[SerializeField] AudioClip SoundEffect;
	AudioSource audioSource;
	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}
	public void SoundPlay()
	{
		audioSource.PlayOneShot(SoundEffect);
	}
}
