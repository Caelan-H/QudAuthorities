using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipSet
{
	private List<AudioClip> clips;

	private int n;

	private static System.Random clipShuffler = new System.Random();

	public void unload()
	{
		if (clips == null)
		{
			return;
		}
		foreach (AudioClip clip in clips)
		{
			Resources.UnloadAsset(clip);
		}
		clips.Clear();
	}

	public AudioClip next()
	{
		if (clips == null)
		{
			return null;
		}
		if (n >= clips.Count)
		{
			if (clips.Count > 1)
			{
				clips.ShuffleInPlace(clipShuffler);
			}
			n = 0;
		}
		if (n < clips.Count)
		{
			AudioClip result = clips[n];
			n++;
			return result;
		}
		return null;
	}

	public void add(string name, AudioClip clip)
	{
		if (!(clip == null))
		{
			if (clips == null)
			{
				clips = new List<AudioClip>();
			}
			clip.name = name;
			clips.RemoveAll((AudioClip clip) => clip.name == name);
			clips.Add(clip);
		}
	}
}
