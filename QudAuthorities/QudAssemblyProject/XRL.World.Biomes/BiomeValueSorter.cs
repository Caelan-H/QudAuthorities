using System.Collections.Generic;

namespace XRL.World.Biomes;

public class BiomeValueSorter : IComparer<string>
{
	private string ZoneID;

	public BiomeValueSorter(string ZoneID)
	{
		this.ZoneID = ZoneID;
	}

	public int Compare(string f1, string f2)
	{
		return BiomeManager.Biomes[f1].GetBiomeValue(ZoneID).CompareTo(BiomeManager.Biomes[f2].GetBiomeValue(ZoneID));
	}
}
