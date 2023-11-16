using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class RifleMark : Effect
{
	public GameObject Marker;

	public int DistanceToMarker = 999;

	public RifleMark()
	{
		base.DisplayName = "{{R|marked}}";
		base.Duration = 1;
	}

	public RifleMark(GameObject Marker)
		: this()
	{
		this.Marker = Marker;
	}

	public override int GetEffectType()
	{
		return 33554433;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		if (GameObject.validate(ref Marker))
		{
			return "{{R|marked by " + Marker.an() + "}}";
		}
		return "{{R|marked}}";
	}

	public override string GetDetails()
	{
		return "Easier to hit by bows and rifles wielded by " + Marker.an() + ".";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyRifleMarked", "Effect", this)))
		{
			DistanceToMarker = 999;
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!GameObject.validate(ref Marker))
		{
			base.Duration = 0;
			base.Object.CleanEffects();
		}
		else if (!Marker.HasLOSTo(base.Object, IncludeSolid: true, UseTargetability: true))
		{
			base.Duration = 0;
			base.Object.CleanEffects();
		}
		else
		{
			int num = base.Object.DistanceTo(Marker);
			if (num != 999 && DistanceToMarker != 999)
			{
				if (base.Duration > 0)
				{
					if (num > DistanceToMarker)
					{
						Marker.FireEvent("MarkMovedAway");
					}
					else if (num < DistanceToMarker)
					{
						Marker.FireEvent("MarkMovedTowards");
					}
				}
				DistanceToMarker = 99;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (!GameObject.validate(ref Marker))
		{
			base.Duration = 0;
		}
		else
		{
			Cell cell = E.Cell;
			if (Marker.CurrentCell != null)
			{
				DistanceToMarker = cell.DistanceTo(Marker);
			}
			else
			{
				DistanceToMarker = 999;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyConfusion");
		Object.RegisterEffectEvent(this, "EndSegment");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ApplyConfusion");
		Object.UnregisterEffectEvent(this, "EndSegment");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 35 && num < 40)
			{
				E.Tile = null;
				E.RenderString = "Î";
				E.ColorString = "^R&k";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyConfusion")
		{
			base.Object.RemoveEffect(this);
		}
		else if (E.ID == "EndSegment")
		{
			if (!GameObject.validate(ref Marker))
			{
				base.Duration = 0;
				return true;
			}
			if (!base.Object.InActiveZone() || base.Object.CurrentZone != Marker.CurrentZone || Marker.HasEffect("Confused"))
			{
				base.Duration = 0;
				base.Object.CleanEffects();
			}
		}
		return base.FireEvent(E);
	}
}
