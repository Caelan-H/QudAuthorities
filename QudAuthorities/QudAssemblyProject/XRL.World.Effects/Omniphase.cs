using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Omniphase : Effect
{
	private int FrameOffset;

	public string Tile;

	public string RenderString = "@";

	public string SourceKey;

	public Omniphase()
	{
		base.DisplayName = "{{Y|omniphase}}";
		FrameOffset = Stat.Random(1, 10000);
		base.Duration = 9999;
	}

	public Omniphase(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Omniphase(string SourceKey)
		: this()
	{
		this.SourceKey = SourceKey;
	}

	public Omniphase(int Duration, string SourceKey)
		: this(Duration)
	{
		this.SourceKey = SourceKey;
	}

	public Omniphase(Omniphase Source)
		: this()
	{
		base.Duration = Source.Duration;
		FrameOffset = Source.FrameOffset;
		Tile = Source.Tile;
		RenderString = Source.RenderString;
		SourceKey = Source.SourceKey;
	}

	public override int GetEffectType()
	{
		return 256;
	}

	public override bool SameAs(Effect e)
	{
		Omniphase omniphase = e as Omniphase;
		if (omniphase.Tile != Tile)
		{
			return false;
		}
		if (omniphase.RenderString != RenderString)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == WasDerivedFromEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.Derivation.ForceApplyEffect(new Omniphase(this));
		return base.HandleEvent(E);
	}

	public override string GetDetails()
	{
		return "Physically interactive with both in-phase and out-of-phase creatures and objects.";
	}

	public override bool Apply(GameObject Object)
	{
		bool flag = Object.HasEffect("Omniphase");
		if (!Object.FireEvent("ApplyOmniphase"))
		{
			return false;
		}
		if (!flag)
		{
			Object.FireEvent("AfterOmniphaseStart");
		}
		Tile = Object.pRender.Tile;
		RenderString = Object.pRender.RenderString;
		FlushNavigationCaches();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (!Object.HasEffectOtherThan("Omniphase", this))
		{
			Object.FireEvent("AfterOmniphaseEnd");
		}
		FlushNavigationCaches();
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = (XRLCore.CurrentFrameLong10 + FrameOffset) % 10000;
			if ((num >= 2000 && num <= 2070) || (num >= 8380 && num <= 8450))
			{
				E.ColorString = "&Y";
			}
			else if ((num >= 3910 && num <= 3980) || (num > 6300 && num <= 6370))
			{
				E.ColorString = "&M";
			}
		}
		return true;
	}
}
