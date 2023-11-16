using System;
using System.Collections.Generic;
using Genkit.SimplexNoise;
using XRL.Core;
using XRL.Rules;
using XRL.World.ZoneBuilders;
using XRL.World.ZoneBuilders.Utility;

namespace XRL.World.Biomes;

[HasGameBasedStaticCache]
public class RustyBiome : IBiome
{
	[NonSerialized]
	[GameBasedStaticCache(CreateInstance = false)]
	public static byte[,,] BiomeLevels;

	public override int GetBiomeValue(string ZoneID)
	{
		try
		{
			if (!ZoneID.Contains("."))
			{
				return 0;
			}
			string[] array = ZoneID.Split('.');
			int num = Convert.ToInt32(array[1]);
			int num2 = Convert.ToInt32(array[2]);
			int num3 = Convert.ToInt32(array[3]);
			int num4 = Convert.ToInt32(array[4]);
			int num5 = Convert.ToInt32(array[5]);
			if (num5 < 10)
			{
				return 0;
			}
			string objectTypeForZone = ZoneManager.GetObjectTypeForZone(num, num2, "JoppaWorld");
			if (GameObjectFactory.Factory.Blueprints.ContainsKey(objectTypeForZone) && GameObjectFactory.Factory.Blueprints[objectTypeForZone].Tags.ContainsKey("NoBiomes"))
			{
				return 0;
			}
			if ((string)XRLCore.Core.Game.ZoneManager.GetZoneProperty(ZoneID, "NoBiomes", bClampToLevel30: false, "") == "Yes")
			{
				return 0;
			}
			int num6 = 240;
			int num7 = 75;
			int num8 = 10;
			int num9 = num * 3 + num3;
			int num10 = num2 * 3 + num4;
			int num11 = num5 % num8;
			if (BiomeLevels == null)
			{
				BiomeLevels = new byte[num6, num7, num8];
				LayeredNoise layeredNoise = LayeredNoise.CreateLinearOctiveLayers(3, 1.33f, 0.12f, XRLCore.Core.Game.GetWorldSeed("RustyNoise").ToString());
				float num12 = 0.66f;
				float num13 = 0f;
				float num14 = 1f;
				float[,,] array2 = layeredNoise.Generate3D(num6, num7, num8);
				for (int i = 0; i < num6; i++)
				{
					for (int j = 0; j < num7; j++)
					{
						for (int k = 0; k < num8; k++)
						{
							float num15 = array2[i, j, k];
							num15 = (array2[i, j, k] = ((!(num15 < num12)) ? ((num15 - num12) * (1f / (1f - num12))) : 0f));
							if (num15 > num13)
							{
								num13 = num15;
							}
							if (num15 < num14)
							{
								num14 = num15;
							}
						}
					}
				}
				for (int l = 0; l < num6; l++)
				{
					for (int m = 0; m < num7; m++)
					{
						for (int n = 0; n < num8; n++)
						{
							int num16 = 4;
							int num17 = (int)((array2[l, m, n] - num14) / ((num13 - num14) / (float)num16));
							if (num17 < 0)
							{
								num17 = 0;
							}
							if (num17 > num16 - 1)
							{
								num17 = num16 - 1;
							}
							BiomeLevels[l, m, n] = (byte)num17;
						}
					}
				}
			}
			return BiomeLevels[num9, num10, num11];
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SlimeBiomeValue", x);
			return 0;
		}
	}

	public override string MutateZoneName(string Name, string ZoneID, int NameOrder)
	{
		return IBiome.MutateZoneNameWith(Name, GetBiomeValue(ZoneID), "rust patch", "rusty", "rust field", "rust-shrouded", "rust bog", null);
	}

