using System;
using XRL.Messages;
using XRL.World;

namespace XRL;

[Serializable]
public class HolyPlaceSystem : IGameSystem
{
	public string lastHolyFaction;

	public string lastZone;

	public override void ZoneActivated(Zone Z)
	{
		if (!Z.IsWorldMap() && !(Z.ZoneID == lastZone))
		{
			string text = Factions.isZoneHoly(Z.ZoneID);
			if (text != null && text != lastHolyFaction)
			{
				MessageQueue.AddPlayerMessage("You feel a sense of holiness here.");
			}
			lastHolyFaction = text;
		}
	}
}
