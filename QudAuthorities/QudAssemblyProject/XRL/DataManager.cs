using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;

namespace XRL;

public static class DataManager
{
	public static Dictionary<string, string> contents = new Dictionary<string, string>();

	public const string STEAM_PATH_REGEX = "^(.+)[/\\\\]steamapps[/\\\\]";

	public static void preloadContents(string path)
	{
		try
		{
			string key = Path.GetFileName(path).ToLower();
			if (!contents.ContainsKey(key))
			{
				string text = Path.Combine(Path.Combine(Application.streamingAssetsPath, "Base"), path);
				WWW wWW = new WWW(text);
				while (!wWW.isDone)
				{
					Thread.Sleep(0);
				}
				contents.Add(key, wWW.text);
				MetricsManager.LogInfo("loaded " + text + " length " + wWW.text.Length);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("error loading " + path, x);
		}
	}

	public static Stream GenerateStreamFromString(string s)
	{
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.Write(s);
		streamWriter.Flush();
		memoryStream.Position = 0L;
		return memoryStream;
	}

	public static XmlDataHelper GetXMLStream(string FileName, ModInfo modInfo)
	{
		string key = Path.GetFileName(FileName).ToLower();
		if (contents.ContainsKey(key))
		{
			return new XmlDataHelper(GenerateStreamFromString(contents[key]), modInfo);
		}
		return new XmlDataHelper(FilePath(FileName), modInfo);
	}

	public static XmlTextReader GetStreamingAssetsXMLStream(string FileName)
	{
		string key = Path.GetFileName(FileName).ToLower();
		if (contents.ContainsKey(key))
		{
			return new XmlTextReader(GenerateStreamFromString(contents[key]));
		}
		return new XmlTextReader(FilePath(FileName));
	}

	public static StreamReader GetStreamingAssetsStreamReader(string FileName)
	{
		string key = Path.GetFileName(FileName).ToLower();
		if (contents.ContainsKey(key))
		{
			return new StreamReader(GenerateStreamFromString(contents[key]));
		}
		return new StreamReader(FilePath(FileName));
	}

	public static string FilePath(string FileName)
	{
		if (XRLCore.DataPath == "")
		{
			XRLCore.DataPath = Path.Combine(Application.streamingAssetsPath, "Base");
		}
		return Path.Combine(XRLCore.DataPath, FileName);
	}

	public static string SavePath(string FileName)
	{
		return Path.Combine(XRLCore.SavePath, FileName);
	}

	public static string SanitizePathForDisplay(string Source)
	{
		return ConsoleLib.Console.ColorUtility.EscapeFormatting(Regex.Replace(Source.Replace(XRLCore.SavePath, "<...>"), "^(.+)[/\\\\]steamapps[/\\\\]", "<...>/steamapps/").Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar));
	}
}
