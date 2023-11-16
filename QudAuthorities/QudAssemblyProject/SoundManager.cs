using System.Collections.Generic;
using System.IO;
using RuntimeAudioClipLoader;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

[HasModSensitiveStaticCache]
public static class SoundManager
{
	private static Dictionary<string, AudioClipSet> Clips = new Dictionary<string, AudioClipSet>();

	private static Dictionary<string, Object> ClipObjects = new Dictionary<string, Object>();

	private static Dictionary<string, int[]> ClipFrames = new Dictionary<string, int[]>();

	public static Queue<SoundRequest> RequestPool = new Queue<SoundRequest>();

	public static Queue<SoundRequest> SoundRequests = new Queue<SoundRequest>();

	private static List<SoundRequest> Requests = new List<SoundRequest>();

	private static Queue<SoundRequest> DelayedRequests = new Queue<SoundRequest>();

	public static GameObject MusicSource = null;

	public static AudioSource MusicAudioSource = null;

	public static GameObject UISource = null;

	public static AudioSource UIAudioSource = null;

	public static GameObject UISource2 = null;

	public static AudioSource UIAudioSource2 = null;

	public static GameObject UISource3 = null;

	public static AudioSource UIAudioSource3 = null;

	private static Queue<GameObject> AudioSourcePool = new Queue<GameObject>();

	private static Queue<GameObject> PlayingAudioSources = new Queue<GameObject>();

	private static string ClipName = null;

	public static bool bPlaying = false;

	public static float _MusicTargetVolume = 0f;

	public static float MasterVolume = 1f;

	public static float SoundVolume = 1f;

	public static float MusicVolume = 1f;

	public static string nextTrack = "";

