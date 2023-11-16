using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SpaceTimeVortex : IPart
{
	public string DestinationZoneID;

	[NonSerialized]
	private static List<string> Summonable;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeingConsumedEvent.ID && ID != BlocksRadarEvent.ID && ID != CanBeInvoluntarilyMovedEvent.ID && ID != EnteredCellEvent.ID && ID != EndTurnEvent.ID && ID != InterruptAutowalkEvent.ID && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == RealityStabilizeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			ParentObject.ParticleBlip("&K-");
			DidX("collapse", "under the pressure of normality", null, null, null, ParentObject);
			ParentObject.Destroy();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!E.Cell.IsGraveyard())
		{
			ApplyVortex(E.Object);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		return false;
	}

	public void CheckDestinationZone()
	{
		if (string.IsNullOrEmpty(DestinationZoneID))
		{
			string world = ParentObject.CurrentZone?.GetZoneWorld();
			DestinationZoneID = GetRandomDestinationZoneID(world);
		}
	}

	public static Cell GetDestinationCellFor(string ZoneID, GameObject Target, Cell Origin = null)
	{
		if (ZoneID != null)
		{
			Zone zone = The.ZoneManager.GetZone(ZoneID);
			if (zone != null)
			{
				Cell cell = Origin ?? Target.CurrentCell;
				Cell cell2 = zone.GetCell(cell.X, cell.Y);
				if (cell2 != null && !cell2.IsPassable(Target))
				{
					cell2 = cell2.getClosestPassableCellFor(Target);
				}
				return cell2;
			}
		}
		return Target.GetRandomTeleportTargetCell();
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!E.Cell.IsGraveyard())
		{
			CheckDestinationZone();
			foreach (GameObject item in E.Cell.GetObjectsWithPartReadonly("Render"))
			{
				if (!ParentObject.IsValid() || ParentObject.IsInGraveyard() || ParentObject.CurrentCell != E.Cell)
				{
					break;
				}
				ApplyVortex(item);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeingConsumedEvent E)
	{
		ApplyVortex(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		SpaceTimeAnomalyPeriodicEvents();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BlocksRadarEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DefendMeleeHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefendMeleeHit")
		{
			ApplyVortex(E.GetGameObjectParameter("Attacker"));
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Stat.RandomCosmetic(1, 60) < 3)
		{
			string text = "&C";
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&W";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&R";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&B";
			}
			Cell cell = ParentObject.CurrentCell;
			XRLCore.ParticleManager.AddRadial(text + "ù", cell.X, cell.Y, Stat.RandomCosmetic(0, 7), Stat.RandomCosmetic(5, 10), 0.01f * (float)Stat.RandomCosmetic(4, 6), -0.01f * (float)Stat.RandomCosmetic(3, 7));
		}
		switch (Stat.RandomCosmetic(0, 4))
		{
		case 0:
			E.ColorString = "&B^k";
			break;
		case 1:
			E.ColorString = "&R^k";
			break;
		case 2:
			E.ColorString = "&C^k";
			break;
		case 3:
			E.ColorString = "&W^k";
			break;
		case 4:
			E.ColorString = "&K^k";
			break;
		}
		switch (Stat.RandomCosmetic(0, 3))
		{
		case 0:
			E.RenderString = "\t";
			break;
		case 1:
			E.RenderString = "é";
			break;
		case 2:
			E.RenderString = "\u0015";
			break;
		case 3:
			E.RenderString = "\u000f";
			break;
		}
		return true;
	}

	public static string GetRandomDestinationZoneID(string World)
	{
		if (World != "JoppaWorld")
		{
			return null;
		}
		string text;
		while (true)
		{
			int parasangX = Stat.Random(0, 79);
			int parasangY = Stat.Random(0, 24);
			int zoneX = Stat.Random(0, 2);
			int zoneY = Stat.Random(0, 2);
			int zoneZ = (50.in100() ? Stat.Random(10, 40) : 10);
			text = ZoneID.Assemble(World, parasangX, parasangY, zoneX, zoneY, zoneZ);
			Zone zone = The.ZoneManager.GetZone(text);
			if (zone.GetEmptyCellCount() < 100)
			{
				continue;
			}
			Cell cell = null;
			int num = 0;
			while (++num < 5)
			{
				Cell randomCell = zone.GetRandomCell(3 - num / 25);
				if (randomCell.IsReachable() && !randomCell.IsSolid())
				{
					cell = randomCell;
					break;
				}
			}
			if (cell != null)
			{
				break;
			}
		}
		return text;
	}

	public static bool IsValidTarget(GameObject Object)
	{
		if (Object?.pRender != null && Object.pRender.RenderLayer != 0 && !Object.HasTag("Nullphase") && !Object.HasTagOrProperty("ExcavatoryTerrainFeature"))
		{
			return !Object.HasTagOrProperty("IgnoreSpaceTimeVortex");
		}
		return false;
	}

	public static bool Teleport(GameObject Object, Cell C, GameObject Device)
	{
		if (Object.CurrentZone != C.ParentZone)
		{
			if (Object.IsPlayer() || Object.WasPlayer())
			{
				if (Object.GetEffect("Lost") is Lost lost)
				{
					lost.DisableUnlost = false;
				}
				else if (C.ParentZone.Z == 10)
				{
					Object.ApplyEffect(new Lost(1));
				}
			}
			else
			{
				if (Object.PartyLeader != null && !Object.HasEffect("Incommunicado"))
				{
					Object.ApplyEffect(new Incommunicado());
				}
				Object.pBrain?.Goals.Clear();
			}
		}
		return Object.CellTeleport(C, null, Device, null, null, null, 1000, Forced: true, VisualEffects: true, !Object.IsPlayer(), SkipRealityDistortion: true, null);
	}

	public bool ApplyVortex(GameObject GO)
	{
		if (ParentObject == GO || !IsValidTarget(GO))
		{
			return false;
		}
		CheckDestinationZone();
		Cell destinationCellFor = GetDestinationCellFor(DestinationZoneID, GO, ParentObject.CurrentCell);
		if (destinationCellFor == null || !GO.FireEvent(Event.New("SpaceTimeVortexContact", "Object", ParentObject, "DestinationCell", destinationCellFor)))
		{
			return false;
		}
		if (GO.IsPlayerLed() && !GO.IsTrifling)
		{
			Popup.Show("Your companion, " + GO.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + "," + GO.GetVerb("have") + " been sucked into " + ParentObject.t() + " " + The.Player.DescribeDirectionToward(ParentObject) + "!");
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(GO, "are", "sucked into", ParentObject, null, "!", null, null, GO, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: true);
		}
		Teleport(GO, destinationCellFor, ParentObject);
		if (GO.IsPlayer())
		{
			AchievementManager.IncrementAchievement("ACH_VORTICES_ENTERED");
		}
		return true;
	}

	public static bool IsBlueprintSummonable(GameObjectBlueprint BP)
	{
		if (!EncountersAPI.IsEligibleForDynamicEncounters(BP))
		{
			return false;
		}
		if (!BP.HasPart("Brain"))
		{
			return false;
		}
		return true;
	}

	public static List<string> GetSummonableBlueprints()
	{
		if (Summonable == null)
		{
			Summonable = new List<string>(128);
			foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
			{
				if (IsBlueprintSummonable(value))
				{
					Summonable.Add(value.Name);
				}
			}
		}
		return Summonable;
	}

	public static string GetSummonableBlueprint()
	{
		return GetSummonableBlueprints().GetRandomElement();
	}

	public virtual int SpaceTimeAnomalyEmergencePermillageChance()
	{
		return 5;
	}

	public virtual int SpaceTimeAnomalyEmergenceExplodePercentageChance()
	{
		return 0;
	}

	public virtual bool SpaceTimeAnomalyStationary()
	{
		return false;
	}

	public void SpaceTimeAnomalyPeriodicEvents()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || cell.IsGraveyard())
		{
			return;
		}
		bool flag = SpaceTimeAnomalyStationary();
		List<Cell> list = (flag ? null : cell.GetAdjacentCells());
		if (!flag)
		{
			cell.RemoveObject(ParentObject);
		}
		if (SpaceTimeAnomalyEmergencePermillageChance().in1000())
		{
			if (list == null)
			{
				list = cell.GetLocalEmptyAdjacentCells();
			}
			Cell cell2 = (flag ? list.GetRandomElement() : cell);
			if (cell2 != null)
			{
				GameObject gameObject = GameObject.create(GetSummonableBlueprint());
				if (gameObject != null)
				{
					cell2.AddObject(gameObject);
					gameObject.MakeActive();
					string verb = (gameObject.IsMobile() ? "climb" : "fall");
					IComponent<GameObject>.XDidYToZ(gameObject, verb, "through", ParentObject, IComponent<GameObject>.ThePlayer?.DescribeDirectionToward(ParentObject), "!", null, gameObject, null, UseFullNames: false, IndefiniteSubject: true);
					if (SpaceTimeAnomalyEmergenceExplodePercentageChance().in100())
					{
						DidX("destabilize", null, "!", null, null, IComponent<GameObject>.ThePlayer);
						ParentObject.Explode(3000, null, "1d200", 1f, Neutron: true);
					}
				}
			}
		}
		if (!flag)
		{
			list?.GetRandomElement()?.AddObject(ParentObject);
		}
	}
}
