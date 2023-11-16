using System;
using System.IO;
using UnityEngine;
using XRL.UI;

namespace XRL;

public static class Prefs
{
	public static NameValueBag Bag;

	public static void Init()
	{
		Debug.Log("Loading user prefs");
		try
		{
			if (!File.Exists(DataManager.SavePath("UserPrefs.json")) && File.Exists(DataManager.FilePath("UserPrefs.json")))
			{
				File.Move(DataManager.FilePath("UserPrefs.json"), DataManager.SavePath("UserPrefs.json"));
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		Bag = new NameValueBag(DataManager.SavePath("UserPrefs.json"));
		Bag.Load();
	}

	public static bool HasString(string Name)
	{
		if (Bag == null)
		{
			Init();
		}
		return Bag.Bag.ContainsKey(Name);
	}

	public static string GetString(string Name, string Default = null)
	{
		if (Bag == null)
		{
			Init();
		}
		lock (Bag)
		{
			if (Bag.Bag.TryGetValue(Name, out var value))
			{
				return value;
			}
			return Default;
		}
	}

	public static void SetString(string Name, string Value)
	{
		if (Bag == null)
		{
			Init();
		}
		lock (Bag)
		{
			Bag.SetValue(Name, Value);
		}
	}
}