	private static HashSet<string> soundsPlayedThisFrame = new HashSet<string>();

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
		Clips = new Dictionary<string, AudioClipSet>();
		ClipFrames = new Dictionary<string, int[]>();
	}

	public static void StopMusic(bool Crossfade = true, float CrossfadeDuration = 12f)
	{
		PlayMusic(null, Crossfade, CrossfadeDuration);
	}

	public static void PlayMusic(string Track, bool Crossfade = true, float CrossfadeDuration = 12f)
	{
		if (RequestPool.Count == 0)
		{
			Debug.LogWarning("sound request pool was empty for request " + Track);
			return;
		}
		SoundRequest soundRequest = RequestPool.Dequeue();
		soundRequest.Clip = Track;
		soundRequest.Crossfade = Crossfade;
		soundRequest.CrossfadeDuration = CrossfadeDuration;
		soundRequest.Type = SoundRequest.SoundRequestType.Music;
		lock (SoundRequests)
		{
			SoundRequests.Enqueue(soundRequest);
		}
	}

	public static void PlayWorldSound(string Clip, int Distance, bool Occluded, float VolumeIntensity, float PitchVariance = 0f, float Delay = 0f)
	{
		if (RequestPool.Count == 0)
		{
			Debug.LogWarning("sound request pool was empty for request " + Clip);
			return;
		}
		VolumeIntensity *= GetSoundFrameVolume(Clip);
		if (Occluded)
		{
			if (Distance != 9999 && !((float)Distance > 40f * VolumeIntensity))
			{
				SoundRequest soundRequest = RequestPool.Dequeue();
				soundRequest.Clip = Clip;
				soundRequest.Type = SoundRequest.SoundRequestType.Spatial;
				soundRequest.LowPass = 0.15f + 0.55f * ((float)(80 - Distance) / 80f);
				soundRequest.Volume = VolumeIntensity * ((float)((40 - Distance) * (40 - Distance)) / 1600f);
				soundRequest.PitchVariance = PitchVariance;
				soundRequest.Delay = Delay;
				lock (SoundRequests)
				{
					SoundRequests.Enqueue(soundRequest);
				}
			}
		}
		else if (Distance <= 40)
		{
			SoundRequest soundRequest2 = RequestPool.Dequeue();
			soundRequest2.Clip = Clip;
			soundRequest2.Type = SoundRequest.SoundRequestType.Spatial;
			soundRequest2.LowPass = 1f;
			soundRequest2.Volume = VolumeIntensity * ((float)((40 - Distance) * (40 - Distance)) / 1600f);
			soundRequest2.PitchVariance = PitchVariance;
			soundRequest2.Delay = Delay;
			lock (SoundRequests)
			{
				SoundRequests.Enqueue(soundRequest2);
			}
		}
	}

	public static void PlaySound(string Clip, float PitchVariance = 0f, float Volume = 1f, float Pitch = 1f)
	{
		Volume *= GetSoundFrameVolume(Clip);
		if (Volume <= 0f)
		{
			return;
		}
		if (RequestPool.Count == 0)
		{
			Debug.LogWarning("sound request pool was empty for request " + Clip);
			return;
		}
		SoundRequest soundRequest = RequestPool.Dequeue();
		soundRequest.Clip = Clip;
		soundRequest.Type = SoundRequest.SoundRequestType.Sound;
		soundRequest.Pitch = Pitch;
		soundRequest.PitchVariance = PitchVariance;
		soundRequest.Volume = Volume;
		lock (SoundRequests)
		{
			SoundRequests.Enqueue(soundRequest);
		}
	}

	public static List<SoundRequest> GetRequests()
	{
		if (Requests.Count > 0)
		{
			Requests.Clear();
		}
		if (SoundRequests.Count > 0)
		{
			lock (SoundRequests)
			{
				while (SoundRequests.Count > 0)
				{
					SoundRequest soundRequest = SoundRequests.Dequeue();
					if (soundRequest.Delay > 0f)
					{
						soundRequest.Delay -= Time.deltaTime;
					}
					if (soundRequest.Delay <= 0f)
					{
						Requests.Add(soundRequest);
					}
					else
					{
						DelayedRequests.Enqueue(soundRequest);
					}
				}
				if (DelayedRequests.Count > 0)
				{
					Queue<SoundRequest> soundRequests = SoundRequests;
					SoundRequests = DelayedRequests;
					DelayedRequests = soundRequests;
				}
			}
		}
		return Requests;
	}

	public static void UnloadClip(string ClipID)
	{
		if (Clips.ContainsKey(ClipID))
		{
			Clips[ClipID].unload();
			Clips.Remove(ClipID);
		}
	}

	public static void CreateNewMusicSource()
	{
		MusicSource = new GameObject();
		MusicSource.AddComponent<AudioSource>();
		MusicAudioSource = MusicSource.GetComponent<AudioSource>();
		MusicSource.transform.position = new Vector3(0f, 0f, 1f);
		MusicSource.name = "Music";
		if (Options.MusicBackground)
		{
			if (!MusicAudioSource.ignoreListenerPause)
			{
				MusicAudioSource.ignoreListenerPause = true;
				MusicAudioSource.enabled = false;
				MusicAudioSource.enabled = true;
			}
		}
		else if (MusicAudioSource.ignoreListenerPause)
		{
			MusicAudioSource.ignoreListenerPause = false;
			MusicAudioSource.enabled = false;
			MusicAudioSource.enabled = true;
		}
		Object.DontDestroyOnLoad(MusicSource);
	}

	public static void Init()
	{
		if (MusicSource == null)
		{
			CreateNewMusicSource();
		}
		if (UIAudioSource == null)
		{
			UISource = new GameObject();
			UISource.AddComponent<AudioSource>();
			UIAudioSource = UISource.GetComponent<AudioSource>();
			UISource.transform.position = new Vector3(0f, 0f, 1f);
			UISource.name = "UISound";
			Object.DontDestroyOnLoad(UISource);
		}
		for (int i = 0; i < 256; i++)
		{
			RequestPool.Enqueue(new SoundRequest());
		}
	}

	public static void _PlayMusic(string Track, bool Crossfade, float CrossfadeDuration = 12f)
	{
		float num = MusicVolume;
		nextTrack = Track;
		AudioSource component = MusicSource.GetComponent<AudioSource>();
		if (component.isPlaying)
		{
			if (Crossfade)
			{
				if (Track == ClipName)
				{
					return;
				}
				MusicSource.AddComponent<FadeAway>().duration = CrossfadeDuration;
				CreateNewMusicSource();
				component = MusicSource.GetComponent<AudioSource>();
				ClipName = null;
				num = 0f;
			}
			else
			{
				if (Track == null)
				{
					component.Stop();
					return;
				}
				nextTrack = Track;
			}
		}
		if (component.enabled)
		{
			ClipName = Track;
			component.Stop();
			component.clip = GetClip(Track)?.next();
			component.loop = false;
			component.volume = (Crossfade ? 0f : num);
			if (Track != null && component.clip != null)
			{
				bPlaying = true;
				component.Play();
				MusicTargetVolume = MusicVolume;
			}
			else
			{
				bPlaying = false;
				MusicTargetVolume = 0f;
			}
		}
	}

	public static void _PlaySound(string Name, float Volume, float Pitch = 1f)
	{
		UIAudioSource.pitch = Pitch;
		UIAudioSource.PlayOneShot(GetClip(Name)?.next(), SoundVolume * Volume);
	}

	public static void _PlayWorldSound(string Name, float Volume, float LowPass, float PitchVariance)
	{
		GameObject gameObject;
		if (AudioSourcePool.Count > 0)
		{
			gameObject = AudioSourcePool.Dequeue();
		}
		else
		{
			gameObject = new GameObject();
			gameObject.transform.position = new Vector3(0f, 0f, 1f);
			gameObject.name = "PooledWorldSound";
			gameObject.AddComponent<AudioSource>();
			gameObject.AddComponent<AudioLowPassFilter>();
			Object.DontDestroyOnLoad(gameObject);
		}
		AudioSource component = gameObject.GetComponent<AudioSource>();
		AudioLowPassFilter component2 = gameObject.GetComponent<AudioLowPassFilter>();
		component.clip = GetClip(Name)?.next();
		component.volume = Volume;
		component.pitch = 1f - ((float)Stat.Rnd5.NextDouble() * 2f - 1f) * PitchVariance;
		component2.cutoffFrequency = 22000f * LowPass * Volume;
		PlayingAudioSources.Enqueue(gameObject);
		component.Play();
	}

	public static void Update()
	{
		soundsPlayedThisFrame.Clear();
		if (!Globals.EnableSound && PlayingAudioSources.Count > 0)
		{
			while (PlayingAudioSources.Count > 0)
			{
				GameObject gameObject = PlayingAudioSources.Dequeue();
				gameObject.GetComponent<AudioSource>().Stop();
				AudioSourcePool.Enqueue(gameObject);
			}
		}
		AudioListener.volume = MasterVolume;
		if (MusicSource != null)
		{
			if (!Globals.EnableMusic)
			{
				if (MusicAudioSource.enabled)
				{
					MusicAudioSource.enabled = false;
				}
			}
			else if (!MusicAudioSource.enabled)
			{
				MusicAudioSource.enabled = true;
			}
			if (nextTrack != "" && !MusicAudioSource.isPlaying)
			{
				_PlayMusic(nextTrack, Crossfade: false);
			}
			if (MusicTargetVolume * MusicVolume > MusicAudioSource.volume)
			{
				MusicAudioSource.volume += 0.2f * Time.deltaTime;
			}
			else if (MusicTargetVolume * MusicVolume < MusicAudioSource.volume)
			{
				MusicAudioSource.volume -= 0.2f * Time.deltaTime;
			}
		}
		while (PlayingAudioSources.Count > 0 && !PlayingAudioSources.Peek().GetComponent<AudioSource>().isPlaying)
		{
			AudioSourcePool.Enqueue(PlayingAudioSources.Dequeue());
		}
		GetRequests();
		for (int i = 0; i < Requests.Count; i++)
		{
			SoundRequest soundRequest = Requests[i];
			if (soundRequest.Type == SoundRequest.SoundRequestType.Spatial || soundRequest.Type == SoundRequest.SoundRequestType.Sound)
			{
				if (soundRequest.Clip == null || soundsPlayedThisFrame.Contains(soundRequest.Clip))
				{
					continue;
				}
				soundsPlayedThisFrame.Add(soundRequest.Clip);
			}
			if (soundRequest.Type == SoundRequest.SoundRequestType.Spatial)
			{
				if (Options.Sound)
				{
					_PlayWorldSound(soundRequest.Clip, soundRequest.Volume, soundRequest.LowPass, soundRequest.PitchVariance);
				}
			}
			else if (soundRequest.Type == SoundRequest.SoundRequestType.Sound)
			{
				if (Options.Sound)
				{
					_PlaySound(soundRequest.Clip, soundRequest.Volume, soundRequest.Pitch);
				}
			}
			else if (soundRequest.Type == SoundRequest.SoundRequestType.Music && Options.Music)
			{
				_PlayMusic(soundRequest.Clip, soundRequest.Crossfade, soundRequest.CrossfadeDuration);
			}
		}
		for (int j = 0; j < Requests.Count; j++)
		{
			RequestPool.Enqueue(Requests[j]);
		}
		Requests.Clear();
	}

	public static AudioClipSet GetClip(string Name)
	{
		if (Name == null)
		{
			return null;
		}
		if (!Clips.ContainsKey(Name))
		{
			string ucName = "\\" + Name.ToUpper();
			ucName = ucName.Replace("/", "\\");
			Object @object = Resources.Load("Sounds/" + Name);
			if (!Clips.ContainsKey(Name))
			{
				Clips.Add(Name, new AudioClipSet());
			}
			Clips[Name].add(Name, @object as AudioClip);
			ModManager.ForEachFileIn("Sounds", delegate(string filePath, ModInfo ModInfo)
			{
				if (Path.ChangeExtension(filePath, null).Replace("/", "\\").ToUpper()
					.EndsWith(ucName))
				{
					if (!Clips.ContainsKey(Name))
					{
						Clips.Add(Name, new AudioClipSet());
					}
					Clips[Name].add(Name, Manager.Load(filePath, doStream: false, loadInBackground: false));
				}
			});
			for (int i = 0; i < 999; i++)
			{
				string variantName = Name + "-" + i.ToString("000");
				string ucVariantName = "\\" + variantName.ToUpper();
				ucVariantName = ucVariantName.Replace("/", "\\");
				bool loaded = false;
				@object = Resources.Load("Sounds/" + variantName);
				if (@object != null)
				{
					loaded = true;
					if (!Clips.ContainsKey(Name))
					{
						Clips.Add(Name, new AudioClipSet());
					}
					Clips[Name].add(variantName, @object as AudioClip);
				}
				ModManager.ForEachFileIn("Sounds", delegate(string filePath, ModInfo ModInfo)
				{
					if (Path.ChangeExtension(filePath, null).Replace("/", "\\").ToUpper()
						.EndsWith(ucVariantName))
					{
						if (!Clips.ContainsKey(Name))
						{
							Clips.Add(variantName, new AudioClipSet());
						}
						AudioClip audioClip = Manager.Load(filePath, doStream: false, loadInBackground: false);
						if (audioClip != null)
						{
							loaded = true;
							Clips[Name].add(variantName, audioClip);
						}
					}
				});
				if (!loaded && i >= 2)
				{
					break;
				}
			}
		}
		return Clips[Name];
	}

	public static float GetSoundFrameVolume(string Clip)
	{
		float num = 1f;
		int currentFrameAtFPS = XRLCore.GetCurrentFrameAtFPS(50);
		if (ClipFrames.TryGetValue(Clip, out var value))
		{
			if (value[0] >= currentFrameAtFPS - 1)
			{
				value[0] = currentFrameAtFPS;
				int num2 = ++value[1];
				if (num2 >= 5)
				{
					return 0f;
				}
				num /= (float)(num2 * num2);
			}
			else
			{
				value[0] = currentFrameAtFPS;
				value[1] = 1;
			}
		}
		else
		{
			ClipFrames[Clip] = new int[2] { currentFrameAtFPS, 1 };
		}
		return num;
	}
}
