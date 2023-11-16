using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Encounters;

[HasModSensitiveStaticCache]
public class EncounterFactory
{
	public Dictionary<string, EncounterTable> EncounterTables = new Dictionary<string, EncounterTable>();

	[ModSensitiveStaticCache(false)]
	private static EncounterFactory _Factory;

	private int BatchID;

	public static EncounterFactory Factory
	{
		get
		{
			if (_Factory == null)
			{
				_Factory = new EncounterFactory();
				Loading.LoadTask("Loading EncounterTables.xml", _Factory.LoadEncounterTables);
			}
			return _Factory;
		}
	}

	public void LoadEncounterTables()
	{
		EncounterTableXmlParser encounterTableXmlParser = new EncounterTableXmlParser(this);
		List<(string, ModInfo)> Paths = new List<(string, ModInfo)>();
		Paths.Add((DataManager.FilePath("EncounterTables.xml"), null));
		string[] files = Directory.GetFiles(DataManager.FilePath("."), "EncounterTables_*.xml", SearchOption.AllDirectories);
		foreach (string item in files)
		{
			Paths.Add((item, null));
		}
		ModManager.ForEachFile("EncounterTables.xml", delegate(string file, ModInfo modInfo)
		{
			Paths.Add((file, modInfo));
		});
		EncounterTables = new Dictionary<string, EncounterTable>();
		foreach (var item4 in Paths)
		{
			string item2 = item4.Item1;
			ModInfo item3 = item4.Item2;
			XmlDataHelper xMLStream = DataManager.GetXMLStream(item2, item3);
			encounterTableXmlParser.HandleNodes(xMLStream);
			xMLStream.Close();
		}
	}

	private bool ApplyEncounterObjectBuilderToObject(string sBuilder, GameObject GO, string Context = null)
	{
		Type type = ModManager.ResolveType("XRL.World.Encounters.EncounterObjectBuilders." + sBuilder);
		if (type == null)
		{
			return true;
		}
		object obj = Activator.CreateInstance(type);
		MethodInfo method = type.GetMethod("BuildObject");
		if (method != null && !(bool)method.Invoke(obj, new object[2] { GO, Context }))
		{
			return false;
		}
		return true;
	}

