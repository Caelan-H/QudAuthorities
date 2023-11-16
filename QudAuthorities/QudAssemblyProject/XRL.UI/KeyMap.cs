using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ConsoleLib.Console;
using UnityEngine;

namespace XRL.UI;

[Serializable]
public class KeyMap
{
	public Dictionary<int, string> PrimaryMapKeyToCommand;

	public Dictionary<string, int> PrimaryMapCommandToKey;

	public Dictionary<int, string> SecondaryMapKeyToCommand;

	public Dictionary<string, int> SecondaryMapCommandToKey;

	[OptionalField]
	public Dictionary<string, Dictionary<int, string>> PrimaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();

	[OptionalField]
	public Dictionary<string, Dictionary<int, string>> SecondaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();

	[OptionalField]
	public Dictionary<string, Dictionary<string, int>> PrimaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();

	[OptionalField]
	public Dictionary<string, Dictionary<string, int>> SecondaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();

	public bool IsKeyMapped(UnityEngine.KeyCode key, string[] layersToInclude = null)
	{
		return IsKeyMapped((int)Keyboard.Keymap[key], layersToInclude);
	}

	public bool IsKeyMapped(Keys key, string[] layersToInclude = null)
	{
		return IsKeyMapped((int)key, layersToInclude);
	}

	public bool IsKeyMapped(int key, string[] layersToInclude = null)
	{
		if (!PrimaryMapKeyToCommandLayer.Any((KeyValuePair<string, Dictionary<int, string>> layer) => (layersToInclude == null || layersToInclude.Contains(layer.Key)) && layer.Value.ContainsKey(key)))
		{
			return SecondaryMapKeyToCommandLayer.Any((KeyValuePair<string, Dictionary<int, string>> layer) => (layersToInclude == null || layersToInclude.Contains(layer.Key)) && layer.Value.ContainsKey(key));
		}
		return true;
	}

	public bool IsKeyMapped(int key, IEnumerable<string> layersToInclude = null)
	{
		if (!PrimaryMapKeyToCommandLayer.Any((KeyValuePair<string, Dictionary<int, string>> layer) => (layersToInclude == null || layersToInclude.Contains(layer.Key)) && layer.Value.ContainsKey(key)))
		{
			return SecondaryMapKeyToCommandLayer.Any((KeyValuePair<string, Dictionary<int, string>> layer) => (layersToInclude == null || layersToInclude.Contains(layer.Key)) && layer.Value.ContainsKey(key));
		}
		return true;
	}

	public string GetCommandMappedTo(int key, string[] layersToInclude = null)
	{
		string text = PrimaryMapKeyToCommandLayer.Where((KeyValuePair<string, Dictionary<int, string>> layer) => (layersToInclude == null || layersToInclude.Contains(layer.Key)) && layer.Value.ContainsKey(key)).FirstOrDefault().Value?[key];
		string result = SecondaryMapKeyToCommandLayer.Where((KeyValuePair<string, Dictionary<int, string>> layer) => (layersToInclude == null || layersToInclude.Contains(layer.Key)) && layer.Value.ContainsKey(key)).FirstOrDefault().Value?[key];
		if (text != null)
		{
			return text;
		}
		return result;
	}

	public void upgradeLayers()
	{
		try
		{
			foreach (KeyValuePair<string, GameCommand> cmd in LegacyKeyMapping.CommandsByID)
			{
				string cmdID = cmd.Key;
				while (true)
				{
					using (IEnumerator<KeyValuePair<string, Dictionary<string, int>>> enumerator2 = PrimaryMapCommandToKeyLayer.Where((KeyValuePair<string, Dictionary<string, int>> layer) => layer.Key != cmd.Value.Layer && layer.Value.ContainsKey(cmdID)).GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							KeyValuePair<string, Dictionary<string, int>> current = enumerator2.Current;
							Dictionary<string, int> value = current.Value;
							Dictionary<int, string> dictionary = PrimaryMapKeyToCommandLayer[current.Key];
							string layer2 = cmd.Value.Layer;
							int num = value[cmd.Key];
							value.Remove(cmd.Key);
							dictionary.Remove(num);
							if (!PrimaryMapKeyToCommandLayer.ContainsKey(layer2))
							{
								PrimaryMapKeyToCommandLayer.Add(layer2, new Dictionary<int, string>());
							}
							if (PrimaryMapKeyToCommandLayer[layer2].ContainsKey(num))
							{
								MetricsManager.LogWarning("duplicate upgrade mapping for " + num);
							}
							else
							{
								PrimaryMapKeyToCommandLayer[layer2].Add(num, cmd.Key);
							}
							if (!PrimaryMapCommandToKeyLayer.ContainsKey(layer2))
							{
								PrimaryMapCommandToKeyLayer.Add(layer2, new Dictionary<string, int>());
							}
							if (PrimaryMapCommandToKeyLayer[layer2].ContainsKey(cmd.Key))
							{
								MetricsManager.LogWarning("duplicate upgrade mapping for command " + cmd.Key);
							}
							else
							{
								PrimaryMapCommandToKeyLayer[layer2].Add(cmd.Key, num);
							}
							continue;
						}
					}
					break;
				}
				while (true)
				{
					using (IEnumerator<KeyValuePair<string, Dictionary<string, int>>> enumerator2 = SecondaryMapCommandToKeyLayer.Where((KeyValuePair<string, Dictionary<string, int>> layer) => layer.Key != cmd.Value.Layer && layer.Value.ContainsKey(cmdID)).GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							KeyValuePair<string, Dictionary<string, int>> current2 = enumerator2.Current;
							Dictionary<string, int> value2 = current2.Value;
							Dictionary<int, string> dictionary2 = SecondaryMapKeyToCommandLayer[current2.Key];
							string layer3 = cmd.Value.Layer;
							int num2 = value2[cmd.Key];
							value2.Remove(cmd.Key);
							dictionary2.Remove(num2);
							if (!SecondaryMapKeyToCommandLayer.ContainsKey(layer3))
							{
								SecondaryMapKeyToCommandLayer.Add(layer3, new Dictionary<int, string>());
							}
							if (SecondaryMapKeyToCommandLayer[layer3].ContainsKey(num2))
							{
								MetricsManager.LogWarning("Duplicate upgrade mapping for secondary key to command " + num2);
							}
							else
							{
								SecondaryMapKeyToCommandLayer[layer3].Add(num2, cmd.Key);
							}
							if (!SecondaryMapCommandToKeyLayer.ContainsKey(layer3))
							{
								SecondaryMapCommandToKeyLayer.Add(layer3, new Dictionary<string, int>());
							}
							if (SecondaryMapCommandToKeyLayer[layer3].ContainsKey(cmd.Key))
							{
								MetricsManager.LogWarning("Duplicate upgrade mapping for command key to layer " + cmd.Key);
							}
							else
							{
								SecondaryMapCommandToKeyLayer[layer3].Add(cmd.Key, num2);
							}
							continue;
						}
					}
					break;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("KeyMap::upgradeLayers", x);
		}
	}

