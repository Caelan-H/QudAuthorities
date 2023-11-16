using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class TerrainTravelFungal : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CheckLostChance");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckLostChance")
		{
			if (The.Core.IDKFA)
			{
				return false;
			}
			Cell cell = IComponent<GameObject>.ThePlayer.CurrentCell;
			ZoneManager zoneManager = The.ZoneManager;
			string zoneWorld = cell.ParentZone.GetZoneWorld();
			int x = cell.X;
			int y = cell.Y;
			Lost lost = new Lost(1);
			if (IComponent<GameObject>.ThePlayer.ApplyEffect(lost))
			{
				int zoneX = Stat.Random(0, 2);
				int zoneY = Stat.Random(0, 2);
				int zoneZ = 10;
				if (x == FungalJungle.UpperLeft.x)
				{
					zoneX = 0;
				}
				if (y == FungalJungle.UpperLeft.y)
				{
					zoneY = 0;
				}
				if (x == FungalJungle.LowerRight.x)
				{
					zoneX = 2;
				}
				if (y == FungalJungle.LowerRight.y)
				{
					zoneY = 2;
				}
				Zone zone = zoneManager.GetZone(ZoneID.Assemble(zoneWorld, x, y, zoneX, zoneY, zoneZ));
				Zone zone2 = zoneManager.SetActiveZone(zone.ZoneID);
				zone2.CheckWeather();
				lost.Visited.Add(zone2.ZoneID);
				IComponent<GameObject>.ThePlayer.SystemMoveTo(zone2.GetPullDownLocation(IComponent<GameObject>.ThePlayer));
				Popup.ShowBlock("You lose your way beneath a dense canopy of spores.");
				The.ZoneManager.ProcessGoToPartyLeader();
				IComponent<GameObject>.ThePlayer.FireEvent(Event.New("AfterLost", "FromCell", cell));
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
