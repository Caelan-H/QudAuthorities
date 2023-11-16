using System;
using System.Collections.Generic;

namespace XRL.World.Biomes;

[Serializable]
public static class BiomeManager
{
	public static Dictionary<string, IBiome> Biomes = new Dictionary<string, IBiome>
	{
		{
			"Slimy",
			new SlimyBiome()
		},
		{
			"Tarry",
			new TarryBiome()
		},
		{
			"Rusty",
			new RustyBiome()
		},
		{
			"Fungal",
			new FungalBiome()
		}
	};

	public static List<string> GetTopBiomes(string ZoneID, int HowMany = 2)
	{
		List<string> list = new List<string>();
		foreach (string key in Biomes.Keys)
		{
			if (Biomes[key].GetBiomeValue(ZoneID) > 0)
			{
				list.Add(key);
			}
		}
		if (list.Count <= 1)
		{
			return list;
		}
		list.Sort(new BiomeValueSorter(ZoneID));
		List<string> list2 = new List<string>();
		for (int i = 0; i < HowMany; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public static int BiomeValue(string Biome, string ZoneID)
	{
		if (!Biomes.ContainsKey(Biome))
		{
			return 0;
		}
		if (!ZoneID.StartsWith("JoppaWorld"))
		{
			return 0;
		}
		return Biomes[Biome].GetBiomeValue(ZoneID);
	}

	public static string MutateZoneName(string Input, string ZoneID)
	{
		if (!ZoneID.StartsWith("JoppaWorld"))
		{
			return Input;
		}
		List<string> topBiomes = GetTopBiomes(ZoneID);
		for (int num = topBiomes.Count - 1; num >= 0; num--)
		{
			Input = Biomes[topBiomes[num]].MutateZoneName(Input, ZoneID, num);
		}
		return Input;
	}

	public static List<GameObject> MutateEncounterObjects(List<GameObject> Input, string ZoneID)
	{
		if (Input == null)
		{
			return null;
		}
		List<string> topBiomes = GetTopBiomes(ZoneID);
		if (topBiomes.Count == 0)
		{
			return Input;
		}
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < Input.Count; i++)
		{
			GameObject gameObject = Input[i];
			for (int j = 0; j < topBiomes.Count; j++)
			{
				gameObject = Biomes[topBiomes[j]].MutateGameObject(gameObject, ZoneID);
			}
			list.Add(gameObject);
		}
		return list;
	}

	public static GameObject MutateEncounterObject(GameObject Input, string ZoneID)
	{
		if (Input == null)
		{
			return null;
		}
		List<string> topBiomes = GetTopBiomes(ZoneID);
		if (topBiomes.Count == 0)
		{
			return Input;
		}
		for (int i = 0; i < topBiomes.Count; i++)
		{
			Input = Biomes[topBiomes[i]].MutateGameObject(Input, ZoneID);
		}
		return Input;
	}

	public static void MutateZone(Zone Z)
	{
		if (Z.GetZoneProperty("NoBiomes") == "Yes" || !Z.ZoneID.StartsWith("JoppaWorld"))
		{
			return;
		}
		MutateEncounterObjects(Z.GetObjectsWithPart("Physics"), Z.ZoneID);
		List<string> topBiomes = GetTopBiomes(Z.ZoneID);
		if (topBiomes.Count != 0)
		{
			for (int i = 0; i < topBiomes.Count; i++)
			{
				Biomes[topBiomes[i]].MutateZone(Z);
			}
		}
	}
}