	private bool ApplyEncounterObjectToEncounter(EncounterObjectBase Object, Encounter NewEncounter, EncounterTable EncounterTable, int Tier, string Context = null)
	{
		BatchID++;
		if (BatchID >= 2147483646)
		{
			BatchID = 0;
		}
		if (Object is EncounterObject encounterObject)
		{
			try
			{
				if (!string.IsNullOrEmpty(encounterObject.Number) && Convert.ToInt32(encounterObject.Chance).in100())
				{
					if (encounterObject.Builders == null)
					{
						encounterObject.Builders = encounterObject.Builder?.Split(',') ?? null;
					}
					int num = encounterObject.Number.RollCached();
					for (int i = 0; i < num; i++)
					{
						GameObject gameObject = GameObject.create(encounterObject.Blueprint);
						gameObject.SetLongProperty("Batch", BatchID);
						if (encounterObject.Builders != null)
						{
							int j = 0;
							for (int num2 = encounterObject.Builders.Length; j < num2; j++)
							{
								ApplyEncounterObjectBuilderToObject(encounterObject.Builders[j], gameObject, Context);
							}
						}
						NewEncounter.Objects.Add(gameObject);
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("processing EncounterObject " + encounterObject.ToString(), x);
			}
		}
		if (Object is EncounterTableObject encounterTableObject)
		{
			try
			{
				if (Convert.ToInt32(encounterTableObject.Chance).in100())
				{
					string text = encounterTableObject.Table;
					if (text.Contains("_"))
					{
						text = text.Replace("_", Tier.ToString());
					}
					if (Factory.EncounterTables.TryGetValue(text, out var value))
					{
						if (value.Objects != null)
						{
							int k = 0;
							for (int count = value.Objects.Count; k < count; k++)
							{
								ApplyEncounterObjectToEncounter(value.Objects[k], NewEncounter, EncounterTable, Tier, Context);
							}
						}
						if (value.MergeTables != null)
						{
							int l = 0;
							for (int count2 = value.MergeTables.Count; l < count2; l++)
							{
								EncounterTable.AddMergeTable(value.MergeTables[l]);
							}
						}
					}
					else
					{
						MetricsManager.LogError("encounter table \"" + text + "\" not found for EncounterTableObject deploy");
					}
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogError("processing EncounterTableObject " + encounterTableObject.Table, x2);
			}
		}
		if (Object is EncounterPopulation encounterPopulation)
		{
			try
			{
				if (Convert.ToInt32(encounterPopulation.Chance).in100())
				{
					int num3 = (string.IsNullOrEmpty(encounterPopulation.Number) ? 1 : encounterPopulation.Number.RollCached());
					Dictionary<string, string> vars = new Dictionary<string, string>
					{
						{
							"zonetier",
							ZoneManager.zoneGenerationContextTier.ToString()
						},
						{
							"zonetier+1",
							XRL.World.Capabilities.Tier.Constrain(ZoneManager.zoneGenerationContextTier + 1).ToString()
						}
					};
					for (int m = 0; m < num3; m++)
					{
						foreach (PopulationResult item in PopulationManager.Generate(encounterPopulation.Table, vars))
						{
							for (int n = 0; n < item.Number; n++)
							{
								GameObject gameObject2 = GameObject.create(item.Blueprint, 0, 0, Context);
								if (!string.IsNullOrEmpty(item.Builder))
								{
									if (item.Builder.Contains(","))
									{
										string[] array = item.Builder.Split(',');
										foreach (string sBuilder in array)
										{
											ApplyEncounterObjectBuilderToObject(sBuilder, gameObject2, Context);
										}
									}
									else
									{
										ApplyEncounterObjectBuilderToObject(item.Builder, gameObject2, Context);
									}
								}
								NewEncounter.Objects.Add(gameObject2);
							}
						}
					}
				}
			}
			catch (Exception x3)
			{
				MetricsManager.LogError("processing EncounterPopulation " + encounterPopulation.Table + "/" + encounterPopulation.Number, x3);
			}
		}
		if (Object is EncounterBuilderObject encounterBuilderObject)
		{
			try
			{
				if (Convert.ToInt32(encounterBuilderObject.Chance).in100())
				{
					NewEncounter.ZoneBuilders.Add(encounterBuilderObject.Builder);
				}
			}
			catch (Exception x4)
			{
				MetricsManager.LogError("processing EncounterBuilderObject " + encounterBuilderObject.Builder, x4);
			}
		}
		return true;
	}

	public string RollOneStringFromTable(string TableName, string Context = null)
	{
		if (!EncounterTables.TryGetValue(TableName, out var value))
		{
			throw new Exception("unknown encounter table '" + TableName + "'");
		}
		int num = Stat.Random(0, value.MaxChance);
		int num2 = 0;
		for (int i = 0; i < value.Objects.Count; i++)
		{
			EncounterObjectBase encounterObjectBase = value.Objects[i];
			num -= Convert.ToInt32(encounterObjectBase.Chance);
			if (num <= 0)
			{
				if (encounterObjectBase is EncounterObject encounterObject)
				{
					try
					{
						return encounterObject.Blueprint;
					}
					catch (Exception)
					{
						XRLCore.LogError("Bad blueprint at table " + TableName + " Chance = " + encounterObjectBase.Chance);
					}
				}
				if (encounterObjectBase is EncounterTableObject encounterTableObject)
				{
					return RollOneStringFromTable(encounterTableObject.Table, Context);
				}
			}
			num2++;
		}
		return null;
	}

	public List<GameObject> RollOneEntryFromTable(string TableName, string Context = null)
	{
		return RollOneEntryFromTable(TableName, 0, Context);
	}

	public List<GameObject> RollOneEntryFromTable(string TableName, int BonusModChance, string Context = null)
	{
		if (!Factory.EncounterTables.TryGetValue(TableName, out var value))
		{
			MetricsManager.LogError("encounter table \"" + TableName + "\" not found for RollOneEntryFromTable in " + (Context ?? "no context"));
			return null;
		}
		int num = Stat.Random(0, value.MaxChance);
		int num2 = 0;
		for (int i = 0; i < value.Objects.Count; i++)
		{
			EncounterObjectBase encounterObjectBase = value.Objects[i];
			num -= Convert.ToInt32(encounterObjectBase.Chance);
			if (num <= 0)
			{
				if (encounterObjectBase is EncounterObject encounterObject)
				{
					try
					{
						int num3 = encounterObject.Number.RollCached();
						List<GameObject> list = new List<GameObject>(num3);
						for (int j = 0; j < num3; j++)
						{
							GameObject gameObject = GameObject.create(encounterObject.Blueprint, BonusModChance, 0, Context);
							if (!string.IsNullOrEmpty(encounterObject.Builder))
							{
								ApplyEncounterObjectBuilderToObject(encounterObject.Builder, gameObject, Context);
							}
							list.Add(gameObject);
						}
						return list;
					}
					catch (Exception)
					{
						XRLCore.LogError("Bad blueprint at table " + TableName + " Chance = " + encounterObjectBase.Chance);
					}
				}
				if (encounterObjectBase is EncounterTableObject encounterTableObject)
				{
					return RollOneEntryFromTable(encounterTableObject.Table, BonusModChance, Context);
				}
			}
			num2++;
		}
		return null;
	}

	public GameObject RollOneFromTable(string TableName, string Context = null, List<GameObject> provideInventory = null)
	{
		return RollOneFromTable(TableName, 0, 0, Context, provideInventory);
	}

	public GameObject RollOneFromTable(string TableName, int BonusModChance, int SetModNumber = 0, string Context = null, List<GameObject> provideInventory = null)
	{
		if (!Factory.EncounterTables.TryGetValue(TableName, out var value))
		{
			MetricsManager.LogError("encounter table \"" + TableName + "\" not found for RollOneFromTable in " + (Context ?? "no context"));
			return null;
		}
		int num = Stat.Random(0, value.MaxChance);
		int num2 = 0;
		for (int i = 0; i < value.Objects.Count; i++)
		{
			EncounterObjectBase encounterObjectBase = value.Objects[i];
			num -= Convert.ToInt32(encounterObjectBase.Chance);
			if (num <= 0)
			{
				if (encounterObjectBase is EncounterObject encounterObject)
				{
					try
					{
						GameObject gameObject = null;
						if (provideInventory != null)
						{
							foreach (GameObject item in provideInventory)
							{
								if (item.Blueprint == encounterObject.Blueprint)
								{
									gameObject = item;
									break;
								}
							}
							if (gameObject != null)
							{
								if (gameObject.Count == 1)
								{
									provideInventory.Remove(gameObject);
								}
								else
								{
									gameObject = gameObject.RemoveOne();
								}
							}
						}
						if (gameObject == null)
						{
							gameObject = GameObject.create(encounterObject.Blueprint, BonusModChance, SetModNumber, Context);
							if (!string.IsNullOrEmpty(encounterObject.Builder))
							{
								ApplyEncounterObjectBuilderToObject(encounterObject.Builder, gameObject, Context);
							}
						}
						return gameObject;
					}
					catch (Exception x)
					{
						MetricsManager.LogError("Bad blueprint at table " + TableName + " Chance = " + encounterObjectBase.Chance, x);
					}
				}
				if (encounterObjectBase is EncounterTableObject encounterTableObject)
				{
					return RollOneFromTable(encounterTableObject.Table, Context);
				}
			}
			num2++;
		}
		return null;
	}

	public Encounter CreateEncounterFromTableName(string Table, string Amount = "minimum", int Tier = 1)
	{
		ZoneEncounterBlueprint zoneEncounterBlueprint = new ZoneEncounterBlueprint();
		zoneEncounterBlueprint.Amount = Amount;
		zoneEncounterBlueprint.Table = Table;
		return CreateEncounter(zoneEncounterBlueprint, Tier);
	}

	public Encounter CreateEncounter(ZoneEncounterBlueprint EncounterBlueprint, int Tier)
	{
		if (!Factory.EncounterTables.TryGetValue(EncounterBlueprint.Table, out var value))
		{
			MetricsManager.LogError("encounter table \"" + EncounterBlueprint.Table + "\" not found for CreateEncounter");
			return null;
		}
		value = value.MakeTempCopy();
		Encounter encounter = new Encounter();
		encounter.Density = value.Density;
		int num = 1;
		if (EncounterBlueprint.Amount == "none")
		{
			num = 0;
		}
		if (EncounterBlueprint.Amount == "minimum")
		{
			num = 1;
		}
		if (EncounterBlueprint.Amount == "very low")
		{
			num = 2;
		}
		if (EncounterBlueprint.Amount == "low-2")
		{
			num = 2;
		}
		if (EncounterBlueprint.Amount == "low-1")
		{
			num = 3;
		}
		if (EncounterBlueprint.Amount == "low")
		{
			num = 4;
		}
		if (EncounterBlueprint.Amount == "medium")
		{
			num = 8;
		}
		if (EncounterBlueprint.Amount == "high")
		{
			num = 16;
		}
		if (EncounterBlueprint.Amount == "very high")
		{
			num = 32;
		}
		if (EncounterBlueprint.Amount == "maximum")
		{
			num = 64;
		}
		for (int i = 0; i < value.Objects.Count; i++)
		{
			ApplyEncounterObjectToEncounter(value.Objects[i], encounter, value, Tier);
		}
		while (num-- > 0)
		{
			EncounterMergeTable encounterMergeTable = value.RollMergeTable();
			if (encounterMergeTable == null)
			{
				continue;
			}
			Encounter encounter2 = new Encounter();
			if (!Factory.EncounterTables.TryGetValue(encounterMergeTable.Table, out var value2))
			{
				MetricsManager.LogError("encounter table \"" + encounterMergeTable.Table + "\" not found for CreateEncounter MergeTable from " + EncounterBlueprint.Table);
				break;
			}
			if (value2 != null)
			{
				int j = 0;
				for (int count = value2.Objects.Count; j < count; j++)
				{
					ApplyEncounterObjectToEncounter(value2.Objects[j], encounter2, value, Tier);
				}
				int k = 0;
				for (int count2 = value2.MergeTables.Count; k < count2; k++)
				{
					value.AddMergeTable(value2.MergeTables[k]);
				}
			}
			if (encounter2.Objects.Count > 0 || encounter2.SubEncounters.Count > 0)
			{
				encounter.SubEncounters.Add(encounter2);
			}
		}
		return encounter;
	}
}
