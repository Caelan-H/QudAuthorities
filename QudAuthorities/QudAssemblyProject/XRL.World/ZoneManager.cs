using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.Language;
using XRL.Liquids;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Biomes;
using XRL.World.Capabilities;
using XRL.World.Encounters;
using XRL.World.Parts;
using XRL.World.ZoneBuilders;

namespace XRL.World;

[Serializable]
[HasGameBasedStaticCache]
public class ZoneManager
{
	private class xyPair
	{
		public int x;

		public int y;

		public GameObject GO;

		public xyPair(int _x, int _y, GameObject _GO)
		{
			x = _x;
			y = _y;
			GO = _GO;
		}
	}

	[NonSerialized]
	public Dictionary<string, Zone> CachedZones = new Dictionary<string, Zone>();

	[NonSerialized]
	public Zone ActiveZone;

	[NonSerialized]
	public long LastZoneTransition;

	[NonSerialized]
	private Dictionary<string, List<ZoneConnection>> ZoneConnections = new Dictionary<string, List<ZoneConnection>>();

	[NonSerialized]
	public Dictionary<string, GameObject> CachedObjects = new Dictionary<string, GameObject>();

	[NonSerialized]
	public List<string> CachedObjectsToRemoveAfterZoneBuild = new List<string>();

	[GameBasedStaticCache]
	private static HashSet<string> FrozenZones = new HashSet<string>();

	[GameBasedStaticCache]
	private static List<string> FreezingZones = new List<string>();

	private Dictionary<string, Dictionary<string, object>> ZoneProperties = new Dictionary<string, Dictionary<string, object>>();

	[NonSerialized]
	private Dictionary<string, List<ZoneBuilderBlueprint>> ZoneBuilderOverrides = new Dictionary<string, List<ZoneBuilderBlueprint>>();

	[NonSerialized]
	private Dictionary<string, List<ZoneBuilderBlueprint>> ZonePreBuilders = new Dictionary<string, List<ZoneBuilderBlueprint>>();

	[NonSerialized]
	private Dictionary<string, List<ZoneBuilderBlueprint>> ZoneMidBuilders = new Dictionary<string, List<ZoneBuilderBlueprint>>();

	[NonSerialized]
	private Dictionary<string, List<ZoneBuilderBlueprint>> ZonePostBuilders = new Dictionary<string, List<ZoneBuilderBlueprint>>();

	public static long Ticker = 0L;

	public static object ZoneLock = new object();

	[NonSerialized]
	private static List<Zone> CheckedZones = new List<Zone>();

	[NonSerialized]
	private Dictionary<string, ZoneBuilderBlueprint> MatchingBlueprintsByBuilderWithAfterTerrain;

	[NonSerialized]
	private Dictionary<string, ZoneBuilderBlueprint> MatchingBlueprintsByBuilder;

	[Obsolete("save compat")]
	internal Dictionary<string, bool> VisitedZones;

	[NonSerialized]
	public Dictionary<string, long> VisitedTime = new Dictionary<string, long>();

	public List<string> PinnedZones = new List<string>();

	[NonSerialized]
	private static SuspendingEvent eSuspending = new SuspendingEvent();

	[NonSerialized]
	public static int ZoneTransitionSaveInterval = 5;

	[NonSerialized]
	[GameBasedStaticCache]
	public static int ZoneTransitionCount = 0;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	[NonSerialized]
	public int NameUpdateTick;

	[NonSerialized]
	public static int zoneGenerationContextTier = 1;

	[NonSerialized]
	public static string zoneGenerationContextZoneID = "none";

	[NonSerialized]
	private static BeforeZoneBuiltEvent eBeforeZoneBuilt = new BeforeZoneBuiltEvent();

	[NonSerialized]
	private static ZoneBuiltEvent eZoneBuilt = new ZoneBuiltEvent();

	[NonSerialized]
	private static AfterZoneBuiltEvent eAfterZoneBuilt = new AfterZoneBuiltEvent();

	[NonSerialized]
	private static GameObject[,] WallSingleTrack = new GameObject[80, 25];

	[NonSerialized]
	private static List<GameObject>[,] WallMultiTrack = new List<GameObject>[80, 25];

	[NonSerialized]
	private static GameObject[,] LiquidTrack = new GameObject[80, 25];

	public static ZoneManager instance => The.Game.ZoneManager;

