using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.Wish;
using XRL.World.Parts;

namespace XRL.World.Tinkering;

[Serializable]
[HasWishCommand]
[HasGameBasedStaticCache]
public class TinkerData
{
	public string DisplayName;

	public string Blueprint;

	public string Category;

	public string Type;

	public int Tier;

	public string Cost;

	public string Ingredient;

	public int DescriptionLineCount = 1;

	[NonSerialized]
	private string _PartName;

	public static List<TinkerData> TinkerRecipes;

	[GameBasedStaticCache]
	public static List<TinkerData> KnownRecipes = new List<TinkerData>();

	public static Dictionary<TinkerData, string> DataDescriptions = new Dictionary<TinkerData, string>();

	public static Dictionary<TinkerData, string> DataNames = new Dictionary<TinkerData, string>();

	public string PartName
	{
		get
		{
			if (_PartName == null)
			{
				_PartName = Blueprint.Replace("[mod]", "");
			}
			return _PartName;
		}
	}

	public string LongDisplayName
	{
		get
		{
			if (!DataNames.ContainsKey(this))
			{
				if (GameObjectFactory.Factory.Blueprints.ContainsKey(Blueprint))
				{
					DataNames.Add(this, TinkeringHelpers.TinkeredItemDisplayName(Blueprint));
				}
				else
				{
					DataNames.Add(this, "<invalid blueprint>");
				}
			}
			return DataNames[this];
		}
	}

	public string Description
	{
		get
		{
			if (!DataDescriptions.ContainsKey(this))
			{
				if (GameObjectFactory.Factory.HasBlueprint(Blueprint))
				{
					GameObject gameObject = GameObject.createSample(Blueprint);
					if (gameObject.HasPart("Description"))
					{
						StringBuilder stringBuilder = Event.NewStringBuilder();
						stringBuilder.Append('\n');
						if (gameObject.GetPart("TinkerItem") is TinkerItem tinkerItem && tinkerItem.NumberMade > 1)
						{
							stringBuilder.Append("{{rules|Makes a batch of ").Append(Grammar.Cardinal(tinkerItem.NumberMade)).Append(".}}\n\n");
						}
						if (gameObject.GetPart("Description") is Description description)
						{
							stringBuilder.Append(description.GetShortDescription(AsIfKnown: true, NoConfusion: true, "Tinkering"));
						}
						stringBuilder.Append('\n');
						DataDescriptions.Add(this, StringFormat.ClipText(stringBuilder.ToString(), 76, KeepNewlines: true));
						DescriptionLineCount = DataDescriptions[this].Count((char c) => c == '\n');
					}
					else
					{
						DataDescriptions.Add(this, "<none>");
					}
					gameObject.Obliterate();
				}
				else
				{
					DataDescriptions.Add(this, "<none>");
				}
			}
			return DataDescriptions[this];
		}
	}

	public void SaveData(SerializationWriter writer)
	{
		writer.Write(DisplayName);
		writer.Write(Blueprint);
		writer.Write(Category);
		writer.Write(Type);
		writer.Write(Tier);
		writer.Write(Cost);
		writer.Write(Ingredient);
		writer.Write(DescriptionLineCount);
	}

	public static TinkerData LoadData(SerializationReader reader)
	{
		return new TinkerData
		{
			DisplayName = reader.ReadString(),
			Blueprint = reader.ReadString(),
			Category = reader.ReadString(),
			Type = reader.ReadString(),
			Tier = reader.ReadInt32(),
			Cost = reader.ReadString(),
			Ingredient = reader.ReadString(),
			DescriptionLineCount = reader.ReadInt32()
		};
	}

	public bool CanMod(string ModTag)
	{
		ModEntry modEntry = ModificationFactory.ModsByPart[PartName];
		if (!modEntry.TinkerAllowed)
		{
			return false;
		}
		List<string> list = ModTag.CachedCommaExpansion();
		string[] tableList = modEntry.TableList;
		foreach (string item in tableList)
		{
			if (list.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static void Reset()
	{
		KnownRecipes = new List<TinkerData>();
		TinkerItem.BitCostMap = new Dictionary<string, string>();
	}

	public bool Known()
	{
		return KnownRecipes.Any((TinkerData r) => r == this || r.Blueprint == Blueprint);
	}

	public static void UnlearnRecipe(string blueprint)
	{
		for (int i = 0; i < KnownRecipes.Count; i++)
		{
			if (KnownRecipes[i].Blueprint == blueprint)
			{
				KnownRecipes.RemoveAt(i);
				break;
			}
		}
	}

	[WishCommand(null, null)]
	public static bool DataDisk(string blueprint)
	{
		if (blueprint.StartsWith("mod:"))
		{
			blueprint = "[mod]" + blueprint.Substring(4);
		}
		blueprint = blueprint.ToLower();
		GameObject gO = createDataDisk(TinkerRecipes.FirstOrDefault((TinkerData t) => t.Blueprint.ToLower() == blueprint));
		XRLCore.Core.Game.Player.Body.TakeObject(gO, Silent: false, 0);
		return true;
	}

	[WishCommand(null, null)]
	public static bool DataDisk()
	{
		int index = Popup.ShowOptionList("Pick a Blueprint:", TinkerRecipes.ConvertAll((TinkerData t) => t.Blueprint).ToArray());
		GameObject gO = createDataDisk(TinkerRecipes[index]);
		XRLCore.Core.Game.Player.Body.TakeObject(gO, Silent: false, 0);
		return true;
	}

	public static GameObject createDataDisk(string blueprint)
	{
		if (blueprint.StartsWith("mod:"))
		{
			string modBlueprint = "[mod]" + blueprint.Substring(4);
			return createDataDisk(TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Mod" && t.Blueprint == modBlueprint));
		}
		return createDataDisk(TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Build" && t.Blueprint == blueprint));
	}

	public static GameObject createDataDisk(TinkerData data)
	{
		return GameObjectFactory.Factory.CreateObject("DataDisk", delegate(GameObject o)
		{
			o.GetPart<DataDisk>().Data = data;
		});
	}

	public static void LearnBlueprint(string blueprint)
	{
		TinkerData tinkerData = TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Build" && t.Blueprint == blueprint);
		if (tinkerData != null && !KnownRecipes.Contains(tinkerData))
		{
			KnownRecipes.Add(tinkerData);
		}
	}

	public static void LearnMod(string blueprint)
	{
		string ModBlueprint = "[mod]" + blueprint;
		TinkerData tinkerData = TinkerRecipes.FirstOrDefault((TinkerData t) => t.Type == "Mod" && t.Blueprint == ModBlueprint);
		if (tinkerData != null && !KnownRecipes.Contains(tinkerData))
		{
			KnownRecipes.Add(tinkerData);
		}
	}

	public static bool RecipeKnown(TinkerData Data)
	{
		foreach (TinkerData knownRecipe in KnownRecipes)
		{
			if (knownRecipe.Blueprint == Data.Blueprint)
			{
				return true;
			}
		}
		return false;
	}
}
