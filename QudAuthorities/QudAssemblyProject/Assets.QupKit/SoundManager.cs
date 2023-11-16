using System.Collections.Generic;
using UnityEngine;
using XRL;

namespace Assets.QupKit;

[HasModSensitiveStaticCache]
public static class SoundManager
{
	private static Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();

	private static Dictionary<string, Object> ClipObjects = new Dictionary<string, Object>();

	public static GameObject MusicSource = null;

	public static AudioSource MusicAudioSource = null;

	private static string ClipName = null;

	public static bool bPlaying = false;

	public static float _MusicTargetVolume = 0f;

	public static float MusicTargetVolume
	{
		get
		{
			return _MusicTargetVolume;
		}
		set
		{
			_MusicTargetVolume = value;
		}
	}

	[ModSensitiveCacheInit]
	public static void ClearCache()
	{
		Clips = new Dictionary<string, AudioClip>();
		ClipObjects = new Dictionary<string, Object>();
	}

	public static void PlaySound(string Clip)
	{
		if (MusicSource == null)
		{
			Init();
		}
		_PlaySound(Clip);
	}

	public static void UnloadClip(string ClipID)
	{
		if (MusicSource == null)
		{
			Init();
		}
		if (Clips.ContainsKey(ClipID))
		{
			Clips.Remove(ClipID);
		}
		if (ClipObjects.ContainsKey(ClipID))
		{
			Resources.UnloadAsset(ClipObjects[ClipID]);
			ClipObjects.Remove(ClipID);
		}
	}

	public static void Init()
	{
		if (MusicSource == null)
		{
			MusicSource = new GameObject();
			MusicSource.AddComponent<AudioSource>();
			MusicAudioSource = MusicSource.GetComponent<AudioSource>();
			MusicSource.transform.position = new Vector3(0f, 0f, 1f);
			MusicSource.name = "Music";
			Object.DontDestroyOnLoad(MusicSource);
		}
	}

	public static void _PlayMusic(string Name)
	{
		if (ClipName != null)
		{
			UnloadClip(ClipName);
		}
		AudioSource component = MusicSource.GetComponent<AudioSource>();
		component.Stop();
		component.clip = GetClip(Name);
		component.loop = false;
		component.volume = 0f;
		bPlaying = true;
		component.Play();
		MusicTargetVolume = 1f;
	}

	public static void _PlaySound(string Name)
	{
		MusicAudioSource.PlayOneShot(GetClip(Name));
	}

	public static void Update()
	{
		if (MusicSource != null)
		{
			if (MusicTargetVolume > MusicAudioSource.volume)
			{
				MusicAudioSource.volume += 0.02f * Time.deltaTime;
			}
			else if (MusicTargetVolume < MusicAudioSource.volume)
			{
				MusicAudioSource.volume -= 0.02f * Time.deltaTime;
			}
		}
	}

	public static AudioClip GetClip(string Name)
	{
		if (!Clips.ContainsKey(Name))
		{
			Object @object = Resources.Load("Sounds/" + Name);
			Clips.Add(Name, @object as AudioClip);
			ClipObjects.Add(Name, @object);
		}
		return Clips[Name];
	}
}
