using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class PhaseSticky : IPart
{
	public bool DestroyOnBreak;

	public int MaxWeight = 1000;

	public int SaveTarget = 15;

	public int Duration = 12;

	private int FrameOffset;

	private int FlickerFrame;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetNavigationWeightEvent.ID && ID != LeftCellEvent.ID && ID != ObjectEnteredCellEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == RealityStabilizeEvent.ID;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = 0;
			if (ParentObject.HasTag("Astral"))
			{
				ParentObject.pRender.Tile = null;
				num = (XRLCore.CurrentFrame + FrameOffset) % 400;
				if (num < 4)
				{
					ParentObject.pRender.ColorString = "&Y";
					ParentObject.pRender.DetailColor = "k";
				}
				else if (num < 8)
				{
					ParentObject.pRender.ColorString = "&y";
					ParentObject.pRender.DetailColor = "K";
				}
				else if (num < 12)
				{
					ParentObject.pRender.ColorString = "&k";
					ParentObject.pRender.DetailColor = "y";
				}
				else
				{
					ParentObject.pRender.ColorString = "&K";
					ParentObject.pRender.DetailColor = "y";
				}
				if (!Options.DisableTextAnimationEffects)
				{
					FrameOffset += Stat.Random(0, 20);
				}
				if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
				{
					ParentObject.pRender.ColorString = "&K";
				}
				return true;
			}
			num = (XRLCore.CurrentFrame + FrameOffset) % 60;
			num /= 2;
			num %= 6;
			if (num == 0)
			{
				E.ColorString = "&k";
			}
			if (num == 1)
			{
				E.ColorString = "&K";
			}
			if (num == 2)
			{
				E.ColorString = "&c";
			}
			if (num == 4)
			{
				E.ColorString = "&y";
			}
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart)
		{
			E.MinWeight(75);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check())
		{
			Sticky sticky = ParentObject.RequirePart<Sticky>();
			sticky.DestroyOnBreak = DestroyOnBreak;
			sticky.SaveTarget = SaveTarget;
			sticky.MaxWeight = MaxWeight;
			sticky.Duration = Duration;
			ParentObject.RemovePart(this);
			if (ParentObject.DisplayNameOnlyDirect == "phase web")
			{
				ParentObject.DisplayName = "web";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!E.Object.HasEffect("Greased") && E.Object.GetMatterPhase() <= 1 && !E.Object.HasTag("ExcavatoryTerrainFeature") && E.Object.PhaseMatches(ParentObject) && E.Object.Weight <= MaxWeight && !ParentObject.IsBroken() && !ParentObject.IsRusted() && E.Object.ApplyEffect(new Stuck(12, SaveTarget, "Web Stuck Restraint", DestroyOnBreak ? ParentObject : null, "stuck", ParentObject.id)))
		{
			E.Object.ApplyEffect(new PhasedWhileStuck(2));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		StripStuck(E.Cell);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		StripStuck(ParentObject.CurrentCell);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyStuck");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck")
		{
			return false;
		}
		return base.FireEvent(E);
	}

	private bool IsOurs(Effect GFX)
	{
		if (GFX is Stuck stuck && stuck.DestroyOnBreak == ParentObject)
		{
			return true;
		}
		return false;
	}

	private void StripStuck(Cell C)
	{
		if (C == null)
		{
			return;
		}
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			Effect effect = C.Objects[i].GetEffect("Stuck", IsOurs);
			if (effect != null)
			{
				effect.Duration = 0;
			}
		}
	}
}
