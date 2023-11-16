using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Lost : Effect
{
	public bool DisableUnlost;

	public long LostOn;

	public List<string> Visited = new List<string>();

	public Lost()
	{
		base.DisplayName = "lost";
	}

	public Lost(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override string GetDetails()
	{
		return "Can't fast travel.";
	}

	public override int GetEffectType()
	{
		return 117440640;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Lost"))
		{
			return false;
		}
		if (LostOn == 0L)
		{
			if (Object.CurrentZone != null)
			{
				Visited.Add(Object.CurrentZone.ZoneID);
			}
			LostOn = XRLCore.CurrentTurn - 1;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!IsPlayer())
		{
			base.Duration = 0;
		}
		else if (base.Duration > 0 && !DisableUnlost)
		{
			Zone zone = E.Zone;
			if (zone?.ZoneWorld == "JoppaWorld")
			{
				if (zone.LastPlayerPresence != -1 && zone.LastPlayerPresence < LostOn)
				{
					Popup.Show("You recognize the area and stop being lost!");
					base.Duration = 0;
				}
				else if (!Visited.Contains(zone.ZoneID) && zone.Z <= 10)
				{
					Visited.Add(zone.ZoneID);
					if ((base.Object.HasSkill("Survival") ? 40 : 20).in100())
					{
						Popup.ShowSpace("You regain your bearings.");
						base.Duration = 0;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}
