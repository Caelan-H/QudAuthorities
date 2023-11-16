using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using XRL;
using XRL.World;

namespace Qud.API;

public static class SavesAPI
{
	private static long GetDirectorySize(string p)
	{
		string[] files = Directory.GetFiles(p, "*.*");
		long num = 0L;
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			FileInfo fileInfo = new FileInfo(array[i]);
			num += fileInfo.Length;
		}
		return num;
	}

	public static List<SaveGameInfo> GetSavedGameInfo()
	{
		List<SaveGameInfo> list = new List<SaveGameInfo>();
		string[] directories = Directory.GetDirectories(XRLGame.GetSaveDirectory());
		foreach (string text in directories)
		{
			try
			{
				if (Path.GetFileNameWithoutExtension(text).EqualsNoCase("mods") || Path.GetFileNameWithoutExtension(text).EqualsNoCase("textures"))
				{
					continue;
				}
				if (File.Exists(Path.Combine(text, "Primary.sav.json")) && Directory.GetFiles(text).Length != 0)
				{
					SaveGameJSON saveGameJSON = JsonConvert.DeserializeObject<SaveGameJSON>(File.ReadAllText(Path.Combine(text, "Primary.sav.json")));
					SaveGameInfo saveGameInfo = new SaveGameInfo
					{
						json = saveGameJSON,
						Directory = text,
						Size = "Total size: " + GetDirectorySize(text) / 1000000 + "mb",
						ID = saveGameJSON.ID,
						Version = saveGameJSON.GameVersion,
						Name = saveGameJSON.Name,
						Description = $"Level {saveGameJSON.Level} {saveGameJSON.GenoSubType} [{saveGameJSON.GameMode}]",
						Info = $"{saveGameJSON.Location}, {saveGameJSON.InGameTime} turn {saveGameJSON.Turn}",
						SaveTime = saveGameJSON.SaveTime,
						ModsEnabled = saveGameJSON.ModsEnabled
					};
					if (saveGameJSON.SaveVersion < 223 || saveGameJSON.SaveVersion > 264)
					{
						saveGameInfo.Name = "{{R|Older Version (" + saveGameJSON.GameVersion + ")}} " + saveGameInfo.Name;
					}
					list.Add(saveGameInfo);
					continue;
				}
				if (!File.Exists(Path.Combine(text, "Primary.sav.info")) && Directory.GetFiles(text).Length == 0)
				{
					Directory.Delete(text, recursive: true);
					continue;
				}
				if (!File.Exists(Path.Combine(text, "Primary.sav.info")) && Directory.GetFiles(text).Length != 0)
				{
					Debug.LogWarning("Weird save directory with no .info file present: " + text);
					continue;
				}
				using FileStream stream = File.Open(Path.Combine(text, "Primary.sav.info"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using SerializationReader serializationReader = new SerializationReader(stream);
				try
				{
					SaveGameInfo saveGameInfo2 = new SaveGameInfo();
					int num = serializationReader.ReadInt32();
					int num2 = serializationReader.ReadInt32();
					string text2 = (saveGameInfo2.Version = serializationReader.ReadString());
					if (num != 123457)
					{
						saveGameInfo2.Name = "&RIncompatible version (2.0.167.0 or earlier)";
						saveGameInfo2.Size = "Total size: " + GetDirectorySize(text) / 1000000 + "mb";
						saveGameInfo2.Directory = text;
						list.Add(saveGameInfo2);
					}
					else if (num2 < 223 || num2 > 264)
					{
						saveGameInfo2.Name = "&ROlder version (" + text2 + ")";
						saveGameInfo2.Size = "Total size: " + GetDirectorySize(text) / 1000000 + "mb";
						saveGameInfo2.Directory = text;
						list.Add(saveGameInfo2);
					}
					else
					{
						saveGameInfo2.ID = serializationReader.ReadString();
						saveGameInfo2.Name = serializationReader.ReadString();
						saveGameInfo2.Description = serializationReader.ReadString();
						saveGameInfo2.Info = serializationReader.ReadString();
						saveGameInfo2.SaveTime = serializationReader.ReadString();
						saveGameInfo2.ModsEnabled = serializationReader.ReadList<string>();
						saveGameInfo2.Directory = text;
						saveGameInfo2.Size = "Total size: " + GetDirectorySize(text) / 1000000 + "mb";
					}
					list.Add(saveGameInfo2);
				}
				catch (Exception ex)
				{
					MetricsManager.LogError(ex.ToString());
					SaveGameInfo saveGameInfo3 = new SaveGameInfo();
					saveGameInfo3.Name = "&RCorrupt info file";
					saveGameInfo3.Size = "Total size: " + GetDirectorySize(text) / 1000000 + "mb";
					saveGameInfo3.Directory = text;
					list.Add(saveGameInfo3);
					throw;
				}
			}
			catch (ThreadInterruptedException ex2)
			{
				throw ex2;
			}
			catch (Exception ex3)
			{
				MetricsManager.LogError(ex3.ToString());
			}
		}
		list.Sort(SortGameByDate);
		return list;
	}

	private static int SortGameByDate(SaveGameInfo I1, SaveGameInfo I2)
	{
		try
		{
			if (string.IsNullOrEmpty(I1.SaveTime) || !I1.SaveTime.Contains(" at "))
			{
				return 1;
			}
			if (string.IsNullOrEmpty(I2.SaveTime) || !I2.SaveTime.Contains(" at "))
			{
				return -1;
			}
			string text = I1.SaveTime.Substring(0, I1.SaveTime.IndexOf(" at "));
			string text2 = I1.SaveTime.Substring(I1.SaveTime.IndexOf(" at ") + 4);
			string text3 = I2.SaveTime.Substring(0, I2.SaveTime.IndexOf(" at "));
			string text4 = I2.SaveTime.Substring(I2.SaveTime.IndexOf(" at ") + 4);
			DateTime value = DateTime.Parse(text + " " + text2);
			return DateTime.Parse(text3 + " " + text4).CompareTo(value);
		}
		catch
		{
			return 0;
		}
	}
}