	public GameObject findObjectByID(string id)
	{
		GameObject gameObject = ActiveZone.findObjectById(id);
		if (gameObject != null)
		{
			return gameObject;
		}
		foreach (Zone value in CachedZones.Values)
		{
			if (value != ActiveZone)
			{
				gameObject = value.findObjectById(id);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public void Release()
	{
		foreach (KeyValuePair<string, Zone> cachedZone in CachedZones)
		{
			cachedZone.Value.Release();
		}
		if (ActiveZone != null)
		{
			ActiveZone.Release();
		}
		CachedZones.Clear();
		ActiveZone = null;
	}

	public bool CachedZonesContains(string ZoneID)
	{
		if (CachedZones == null)
		{
			return false;
		}
		Dictionary<string, Zone>.Enumerator enumerator = CachedZones.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string key = enumerator.Current.Key;
				if (ZoneID == key)
				{
					return true;
				}
			}
		}
		finally
		{
			enumerator.Dispose();
		}
		return false;
	}

	public GameObject peekCachedObject(string ID)
	{
		if (ID != null && CachedObjects.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public GameObject PullCachedObject(string ID)
	{
		if (ID != null && CachedObjects.TryGetValue(ID, out var value))
		{
			GameObject result = value.DeepCopy(CopyEffects: false, CopyID: true);
			CachedObjects.Remove(ID);
			return result;
		}
		return null;
	}

	public void UncacheObject(string ID)
	{
		CachedObjects.Remove(ID);
	}

	public GameObject GetCachedObjects(string ID)
	{
		if (ID == null || !CachedObjects.ContainsKey(ID))
		{
			GameObject gameObject = GameObject.create("PhysicalObject");
			gameObject.pRender.DisplayName = "INVALID CACHE OBJECT: " + ID;
			gameObject.pRender.ColorString = "&M^W";
			gameObject.pRender.DetailColor = "W";
			return gameObject;
		}
		CachedObjectsToRemoveAfterZoneBuild.Add(ID);
		return CachedObjects[ID].DeepCopy(CopyEffects: false, CopyID: true);
	}

	public string CacheObject(GameObject GO, bool cacheTwiceOk = false, bool replaceIfAlreadyCached = false)
	{
		if (CachedObjects.ContainsKey(GO.id))
		{
			if (replaceIfAlreadyCached)
			{
				CachedObjects[GO.id] = GO;
				return GO.id;
			}
			if (!cacheTwiceOk)
			{
				Debug.LogWarning("Did you mean to cache " + GO.DisplayName + " twice?");
			}
			return GO.id;
		}
		CachedObjects.Add(GO.id, GO);
		return GO.id;
	}

	public static string GetObjectTypeForZone(string zoneId)
	{
		if (!ZoneID.Parse(zoneId, out var World, out var ParasangX, out var ParasangY))
		{
			return "";
		}
		Cell cell = The.Game.ZoneManager.GetZone(World).GetCell(ParasangX, ParasangY);
		if (cell == null)
		{
			return "";
		}
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart == null)
		{
			return "";
		}
		return firstObjectWithPart.Blueprint;
	}

	public static string GetObjectTypeForZone(int wx, int wy, string World)
	{
		Cell cell = The.Game.ZoneManager.GetZone(World).GetCell(wx, wy);
		if (cell == null)
		{
			return "";
		}
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("TerrainTravel");
		if (firstObjectWithPart == null)
		{
			return "";
		}
		return firstObjectWithPart.Blueprint;
	}

	public static GameObject GetTerrainObjectForZone(int wx, int wy, string World)
	{
		return The.Game.ZoneManager.GetZone(World).GetCell(wx, wy)?.GetFirstObjectWithPart("TerrainTravel");
	}

	public static string GetRegionForZone(Zone z)
	{
		Cell cell = The.Game.ZoneManager.GetZone(z.GetZoneWorld()).GetCell(z.wX, z.wY);
		if (cell == null)
		{
			return null;
		}
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("TerrainTravel");
		return firstObjectWithPart?.GetTag("Terrain", firstObjectWithPart.Blueprint);
	}

	public List<ZoneBuilderBlueprint> GetBuildersFor(Zone zone)
	{
		List<ZoneBuilderBlueprint> list = new List<ZoneBuilderBlueprint>();
		if (ZoneBuilderOverrides.ContainsKey(zone.ZoneID))
		{
			list.AddRange(ZoneBuilderOverrides[zone.ZoneID]);
		}
		if (ZonePreBuilders.ContainsKey(zone.ZoneID))
		{
			list.AddRange(ZonePreBuilders[zone.ZoneID]);
		}
		if (ZoneMidBuilders.ContainsKey(zone.ZoneID))
		{
			list.AddRange(ZoneMidBuilders[zone.ZoneID]);
		}
		if (ZonePostBuilders.ContainsKey(zone.ZoneID))
		{
			list.AddRange(ZonePostBuilders[zone.ZoneID]);
		}
		return list;
	}

	public void SaveBuilderList(Dictionary<string, List<ZoneBuilderBlueprint>> list, SerializationWriter writer)
	{
		writer.Write(list.Keys.Count);
		foreach (string key in list.Keys)
		{
			writer.Write(key);
			writer.Write(list[key].Count);
			for (int i = 0; i < list[key].Count; i++)
			{
				list[key][i].Save(writer);
			}
		}
	}

	public bool ZoneHasBuilder(string zoneID, string builder)
	{
		if (ZoneBuilderOverrides.ContainsKey(zoneID) && ZoneBuilderOverrides[zoneID] != null && ZoneBuilderOverrides[zoneID].Any((ZoneBuilderBlueprint b) => b.Class == builder))
		{
			return true;
		}
		if (ZonePreBuilders.ContainsKey(zoneID) && ZonePreBuilders[zoneID] != null && ZonePreBuilders[zoneID].Any((ZoneBuilderBlueprint b) => b.Class == builder))
		{
			return true;
		}
		if (ZoneMidBuilders.ContainsKey(zoneID) && ZoneMidBuilders[zoneID] != null && ZoneMidBuilders[zoneID].Any((ZoneBuilderBlueprint b) => b.Class == builder))
		{
			return true;
		}
		if (ZonePostBuilders.ContainsKey(zoneID) && ZonePostBuilders[zoneID] != null && ZonePostBuilders[zoneID].Any((ZoneBuilderBlueprint b) => b.Class == builder))
		{
			return true;
		}
		return false;
	}

	public static Dictionary<string, List<ZoneBuilderBlueprint>> LoadBuilderList(SerializationReader reader)
	{
		Dictionary<string, List<ZoneBuilderBlueprint>> dictionary = new Dictionary<string, List<ZoneBuilderBlueprint>>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = reader.ReadString();
			int num2 = reader.ReadInt32();
			List<ZoneBuilderBlueprint> list = new List<ZoneBuilderBlueprint>();
			dictionary.Add(key, list);
			for (int j = 0; j < num2; j++)
			{
				list.Add(ZoneBuilderBlueprint.Load(reader));
			}
		}
		return dictionary;
	}

	public void Save(SerializationWriter Writer)
	{
		lock (ZoneLock)
		{
			Writer.WriteObject(this);
			Writer.Write(CachedZones.Count);
			foreach (Zone value in CachedZones.Values)
			{
				value.Save(Writer);
			}
			Writer.Write(0);
			Writer.Write(LastZoneTransition);
			Writer.Write(FrozenZones.Count);
			foreach (string frozenZone in FrozenZones)
			{
				Writer.Write(frozenZone);
			}
			Writer.Write(PinnedZones.Count);
			foreach (string pinnedZone in PinnedZones)
			{
				Writer.Write(pinnedZone);
			}
			Writer.Write(VisitedTime);
			Writer.Write(Ticker);
			Writer.Write(ZoneConnections.Count);
			foreach (string key in ZoneConnections.Keys)
			{
				List<ZoneConnection> list = ZoneConnections[key];
				Writer.Write(key);
				Writer.Write(list.Count);
				foreach (ZoneConnection item in list)
				{
					Writer.Write(item.Object);
					Writer.Write(item.Type);
					Writer.Write(item.X);
					Writer.Write(item.Y);
				}
			}
			Writer.Write(ActiveZone.ZoneID);
			Writer.Write(CachedObjects.Keys.Count);
			foreach (string key2 in CachedObjects.Keys)
			{
				if (CachedObjects.ContainsKey(key2) && CachedObjects[key2] != null)
				{
					Writer.Write(key2);
					if (CachedObjects[key2].pPhysics != null)
					{
						CachedObjects[key2].pPhysics._CurrentCell = null;
						CachedObjects[key2].pPhysics._Equipped = null;
						CachedObjects[key2].pPhysics._InInventory = null;
					}
					Writer.WriteGameObject(CachedObjects[key2]);
				}
			}
			Hills.Save(Writer);
			BananaGrove.Save(Writer);
			Watervine.Save(Writer);
			Cave.Save(Writer);
			SaveBuilderList(ZoneBuilderOverrides, Writer);
			SaveBuilderList(ZonePreBuilders, Writer);
			SaveBuilderList(ZoneMidBuilders, Writer);
			SaveBuilderList(ZonePostBuilders, Writer);
		}
	}

	public static ZoneManager Load(SerializationReader Reader)
	{
		lock (ZoneLock)
		{
			ZoneManager zoneManager = (ZoneManager)Reader.ReadObject();
			zoneManager.CachedZones = new Dictionary<string, Zone>();
			int num = Reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Zone zone = Zone.Load(Reader);
				if (!zoneManager.CachedZones.ContainsKey(zone.ZoneID))
				{
					zoneManager.CachedZones.Add(zone.ZoneID, zone);
				}
				else
				{
					MetricsManager.LogWarning("Duplicate zone ID found on load: " + zone.ZoneID);
				}
			}
			num = Reader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				Reader.ReadString();
			}
			zoneManager.LastZoneTransition = Reader.ReadInt64();
			num = Reader.ReadInt32();
			FrozenZones.Clear();
			for (int k = 0; k < num; k++)
			{
				FrozenZones.Add(Reader.ReadString());
			}
			FrozenZones.TrimExcess();
			num = Reader.ReadInt32();
			zoneManager.PinnedZones = new List<string>(num);
			for (int l = 0; l < num; l++)
			{
				zoneManager.PinnedZones.Add(Reader.ReadString());
			}
			if (Reader.FileVersion >= 260)
			{
				zoneManager.VisitedTime = Reader.ReadDictionary<string, long>();
			}
			else
			{
				ZoneManager zoneManager2 = zoneManager;
				if (zoneManager2.VisitedTime == null)
				{
					zoneManager2.VisitedTime = new Dictionary<string, long>();
				}
			}
			if (!zoneManager.VisitedZones.IsNullOrEmpty())
			{
				foreach (KeyValuePair<string, bool> visitedZone in zoneManager.VisitedZones)
				{
					if (visitedZone.Value)
					{
						zoneManager.VisitedTime[visitedZone.Key] = -1L;
					}
				}
				zoneManager.VisitedZones = null;
			}
			Ticker = Reader.ReadInt64();
			zoneManager.ZoneConnections = new Dictionary<string, List<ZoneConnection>>();
			int num2 = Reader.ReadInt32();
			for (int m = 0; m < num2; m++)
			{
				string key = Reader.ReadString();
				List<ZoneConnection> list = new List<ZoneConnection>();
				int num3 = Reader.ReadInt32();
				for (int n = 0; n < num3; n++)
				{
					ZoneConnection zoneConnection = new ZoneConnection();
					zoneConnection.Object = Reader.ReadString();
					zoneConnection.Type = Reader.ReadString();
					zoneConnection.X = Reader.ReadInt32();
					zoneConnection.Y = Reader.ReadInt32();
					list.Add(zoneConnection);
				}
				zoneManager.ZoneConnections.Add(key, list);
			}
			zoneManager.ActiveZone = zoneManager.GetZone(Reader.ReadString());
			zoneManager.CachedObjects = new Dictionary<string, GameObject>();
			int num4 = Reader.ReadInt32();
			for (int num5 = 0; num5 < num4; num5++)
			{
				string key2 = Reader.ReadString();
				zoneManager.CachedObjects.Add(key2, Reader.ReadGameObject("zone cache"));
				if (zoneManager.CachedObjects[key2].pPhysics != null)
				{
					zoneManager.CachedObjects[key2].pPhysics._CurrentCell = null;
					zoneManager.CachedObjects[key2].pPhysics._Equipped = null;
					zoneManager.CachedObjects[key2].pPhysics._InInventory = null;
				}
			}
			Hills.Load(Reader);
			BananaGrove.Load(Reader);
			Watervine.Load(Reader);
			Cave.Load(Reader);
			zoneManager.MatchingBlueprintsByBuilder = new Dictionary<string, ZoneBuilderBlueprint>();
			zoneManager.ZoneBuilderOverrides = LoadBuilderList(Reader);
			zoneManager.ZonePreBuilders = LoadBuilderList(Reader);
			zoneManager.ZoneMidBuilders = LoadBuilderList(Reader);
			zoneManager.ZonePostBuilders = LoadBuilderList(Reader);
			return zoneManager;
		}
	}

	public static long CopyTo(Stream source, Stream destination)
	{
		byte[] array = new byte[2048];
		long num = 0L;
		int num2;
		while ((num2 = source.Read(array, 0, array.Length)) > 0)
		{
			destination.Write(array, 0, num2);
			num += num2;
		}
		return num;
	}

	public static void ForceCollect()
	{
		MemoryHelper.GCCollectMax();
	}

	public static Zone ThawZone(string ZoneID)
	{
		string description = "Thawing zone...";
		Debug.Log("Thawing " + ZoneID);
		if (Options.DebugShowFullZoneDuringBuild)
		{
			description = "Thawing " + ZoneID;
		}
		Zone Zone = null;
		Loading.LoadTask(description, delegate
		{
			try
			{
				try
				{
					Stream stream = null;
					string path = Path.Combine(Path.Combine(XRLCore.Core.Game.GetCacheDirectory(), "ZoneCache"), ZoneID + ".zone.gz");
					if (File.Exists(path))
					{
						using FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
						using (GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Decompress))
						{
							stream = new MemoryStream();
							gZipStream.CopyTo(stream);
							stream.Position = 0L;
							gZipStream.Close();
						}
						fileStream.Close();
					}
					else
					{
						path = Path.Combine(Path.Combine(XRLCore.Core.Game.GetCacheDirectory(), "ZoneCache"), ZoneID + ".zone");
						if (!File.Exists(path))
						{
							throw new FileNotFoundException("Cache file not found for zone '" + ZoneID + "'.");
						}
						stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					}
					using (stream)
					{
						using SerializationReader serializationReader = new SerializationReader(stream);
						try
						{
							if (serializationReader.ReadInt32() == 123457)
							{
								serializationReader.FileVersion = serializationReader.ReadInt32();
								serializationReader.ReadString();
							}
							else
							{
								serializationReader.FileVersion = serializationReader.ReadInt32();
							}
						}
						catch
						{
							serializationReader.FileVersion = serializationReader.ReadInt32();
						}
						bool flag = false;
						if (GameObject.ExternalLoadBindings == null)
						{
							GameObject.ExternalLoadBindings = new Dictionary<GameObject, List<ExternalEventBind>>();
							flag = true;
						}
						Zone = Zone.Load(serializationReader);
						Zone.MarkActive();
						The.Game.ZoneManager.CachedZones.Add(Zone.ZoneID, Zone);
						serializationReader.ReadGameObjects();
						PaintWalls(Zone);
						PaintWater(Zone);
						if (flag)
						{
							GameObject.ExternalLoadBindings = null;
						}
						if (serializationReader.Errors > 0)
						{
							Popup.DisplayLoadError("zone", serializationReader.Errors);
						}
					}
					ForceCollect();
				}
				catch (Exception ex)
				{
					string text = "ZoneManager::ThawZone::";
					text = ((!(ex is FileNotFoundException)) ? (text + "ReadError::") : (text + "CacheMiss::"));
					if (ModManager.TryGetStackMod(ex, out var Mod, out var Frame))
					{
						MethodBase method = Frame.GetMethod();
						Mod.Error(method.DeclaringType?.FullName + "::" + method.Name + "::" + ex);
					}
					else
					{
						MetricsManager.LogException(text, ex, "serialization_error");
					}
					MessageQueue.AddPlayerMessage("ThawZone exception", 'W');
					Zone = null;
					if (XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(ZoneID))
					{
						The.Game.ZoneManager.CachedZones.Remove(ZoneID);
					}
				}
				if (Zone != null)
				{
					FrozenZones.Remove(ZoneID);
					Zone.Thawed();
					Debug.Log("Thaw complete");
				}
				else
				{
					MetricsManager.LogError("Thaw error: " + ZoneID);
				}
			}
			catch (Exception x)
			{
				MessageQueue.AddPlayerMessage("ThawZone exception", 'r');
				MetricsManager.LogException("ZoneManager::ThawZone", x, "serialization_error");
			}
		});
		return Zone;
	}

	public static void FreezeZoneThread(object oZ)
	{
		Zone Z = (Zone)oZ;
		string description = "Freezing zone...";
		if (Options.DebugShowFullZoneDuringBuild)
		{
			description = "Freezing " + ((Zone)oZ).ZoneID;
		}
		Z.FireEvent("ZoneFreezing");
		bool Debug = Options.DebugZoneCaching;
		Loading.LoadTask(description, delegate
		{
			try
			{
				if (Debug)
				{
					MessageQueue.AddPlayerMessage("--beginning freeze " + Z.ZoneID);
				}
				lock (ZoneLock)
				{
					if (FreezingZones.CleanContains(Z.ZoneID))
					{
						if (Debug)
						{
							MessageQueue.AddPlayerMessage("Freezing zones early exit");
						}
						return;
					}
					The.Game.ZoneManager.CachedZones.Remove(Z.ZoneID);
					if (FreezingZones.CleanContains(Z.ZoneID))
					{
						if (Debug)
						{
							MessageQueue.AddPlayerMessage("Freezing zones early exit");
						}
						return;
					}
					FreezingZones.Add(Z.ZoneID);
					try
					{
						Directory.CreateDirectory(Path.Combine(XRLCore.Core.Game.GetCacheDirectory(), "ZoneCache"));
						Stream stream = null;
						int i = 0;
						string text = Path.Combine(Path.Combine(XRLCore.Core.Game.GetCacheDirectory(), "ZoneCache"), Z.ZoneID + ".zone.gz");
						for (; i < 5; i++)
						{
							try
							{
								stream = new GZipStream(File.Open(text, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite), CompressionLevel.Optimal);
							}
							catch (Exception)
							{
								Thread.Sleep(1000);
								continue;
							}
							break;
						}
						if (stream == null)
						{
							throw new IOException("Exception opening " + text + " for writing.");
						}
						using (MemoryStream memoryStream = new MemoryStream())
						{
							using (SerializationWriter serializationWriter = new SerializationWriter(memoryStream, _bSerializePlayer: false))
							{
								serializationWriter.Write(123457);
								serializationWriter.Write(264);
								serializationWriter.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
								Z.Save(serializationWriter);
								serializationWriter.WriteGameObjects();
								serializationWriter.AppendTokenTables();
								serializationWriter.Flush();
								using (stream)
								{
									memoryStream.Flush();
									stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
									stream.Flush();
									stream.Close();
								}
								serializationWriter.Close();
							}
							memoryStream.Close();
						}
						FrozenZones.Add(Z.ZoneID);
					}
					catch (Exception ex2)
					{
						string text2 = "ZoneManager::FreezeZoneThread::";
						text2 = ((!(ex2 is IOException)) ? (text2 + "WriteError::") : (text2 + "IO::"));
						if (ModManager.TryGetStackMod(ex2, out var Mod, out var Frame))
						{
							MethodBase method = Frame.GetMethod();
							Mod.Error(method.DeclaringType?.FullName + "::" + method.Name + "::" + ex2);
						}
						else
						{
							MetricsManager.LogException(text2, ex2, "serialization_error");
						}
					}
					FreezingZones.Remove(Z.ZoneID);
					foreach (Cell cell in Z.GetCells())
					{
						foreach (GameObject @object in cell.Objects)
						{
							GameObjectFactory.Factory.Pool(@object);
						}
						cell.Objects.Clear();
					}
					if (Options.CollectEarly)
					{
						ForceCollect();
					}
				}
			}
			catch (Exception ex3)
			{
				if (Debug)
				{
					MessageQueue.AddPlayerMessage("Freeze zone exception: " + ex3.ToString());
				}
				MetricsManager.LogException("ZoneManager::FreezeZoneThread", ex3, "serialization_error");
			}
			finally
			{
				_ = Options.SynchronousZoneCaching;
				ForceCollect();
			}
			if (Debug)
			{
				MessageQueue.AddPlayerMessage("Zone freeze complete!");
			}
		});
	}

	public static void FreezeZone(Zone Z)
	{
		try
		{
			_ = Options.SynchronousZoneCaching;
			FreezeZoneThread(Z);
		}
		catch (Exception ex)
		{
			if (Options.DebugZoneCaching)
			{
				MessageQueue.AddPlayerMessage(ex.ToString());
			}
			MetricsManager.LogError("Freeze zone exception", ex);
		}
	}

	public void Tick(bool bAllowFreeze)
	{
		bool debugZoneCaching = Options.DebugZoneCaching;
		try
		{
			Ticker++;
			if (Options.DisableZoneCaching2)
			{
				if (Ticker % 100 == 0L)
				{
					MessageQueue.AddPlayerMessage("&RWARNING: You have the Disable Zone Caching option enabled, this will cause massive memory use over time.");
				}
				return;
			}
			Zone zone = null;
			bool flag = FreezingZones.Count > 0;
			int freezabilityTurns = Zone.GetFreezabilityTurns();
			int suspendabilityTurns = Zone.GetSuspendabilityTurns();
			CheckedZones.Clear();
			foreach (Zone value in CachedZones.Values)
			{
				if (value.bSuspended)
				{
					if (bAllowFreeze && !flag)
					{
						CheckedZones.Add(value);
						Freezability freezability = value.GetFreezability(freezabilityTurns);
						if (debugZoneCaching)
						{
							MessageQueue.AddPlayerMessage("Zone " + value.ZoneID + " freezability: " + freezability);
						}
						if (freezability == Freezability.Freezable)
						{
							flag = true;
							zone = value;
						}
					}
					continue;
				}
				CheckedZones.Add(value);
				if (ActiveZone != value && !XRLCore.Core.Game.ActionManager.HasAction("SuspendZone", value.ZoneID))
				{
					Suspendability suspendability = value.GetSuspendability(suspendabilityTurns);
					if (debugZoneCaching && suspendability != Suspendability.TooRecentlyActive)
					{
						MessageQueue.AddPlayerMessage("Zone " + value.ZoneID + " suspendability: " + suspendability);
					}
					if (suspendability == Suspendability.Suspendable)
					{
						The.Game.ActionManager.EnqueAction("SuspendZone", value.ZoneID, freezabilityTurns);
						Debug.Log("Queueing suspendzone " + value.ZoneID + " for " + freezabilityTurns);
					}
				}
			}
			int num = 100;
			while (zone != null && num >= 0)
			{
				num--;
				FreezeZone(zone);
				zone = null;
				if (!bAllowFreeze)
				{
					continue;
				}
				foreach (Zone value2 in CachedZones.Values)
				{
					if (value2.bSuspended && !CheckedZones.Contains(value2))
					{
						CheckedZones.Add(value2);
						Freezability freezability2 = value2.GetFreezability(freezabilityTurns);
						if (debugZoneCaching)
						{
							MessageQueue.AddPlayerMessage("Zone " + value2.ZoneID + ": " + freezability2);
						}
						if (freezability2 == Freezability.Freezable)
						{
							flag = true;
							zone = value2;
							break;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (debugZoneCaching)
			{
				MessageQueue.AddPlayerMessage(ex.ToString());
			}
			MetricsManager.LogError("Freeze zone exception", ex);
		}
		finally
		{
			CheckedZones.Clear();
		}
	}

	public bool IsZoneBuilt(string ZoneID)
	{
		if (CachedZones.ContainsKey(ZoneID))
		{
			return true;
		}
		if (FrozenZones.Contains(ZoneID))
		{
			return true;
		}
		if (IsInZoneStore(ZoneID))
		{
			return true;
		}
		return false;
	}

	public void ClearFrozen()
	{
		foreach (string frozenZone in FrozenZones)
		{
			if (ZoneConnections.TryGetValue(frozenZone, out var value))
			{
				value.Clear();
			}
		}
		FrozenZones.Clear();
		Directory.Delete(The.Game.GetCacheDirectory("ZoneCache"), recursive: true);
	}

	private ZoneBuilderBlueprint GetZBBByBuilderNameWithAfterTerrain(string Builder)
	{
		if (MatchingBlueprintsByBuilderWithAfterTerrain == null)
		{
			MatchingBlueprintsByBuilderWithAfterTerrain = new Dictionary<string, ZoneBuilderBlueprint>();
		}
		if (MatchingBlueprintsByBuilderWithAfterTerrain.TryGetValue(Builder, out var value))
		{
			return value;
		}
		value = new ZoneBuilderBlueprint(Builder);
		value.AddParameter("AfterTerrain", true);
		MatchingBlueprintsByBuilderWithAfterTerrain[Builder] = value;
		return value;
	}

	private ZoneBuilderBlueprint GetZBBByBuilderName(string Builder)
	{
		if (MatchingBlueprintsByBuilder == null)
		{
			MatchingBlueprintsByBuilder = new Dictionary<string, ZoneBuilderBlueprint>();
		}
		if (MatchingBlueprintsByBuilder.TryGetValue(Builder, out var value))
		{
			return value;
		}
		value = new ZoneBuilderBlueprint(Builder);
		MatchingBlueprintsByBuilder[Builder] = value;
		return value;
	}

	public void AddZoneBuilderOverride(string ZoneID, string Builder)
	{
		AddZoneBuilderOverride(ZoneID, GetZBBByBuilderName(Builder));
	}

	public void AddZoneBuilderOverride(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		if (!ZoneBuilderOverrides.TryGetValue(ZoneID, out var value))
		{
			value = new List<ZoneBuilderBlueprint>();
			ZoneBuilderOverrides[ZoneID] = value;
		}
		value.Add(Builder);
	}

	public void AddZonePreBuilder(string ZoneID, string Builder)
	{
		AddZonePreBuilder(ZoneID, GetZBBByBuilderName(Builder));
	}

	public void AddZonePreBuilder(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		if (ZonePreBuilders.TryGetValue(ZoneID, out var value))
		{
			value.Add(Builder);
			return;
		}
		ZonePreBuilders[ZoneID] = new List<ZoneBuilderBlueprint> { Builder };
	}

	public void AddZoneMidBuilderAtStart(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		if (ZoneMidBuilders.TryGetValue(ZoneID, out var value))
		{
			value.Insert(0, Builder);
			return;
		}
		ZoneMidBuilders[ZoneID] = new List<ZoneBuilderBlueprint> { Builder };
	}

	public void AddZoneMidBuilder(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		if (ZoneMidBuilders.TryGetValue(ZoneID, out var value))
		{
			value.Add(Builder);
			return;
		}
		ZoneMidBuilders[ZoneID] = new List<ZoneBuilderBlueprint> { Builder };
	}

	public void AddZoneMidBuilder(string ZoneID, string Builder)
	{
		AddZoneMidBuilder(ZoneID, GetZBBByBuilderName(Builder));
	}

	public void AddZoneMidBuilderAtStart(string ZoneID, string Builder)
	{
		AddZoneMidBuilderAtStart(ZoneID, GetZBBByBuilderName(Builder));
	}

	public void AddZonePostBuilderIfNotAlreadyPresent(string ZoneID, string Builder)
	{
		ZoneBuilderBlueprint zBBByBuilderName = GetZBBByBuilderName(Builder);
		if (ZonePostBuilders.TryGetValue(ZoneID, out var value))
		{
			if (!value.Contains(zBBByBuilderName))
			{
				value.Add(zBBByBuilderName);
			}
		}
		else
		{
			ZonePostBuilders[ZoneID] = new List<ZoneBuilderBlueprint> { zBBByBuilderName };
		}
	}

	public void AddZonePostBuilder(string ZoneID, string Builder)
	{
		AddZonePostBuilder(ZoneID, GetZBBByBuilderName(Builder));
	}

	public void AddZonePostBuilderAtStart(string ZoneID, string Builder)
	{
		AddZonePostBuilderAtStart(ZoneID, GetZBBByBuilderName(Builder));
	}

	public void AddZonePostBuilderAfterTerrain(string ZoneID, string Builder)
	{
		AddZonePostBuilder(ZoneID, GetZBBByBuilderNameWithAfterTerrain(Builder));
	}

	public void AddZonePostBuilderAfterTerrain(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		Builder.AddParameter("AfterTerrain", true);
		AddZonePostBuilder(ZoneID, Builder);
	}

	public void AddZonePostBuilder(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		if (ZonePostBuilders.TryGetValue(ZoneID, out var value))
		{
			value.Add(Builder);
			return;
		}
		ZonePostBuilders[ZoneID] = new List<ZoneBuilderBlueprint> { Builder };
	}

	public void AddZonePostBuilderAtStart(string ZoneID, ZoneBuilderBlueprint Builder)
	{
		if (ZonePostBuilders.TryGetValue(ZoneID, out var value))
		{
			value.Insert(0, Builder);
			return;
		}
		ZonePostBuilders[ZoneID] = new List<ZoneBuilderBlueprint> { Builder };
	}

	public void ClearZoneBuilders(string ZoneID)
	{
		ZonePreBuilders.Remove(ZoneID);
		ZoneMidBuilders.Remove(ZoneID);
		ZonePostBuilders.Remove(ZoneID);
	}

	public void SetZoneColumnProperty(string ZoneID, string Name, object Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
		SetZoneProperty(ZoneID, Name, Value);
	}

	public void SetWorldCellProperty(string ZoneID, string Name, object Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.UpToNthIndex('.', 3));
		SetZoneProperty(ZoneID, Name, Value);
	}

	public void SetZoneProperty(string ZoneID, string Name, object Value)
	{
		if (!ZoneProperties.TryGetValue(ZoneID, out var value))
		{
			value = new Dictionary<string, object>();
			ZoneProperties.Add(ZoneID, value);
		}
		value[Name] = Value;
	}

	public bool HasZoneProperty(string ZoneID, string Name)
	{
		if (ZoneProperties.TryGetValue(ZoneID, out var value))
		{
			return value.ContainsKey(Name);
		}
		return false;
	}

	public bool HasZoneColumnProperty(string ZoneID, string Name)
	{
		if (ZoneID != null)
		{
			ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
			if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.ContainsKey(Name))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetZoneColumnProperty<T>(string ZoneID, string Name, out T Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
		if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
		{
			Value = (T)value2;
			return true;
		}
		Value = default(T);
		return false;
	}

	public object GetZoneColumnProperty(string ZoneID, string Name, object Default = null)
	{
		if (ZoneID != null)
		{
			ZoneID = ZoneID.Substring(0, ZoneID.LastIndexOf('.'));
			if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
			{
				return value2;
			}
		}
		return Default;
	}

	public bool TryGetWorldCellProperty<T>(string ZoneID, string Name, out T Value)
	{
		ZoneID = ZoneID.Substring(0, ZoneID.UpToNthIndex('.', 3));
		if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
		{
			Value = (T)value2;
			return true;
		}
		Value = default(T);
		return false;
	}

	public object GetWorldCellProperty(string ZoneID, string Name, object Default = null)
	{
		if (ZoneID != null)
		{
			ZoneID = ZoneID.Substring(0, ZoneID.UpToNthIndex('.', 3));
			if (ZoneProperties.TryGetValue(ZoneID, out var value) && value.TryGetValue(Name, out var value2))
			{
				return value2;
			}
		}
		return Default;
	}

	public object GetZoneProperty(string zID, string Name, bool bClampToLevel30 = false, string defaultvalue = null)
	{
		if (zID != null)
		{
			if (ZoneProperties.TryGetValue(zID, out var value) && value.TryGetValue(Name, out var value2))
			{
				return value2;
			}
			if (bClampToLevel30 && zID.Contains(".") && ZoneID.Parse(zID, out var World, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var _))
			{
				zID = ZoneID.Assemble(World, ParasangX, ParasangY, ZoneX, ZoneY, 29);
				return GetZoneProperty(zID, Name, bClampToLevel30: false, defaultvalue);
			}
		}
		return defaultvalue;
	}

	public List<ZoneConnection> GetZoneConnections(string ZoneID)
	{
		if (!ZoneConnections.TryGetValue(ZoneID, out var value))
		{
			return new List<ZoneConnection>();
		}
		return value;
	}

	public List<ZoneConnection> GetZoneConnectionsCopy(string ZoneID)
	{
		if (!ZoneConnections.TryGetValue(ZoneID, out var value))
		{
			return new List<ZoneConnection>();
		}
		return new List<ZoneConnection>(value);
	}

	public void AddZoneConnection(string ZoneID, string TargetDirection, int X, int Y, string Type, string ConnectionObject = null)
	{
		string zoneFromIDAndDirection = GetZoneFromIDAndDirection(ZoneID, TargetDirection);
		if (!ZoneConnections.ContainsKey(zoneFromIDAndDirection))
		{
			ZoneConnections.Add(zoneFromIDAndDirection, new List<ZoneConnection>());
		}
		ZoneConnection zoneConnection = new ZoneConnection();
		zoneConnection.X = X;
		zoneConnection.Y = Y;
		zoneConnection.Type = Type;
		zoneConnection.Object = ConnectionObject;
		ZoneConnections[zoneFromIDAndDirection].Add(zoneConnection);
	}

	public string GetZoneFromIDAndDirection(string ZoneID, string Direction)
	{
		if (string.IsNullOrEmpty(ZoneID) || !ZoneID.Contains("."))
		{
			return "";
		}
		string[] array = ZoneID.Split('.');
		string text = array[0];
		int num = Convert.ToInt32(array[1]);
		int num2 = Convert.ToInt32(array[2]);
		int num3 = Convert.ToInt32(array[3]);
		int num4 = Convert.ToInt32(array[4]);
		int num5 = Convert.ToInt32(array[5]);
		Direction = Direction.ToLower();
		if (Direction == "u")
		{
			num5--;
		}
		if (Direction == "d")
		{
			num5++;
		}
		if (Direction == "n")
		{
			num4--;
		}
		if (Direction == "s")
		{
			num4++;
		}
		if (Direction == "e")
		{
			num3++;
		}
		if (Direction == "w")
		{
			num3--;
		}
		if (Direction == "nw")
		{
			num3--;
			num4--;
		}
		if (Direction == "ne")
		{
			num3++;
			num4--;
		}
		if (Direction == "sw")
		{
			num3--;
			num4++;
		}
		if (Direction == "se")
		{
			num3++;
			num4++;
		}
		if (num3 < 0)
		{
			num3 = Definitions.Width - 1;
			num--;
		}
		if (num3 >= Definitions.Width)
		{
			num3 = 0;
			num++;
		}
		if (num4 < 0)
		{
			num4 = Definitions.Height - 1;
			num2--;
		}
		if (num4 >= Definitions.Width)
		{
			num4 = 0;
			num2++;
		}
		if (num5 < 0)
		{
			num5 = Definitions.Layers - 1;
		}
		if (num5 >= Definitions.Layers)
		{
			num5 = 0;
		}
		return text + "." + num + "." + num2 + "." + num3 + "." + num4 + "." + num5;
	}

	public void DeleteZone(Zone Z)
	{
		CachedZones.Remove(Z.ZoneID);
	}

	public bool SuspendZone(Zone Z)
	{
		if (Z != null)
		{
			Z.HandleEvent(eSuspending);
			foreach (GameObject item in The.Game.ActionManager.ActionQueue.Items)
			{
				if (item != null && item.CurrentZone == Z)
				{
					item.MakeInactive();
				}
			}
			Z.bSuspended = true;
		}
		return true;
	}

	private static void Activate(GameObject GO)
	{
		XRLCore.Core.Game.ActionManager.AddActiveObject(GO);
	}

	public static void ActivateBrainHavers(Zone Z)
	{
		Z.ForeachObjectWithPart("Brain", Activate);
	}

	public Zone SetActiveZone(string ZoneID, Action afterGet = null)
	{
		if (ActiveZone != null)
		{
			if (ActiveZone.ZoneID == ZoneID)
			{
				return ActiveZone;
			}
			ActiveZone.Deactivated();
		}
		XRLCore.ParticleManager.Particles.Clear();
		Zone zone = GetZone(ZoneID);
		afterGet?.Invoke();
		return SetActiveZone(zone);
	}

	public Zone SetActiveZone(Zone Z)
	{
		if (ActiveZone != Z)
		{
			try
			{
				GameManager.Instance?.uiQueue?.queueTask(delegate
				{
					CombatJuiceManager.finishAll();
				});
			}
			catch (Exception x)
			{
				MetricsManager.LogException("SetActiveZone::FinishJuice", x);
			}
		}
		ActiveZone = Z;
		LastZoneTransition = XRLCore.Core.Game.Turns;
		string zoneID = ActiveZone.ZoneID;
		if (ActiveZone != null)
		{
			zoneGenerationContextTier = ActiveZone.NewTier;
			zoneGenerationContextZoneID = ActiveZone.ZoneID;
		}
		if (!XRLCore.Core.Game.bZoned && LastZoneTransition > 0)
		{
			if (zoneID.Contains(".") && zoneID.StartsWith("JoppaWorld."))
			{
				Popup.ShowBlock("{{G|Important Tip}}:\nYou can go to the world map by pressing {{W|-}} on the numpad or {{W|<}} (i.e. go up) from any surface zone. You can return to the region surface by pressing {{W|+}} on the numpad or {{W|>}} (i.e. go down)");
			}
			The.Game.bZoned = true;
		}
		if (!VisitedTime.ContainsKey(zoneID))
		{
			if (XRLCore.Core.Game.Player.Body != null && zoneID.Contains("."))
			{
				The.Game.Player.Body.FireEvent(Event.New("VisitingNewZone", "ID", zoneID));
			}
			string text = WorldFactory.Factory.ZoneDisplayName(ActiveZone.ZoneID);
			if (text.Contains("Kyakukya"))
			{
				if (XRLCore.CurrentTurn - The.Game.GetInt64GameState("LastKyakukyaVisit", 0L) > 1000)
				{
					JournalAPI.AddAccomplishment("You journeyed to Kyakukya.", "Done trekking through the root-strangled earth, =name= arrived in Kyakukya and was greeted by the village with warmth and reverence. Upon leaving, =name= was named Friend to Oboroqoru.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				}
				The.Game.SetInt64GameState("LastKyakukyaVisit", XRLCore.CurrentTurn);
			}
			if (text.Contains("Omonporch"))
			{
				if (XRLCore.CurrentTurn - The.Game.GetInt64GameState("LastOmonporchVisit", 0L) > 1000)
				{
					JournalAPI.AddAccomplishment("You journeyed to Omonporch.", "=name= journeyed to the Spindle at Omonporch, voiced a prayer for the Fossilized Saads, and observed a star fall along the planet's axis.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				}
				The.Game.SetInt64GameState("LastOmonporchVisit", XRLCore.CurrentTurn);
			}
			if ((text.Contains("Stiltgrounds") || text.Contains("Six Day Stilt")) && The.Game.GetIntGameState("VisitedSixDayStilt") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Six Day Stilt.", "=name= trekked through the salt pans, north and west, to the merchant bazaar and grand cathedral of the Six Day Stilt. There, the stiltfolk sang hymns in the sultan's honor.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedSixDayStilt", 1);
				AchievementManager.SetAchievement("ACH_SIX_DAY_STILT");
			}
			if (text.Contains("Red Rock") && The.Game.GetIntGameState("VisitedRedrock") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Red Rock.", "=name= hiked the salted marshes and came upon the ancient gathering place called Red Rock, where a bevy of snapjaws cooked the sultan a satisfying meal.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedRedrock", 1);
			}
			if (text.Contains("rusted archway") && The.Game.GetIntGameState("VisitedRustedArchway") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to a rusted archway.", "To commemorate the imperial conequests of =name=, a triumphal arch was erected across the fruiting gorges.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedRustedArchway", 1);
			}
			if (text.Contains("Rustwell") && The.Game.GetIntGameState("VisitedRustwells") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the rust wells.", "Hillfolk dug wells deep into the earth and drew spring water to honor the sultan =name= in sacred ritual.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedRustwells", 1);
			}
			if (text.Contains("Grit Gate") && The.Game.GetIntGameState("VisitedGritGate") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Grit Gate.", "The scholar-sultan =name= visited Grit Gate and lectured the monastic order there on the subject of light refraction.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedGritGate", 1);
			}
			if (text.Contains("Golgotha") && The.Game.GetIntGameState("VisitedGolgotha") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Golgotha.", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + " AR, =name= ascended the trash chutes of Golgotha, victorious and bathed in slime.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedGolgotha", 1);
			}
			if (text.Contains("Bethesda Susa") && The.Game.GetIntGameState("VisitedBethesda") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to Bethesda Susa.", "=name= trekked to the frozen sauna at Bethesda Susa and soaked in the company of trolls.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedBethesda", 1);
			}
			if (text.Contains("Tomb of the Eaters") && The.Game.GetIntGameState("VisitedTomboftheEaters") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Tomb of the Eaters.", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + " AR, =name= trekked to the Tomb of the Eaters and traced a sigil across the Death Gate. O wise sultan!", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
				The.Game.SetIntGameState("VisitedTomboftheEaters", 1);
			}
			if (text.Contains("Asphalt Mines") && The.Game.GetIntGameState("VisitedAsphaltMines") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the asphalt mines.", "Strong =name= trekked to the asphalt mines to breathe the tarry vapor and bathe in the black blood of the earth.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedAsphaltMines", 1);
			}
			if (text.Contains("City of Bones") && The.Game.GetIntGameState("VisitedCityofBones") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the City of Bones.", "Strong =name= trekked to the City of Bones to breathe the tarry vapor and bathe in the black blood of the earth.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedCityofBones", 1);
			}
			if (text.Contains("Great Sea") && The.Game.GetIntGameState("VisitedGreatSea") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Great Asphalt Sea.", "Strong =name= trekked to the Great Asphalt Sea to breathe the tarry vapor and bathe in the black blood of the earth.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedGreatSea", 1);
			}
			if (text.Contains("Tunnels of Ur") && The.Game.GetIntGameState("VisitedTunnelsUr") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Tunnels of Ur.", "Strong =name= trekked to the Tunnels of Ur to breathe the tarry vapor and bathe in the black blood of the earth.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedTunnelsUr", 1);
			}
			if (text.Contains("Swilling Vast") && The.Game.GetIntGameState("VisitedSwillingVast") != 1)
			{
				JournalAPI.AddAccomplishment("You journeyed to the Swilling Vast.", "Strong =name= trekked to the Swilling Vast to breathe the tarry vapor and bathe in the black blood of the earth.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
				The.Game.SetIntGameState("VisitedSwillingVast", 1);
			}
		}
		VisitedTime[zoneID] = Calendar.TotalTimeTicks;
		ActiveZone.Activated();
		The.Game.ActionManager.DequeAction("SuspendZone", ActiveZone.ZoneID);
		ActivateBrainHavers(ActiveZone);
		ActiveZone.bSuspended = false;
		The.Game.ZoneManager.Tick(bAllowFreeze: true);
		string text2 = zoneID;
		if (!string.IsNullOrEmpty(Calendar.getTime(text2)))
		{
			MessageQueue.AddPlayerMessage(WorldFactory.Factory.ZoneDisplayName(text2) + ", " + Calendar.getTime(text2), 'C');
		}
		else
		{
			MessageQueue.AddPlayerMessage(WorldFactory.Factory.ZoneDisplayName(text2), 'C');
		}
		try
		{
			List<JournalMapNote> mapNotesForZone = JournalAPI.GetMapNotesForZone(ActiveZone.ZoneID);
			if (mapNotesForZone.Count > 0)
			{
				string text3 = "Notes: ";
				int num = 0;
				for (int i = 0; i < mapNotesForZone.Count; i++)
				{
					if (mapNotesForZone[i].revealed)
					{
						if (num > 0)
						{
							text3 += ", ";
						}
						text3 += mapNotesForZone[i].text;
						num++;
					}
				}
				if (num > 0)
				{
					MessageQueue.AddPlayerMessage(text3);
				}
			}
			AutoAct.Interrupt();
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception during zone note retrieval: " + ex.ToString());
		}
		try
		{
			ZoneTransitionCount++;
			if (ZoneTransitionCount >= ZoneTransitionSaveInterval && The.Player?.CurrentCell != null)
			{
				ZoneTransitionCount = 0;
				XRLCore.Core.SaveGame("Primary.sav");
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Exception during autosave: " + ex2.ToString());
		}
		ProcessGoToPartyLeader();
		return ActiveZone;
	}

	public void CheckEventQueue()
	{
		if (ActiveZone != null)
		{
			ActiveZone.CheckEventQueue();
		}
	}

	public bool IsZoneLive(string ZoneID)
	{
		if (ZoneID == null)
		{
			return false;
		}
		return CachedZones.ContainsKey(ZoneID);
	}

	public bool IsZoneLive(string World, int wX, int wY, int X, int Y, int Z)
	{
		return CachedZones.ContainsKey(ZoneID.Assemble(World, wX, wY, X, Y, Z));
	}

	public Zone GetZone(string World, int wX, int wY, int X, int Y, int Z)
	{
		if (string.IsNullOrEmpty(World))
		{
			return null;
		}
		return GetZone(ZoneID.Assemble(World, wX, wY, X, Y, Z));
	}

	public Zone GetZone(string ZoneID)
	{
		try
		{
			lock (ZoneLock)
			{
				if (string.IsNullOrEmpty(ZoneID))
				{
					return null;
				}
				if (CachedZones.TryGetValue(ZoneID, out var value))
				{
					return value;
				}
				if (FrozenZones.Contains(ZoneID))
				{
					value = ThawZone(ZoneID);
					if (value != null)
					{
						return value;
					}
				}
				if (IsInZoneStore(ZoneID))
				{
					return GetFromZoneStore(ZoneID);
				}
				GenerateZone(ZoneID);
				return CachedZones[ZoneID];
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error in GetZone " + ZoneID, x);
			GenerateZone(ZoneID);
			return CachedZones[ZoneID];
		}
	}

	private bool IsInZoneStore(string ZoneID)
	{
		return false;
	}

	private Zone GetFromZoneStore(string ZoneID)
	{
		return null;
	}

	public List<CellBlueprint> GetCellBlueprints(string ZoneID)
	{
		List<CellBlueprint> list = new List<CellBlueprint>();
		if (ZoneID.IndexOf('.') != -1)
		{
			string[] array = ZoneID.Split('.');
			string text = array[0];
			int wx = Convert.ToInt32(array[1]);
			int wy = Convert.ToInt32(array[2]);
			GameObject terrainObjectForZone = GetTerrainObjectForZone(wx, wy, text);
			if (terrainObjectForZone != null)
			{
				if (WorldFactory.Factory.getWorld(text).CellBlueprintsByApplication.ContainsKey(terrainObjectForZone.Blueprint))
				{
					CellBlueprint item = WorldFactory.Factory.getWorld(text).CellBlueprintsByApplication[terrainObjectForZone.Blueprint];
					list.Add(item);
				}
				string key = wx + "." + wy;
				if (WorldFactory.Factory.getWorld(text).CellBlueprintsByApplication.ContainsKey(key))
				{
					CellBlueprint item2 = WorldFactory.Factory.getWorld(text).CellBlueprintsByApplication[key];
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public void SetZoneDisplayName(string ZoneID, string Name, bool Sync = true)
	{
		SetZoneBaseDisplayName(ZoneID, Name, Sync);
	}

	public ZoneBlueprint GetZoneBlueprint(string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (ZPos < 0)
		{
			ZPos = 0;
		}
		if (ZPos > 49)
		{
			ZPos = 49;
		}
		string key = SB.Clear().Append(wXPos).Append('.')
			.Append(wYPos)
			.ToString();
		if (WorldFactory.Factory.getWorld(WorldID).CellBlueprintsByApplication.TryGetValue(key, out var value))
		{
			ZoneBlueprint zoneBlueprint = value.LevelBlueprint[XPos, YPos, ZPos];
			if (zoneBlueprint != null)
			{
				return zoneBlueprint;
			}
		}
		GameObject terrainObjectForZone = GetTerrainObjectForZone(wXPos, wYPos, WorldID);
		if (terrainObjectForZone != null)
		{
			if (WorldFactory.Factory.getWorld(WorldID).CellBlueprintsByApplication.TryGetValue(terrainObjectForZone.Blueprint, out value))
			{
				ZoneBlueprint zoneBlueprint2 = value.LevelBlueprint[XPos, YPos, ZPos];
				if (zoneBlueprint2 != null)
				{
					return zoneBlueprint2;
				}
			}
		}
		else
		{
			MetricsManager.LogError($"No terrain object in world map of {WorldID} at {wXPos} {wYPos}");
		}
		return null;
	}

	public ZoneBlueprint GetZoneBlueprint(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (ZoneID.IndexOf('.') == -1)
		{
			return null;
		}
		return GetZoneBlueprint(WorldID, wXPos, wYPos, XPos, YPos, ZPos);
	}

	public ZoneBlueprint GetZoneBlueprint(string ZoneID, out int ZPos)
	{
		ZPos = 10;
		if (ZoneID == null)
		{
			return null;
		}
		if (ZoneID.IndexOf('.') == -1)
		{
			return null;
		}
		string[] array = ZoneID.Split('.');
		string worldID = array[0];
		int wXPos = Convert.ToInt32(array[1]);
		int wYPos = Convert.ToInt32(array[2]);
		int xPos = Convert.ToInt32(array[3]);
		int yPos = Convert.ToInt32(array[4]);
		ZPos = Convert.ToInt32(array[5]);
		return GetZoneBlueprint(worldID, wXPos, wYPos, xPos, yPos, ZPos);
	}

	public ZoneBlueprint GetZoneBlueprint(string ZoneID)
	{
		int ZPos;
		return GetZoneBlueprint(ZoneID, out ZPos);
	}

	public string GetZoneDisplayName(string ZoneID, int ZPos, ZoneBlueprint ZBP, bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, bool Mutate = true)
	{
		string zoneBaseDisplayName = GetZoneBaseDisplayName(ZoneID, ZBP, Mutate);
		if (zoneBaseDisplayName != null && ZoneID != null && ZoneID.StartsWith("ThinWorld"))
		{
			return zoneBaseDisplayName;
		}
		SB.Clear();
		if (!string.IsNullOrEmpty(zoneBaseDisplayName))
		{
			if (WithIndefiniteArticle)
			{
				string zoneIndefiniteArticle = GetZoneIndefiniteArticle(ZoneID, ZBP);
				if (zoneIndefiniteArticle == null)
				{
					if (!GetZoneHasProperName(ZoneID, ZBP))
					{
						SB.Append(Grammar.IndefiniteArticleShouldBeAn(zoneBaseDisplayName) ? "an " : "a ");
					}
				}
				else if (zoneIndefiniteArticle != "")
				{
					SB.Append(zoneIndefiniteArticle).Append(' ');
				}
			}
			else if (WithDefiniteArticle)
			{
				string zoneDefiniteArticle = GetZoneDefiniteArticle(ZoneID, ZBP);
				if (zoneDefiniteArticle == null)
				{
					if (!GetZoneHasProperName(ZoneID, ZBP))
					{
						SB.Append("the ");
					}
				}
				else if (zoneDefiniteArticle != "")
				{
					SB.Append(zoneDefiniteArticle).Append(' ');
				}
			}
			SB.Append(zoneBaseDisplayName);
		}
		if (string.IsNullOrEmpty(zoneBaseDisplayName) || GetZoneIncludeContextInZoneDisplay(ZoneID, ZBP))
		{
			string zoneNameContext = GetZoneNameContext(ZoneID, ZBP);
			if (zoneNameContext != null)
			{
				if (SB.Length > 0)
				{
					SB.Append(", ");
				}
				SB.Append(zoneNameContext);
			}
		}
		if (GetZoneIncludeStratumInZoneDisplay(ZoneID, ZBP))
		{
			int num = 10 - ZPos;
			if (num == 0)
			{
				SB.Compound("surface", ", ");
				if (ZBP != null && ZBP.AnyBuilder((ZoneBuilderBlueprint b) => b.Class == "FlagInside"))
				{
					SB.Append(" level");
				}
			}
			else
			{
				int num2 = Math.Abs(num);
				SB.Compound(num2, ", ").Append((num2 == 1) ? " stratum " : " strata ");
				if (num < 0)
				{
					SB.Append("deep");
				}
				else
				{
					SB.Append("high");
				}
			}
		}
		return SB.ToString();
	}

	public void SynchronizeZoneName(string ZoneID, int ZPos, ZoneBlueprint ZBP)
	{
		WorldFactory.Factory?.UpdateZoneDisplayName(ZoneID, GetZoneDisplayName(ZoneID, ZPos, ZBP));
		NameUpdateTick++;
	}

	public void SynchronizeZoneName(string ZoneID)
	{
		int ZPos;
		ZoneBlueprint zoneBlueprint = GetZoneBlueprint(ZoneID, out ZPos);
		SynchronizeZoneName(ZoneID, ZPos, zoneBlueprint);
	}

	public string GetZoneDisplayName(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos, bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, bool Mutate = true)
	{
		return GetZoneDisplayName(ZoneID, ZPos, GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos), WithIndefiniteArticle, WithDefiniteArticle, Mutate);
	}

	public string GetZoneDisplayName(string ZoneID, bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, bool Mutate = true)
	{
		int ZPos;
		ZoneBlueprint zoneBlueprint = GetZoneBlueprint(ZoneID, out ZPos);
		return GetZoneDisplayName(ZoneID, ZPos, zoneBlueprint, WithIndefiniteArticle, WithDefiniteArticle, Mutate);
	}

	public string GetZoneBaseDisplayName(string ZoneID, ZoneBlueprint ZBP, bool Mutate = true)
	{
		string text = The.Game.GetStringGameState("ZoneName_" + ZoneID, null);
		if (string.IsNullOrEmpty(text))
		{
			if (ZBP == null)
			{
				if (ZoneID.IndexOf('.') == -1)
				{
					text = WorldFactory.Factory.getWorld(ZoneID).DisplayName;
				}
			}
			else
			{
				text = ZBP.Name;
			}
		}
		if (!Mutate)
		{
			return text;
		}
		return BiomeManager.MutateZoneName(text, ZoneID);
	}

	public string GetZoneBaseDisplayName(string ZoneID, bool Mutate = true)
	{
		return GetZoneBaseDisplayName(ZoneID, GetZoneBlueprint(ZoneID), Mutate);
	}

	public void SetZoneBaseDisplayName(string ZoneID, string Name, bool Sync = true)
	{
		The.Game.SetStringGameState("ZoneName_" + ZoneID, Name);
		if (Sync)
		{
			SynchronizeZoneName(ZoneID);
		}
		else
		{
			WorldFactory.Factory?.UpdateZoneDisplayName(ZoneID, Name);
		}
	}

	public string GetZoneReferenceDisplayName(string ZoneID, int ZPos, ZoneBlueprint ZBP)
	{
		string text = GetZoneBaseDisplayName(ZoneID, ZBP);
		if (text != null)
		{
			if (text.StartsWith("some "))
			{
				text = text.Substring(5);
			}
			if (text.StartsWith("the ") || text.StartsWith("The "))
			{
				text = text.Substring(4);
			}
		}
		SB.Clear();
		if (string.IsNullOrEmpty(text) || GetZoneIncludeContextInZoneDisplay(ZoneID, ZBP))
		{
			string zoneNameContext = GetZoneNameContext(ZoneID, ZBP);
			if (zoneNameContext != null)
			{
				SB.Append(zoneNameContext);
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			SB.Compound(text);
		}
		if (GetZoneIncludeStratumInZoneDisplay(ZoneID, ZBP))
		{
			int num = 10 - ZPos;
			if (num != 0)
			{
				int num2 = Math.Abs(num);
				SB.Compound(num2).Append((num2 == 1) ? " stratum " : " strata ");
				if (num < 0)
				{
					SB.Append("deep");
				}
				else
				{
					SB.Append("high");
				}
			}
		}
		return SB.ToString();
	}

	public string GetZoneReferenceDisplayName(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneReferenceDisplayName(ZoneID, ZPos, GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos));
	}

	public string GetZoneReferenceDisplayName(string ZoneID)
	{
		int ZPos;
		ZoneBlueprint zoneBlueprint = GetZoneBlueprint(ZoneID, out ZPos);
		return GetZoneReferenceDisplayName(ZoneID, ZPos, zoneBlueprint);
	}

	public void SetZoneNameContext(string ZoneID, string Value, bool Sync = true)
	{
		The.Game.SetStringGameState("ZoneNameContext_" + ZoneID, Value);
		if (Sync)
		{
			SynchronizeZoneName(ZoneID);
		}
	}

	public string GetZoneNameContext(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneNameContext_" + ZoneID, null);
		if (stringGameState != null)
		{
			if (stringGameState == "")
			{
				return null;
			}
			return stringGameState;
		}
		return ZBP?.NameContext;
	}

	public string GetZoneNameContext(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneNameContext_" + ZoneID, null);
		if (stringGameState != null)
		{
			if (stringGameState == "")
			{
				return null;
			}
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.NameContext;
	}

	public string GetZoneNameContext(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneNameContext_" + ZoneID, null);
		if (stringGameState != null)
		{
			if (stringGameState == "")
			{
				return null;
			}
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.NameContext;
	}

	public void SetZoneHasProperName(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneProperName_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneProperName_" + ZoneID, State.Value);
		}
		SynchronizeZoneName(ZoneID);
	}

	public bool GetZoneHasProperName(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneProperName_" + ZoneID, out var Result))
		{
			return Result;
		}
		if (ZBP != null)
		{
			return ZBP.ProperName;
		}
		if (ZoneID.IndexOf('.') == -1)
		{
			return true;
		}
		return false;
	}

	public bool GetZoneHasProperName(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneProperName_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.ProperName ?? false;
	}

	public bool GetZoneHasProperName(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneProperName_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.ProperName ?? false;
	}

	public void SetZoneIndefiniteArticle(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneIndefiniteArticle_" + ZoneID, Value);
		SynchronizeZoneName(ZoneID);
	}

	public string GetZoneIndefiniteArticle(string ZoneID, ZoneBlueprint ZBP)
	{
		string text = "ZoneIndefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return ZBP?.IndefiniteArticle;
	}

	public string GetZoneIndefiniteArticle(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string text = "ZoneIndefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.IndefiniteArticle;
	}

	public string GetZoneIndefiniteArticle(string ZoneID)
	{
		string text = "ZoneIndefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.IndefiniteArticle;
	}

	public void SetZoneDefiniteArticle(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneDefiniteArticle_" + ZoneID, Value);
		SynchronizeZoneName(ZoneID);
	}

	public string GetZoneDefiniteArticle(string ZoneID, ZoneBlueprint ZBP)
	{
		string text = "ZoneDefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return ZBP?.DefiniteArticle;
	}

	public string GetZoneDefiniteArticle(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string text = "ZoneDefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.DefiniteArticle;
	}

	public string GetZoneDefiniteArticle(string ZoneID)
	{
		string text = "ZoneDefiniteArticle_" + ZoneID;
		string stringGameState = The.Game.GetStringGameState(text, null);
		if (stringGameState != null || The.Game.HasStringGameState(text))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.DefiniteArticle;
	}

	public void SetZoneName(string ZoneID, string Name, string Context = null, string IndefiniteArticle = null, string DefiniteArticle = null, string Article = null, bool Proper = false, bool Sync = true)
	{
		if (Name != null)
		{
			if (Name.StartsWith("the ") || Name.StartsWith("The "))
			{
				Name = Name.Substring(4);
				Article = "the";
				Proper = true;
			}
			else if (Name.StartsWith("some "))
			{
				Name = Name.Substring(5);
				Article = "some";
				Proper = false;
			}
		}
		if (Article != null)
		{
			IndefiniteArticle = Article;
			DefiniteArticle = Article;
		}
		if (Proper)
		{
			if (IndefiniteArticle == null)
			{
				IndefiniteArticle = "";
			}
			if (DefiniteArticle == null)
			{
				DefiniteArticle = "";
			}
		}
		SetZoneDisplayName(ZoneID, Name, Sync);
		SetZoneNameContext(ZoneID, Context ?? "", Sync);
		SetZoneHasProperName(ZoneID, Proper);
		SetZoneIndefiniteArticle(ZoneID, IndefiniteArticle);
		SetZoneDefiniteArticle(ZoneID, DefiniteArticle);
	}

	public void SetZoneIncludeContextInZoneDisplay(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, State.Value);
		}
		SynchronizeZoneName(ZoneID);
	}

	public bool GetZoneIncludeContextInZoneDisplay(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return ZBP?.IncludeContextInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeContextInZoneDisplay(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.IncludeContextInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeContextInZoneDisplay(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeContextInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.IncludeContextInZoneDisplay ?? true;
	}

	public void SetZoneIncludeStratumInZoneDisplay(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, State.Value);
		}
		SynchronizeZoneName(ZoneID);
	}

	public bool GetZoneIncludeStratumInZoneDisplay(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return ZBP?.IncludeStratumInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeStratumInZoneDisplay(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.IncludeStratumInZoneDisplay ?? true;
	}

	public bool GetZoneIncludeStratumInZoneDisplay(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneIncludeStratumInZoneDisplay_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.IncludeContextInZoneDisplay ?? true;
	}

	public void SetZoneNamedByPlayer(string ZoneID, bool State)
	{
		if (State)
		{
			The.Game.SetBooleanGameState("ZoneNamedByPlayer_" + ZoneID, Value: true);
		}
		else
		{
			The.Game.RemoveBooleanGameState("ZoneNamedByPlayer_" + ZoneID);
		}
	}

	public bool GetZoneNamedByPlayer(string ZoneID)
	{
		return The.Game.GetBooleanGameState("ZoneNamedByPlayer_" + ZoneID);
	}

	public void SetZoneHasWeather(string ZoneID, bool? State)
	{
		if (!State.HasValue)
		{
			The.Game.RemoveBooleanGameState("ZoneHasWeather_" + ZoneID);
		}
		else
		{
			The.Game.SetBooleanGameState("ZoneHasWeather_" + ZoneID, State.Value);
		}
	}

	public bool GetZoneHasWeather(string ZoneID, ZoneBlueprint ZBP)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneHasWeather_" + ZoneID, out var Result))
		{
			return Result;
		}
		return ZBP?.HasWeather ?? false;
	}

	public bool GetZoneHasWeather(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneHasWeather_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.HasWeather ?? false;
	}

	public bool GetZoneHasWeather(string ZoneID)
	{
		if (XRLCore.Core.Game.TryGetBooleanGameState("ZoneHasWeather_" + ZoneID, out var Result))
		{
			return Result;
		}
		return GetZoneBlueprint(ZoneID)?.HasWeather ?? false;
	}

	public void SetZoneWindSpeed(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneWindSpeed_" + ZoneID, Value);
	}

	public string GetZoneWindSpeed(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindSpeed_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return ZBP?.WindSpeed;
	}

	public string GetZoneWindSpeed(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindSpeed_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.WindSpeed;
	}

	public string GetZoneWindSpeed(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindSpeed_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.WindSpeed;
	}

	public void SetZoneWindDirections(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneWindDirections_" + ZoneID, Value);
	}

	public string GetZoneWindDirections(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDirections_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return ZBP?.WindDirections;
	}

	public string GetZoneWindDirections(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDirections_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.WindDirections;
	}

	public string GetZoneWindDirections(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDirections_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.WindDirections;
	}

	public void SetZoneWindDuration(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneWindDuration_" + ZoneID, Value);
	}

	public string GetZoneWindDuration(string ZoneID, ZoneBlueprint ZBP)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDuration_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return ZBP?.WindDuration;
	}

	public string GetZoneWindDuration(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDuration_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID, WorldID, wXPos, wYPos, XPos, YPos, ZPos)?.WindDuration;
	}

	public string GetZoneWindDuration(string ZoneID)
	{
		string stringGameState = The.Game.GetStringGameState("ZoneWindDuration_" + ZoneID);
		if (!string.IsNullOrEmpty(stringGameState))
		{
			return stringGameState;
		}
		return GetZoneBlueprint(ZoneID)?.WindDuration;
	}

	public void SetZoneCurrentWindSpeed(string ZoneID, int Value)
	{
		The.Game.SetIntGameState("ZoneCurrentWindSpeed_" + ZoneID, Value);
	}

	public int GetZoneCurrentWindSpeed(string ZoneID, ZoneBlueprint ZBP)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public int GetZoneCurrentWindSpeed(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public int GetZoneCurrentWindSpeed(string ZoneID)
	{
		return The.Game.GetIntGameState("ZoneCurrentWindSpeed_" + ZoneID);
	}

	public void SetZoneCurrentWindDirection(string ZoneID, string Value)
	{
		The.Game.SetStringGameState("ZoneCurrentWindDirection_" + ZoneID, Value);
	}

	public string GetZoneCurrentWindDirection(string ZoneID, ZoneBlueprint ZBP)
	{
		return GetZoneCurrentWindDirection(ZoneID);
	}

	public string GetZoneCurrentWindDirection(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneCurrentWindDirection(ZoneID);
	}

	public string GetZoneCurrentWindDirection(string ZoneID)
	{
		return The.Game.GetStringGameState("ZoneCurrentWindDirection_" + ZoneID);
	}

	public void SetZoneNextWindChange(string ZoneID, long Value)
	{
		The.Game.SetInt64GameState("ZoneNextWindChange_" + ZoneID, Value);
	}

	public long GetZoneNextWindChange(string ZoneID, ZoneBlueprint ZBP)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public long GetZoneNextWindChange(string ZoneID, string WorldID, int wXPos, int wYPos, int XPos, int YPos, int ZPos)
	{
		return GetZoneCurrentWindSpeed(ZoneID);
	}

	public long GetZoneNextWindChange(string ZoneID)
	{
		return The.Game.GetInt64GameState("ZoneNextWindChange_" + ZoneID, 0L);
	}

	public GameObject GetOneCreatureFromZone(string ZoneID)
	{
		string populationName = "LairBosses" + The.Game.ZoneManager.GetZoneTier(ZoneID);
		string blueprint;
		GameObjectBlueprint gameObjectBlueprint;
		do
		{
			blueprint = PopulationManager.RollOneFrom(populationName).Blueprint;
			gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[blueprint];
		}
		while (!gameObjectBlueprint.HasPart("Combat") || !gameObjectBlueprint.HasPart("Brain") || (gameObjectBlueprint.Props.ContainsKey("Role") && gameObjectBlueprint.Props["Role"] == "Minion") || gameObjectBlueprint.Props.ContainsKey("Minion") || gameObjectBlueprint.HasPart("DromadWares") || gameObjectBlueprint.HasPart("Tier1Wares") || gameObjectBlueprint.HasPart("Tier2Wares") || gameObjectBlueprint.HasPart("Tier3Wares") || gameObjectBlueprint.HasPart("Tier4Wares") || gameObjectBlueprint.HasPart("Tier5Wares") || gameObjectBlueprint.HasPart("Tier6Wares") || gameObjectBlueprint.HasPart("Tier7Wares") || gameObjectBlueprint.HasPart("Tier8Wares") || gameObjectBlueprint.Builders.ContainsKey("DromadWares") || gameObjectBlueprint.Builders.ContainsKey("Tier1Wares") || gameObjectBlueprint.Builders.ContainsKey("Tier2Wares") || gameObjectBlueprint.Builders.ContainsKey("Tier3Wares") || gameObjectBlueprint.Builders.ContainsKey("Tier4Wares") || gameObjectBlueprint.Builders.ContainsKey("Tier5Wares") || gameObjectBlueprint.Builders.ContainsKey("Tier6Wares") || gameObjectBlueprint.Builders.ContainsKey("Tier7Wares") || gameObjectBlueprint.Builders.ContainsKey("Tier8Wares"));
		return GameObjectFactory.Factory.CreateObject(blueprint);
	}

	public int GetZoneTier(string ZoneID)
	{
		string[] array = ZoneID.Split('.');
		int wXPos = Convert.ToInt32(array[1]);
		int wYPos = Convert.ToInt32(array[2]);
		int zPos = Convert.ToInt32(array[5]);
		return GetZoneTier(array[0], wXPos, wYPos, zPos);
	}

	public int GetZoneTier(string world, int wXPos, int wYPos, int ZPos)
	{
		Zone zone = GetZone(world);
		int num = 1;
		foreach (GameObject @object in zone.GetCell(wXPos, wYPos).Objects)
		{
			int num2 = Convert.ToInt32(@object.GetTag("RegionTier", "1"));
			if (num2 > num)
			{
				num = num2;
			}
		}
		if (ZPos > 15)
		{
			num = Math.Abs(ZPos - 16) / 5 + 2;
		}
		if (num < 1)
		{
			num = 1;
		}
		if (num > 8)
		{
			num = 8;
		}
		return num;
	}

	public GameObject GetZoneTerrain(string world, int wXPos, int wYPos)
	{
		return GetZone(world).GetCell(wXPos, wYPos).GetFirstObjectWithPart("TerrainTravel");
	}

	private bool GenerateFactoryZone(WorldBlueprint world, string ZoneID, out Zone result)
	{
		result = null;
		IZoneFactory zoneFactory = Activator.CreateInstance(ModManager.ResolveType("XRL.World.ZoneFactories." + world.ZoneFactory)) as IZoneFactory;
		if (!zoneFactory.CanBuildZone(ZoneID))
		{
			return false;
		}
		Zone zone = zoneFactory.BuildZone(ZoneID);
		if (zone.ZoneID == null)
		{
			zone.ZoneID = ZoneID;
		}
		CachedZones.Add(ZoneID, zone);
		zone.GeneratedOn = The.Game.Turns;
		zone.HandleEvent(eBeforeZoneBuilt);
		zone.HandleEvent(eZoneBuilt);
		PaintWater(zone);
		zone.HandleEvent(eAfterZoneBuilt);
		zoneFactory.AfterBuildZone(zone, this);
		result = zone;
		return true;
	}

	private void GenerateZone(string ZoneID)
	{
		Event.PinCurrentPool();
		string description = "Building zone...";
		if (Options.DebugShowFullZoneDuringBuild)
		{
			description = "Building " + ZoneID;
		}
		Zone result;
		Loading.LoadTask(description, delegate
		{
			try
			{
				Coach.StartSection("GenerateZone", bTrackGarbage: true);
				int num = 0;
				string FailedBuilder = "<none>";
				while (true)
				{
					IL_0019:
					The.Game.ZoneManager.CachedObjectsToRemoveAfterZoneBuild = new List<string>();
					Event.ResetToPin();
					_ = FailedBuilder != "<none>";
					num++;
					bool flag = false;
					if (num >= 20 && num % 5 == 0 && Popup.ShowYesNo("This zone isn't building properly. Do you want to force it to stop and build immediately?") == DialogResult.Yes)
					{
						flag = true;
					}
					if (!string.IsNullOrEmpty(ZoneID))
					{
						foreach (WorldBlueprint world in WorldFactory.Factory.getWorlds())
						{
							if (world.testZoneFactoryRegex(ZoneID) && GenerateFactoryZone(world, ZoneID, out result))
							{
								The.Game.Systems.ForEach(delegate(IGameSystem s)
								{
									s.NewZoneGenerated(result);
								});
								return;
							}
						}
					}
					if (ZoneID.IndexOf('.') == -1)
					{
						if (string.IsNullOrEmpty(WorldFactory.Factory.getWorld(ZoneID).ZoneFactory) || !GenerateFactoryZone(WorldFactory.Factory.getWorld(ZoneID), ZoneID, out result))
						{
							Zone NewWorldZone = new Zone(80, 25);
							NewWorldZone.ZoneID = ZoneID;
							CachedZones.Add(ZoneID, NewWorldZone);
							zoneGenerationContextTier = 1;
							zoneGenerationContextZoneID = ZoneID;
							if (!string.IsNullOrEmpty(WorldFactory.Factory.getWorld(ZoneID).Map))
							{
								MapBuilder mapBuilder = new MapBuilder();
								mapBuilder.FileName = WorldFactory.Factory.getWorld(ZoneID).Map;
								mapBuilder.BuildZone(NewWorldZone);
							}
							NewWorldZone.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("AmbientLight"));
							NewWorldZone.GeneratedOn = The.Game.Turns;
							NewWorldZone.HandleEvent(eZoneBuilt);
							PaintWater(NewWorldZone);
							The.Game.Systems.ForEach(delegate(IGameSystem s)
							{
								s.NewZoneGenerated(NewWorldZone);
							});
							break;
						}
						The.Game.Systems.ForEach(delegate(IGameSystem s)
						{
							s.NewZoneGenerated(result);
						});
					}
					else
					{
						string[] array = ZoneID.Split('.');
						if (string.IsNullOrEmpty(WorldFactory.Factory.getWorld(array[0]).ZoneFactory) || !GenerateFactoryZone(WorldFactory.Factory.getWorld(array[0]), ZoneID, out result))
						{
							string text = array[0];
							int wx = Convert.ToInt32(array[1]);
							int wy = Convert.ToInt32(array[2]);
							int num2 = Convert.ToInt32(array[3]);
							int num3 = Convert.ToInt32(array[4]);
							int num4 = Convert.ToInt32(array[5]);
							Zone NewZone = new Zone(80, 25);
							NewZone.ZoneID = ZoneID;
							NewZone.BuildTries = num;
							NewZone.Tier = GetZoneTier(ZoneID);
							CachedZones[ZoneID] = NewZone;
							zoneGenerationContextTier = NewZone.NewTier;
							zoneGenerationContextZoneID = ZoneID;
							bool flag2 = false;
							string text2 = "ZONESEED" + ZoneID;
							string text3 = The.Game.GetWorldSeed(text2).ToString();
							if (num > 30 && !Options.DisableTryLimit)
							{
								NewZone.ClearReachableMap(bValue: true);
								MessageQueue.AddPlayerMessage("Zone build failure:" + FailedBuilder, 'R');
							}
							else
							{
								Stat.ReseedFrom(text3 + num);
								MetricsManager.rngCheckpoint(text2 + "startbuild" + num);
								if (ZoneBuilderOverrides.TryGetValue(ZoneID, out var value) && value.Count > 0)
								{
									if (!ApplyBuildersToZone(value, NewZone, text2, flag, out FailedBuilder))
									{
										continue;
									}
								}
								else
								{
									if (ZonePreBuilders.TryGetValue(ZoneID, out value) && !ApplyBuildersToZone(value, NewZone, text2, flag, out FailedBuilder))
									{
										continue;
									}
									GameObject terrainObjectForZone = GetTerrainObjectForZone(wx, wy, text);
									string key = wx + "." + wy;
									if (num4 < 0)
									{
										num4 = 0;
									}
									if (num4 > 49)
									{
										num4 = 49;
									}
									bool flag3 = !HasZoneProperty(ZoneID, "SkipTerrainBuilders");
									Dictionary<string, CellBlueprint> obj = WorldFactory.Factory.getWorld(text)?.CellBlueprintsByApplication;
									object obj2;
									if (obj == null)
									{
										obj2 = null;
									}
									else
									{
										CellBlueprint value2 = obj.GetValue(terrainObjectForZone?.Blueprint);
										obj2 = ((value2 != null) ? value2.LevelBlueprint[num2, num3, num4] : null);
									}
									ZoneBlueprint zoneBlueprint = (ZoneBlueprint)obj2;
									object obj3;
									if (obj == null)
									{
										obj3 = null;
									}
									else
									{
										CellBlueprint value3 = obj.GetValue(key);
										obj3 = ((value3 != null) ? value3.LevelBlueprint[num2, num3, num4] : null);
									}
									ZoneBlueprint zoneBlueprint2 = (ZoneBlueprint)obj3;
									if (flag3)
									{
										if (zoneBlueprint != null)
										{
											if (zoneBlueprint.disableForcedConnections)
											{
												flag2 = true;
											}
											if (zoneBlueprint.GroundLiquid != null)
											{
												NewZone.GroundLiquid = zoneBlueprint.GroundLiquid;
											}
											ApplyMapsToZone(zoneBlueprint.Maps, NewZone, text2);
											if (!ApplyBuildersToZone(zoneBlueprint.Builders, NewZone, text2, flag, out FailedBuilder))
											{
												continue;
											}
										}
										if (zoneBlueprint2 != null)
										{
											if (zoneBlueprint2.disableForcedConnections)
											{
												flag2 = true;
											}
											if (zoneBlueprint2.GroundLiquid != null)
											{
												NewZone.GroundLiquid = zoneBlueprint2.GroundLiquid;
											}
											ApplyMapsToZone(zoneBlueprint2.Maps, NewZone, text2);
											if (!ApplyBuildersToZone(zoneBlueprint2.Builders, NewZone, text2, flag, out FailedBuilder))
											{
												continue;
											}
										}
									}
									if ((ZoneMidBuilders.TryGetValue(ZoneID, out value) && !ApplyBuildersToZone(value, NewZone, text2, flag, out FailedBuilder)) || (flag3 && (!ApplyEncountersToZone(zoneBlueprint?.Encounters, NewZone, text2, flag, out FailedBuilder) || !ApplyEncountersToZone(zoneBlueprint2?.Encounters, NewZone, text2, flag, out FailedBuilder))))
									{
										continue;
									}
									if (ZonePostBuilders.TryGetValue(ZoneID, out value))
									{
										ZoneBuilderBlueprint[] builders = value.Where((ZoneBuilderBlueprint x) => x.Parameters == null || !x.Parameters.ContainsKey("AfterTerrain")).ToArray();
										if (!ApplyBuildersToZone(builders, NewZone, text2, flag, out FailedBuilder))
										{
											continue;
										}
									}
									if (flag3 && (!ApplyBuildersToZone(zoneBlueprint2?.PostBuilders, NewZone, text2, flag, out FailedBuilder) || !ApplyBuildersToZone(zoneBlueprint?.PostBuilders, NewZone, text2, flag, out FailedBuilder)))
									{
										continue;
									}
									if (ZonePostBuilders.TryGetValue(ZoneID, out value))
									{
										ZoneBuilderBlueprint[] builders2 = value.Where((ZoneBuilderBlueprint x) => x.Parameters != null && x.Parameters.ContainsKey("AfterTerrain")).ToArray();
										if (!ApplyBuildersToZone(builders2, NewZone, text2, flag, out FailedBuilder))
										{
											continue;
										}
									}
									if (Options.ShowReachable)
									{
										ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
										for (int i = 0; i < 80; i++)
										{
											for (int j = 0; j < 25; j++)
											{
												scrapBuffer.Goto(i, j);
												if (NewZone.IsReachable(i, j))
												{
													scrapBuffer.Write(".");
												}
												else
												{
													scrapBuffer.Write("#");
												}
											}
										}
										foreach (ZoneConnection zoneConnection in The.Game.ZoneManager.GetZoneConnections(NewZone.ZoneID))
										{
											scrapBuffer.Goto(zoneConnection.X + 1, zoneConnection.Y + 1);
											scrapBuffer.Write(zoneConnection.ToString());
										}
										foreach (CachedZoneConnection item in NewZone.ZoneConnectionCache)
										{
											scrapBuffer.Goto(item.X + 1, item.Y + 1);
											scrapBuffer.Write(item.ToString());
										}
										foreach (ZoneConnection zoneConnection2 in The.Game.ZoneManager.GetZoneConnections(NewZone.ZoneID))
										{
											scrapBuffer.Goto(zoneConnection2.X, zoneConnection2.Y);
											if (!NewZone.IsReachable(zoneConnection2.X, zoneConnection2.Y))
											{
												scrapBuffer.Write("&RX");
											}
											else
											{
												scrapBuffer.Write("&GX");
											}
										}
										foreach (CachedZoneConnection item2 in NewZone.ZoneConnectionCache)
										{
											if (item2.TargetDirection == "-")
											{
												scrapBuffer.Goto(item2.X, item2.Y);
												if (!NewZone.IsReachable(item2.X, item2.Y))
												{
													scrapBuffer.Write("&RX");
												}
												else
												{
													scrapBuffer.Write("&GX");
												}
											}
										}
										Popup._TextConsole.DrawBuffer(scrapBuffer);
										Keyboard.getch();
									}
									flag2 |= NewZone.GetZoneProperty("DisableForcedConnections") == "Yes";
									if (!flag2)
									{
										List<Point2D> list = new List<Point2D>();
										for (int k = 0; k < 80; k++)
										{
											for (int l = 0; l < 24; l++)
											{
												Cell cell = NewZone.GetCell(k, l);
												if (cell.HasObjectWithBlueprint("StairsUp") || cell.HasObjectWithBlueprint("StairsDown"))
												{
													cell.ClearObjectsWithIntProperty("Wall");
													list.Add(cell.Pos2D);
												}
											}
										}
										new ForceConnections()._BuildZone(NewZone, list);
									}
									Coach.StartSection("ConnectionChecks", bTrackGarbage: true);
									if (!flag2)
									{
										foreach (ZoneConnection zoneConnection3 in The.Game.ZoneManager.GetZoneConnections(NewZone.ZoneID))
										{
											if (!NewZone.IsReachable(zoneConnection3.X, zoneConnection3.Y) && !flag)
											{
												FailedBuilder = "Connection ZC:" + zoneConnection3.Type + "," + zoneConnection3.X + "," + zoneConnection3.Y;
												Coach.EndSection();
												goto IL_0019;
											}
										}
										foreach (CachedZoneConnection item3 in NewZone.ZoneConnectionCache)
										{
											if (item3.TargetDirection == "-" && !NewZone.IsReachable(item3.X, item3.Y) && !flag)
											{
												FailedBuilder = "Connection Cached:" + item3.Type + "," + item3.X + "," + item3.Y + " in cell: " + NewZone.GetCell(item3.X, item3.Y).ToString();
												Coach.EndSection();
												goto IL_0019;
											}
										}
									}
									foreach (ZoneConnection item4 in The.Game.ZoneManager.GetZoneConnectionsCopy(NewZone.ZoneID))
									{
										Cell cell2 = NewZone.GetCell(item4.X, item4.Y);
										cell2.ClearObjectsWithIntProperty("Wall");
										if (!string.IsNullOrEmpty(item4.Object) && !cell2.HasObject(item4.Object))
										{
											cell2.AddObject(item4.Object);
										}
									}
								}
								The.Game.ZoneManager.CachedObjectsToRemoveAfterZoneBuild.Clear();
							}
							Coach.EndSection();
							NewZone.Built = true;
							NewZone.WriteZoneConnectionCache();
							if (NewZone.Z <= 10 && !NewZone.HasZoneProperty("inside"))
							{
								NewZone.GetCell(0, 0).AddObject("DaylightWidget");
							}
							BiomeManager.MutateZone(NewZone);
							try
							{
								if (XRLCore.Core.Game.ZoneManager.CachedZones != null)
								{
									List<Point2D> list2 = new List<Point2D>();
									for (int m = 0; m < 80; m++)
									{
										for (int n = 0; n < 24; n++)
										{
											Cell cell3 = NewZone.GetCell(m, n);
											if (cell3.HasObjectWithBlueprint("StairsUp") || cell3.HasObjectWithBlueprint("StairsDown"))
											{
												cell3.ClearObjectsWithIntProperty("Wall");
												list2.Add(cell3.Pos2D);
											}
										}
									}
									string zoneIDFromDirection = NewZone.GetZoneIDFromDirection("U");
									if (zoneIDFromDirection != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection))
									{
										Zone zone = The.Game.ZoneManager.CachedZones[zoneIDFromDirection];
										for (int num5 = 0; num5 < 80; num5++)
										{
											for (int num6 = 0; num6 < 24; num6++)
											{
												Cell cell4 = zone.GetCell(num5, num6);
												if (cell4.HasObjectWithBlueprint("OpenShaft"))
												{
													NewZone.GetCell(num5, num6).ClearObjectsWithIntProperty("Wall");
													list2.Add(NewZone.GetCell(num5, num6).Pos2D);
												}
												if (cell4.HasObjectWithBlueprint("StairsDown"))
												{
													NewZone.GetCell(num5, num6).ClearObjectsWithIntProperty("Wall");
													NewZone.GetCell(num5, num6).AddObject("StairsUp");
													list2.Add(NewZone.GetCell(num5, num6).Pos2D);
												}
											}
										}
									}
									string zoneIDFromDirection2 = NewZone.GetZoneIDFromDirection("D");
									if (zoneIDFromDirection2 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection2))
									{
										Zone zone2 = The.Game.ZoneManager.CachedZones[zoneIDFromDirection2];
										for (int num7 = 0; num7 < 80; num7++)
										{
											for (int num8 = 0; num8 < 24; num8++)
											{
												if (zone2.GetCell(num7, num8).HasObjectWithBlueprint("StairsUp"))
												{
													NewZone.GetCell(num7, num8).ClearObjectsWithIntProperty("Wall");
													NewZone.GetCell(num7, num8).AddObject("StairsDown");
													list2.Add(NewZone.GetCell(num7, num8).Pos2D);
												}
											}
										}
									}
									string zoneIDFromDirection3 = NewZone.GetZoneIDFromDirection("N");
									if (zoneIDFromDirection3 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection3))
									{
										for (int num9 = 0; num9 < 80; num9++)
										{
											if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection3].GetCell(num9, 24).IsSolid())
											{
												NewZone.GetCell(num9, 0).ClearObjectsWithIntProperty("Wall");
											}
										}
									}
									string zoneIDFromDirection4 = NewZone.GetZoneIDFromDirection("S");
									if (zoneIDFromDirection4 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection4))
									{
										for (int num10 = 0; num10 < 80; num10++)
										{
											if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection4].GetCell(num10, 0).IsSolid())
											{
												NewZone.GetCell(num10, 24).ClearObjectsWithIntProperty("Wall");
											}
										}
									}
									string zoneIDFromDirection5 = NewZone.GetZoneIDFromDirection("E");
									if (zoneIDFromDirection5 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection5))
									{
										for (int num11 = 0; num11 < 24; num11++)
										{
											if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection5].GetCell(0, num11).IsSolid())
											{
												NewZone.GetCell(79, num11).ClearObjectsWithIntProperty("Wall");
											}
										}
									}
									string zoneIDFromDirection6 = NewZone.GetZoneIDFromDirection("W");
									if (zoneIDFromDirection6 != null && XRLCore.Core.Game.ZoneManager.CachedZones.ContainsKey(zoneIDFromDirection6))
									{
										for (int num12 = 0; num12 < 24; num12++)
										{
											if (!XRLCore.Core.Game.ZoneManager.CachedZones[zoneIDFromDirection6].GetCell(0, num12).IsSolid())
											{
												NewZone.GetCell(0, num12).ClearObjectsWithIntProperty("Wall");
											}
										}
									}
									try
									{
										if (!flag2 && NewZone.GetZoneProperty("DisableForcedConnections") != "Yes")
										{
											new ForceConnections()._BuildZone(NewZone, list2);
										}
									}
									catch (Exception x2)
									{
										MetricsManager.LogException("Exception connecting stairs", x2);
									}
								}
							}
							catch
							{
							}
							SanityCheck(NewZone);
							NewZone.GeneratedOn = The.Game.Turns;
							NewZone.HandleEvent(eBeforeZoneBuilt);
							NewZone.HandleEvent(eZoneBuilt);
							PaintWalls(NewZone);
							PaintWater(NewZone);
							NewZone.HandleEvent(eAfterZoneBuilt);
							ForceCollect();
							The.Game.Systems.ForEach(delegate(IGameSystem s)
							{
								s.NewZoneGenerated(NewZone);
							});
							break;
						}
						The.Game.Systems.ForEach(delegate(IGameSystem s)
						{
							s.NewZoneGenerated(result);
						});
					}
					return;
				}
				Coach.EndSection();
			}
			catch (Exception ex)
			{
				if (Popup.ShowYesNo("There was an issue building this zone. Automatically report it to us? " + ex.ToString()) == DialogResult.Yes)
				{
					MetricsManager.LogException("Zone build", ex);
				}
				Zone value4 = new Zone(80, 25)
				{
					ZoneID = ZoneID,
					BuildTries = 0,
					Tier = 0,
					GeneratedOn = Ticker
				};
				if (!CachedZones.ContainsKey(ZoneID))
				{
					CachedZones.Add(ZoneID, value4);
				}
			}
		});
		Event.ResetToPin();
	}

	public static void SanityCheck(Zone Z)
	{
		try
		{
			for (int i = 0; i < Z.Width; i++)
			{
				for (int j = 0; j < Z.Height; j++)
				{
					Cell cell = Z.Map[i][j];
					if (cell.CountObjectsWithTag("Stairs") <= 1)
					{
						continue;
					}
					foreach (GameObject item in cell.GetObjectsWithTag("Stairs"))
					{
						if (item.GetPart("StairsDown") is XRL.World.Parts.StairsDown stairsDown && stairsDown.PullDown)
						{
							item.Obliterate();
						}
					}
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SanityCheck::NoStairsOnShafts", x);
		}
		try
		{
			for (int k = 0; k < Z.Width; k++)
			{
				for (int l = 0; l < Z.Height; l++)
				{
					Cell cell2 = Z.Map[k][l];
					if (!cell2.HasWall() || !cell2.HasCombatObject())
					{
						continue;
					}
					foreach (GameObject item2 in cell2.GetObjectsWithPart("Combat"))
					{
						if (item2.pBrain != null && !item2.pBrain.LivesOnWalls && !item2.HasTagOrProperty("IgnoreWallSanityCheck"))
						{
							Cell closestPassableCellFor = cell2.getClosestPassableCellFor(item2);
							item2.DirectMoveTo(closestPassableCellFor);
						}
					}
				}
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("SanityCheck::NoMonstersInWalls", x2);
		}
		try
		{
			for (int m = 0; m < Z.Width; m++)
			{
				for (int n = 0; n < Z.Height; n++)
				{
					Cell cell3 = Z.Map[m][n];
					if (!cell3.HasStairs())
					{
						continue;
					}
					cell3.ClearWalls();
					if (!cell3.IsSolid())
					{
						continue;
					}
					foreach (GameObject solidObject in cell3.GetSolidObjects())
					{
						if (solidObject.CanClear())
						{
							solidObject.Obliterate();
						}
					}
				}
			}
		}
		catch (Exception x3)
		{
			MetricsManager.LogException("SanityCheck::NoStairsOnSolids", x3);
		}
	}

	public void PaintWalls()
	{
		PaintWalls(ActiveZone);
	}

	public static void PaintWalls(Zone Z, int x1 = 0, int y1 = 0, int x2 = -1, int y2 = -1)
	{
		if (Z == null)
		{
			return;
		}
		if (x2 == -1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 == -1)
		{
			y2 = Z.Height - 1;
		}
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 > Z.Width - 1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 > Z.Height - 1)
		{
			y2 = Z.Height - 1;
		}
		Array.Clear(WallSingleTrack, 0, WallSingleTrack.Length);
		Array.Clear(WallMultiTrack, 0, WallMultiTrack.Length);
		string text = null;
		List<string> list = null;
		for (int i = x1 - 1; i <= x2 + 1; i++)
		{
			for (int j = y1 - 1; j <= y2 + 1; j++)
			{
				if (i < 0 || j < 0 || i > Z.Width - 1 || j > Z.Height - 1)
				{
					continue;
				}
				Cell cell = Z.GetCell(i, j);
				if (cell == null)
				{
					continue;
				}
				int objectCountWithTagsOrProperties = cell.GetObjectCountWithTagsOrProperties("PaintedWall", "PaintedFence", "PaintWith", CheckPaintabilityEvent.Check);
				if (objectCountWithTagsOrProperties == 0)
				{
					WallSingleTrack[i, j] = null;
					WallMultiTrack[i, j] = null;
					continue;
				}
				if (objectCountWithTagsOrProperties > 1)
				{
					WallSingleTrack[i, j] = null;
					List<GameObject> list2 = new List<GameObject>(objectCountWithTagsOrProperties);
					cell.GetObjectsWithTagsOrProperties("PaintedWall", "PaintedFence", "PaintWith", CheckPaintabilityEvent.Check, list2);
					WallMultiTrack[i, j] = list2;
					foreach (GameObject item in list2)
					{
						string propertyOrTag = item.GetPropertyOrTag("PaintPart");
						if (propertyOrTag != null && propertyOrTag != text)
						{
							if (list == null)
							{
								list = new List<string>(2) { text, propertyOrTag };
							}
							else if (!list.Contains(propertyOrTag))
							{
								list.Add(propertyOrTag);
							}
						}
					}
					continue;
				}
				GameObject firstObjectWithTagsOrProperties = cell.GetFirstObjectWithTagsOrProperties("PaintedWall", "PaintedFence", "PaintWith", CheckPaintabilityEvent.Check);
				WallSingleTrack[i, j] = firstObjectWithTagsOrProperties;
				WallMultiTrack[i, j] = null;
				string propertyOrTag2 = firstObjectWithTagsOrProperties.GetPropertyOrTag("PaintPart");
				if (propertyOrTag2 != null && propertyOrTag2 != text)
				{
					if (list == null)
					{
						list = new List<string>(2) { text, propertyOrTag2 };
					}
					else if (!list.Contains(propertyOrTag2))
					{
						list.Add(propertyOrTag2);
					}
				}
			}
		}
		if (list != null)
		{
			for (int k = x1 - 1; k <= x2 + 1; k++)
			{
				for (int l = y1 - 1; l <= y2 + 1; l++)
				{
					if (k < 0 || l < 0 || k > Z.Width - 1 || l > Z.Height - 1)
					{
						continue;
					}
					Cell cell2 = Z.GetCell(k, l);
					if (cell2 == null)
					{
						continue;
					}
					int objectCountWithPart = cell2.GetObjectCountWithPart(list);
					if (objectCountWithPart > 1)
					{
						List<GameObject> list3 = new List<GameObject>(objectCountWithPart);
						cell2.GetObjectsWithPart(text, list3);
						if (WallMultiTrack[k, l] != null)
						{
							WallMultiTrack[k, l].AddRange(list3);
						}
						else if (WallSingleTrack[k, l] != null)
						{
							WallMultiTrack[k, l] = new List<GameObject>(objectCountWithPart + 1) { WallSingleTrack[k, l] };
							WallMultiTrack[k, l].AddRange(list3);
							WallSingleTrack[k, l] = null;
						}
						else
						{
							WallMultiTrack[k, l] = list3;
						}
					}
					else if (objectCountWithPart == 1)
					{
						GameObject firstObjectWithPart = cell2.GetFirstObjectWithPart(list);
						if (WallMultiTrack[k, l] != null)
						{
							WallMultiTrack[k, l].Add(firstObjectWithPart);
						}
						else if (WallSingleTrack[k, l] != null)
						{
							WallMultiTrack[k, l] = new List<GameObject>(2)
							{
								WallSingleTrack[k, l],
								firstObjectWithPart
							};
							WallSingleTrack[k, l] = null;
						}
						else
						{
							WallSingleTrack[k, l] = firstObjectWithPart;
						}
					}
				}
			}
		}
		else if (text != null)
		{
			for (int m = x1 - 1; m <= x2 + 1; m++)
			{
				for (int n = y1 - 1; n <= y2 + 1; n++)
				{
					if (m < 0 || n < 0 || m > Z.Width - 1 || n > Z.Height - 1)
					{
						continue;
					}
					Cell cell3 = Z.GetCell(m, n);
					if (cell3 == null)
					{
						continue;
					}
					int objectCountWithPart2 = cell3.GetObjectCountWithPart(text);
					if (objectCountWithPart2 > 1)
					{
						List<GameObject> list4 = new List<GameObject>(objectCountWithPart2);
						cell3.GetObjectsWithPart(text, list4);
						if (WallMultiTrack[m, n] != null)
						{
							WallMultiTrack[m, n].AddRange(list4);
						}
						else if (WallSingleTrack[m, n] != null)
						{
							WallMultiTrack[m, n] = new List<GameObject>(objectCountWithPart2 + 1) { WallSingleTrack[m, n] };
							WallMultiTrack[m, n].AddRange(list4);
							WallSingleTrack[m, n] = null;
						}
						else
						{
							WallMultiTrack[m, n] = list4;
						}
					}
					else if (objectCountWithPart2 == 1)
					{
						GameObject firstObjectWithPart2 = cell3.GetFirstObjectWithPart(text);
						if (WallMultiTrack[m, n] != null)
						{
							WallMultiTrack[m, n].Add(firstObjectWithPart2);
						}
						else if (WallSingleTrack[m, n] != null)
						{
							WallMultiTrack[m, n] = new List<GameObject>(2)
							{
								WallSingleTrack[m, n],
								firstObjectWithPart2
							};
							WallSingleTrack[m, n] = null;
						}
						else
						{
							WallSingleTrack[m, n] = firstObjectWithPart2;
						}
					}
				}
			}
		}
		StringBuilder f = Event.NewStringBuilder();
		StringBuilder s = Event.NewStringBuilder();
		StringBuilder b = Event.NewStringBuilder();
		for (int num = x1; num <= x2; num++)
		{
			for (int num2 = y1; num2 <= y2; num2++)
			{
				GameObject gameObject = WallSingleTrack[num, num2];
				if (gameObject != null)
				{
					PaintWall(gameObject, num, num2, f, s, b, WallSingleTrack, WallMultiTrack);
				}
				List<GameObject> list5 = WallMultiTrack[num, num2];
				if (list5 == null)
				{
					continue;
				}
				foreach (GameObject item2 in list5)
				{
					PaintWall(item2, num, num2, f, s, b, WallSingleTrack, WallMultiTrack);
				}
			}
		}
	}

	private static void PaintWall(GameObject obj, int x, int y, StringBuilder f, StringBuilder s, StringBuilder b, GameObject[,] SingleTrack, List<GameObject>[,] MultiTrack)
	{
		string tagOrStringProperty = obj.GetTagOrStringProperty("PaintedWall");
		string tagOrStringProperty2 = obj.GetTagOrStringProperty("PaintedFence");
		if ((!string.IsNullOrEmpty(tagOrStringProperty) || !string.IsNullOrEmpty(tagOrStringProperty2)) && CheckTileChangeEvent.Check(obj))
		{
			string tagOrStringProperty3 = obj.GetTagOrStringProperty("PaintPart");
			string tagOrStringProperty4 = obj.GetTagOrStringProperty("PaintWith");
			s.Length = 0;
			s.Append('-');
			s.Append(HasWallInDirection(obj, x, y, "N", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			s.Append(HasWallInDirection(obj, x, y, "NE", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			s.Append(HasWallInDirection(obj, x, y, "E", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			s.Append(HasWallInDirection(obj, x, y, "SE", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			s.Append(HasWallInDirection(obj, x, y, "S", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			s.Append(HasWallInDirection(obj, x, y, "SW", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			s.Append(HasWallInDirection(obj, x, y, "W", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			s.Append(HasWallInDirection(obj, x, y, "NW", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4) ? "1" : "0");
			f.Length = 0;
			f.Append('_');
			if (HasWallInDirection(obj, x, y, "N", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
			{
				f.Append('n');
			}
			if (HasWallInDirection(obj, x, y, "S", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
			{
				f.Append('s');
			}
			if (HasWallInDirection(obj, x, y, "E", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
			{
				f.Append('e');
			}
			if (HasWallInDirection(obj, x, y, "W", SingleTrack, MultiTrack, tagOrStringProperty3, tagOrStringProperty4))
			{
				f.Append('w');
			}
			bool flag = false;
			if (!string.IsNullOrEmpty(tagOrStringProperty))
			{
				b.Length = 0;
				b.Append(obj.GetTagOrStringProperty("PaintedWallAtlas", "Assets_Content_Textures_Tiles_")).Append(tagOrStringProperty.Contains(",") ? tagOrStringProperty.CachedCommaExpansion().GetRandomElement() : tagOrStringProperty).Append(s)
					.Append(obj.GetTagOrStringProperty("PaintedWallExtension", ".bmp"));
				obj.pRender.Tile = b.ToString();
				flag = true;
			}
			if (!string.IsNullOrEmpty(tagOrStringProperty2))
			{
				b.Length = 0;
				b.Append(obj.GetTagOrStringProperty("PaintedFenceAtlas", "Assets_Content_Textures_Tiles_")).Append(tagOrStringProperty2.Contains(",") ? tagOrStringProperty2.CachedCommaExpansion().GetRandomElement() : tagOrStringProperty2).Append(f)
					.Append(obj.GetTagOrStringProperty("PaintedFenceExtension", ".bmp"));
				obj.pRender.Tile = b.ToString();
				flag = true;
			}
			if (flag)
			{
				RepaintedEvent.Send(obj);
			}
		}
	}

	public static bool HasWallInDirection(GameObject obj, int x, int y, string D, GameObject[,] SingleTrack, List<GameObject>[,] MultiTrack, string PaintPartName, string PaintWith, bool bEdgeFlag = false)
	{
		if (PaintWith == "!PitVoid")
		{
			return !obj.CurrentCell.GetCellFromDirection(D).HasObject((GameObject o) => o.GetStringProperty("PaintWith") == "PitVoid");
		}
		bool flag = _HasWallInDirection(obj, x, y, D, SingleTrack, MultiTrack, PaintPartName, PaintWith, bEdgeFlag);
		if (obj.HasTag("PaintedWallInvert"))
		{
			return !flag;
		}
		return flag;
	}

	public static bool _HasWallInDirection(GameObject obj, int x, int y, string D, GameObject[,] SingleTrack, List<GameObject>[,] MultiTrack, string PaintPartName, string PaintWith, bool bEdgeFlag = false)
	{
		if (D.Contains("N"))
		{
			y--;
		}
		else if (D.Contains("S"))
		{
			y++;
		}
		if (D.Contains("E"))
		{
			x++;
		}
		else if (D.Contains("W"))
		{
			x--;
		}
		if (y < 0)
		{
			return bEdgeFlag;
		}
		if (x < 0)
		{
			return bEdgeFlag;
		}
		if (x > SingleTrack.GetUpperBound(0))
		{
			return bEdgeFlag;
		}
		if (y > SingleTrack.GetUpperBound(1))
		{
			return bEdgeFlag;
		}
		GameObject gameObject = SingleTrack[x, y];
		if (gameObject != null)
		{
			if (obj.Blueprint == gameObject.Blueprint)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(PaintPartName) && gameObject.HasPart(PaintPartName))
			{
				return true;
			}
			string tagOrStringProperty = gameObject.GetTagOrStringProperty("PaintWith");
			if (tagOrStringProperty == "*")
			{
				return true;
			}
			if (!string.IsNullOrEmpty(PaintWith) && tagOrStringProperty == PaintWith)
			{
				return true;
			}
		}
		List<GameObject> list = MultiTrack[x, y];
		if (list != null)
		{
			foreach (GameObject item in list)
			{
				if (obj.Blueprint == item.Blueprint)
				{
					return true;
				}
			}
			if (!string.IsNullOrEmpty(PaintPartName))
			{
				foreach (GameObject item2 in list)
				{
					if (item2.HasPart(PaintPartName))
					{
						return true;
					}
				}
			}
			if (!string.IsNullOrEmpty(PaintWith))
			{
				foreach (GameObject item3 in list)
				{
					string tagOrStringProperty2 = item3.GetTagOrStringProperty("PaintWith");
					if (tagOrStringProperty2 == "*")
					{
						return true;
					}
					if (!string.IsNullOrEmpty(PaintWith) && tagOrStringProperty2 == PaintWith)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static void PaintWater(Zone Z, int x1 = 0, int y1 = 0, int x2 = -1, int y2 = -1)
	{
		if (Z == null)
		{
			return;
		}
		if (x2 == -1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 == -1)
		{
			y2 = Z.Height - 1;
		}
		if (x1 < 0)
		{
			x1 = 0;
		}
		if (y1 < 0)
		{
			y1 = 0;
		}
		if (x2 > Z.Width - 1)
		{
			x2 = Z.Width - 1;
		}
		if (y2 > Z.Height - 1)
		{
			y2 = Z.Height - 1;
		}
		Array.Clear(LiquidTrack, 0, LiquidTrack.Length);
		int num = 0;
		for (int i = x1 - 1; i <= x2 + 1; i++)
		{
			for (int j = y1 - 1; j <= y2 + 1; j++)
			{
				if (i >= 0 && j >= 0 && x2 <= Z.Width - 1 && y2 <= Z.Height - 1)
				{
					Cell cell = Z.GetCell(i, j);
					if (cell != null)
					{
						LiquidTrack[i, j] = cell.GetFirstObjectWithTag("PaintedLiquidAtlas", CheckPaintabilityEvent.Check);
						num++;
					}
				}
			}
		}
		if (num <= 0)
		{
			return;
		}
		int num2 = 0;
		for (int k = x1; k <= x2; k++)
		{
			for (int l = y1; l <= y2; l++)
			{
				GameObject gameObject = LiquidTrack[k, l];
				if (gameObject == null || !CheckTileChangeEvent.Check(gameObject))
				{
					continue;
				}
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				BaseLiquid primaryLiquid = liquidVolume.GetPrimaryLiquid();
				if (primaryLiquid == null)
				{
					continue;
				}
				int paintGroup = primaryLiquid.GetPaintGroup(liquidVolume);
				int num3 = (LiquidInDirection(k, l, paintGroup, "N", LiquidTrack, 1) << 7) | (LiquidInDirection(k, l, paintGroup, "E", LiquidTrack, 1) << 5) | (LiquidInDirection(k, l, paintGroup, "S", LiquidTrack, 1) << 3) | (LiquidInDirection(k, l, paintGroup, "W", LiquidTrack, 1) << 1);
				if (liquidVolume.IsWadingDepth())
				{
					if (num3.HasAllBits(160))
					{
						num3 |= LiquidInDirection(k, l, paintGroup, "NE", LiquidTrack, 1) << 6;
					}
					if (num3.HasAllBits(40))
					{
						num3 |= LiquidInDirection(k, l, paintGroup, "SE", LiquidTrack, 1) << 4;
					}
					if (num3.HasAllBits(10))
					{
						num3 |= LiquidInDirection(k, l, paintGroup, "SW", LiquidTrack, 1) << 2;
					}
					if (num3.HasAllBits(130))
					{
						num3 |= LiquidInDirection(k, l, paintGroup, "NW", LiquidTrack, 1);
					}
				}
				liquidVolume.Paint(num3);
				if (++num2 >= num)
				{
					return;
				}
			}
		}
	}

	private static int LiquidInDirection(int x, int y, int group, string D, GameObject[,] Track, int EdgeValue = 0)
	{
		if (D.Contains("N"))
		{
			y--;
		}
		else if (D.Contains("S"))
		{
			y++;
		}
		if (D.Contains("E"))
		{
			x++;
		}
		else if (D.Contains("W"))
		{
			x--;
		}
		if (y < 0)
		{
			return EdgeValue;
		}
		if (x < 0)
		{
			return EdgeValue;
		}
		if (x > Track.GetUpperBound(0))
		{
			return EdgeValue;
		}
		if (y > Track.GetUpperBound(1))
		{
			return EdgeValue;
		}
		GameObject gameObject = Track[x, y];
		if (gameObject != null)
		{
			if (!gameObject.LiquidVolume.CanPaintWith(group))
			{
				return 0;
			}
			return 1;
		}
		return 0;
	}

	private static bool ApplyEncounterObjectsToZone(List<GameObject> Objects, string Density, Zone NewZone)
	{
		foreach (GameObject Object in Objects)
		{
			ZoneBuilderSandbox.PlaceObject(Object, NewZone);
		}
		return true;
	}

	private bool ApplyEncounterBuilderObjectToZone(string sBuilder, Zone NewZone)
	{
		Type type = ModManager.ResolveType("XRL.World.Encounters.EncounterBuilders." + sBuilder);
		if (type == null)
		{
			return true;
		}
		object obj = Activator.CreateInstance(type);
		MethodInfo method = type.GetMethod("BuildEncounter");
		if (method != null && !(bool)method.Invoke(obj, new object[1] { NewZone }))
		{
			return false;
		}
		return true;
	}

	private bool ApplyEncounterObjectBuilderToObject(string sBuilder, GameObject GO)
	{
		Type type = ModManager.ResolveType("XRL.World.Encounters.EncounterObjectBuilders." + sBuilder);
		if (type == null)
		{
			return true;
		}
		object obj = Activator.CreateInstance(type);
		MethodInfo method = type.GetMethod("BuildObject");
		if (method != null && !(bool)method.Invoke(obj, new object[2] { GO, null }))
		{
			return false;
		}
		return true;
	}

	public static bool ApplyEncounterToZone(Encounter Encounter, Zone NewZone)
	{
		foreach (string zoneBuilder in Encounter.ZoneBuilders)
		{
			ApplyBuilderToZone(zoneBuilder, NewZone);
		}
		ApplyEncounterObjectsToZone(Encounter.Objects, Encounter.Density, NewZone);
		foreach (Encounter subEncounter in Encounter.SubEncounters)
		{
			ApplyEncounterToZone(subEncounter, NewZone);
		}
		return true;
	}

	public static bool ApplyEncounterBlueprintToZone(ZoneEncounterBlueprint EncounterBlueprint, Zone NewZone)
	{
		ApplyEncounterToZone(EncounterFactory.Factory.CreateEncounter(EncounterBlueprint, NewZone.NewTier), NewZone);
		return true;
	}

	public void ProcessGoToPartyLeader()
	{
		int num = 0;
		bool flag = true;
		while (flag && ++num < 100)
		{
			flag = false;
			int count = CachedZones.Count;
			foreach (Zone value in CachedZones.Values)
			{
				for (int i = 0; i < value.Height; i++)
				{
					for (int j = 0; j < value.Width; j++)
					{
						Cell cell = value.GetCell(j, i);
						int k = 0;
						for (int num2 = cell.Objects.Count; k < num2; k++)
						{
							if (cell.Objects[k].GoToPartyLeader())
							{
								flag = true;
								k--;
								num2--;
								if (num2 > cell.Objects.Count)
								{
									num2 = cell.Objects.Count;
								}
							}
						}
					}
				}
				if (CachedZones.Count != count)
				{
					break;
				}
			}
		}
	}

	public static bool ApplyBuilderToZone(string Builder, Zone NewZone)
	{
		string text = "XRL.World.ZoneBuilders." + Builder;
		Type type = ModManager.ResolveType(text);
		if (type == null)
		{
			MetricsManager.LogError("Unknown builder " + text + "!");
			return true;
		}
		object obj = Activator.CreateInstance(type);
		MethodInfo method = type.GetMethod("BuildZone");
		if (method != null && !(bool)method.Invoke(obj, new object[1] { NewZone }))
		{
			return false;
		}
		return true;
	}

	private bool ApplyBuilderToZone(ZoneBuilderBlueprint Builder, Zone NewZone)
	{
		if (Builder == null || string.IsNullOrEmpty(Builder.Class))
		{
			if (Builder == null)
			{
				MetricsManager.LogError("null Blueprint" + Builder);
			}
			else
			{
				MetricsManager.LogError("null Blueprint class" + Builder.Class);
			}
			return true;
		}
		if (Builder.Class.StartsWith("ZoneTemplate:"))
		{
			try
			{
				ZoneTemplateManager.Templates[Builder.Class.Split(':')[1]].Execute(NewZone);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Applying zone template: " + Builder.Class, x);
			}
			return true;
		}
		string text = "XRL.World.ZoneBuilders." + Builder.Class;
		Type type = ModManager.ResolveType(text);
		if (type == null)
		{
			MetricsManager.LogError("Unknown builder " + text + "!");
			return true;
		}
		object obj = Activator.CreateInstance(type);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (Builder.Parameters != null && Builder.Parameters.ContainsKey(fieldInfo.Name))
			{
				if (fieldInfo.FieldType == typeof(bool))
				{
					fieldInfo.SetValue(obj, Convert.ToBoolean(Builder.Parameters[fieldInfo.Name]));
				}
				else if (fieldInfo.FieldType == typeof(int))
				{
					fieldInfo.SetValue(obj, Convert.ToInt32(Builder.Parameters[fieldInfo.Name]));
				}
				else if (fieldInfo.FieldType == typeof(short))
				{
					fieldInfo.SetValue(obj, Convert.ToInt16(Builder.Parameters[fieldInfo.Name]));
				}
				else
				{
					fieldInfo.SetValue(obj, Builder.Parameters[fieldInfo.Name]);
				}
			}
		}
		MethodInfo method = type.GetMethod("BuildZone");
		if (method != null)
		{
			object[] parameters = new object[1] { NewZone };
			try
			{
				if (!(bool)method.Invoke(obj, parameters))
				{
					return false;
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogError("exception running builder " + Builder.Class, x2);
				return false;
			}
		}
		return true;
	}

	private bool ApplyBuildersToZone(IList<ZoneBuilderBlueprint> Builders, Zone Z, string Seed, bool Force, out string FailedBuilder)
	{
		if (!Builders.IsNullOrEmpty())
		{
			for (int i = 0; i < Builders.Count; i++)
			{
				ZoneBuilderBlueprint zoneBuilderBlueprint = Builders[i];
				string text = Seed + zoneBuilderBlueprint?.ToString() + i;
				MetricsManager.rngCheckpoint(text);
				Stat.ReseedFrom(text);
				Coach.StartSection(zoneBuilderBlueprint.ToString(), bTrackGarbage: true);
				if (!ApplyBuilderToZone(zoneBuilderBlueprint, Z) && !Force)
				{
					FailedBuilder = zoneBuilderBlueprint.ToString();
					Coach.EndSection();
					return false;
				}
				Coach.EndSection();
			}
		}
		FailedBuilder = null;
		return true;
	}

	private bool ApplyEncountersToZone(IList<ZoneEncounterBlueprint> Encounters, Zone Z, string Seed, bool Force, out string FailedBuilder)
	{
		if (!Encounters.IsNullOrEmpty())
		{
			for (int i = 0; i < Encounters.Count; i++)
			{
				ZoneEncounterBlueprint zoneEncounterBlueprint = Encounters[i];
				string text = Seed + zoneEncounterBlueprint?.ToString() + i;
				MetricsManager.rngCheckpoint(text);
				Stat.ReseedFrom(text);
				Coach.StartSection(zoneEncounterBlueprint.ToString(), bTrackGarbage: true);
				if (!ApplyEncounterBlueprintToZone(zoneEncounterBlueprint, Z) && !Force)
				{
					FailedBuilder = zoneEncounterBlueprint.ToString();
					Coach.EndSection();
					return false;
				}
				Coach.EndSection();
			}
		}
		FailedBuilder = null;
		return true;
	}

	private void ApplyMapsToZone(IList<ZoneMapBlueprint> Maps, Zone Z, string Seed)
	{
		if (!Maps.IsNullOrEmpty())
		{
			for (int i = 0; i < Maps.Count; i++)
			{
				ZoneMapBlueprint zoneMapBlueprint = Maps[i];
				string text = Seed + zoneMapBlueprint?.ToString() + i;
				MetricsManager.rngCheckpoint(text);
				Stat.ReseedFrom(text);
				Coach.StartSection(zoneMapBlueprint.ToString(), bTrackGarbage: true);
				MapBuilder.BuildFromFile(Z, zoneMapBlueprint.File);
				Coach.EndSection();
			}
		}
	}

	public bool WantEvent(int ID, int cascade)
	{
		if (ActiveZone != null && ActiveZone.WantEvent(ID, cascade))
		{
			return true;
		}
		foreach (KeyValuePair<string, Zone> cachedZone in CachedZones)
		{
			if (cachedZone.Value != ActiveZone && cachedZone.Value.WantEvent(ID, cascade))
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleEvent<T>(T E) where T : MinEvent
	{
		if (ActiveZone != null && !ActiveZone.HandleEvent(E))
		{
			return false;
		}
		foreach (KeyValuePair<string, Zone> cachedZone in CachedZones)
		{
			if (cachedZone.Value != ActiveZone && !cachedZone.Value.HandleEvent(E))
			{
				return false;
			}
		}
		return true;
	}
}
