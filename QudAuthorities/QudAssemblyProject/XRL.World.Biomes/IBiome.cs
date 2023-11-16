using XRL.Rules;

namespace XRL.World.Biomes;

public class IBiome
{
	public virtual int GetBiomeValue(string ZoneID)
	{
		return Stat.SeededRandom("BiomeValue" + ZoneID, 0, 3);
	}

	public virtual string MutateZoneName(string Name, string ZoneID, int NameOrder)
	{
		return Name;
	}

	protected static string MutateZoneNameWith(string Name, int Which, string Name1, string Adjective1, string Name2, string Adjective2, string Name3, string Adjective3)
	{
		if (string.IsNullOrEmpty(Name) || Name == "surface")
		{
			switch (Which)
			{
			case 1:
				return Name1;
			case 2:
				return Name2;
			case 3:
				return Name3;
			}
		}
		else
		{
			string text = null;
			switch (Which)
			{
			case 1:
				text = Adjective1;
				break;
			case 2:
				text = Adjective2;
				break;
			case 3:
				text = Adjective3;
				break;
			}
			if (text != null)
			{
				if (Name.StartsWith("some "))
				{
					return "some " + text + " " + Name.Substring(5);
				}
				return text + " " + Name;
			}
			switch (Which)
			{
			case 1:
				return Name + " and " + Name1;
			case 2:
				return Name + " and " + Name2;
			case 3:
				return Name + " and " + Name3;
			}
		}
		return Name;
	}

	protected static string MutateZoneNameWith(string Name, int Which, string Name1, string Name2, string Name3)
	{
		return MutateZoneNameWith(Name, Which, Name1, null, Name2, null, Name3, null);
	}

	public virtual GameObject MutateGameObject(GameObject GO, string ZoneID)
	{
		return GO;
	}

	public virtual void MutateZone(Zone Z)
	{
	}
}
