using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect
{
	//î≠ê∂à íuÇÃÇÌÇ©ÇÈSE
    static public void Play3D(AudioClip clip, Vector3 position, float volume=1, float pitch=1,float startTime = 0)
    {
        PlaySe(clip, position, 1, volume, pitch, startTime);
    }
	
	//î≠ê∂à íuÇÃÇÌÇ©ÇÁÇ»Ç¢SE,éÂÇ…BGM
    static public void Play2D(AudioClip clip, float volume = 1, float pitch = 1, float startTime = 0)
    {
        PlaySe(clip, Vector3.zero, 0, volume, pitch, startTime);
    }


    static void PlaySe(AudioClip clip, Vector3 position, float spatialBlend, float volume, float pitch, float startTime)
    {
        GameObject obj = new GameObject(clip.name);

        AudioSource audio = obj.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.transform.position = position;
        audio.spatialBlend = spatialBlend;
        audio.loop = false;
        audio.volume = volume;
        audio.pitch = pitch;
		audio.time = startTime;
        audio.Play();

        MonoBehaviour.Destroy(obj, clip.length * (1.0f / pitch));
    }
}