	public override void MutateZone(Zone Z)
	{
		int biomeValue = GetBiomeValue(Z.ZoneID);
		if (Z.GetZoneProperty("relaxedbiomes") != "true")
		{
			if (biomeValue == 1)
			{
				new PopTableZoneBuilder().BuildZone(Z, "Rusty1");
			}
			if (biomeValue == 2)
			{
				new PopTableZoneBuilder().BuildZone(Z, "Rusty2");
			}
			if (biomeValue == 3)
			{
				new PopTableZoneBuilder().BuildZone(Z, "Rusty3");
			}
		}
		List<NoiseMapNode> list = new List<NoiseMapNode>();
		foreach (ZoneConnection zoneConnection in XRLCore.Core.Game.ZoneManager.GetZoneConnections(Z.ZoneID))
		{
			list.Add(new NoiseMapNode(zoneConnection.X, zoneConnection.Y));
		}
		NoiseMap noiseMap = new NoiseMap(80, 25, 10, 3, 3, biomeValue * 3, 20, 20, 4, 3, 0, 1, list);
		int num = -1;
		for (int i = 0; i < noiseMap.nAreas; i++)
		{
			if (noiseMap.AreaNodes[i].Count > num)
			{
				num = noiseMap.AreaNodes[i].Count;
			}
		}
		for (int j = 0; j < Z.Width; j++)
		{
			for (int k = 0; k < Z.Height; k++)
			{
				if ((double)noiseMap.Noise[j, k] >= 0.8 && Z.GetCell(j, k).HasObjectWithTag("Wall") && Z.GetCell(j, k).AnyAdjacentCell((Cell c) => !c.HasWall()) && Stat.Random(1, 100) <= biomeValue * 5)
				{
					if (Z.GetZoneProperty("faction", null) != null)
					{
						GameObject gameObject = GameObjectFactory.Factory.CreateObject("Qudzu");
						gameObject.pBrain.Factions = "";
						gameObject.pBrain.FactionMembership.Clear();
						gameObject.pBrain.FactionMembership.Add(Z.GetZoneProperty("faction"), 100);
						gameObject.pRender.DisplayName = "domesticated " + gameObject.pRender.DisplayName;
						Z.GetCell(j, k).AddObject(gameObject);
					}
					else
					{
						Z.GetCell(j, k).AddObject("Qudzu");
					}
				}
			}
		}
	}

	public override GameObject MutateGameObject(GameObject GO, string ZoneID)
	{
		long num = 0L;
		Zone zone = XRLCore.Core.Game.ZoneManager.GetZone(ZoneID);
		num = ((!GO.HasProperty("Batch")) ? Stat.Random(0, 2147483646) : GO.GetLongProperty("Batch"));
		int biomeValue = GetBiomeValue(ZoneID);
		if (biomeValue == 1 && zone.GetZoneProperty("relaxedbiomes") != "true")
		{
			if (Stat.SeededRandom(num.ToString(), 1, 100) <= 50)
			{
				RustedTemplate.Apply(GO);
			}
			if (Stat.SeededRandom(num.ToString(), 1, 100) <= 25)
			{
				RustedInventoryTemplate.Apply(GO);
			}
		}
		if (biomeValue == 2)
		{
			if (zone.GetZoneProperty("relaxedbiomes") != "true")
			{
				RustedTemplate.Apply(GO);
				if (Stat.SeededRandom(num.ToString(), 1, 100) <= 75)
				{
					RustedInventoryTemplate.Apply(GO);
				}
			}
			if (Stat.SeededRandom(num.ToString(), 1, 100) <= 25)
			{
				QudzuSymbioteTemplate.Apply(GO, zone);
			}
		}
		if (biomeValue == 3)
		{
			if (zone.GetZoneProperty("relaxedbiomes") != "true")
			{
				RustedTemplate.Apply(GO);
				if (Stat.SeededRandom(num.ToString(), 1, 100) <= 100)
				{
					RustedInventoryTemplate.Apply(GO);
				}
			}
			if (Stat.SeededRandom(num.ToString(), 1, 100) <= 60)
			{
				QudzuSymbioteTemplate.Apply(GO, zone);
			}
		}
		return GO;
	}
}
