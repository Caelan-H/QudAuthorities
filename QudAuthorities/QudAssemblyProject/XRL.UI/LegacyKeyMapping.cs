using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using ConsoleLib.Console;
using Newtonsoft.Json;
using UnityEngine;

namespace XRL.UI;

public class LegacyKeyMapping
{
	public static Dictionary<string, List<GameCommand>> CommandsByCategory;

	public static Dictionary<string, GameCommand> CommandsByID;

	public static List<string> CategoriesInOrder;

	public static KeyMap CurrentMap;

	private static string[] default_exclusions = new string[1] { "Chargen" };

	private static Dictionary<string, Action<XmlDataHelper>> _Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "commands", HandleNodes },
		{ "command", HandleCommandNode }
	};

	public static bool IsKeyMapped(int key)
	{
		return CurrentMap.IsKeyMapped(key, null);
	}

	public static bool IsKeyMapped(UnityEngine.KeyCode key)
	{
		return IsKeyMapped((int)Keyboard.Keymap[key], null);
	}

	public static bool IsKeyMapped(UnityEngine.KeyCode key, IEnumerable<string> layersToInclude)
	{
		return IsKeyMapped((int)Keyboard.Keymap[key], layersToInclude);
	}

	public static bool IsKeyMapped(Keys key, string[] layersToInclude)
	{
		return IsKeyMapped((int)key, layersToInclude);
	}

	public static bool IsKeyMapped(int key, string[] layerToInclude)
	{
		return CurrentMap.IsKeyMapped(key, layerToInclude);
	}

	public static bool IsKeyMapped(int key, IEnumerable<string> layerToInclude)
	{
		return CurrentMap.IsKeyMapped(key, layerToInclude);
	}

	public static string GetCommandMappedTo(int key, string[] layersToInclude = null)
	{
		return CurrentMap.GetCommandMappedTo(key, layersToInclude);
	}

	public static string GetCommandFromKey(int c)
	{
		foreach (KeyValuePair<string, Dictionary<int, string>> item in CurrentMap.PrimaryMapKeyToCommandLayer)
		{
			if (item.Value.ContainsKey(c))
			{
				return item.Value[c];
			}
		}
		foreach (KeyValuePair<string, Dictionary<int, string>> item2 in CurrentMap.SecondaryMapKeyToCommandLayer)
		{
			if (item2.Value.ContainsKey(c))
			{
				return item2.Value[c];
			}
		}
		return "CmdUnknown";
	}

	public static string GetCommandFromKey(Keys k)
	{
		return GetCommandFromKey((int)k);
	}

	public static string MapKeyToCommand(int Meta, string[] exclusions = null)
	{
		if (exclusions == null)
		{
			exclusions = default_exclusions;
		}
		foreach (KeyValuePair<string, Dictionary<int, string>> item in CurrentMap.PrimaryMapKeyToCommandLayer)
		{
			if (!exclusions.Contains(item.Key) && item.Value.ContainsKey(Meta))
			{
				return item.Value[Meta];
			}
		}
		foreach (KeyValuePair<string, Dictionary<int, string>> item2 in CurrentMap.SecondaryMapKeyToCommandLayer)
		{
			if (!exclusions.Contains(item2.Key) && item2.Value.ContainsKey(Meta))
			{
				return item2.Value[Meta];
			}
		}
		return "CmdUnknown";
	}

	public static string GetNextCommand(string[] exclusions = null)
	{
		return MapKeyToCommand(Keyboard.getmeta(MapDirectionToArrows: false));
	}

	public static (int, int) GetAllKeysFromCommand(string Cmd)
	{
		int value = -1;
		int value2 = -1;
		if (CurrentMap?.PrimaryMapCommandToKeyLayer != null)
		{
			using Dictionary<string, Dictionary<string, int>>.Enumerator enumerator = CurrentMap.PrimaryMapCommandToKeyLayer.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.Value.TryGetValue(Cmd, out value))
			{
			}
		}
		if (CurrentMap?.SecondaryMapCommandToKeyLayer != null)
		{
			using Dictionary<string, Dictionary<string, int>>.Enumerator enumerator = CurrentMap.SecondaryMapCommandToKeyLayer.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.Value.TryGetValue(Cmd, out value2))
			{
			}
		}
		return (value, value2);
	}

	public static int GetKeyFromCommand(string Cmd)
	{
		if (CurrentMap?.PrimaryMapCommandToKeyLayer != null)
		{
			foreach (KeyValuePair<string, Dictionary<string, int>> item in CurrentMap.PrimaryMapCommandToKeyLayer)
			{
				if (item.Value.TryGetValue(Cmd, out var value))
				{
					return value;
				}
			}
		}
		if (CurrentMap?.SecondaryMapCommandToKeyLayer != null)
		{
			foreach (KeyValuePair<string, Dictionary<string, int>> item2 in CurrentMap.SecondaryMapCommandToKeyLayer)
			{
				if (item2.Value.TryGetValue(Cmd, out var value2))
				{
					return value2;
				}
			}
		}
		return 0;
	}

	public static string GetCurrentKeymapPath()
	{
		return DataManager.SavePath(Environment.UserName + ".Keymap.json");
	}

	public static void SaveCurrentKeymap()
	{
		SaveKeymap(CurrentMap, GetCurrentKeymapPath());
	}

	public static void SaveKeymap(KeyMap Map, string FileName)
	{
		File.WriteAllText(FileName, JsonConvert.SerializeObject(Map));
	}

	public static void LoadCurrentKeymap()
	{
		CurrentMap = LoadKeymap(GetCurrentKeymapPath());
	}

	public static KeyMap LoadKeymap(string FileName)
	{
		if (!File.Exists(FileName))
		{
			return null;
		}
		return JsonConvert.DeserializeObject<KeyMap>(File.ReadAllText(FileName));
	}

	public static void SaveLegacyKeymap(KeyMap Map, string FileName)
	{
		FileStream fileStream = File.OpenWrite(FileName);
		((IFormatter)new BinaryFormatter()).Serialize((Stream)fileStream, (object)Map);
		fileStream.Close();
	}

	public static KeyMap LoadLegacyKeymap(string FileName)
	{
		Stream stream = File.OpenRead(FileName);
		KeyMap keyMap = ((IFormatter)new BinaryFormatter()).Deserialize(stream) as KeyMap;
		keyMap.PrimaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();
		keyMap.PrimaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();
		keyMap.SecondaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();
		keyMap.SecondaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();
		keyMap.PrimaryMapCommandToKeyLayer.Add("*default", keyMap.PrimaryMapCommandToKey);
		keyMap.PrimaryMapKeyToCommandLayer.Add("*default", keyMap.PrimaryMapKeyToCommand);
		keyMap.SecondaryMapKeyToCommandLayer.Add("*default", keyMap.SecondaryMapKeyToCommand);
		keyMap.SecondaryMapCommandToKeyLayer.Add("*default", keyMap.SecondaryMapCommandToKey);
		keyMap.PrimaryMapCommandToKey = null;
		keyMap.PrimaryMapKeyToCommand = null;
		keyMap.SecondaryMapCommandToKey = null;
		keyMap.SecondaryMapKeyToCommand = null;
		stream.Close();
		keyMap.upgradeLayers();
		CleanKeymapCommands(keyMap);
		return keyMap;
	}

	public static void CleanKeymapCommands(KeyMap Map)
	{
	}

	public static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(_Nodes);
	}

	public static void HandleCommandNode(XmlDataHelper xml)
	{
		GameCommand gameCommand = new GameCommand();
		gameCommand.ID = xml.GetAttribute("ID");
		gameCommand.DisplayText = xml.GetAttribute("DisplayText");
		gameCommand.Category = xml.GetAttribute("Category");
		gameCommand.Layer = xml.GetAttribute("Layer");
		if (string.IsNullOrEmpty(gameCommand.Layer))
		{
			gameCommand.Layer = "*default";
		}
		xml.DoneWithElement();
		if (!CommandsByCategory.ContainsKey(gameCommand.Category))
		{
			CommandsByCategory.Add(gameCommand.Category, new List<GameCommand>());
		}
		CommandsByCategory[gameCommand.Category].Add(gameCommand);
		if (CommandsByID.ContainsKey(gameCommand.ID))
		{
			CommandsByID[gameCommand.ID] = gameCommand;
		}
		else
		{
			CommandsByID.Add(gameCommand.ID, gameCommand);
		}
		if (!CategoriesInOrder.Contains(gameCommand.Category))
		{
			CategoriesInOrder.Add(gameCommand.Category);
		}
	}

	public static void LoadCommands()
	{
		CommandsByCategory = new Dictionary<string, List<GameCommand>>();
		CommandsByID = new Dictionary<string, GameCommand>();
		CategoriesInOrder = new List<string>();
		using (XmlDataHelper xmlDataHelper = DataManager.GetXMLStream("Commands.xml", null))
		{
			HandleNodes(xmlDataHelper);
			xmlDataHelper.Close();
		}
		ModManager.ForEachFile("Commands.xml", delegate(string path, ModInfo modInfo)
		{
			using XmlDataHelper xmlDataHelper2 = DataManager.GetXMLStream(path, modInfo);
			HandleNodes(xmlDataHelper2);
			xmlDataHelper2.Close();
		});
		CurrentMap = new KeyMap();
		try
		{
			if (GameManager.Instance.PrereleaseInput && File.Exists(DataManager.FilePath("RewiredKeymap.json")))
			{
				CurrentMap = LoadKeymap(DataManager.FilePath("RewiredKeymap.json"));
			}
			else if (GameManager.Instance.PrereleaseInput && File.Exists(DataManager.FilePath("RewiredKeymap.xml")))
			{
				CurrentMap = LoadLegacyKeymap(DataManager.FilePath("RewiredKeymap.xml"));
			}
			else if (File.Exists(DataManager.SavePath(Environment.UserName + ".Keymap.json")))
			{
				try
				{
					CurrentMap = LoadKeymap(DataManager.SavePath(Environment.UserName + ".Keymap.json"));
				}
				catch (Exception)
				{
					if (File.Exists(DataManager.SavePath(Environment.UserName + ".Keymap.xml")))
					{
						CurrentMap = LoadLegacyKeymap(DataManager.SavePath(Environment.UserName + ".Keymap.xml"));
					}
					else if (File.Exists(DataManager.FilePath("DefaultKeymap.json")))
					{
						CurrentMap = LoadKeymap(DataManager.FilePath("DefaultKeymap.json"));
					}
				}
			}
			else if (File.Exists(DataManager.SavePath(Environment.UserName + ".Keymap.xml")))
			{
				try
				{
					CurrentMap = LoadLegacyKeymap(DataManager.SavePath(Environment.UserName + ".Keymap.xml"));
				}
				catch (Exception)
				{
					if (File.Exists(DataManager.FilePath("DefaultKeymap.json")))
					{
						CurrentMap = LoadKeymap(DataManager.FilePath("DefaultKeymap.json"));
					}
				}
			}
			else if (File.Exists(DataManager.FilePath("DefaultKeymap.json")))
			{
				try
				{
					CurrentMap = LoadKeymap(DataManager.FilePath("DefaultKeymap.json"));
				}
				catch (Exception)
				{
					if (File.Exists(DataManager.FilePath("DefaultKeymap.json")))
					{
						CurrentMap = LoadKeymap(DataManager.FilePath("DefaultKeymap.json"));
					}
					else
					{
						CurrentMap = new KeyMap();
					}
				}
			}
			else if (File.Exists(DataManager.FilePath("DefaultKeymap.json")))
			{
				try
				{
					if (File.Exists(DataManager.FilePath("DefaultKeymap.json")))
					{
						CurrentMap = LoadKeymap(DataManager.FilePath("DefaultKeymap.json"));
					}
				}
				catch (Exception)
				{
					CurrentMap = new KeyMap();
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Loading keymapping", x);
			CurrentMap = new KeyMap();
		}
		CurrentMap.upgradeLayers();
		CurrentMap.ApplyDefaults();
	}

	private static void LoadCommandsNode(XmlTextReader Reader)
	{
		while (Reader.Read())
		{
			_ = Reader.Name == "command";
		}
	}
}
