using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Flying : Effect
{
	public int Level = 1;

	public GameObject Source;

	public Flying()
	{
		base.DisplayName = "flying";
		base.Duration = 1;
	}

	public Flying(int Level)
		: this()
	{
		this.Level = Level;
	}

	public Flying(int Level, GameObject Source)
		: this()
	{
		this.Level = Level;
		this.Source = Source;
	}

	public override int GetEffectType()
	{
		return 16777344;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		if (base.Object.GetEffect(base.ClassName) != this)
		{
			return null;
		}
		return "{{B|flying}}";
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "Can't be attacked in melee by non-flying creatures.\nIsn't affected by terrain.\nFast travels much faster.";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Object.GetEffect(base.ClassName) != this)
		{
			return true;
		}
		int num = XRLCore.CurrentFrame % 60;
		if (num > 5 && num < 15)
		{
			E.Tile = null;
			E.RenderString = "\u0018";
			E.ColorString = "&B";
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (base.Object.GetEffect(base.ClassName) == this)
		{
			E.AddTag("[{{B|flying}}]");
		}
		return base.HandleEvent(E);
	}
}