	public KeyMap()
	{
		PrimaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();
		SecondaryMapKeyToCommandLayer = new Dictionary<string, Dictionary<int, string>>();
		PrimaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();
		SecondaryMapCommandToKeyLayer = new Dictionary<string, Dictionary<string, int>>();
		PrimaryMapCommandToKeyLayer.Add("*default", new Dictionary<string, int>());
		SecondaryMapCommandToKeyLayer.Add("*default", new Dictionary<string, int>());
		PrimaryMapKeyToCommandLayer.Add("*default", new Dictionary<int, string>());
		SecondaryMapKeyToCommandLayer.Add("*default", new Dictionary<int, string>());
	}

	public Dictionary<int, string> getPrimaryKeyToCommand(string commandId)
	{
		string layer = LegacyKeyMapping.CommandsByID[commandId].Layer;
		if (!PrimaryMapKeyToCommandLayer.ContainsKey(layer))
		{
			PrimaryMapKeyToCommandLayer.Add(layer, new Dictionary<int, string>());
		}
		return PrimaryMapKeyToCommandLayer[layer];
	}

	public Dictionary<string, int> getPrimaryCommandToKey(string commandId)
	{
		string layer = LegacyKeyMapping.CommandsByID[commandId].Layer;
		if (!PrimaryMapCommandToKeyLayer.ContainsKey(layer))
		{
			PrimaryMapCommandToKeyLayer.Add(layer, new Dictionary<string, int>());
		}
		return PrimaryMapCommandToKeyLayer[layer];
	}

	public Dictionary<int, string> getSecondaryKeyToCommand(string commandId)
	{
		string layer = LegacyKeyMapping.CommandsByID[commandId].Layer;
		if (!SecondaryMapKeyToCommandLayer.ContainsKey(layer))
		{
			SecondaryMapKeyToCommandLayer.Add(layer, new Dictionary<int, string>());
		}
		return SecondaryMapKeyToCommandLayer[layer];
	}

	public Dictionary<string, int> getSecondaryCommandToKey(string commandId)
	{
		string layer = LegacyKeyMapping.CommandsByID[commandId].Layer;
		if (!SecondaryMapCommandToKeyLayer.ContainsKey(layer))
		{
			SecondaryMapCommandToKeyLayer.Add(layer, new Dictionary<string, int>());
		}
		return SecondaryMapCommandToKeyLayer[layer];
	}

	public void ApplyDefault(string Cmd, int Key)
	{
		if (!getPrimaryCommandToKey(Cmd).ContainsKey(Cmd) && !getSecondaryKeyToCommand(Cmd).ContainsKey(Key) && !getPrimaryKeyToCommand(Cmd).ContainsKey(Key))
		{
			getPrimaryCommandToKey(Cmd).Add(Cmd, Key);
			getPrimaryKeyToCommand(Cmd).Add(Key, Cmd);
		}
	}

	public void ApplyDefault(string Cmd, Keys Key)
	{
		ApplyDefault(Cmd, (int)Key);
	}

	public void ApplyDefaults()
	{
		ApplyDefault("CmdSystemMenu", Keys.Escape);
		ApplyDefault("CmdAttackDirection", Keys.Oem102);
		ApplyDefault("CmdAttackCell", (Keys)65762);
		ApplyDefault("CmdAttackNearest", (Keys)65601);
		ApplyDefault("CmdAutoAttack", (Keys)131137);
		ApplyDefault("CmdCancel", Keys.Escape);
		ApplyDefault("CmdGetFrom", (Keys)131143);
		ApplyDefault("CmdJournal", Keys.J);
		ApplyDefault("CmdMoveTo", (Keys)131085);
		ApplyDefault("CmdMoveToEdge", (Keys)65549);
		ApplyDefault("CmdMoveToPointOfInterest", Keys.Back);
		ApplyDefault("CmdReload", Keys.R);
		ApplyDefault("CmdChargenRandom", Keys.R);
		ApplyDefault("CmdChargenReset", Keys.Delete);
		ApplyDefault("CmdChargenMutationVariant", Keys.V);
		ApplyDefault("CmdChargenItemOptions", Keys.O);
		ApplyDefault("CmdCancel", Keys.Escape);
		ApplyDefault("CmdMissileWeaponMenu", Keys.Multiply);
	}
}
