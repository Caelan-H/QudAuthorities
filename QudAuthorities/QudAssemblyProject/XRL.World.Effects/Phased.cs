using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Phased : Effect
{
	private int FrameOffset;

	private int FlickerFrame;

	public string Tile;

	public string RenderString = "@";

	public Phased()
	{
		base.DisplayName = "{{g|phased}}";
		FrameOffset = Stat.Random(1, 60);
	}

	public Phased(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Phased(Phased Source)
		: this()
	{
		base.Duration = Source.Duration;
		FrameOffset = Source.FrameOffset;
		FlickerFrame = Source.FlickerFrame;
		Tile = Source.Tile;
		RenderString = Source.RenderString;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 256;
	}

	public override bool SameAs(Effect e)
	{
		Phased phased = e as Phased;
		if (phased.Tile != Tile)
		{
			return false;
		}
		if (phased.RenderString != RenderString)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != EnteredCellEvent.ID && ID != RealityStabilizeEvent.ID)
		{
			return ID == WasDerivedFromEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.Derivation.ForceApplyEffect(new Phased(this));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0 && base.Duration != 9999 && base.Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You will phase back in in " + base.Duration.Things("round") + ".");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (E.Cell.OnWorldMap())
		{
			base.Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			GameObject @object = base.Object;
			@object.RemoveEffect(this);
			if (!@object.IsValid() || @object.IsInGraveyard())
			{
				return false;
			}
			if (@object.GetPhase() == 1)
			{
				@object.TakeDamage("2d6".RollCached(), "from being forced into phase.", "Normality Phase Unavoidable", null, null, null, E.Effect.Owner);
				if (!@object.IsValid() || @object.IsInGraveyard())
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override string GetDetails()
	{
		return "Can't physically interact with creatures and objects unless they're also phased.\nCan pass through solids.";
	}

	public override bool Apply(GameObject Object)
	{
		bool flag = Object.HasEffect("Phased");
		if (!Object.FireEvent("ApplyPhased"))
		{
			return false;
		}
		if (!flag)
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You phase out.");
			}
			Object.FireEvent("AfterPhaseOut");
		}
		Tile = Object.pRender.Tile;
		RenderString = Object.pRender.RenderString;
		FlushNavigationCaches();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (!Object.HasEffectOtherThan("Phased", this))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You phase back in.");
			}
			if (Object.CurrentCell != null && !Object.OnWorldMap())
			{
				foreach (GameObject item in Object.CurrentCell.GetObjectsWithPart("Physics"))
				{
					if (item == Object || !item.pPhysics.Solid || (item.HasTagOrProperty("Flyover") && Object.IsFlying))
					{
						continue;
					}
					List<Cell> list = new List<Cell>(8);
					Object.CurrentCell.GetAdjacentCells(1, list, LocalOnly: false);
					Cell cell = null;
					for (int i = 0; i < list.Count; i++)
					{
						cell = list[i];
						for (int j = 0; j < list[i].Objects.Count; j++)
						{
							if (list[i].Objects[j].pPhysics != null && list[i].Objects[j].pPhysics.Solid && (!list[i].Objects[j].HasTagOrProperty("Flyover") || !Object.IsFlying))
							{
								cell = null;
								break;
							}
						}
						if (cell != null)
						{
							break;
						}
					}
					if (cell == null)
					{
						if (Object.IsPlayer())
						{
							AchievementManager.SetAchievement("ACH_VIOLATE_PAULI");
						}
						Object.DilationSplat();
						Object.Die(item, null, "You violated the Pauli exclusion principle.", Object.It + " @@violated the Pauli exclusion principle.", Accidental: true);
						continue;
					}
					Object.CurrentCell.RemoveObject(Object);
					cell.AddObject(Object);
					break;
				}
			}
			FlushNavigationCaches();
			Object.FireEvent("AfterPhaseIn");
			Object.Gravitate();
		}
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = 0;
			if (base.Object.HasTag("Astral"))
			{
				base.Object.pRender.Tile = null;
				num = (XRLCore.CurrentFrame + FrameOffset) % 400;
				if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
				{
					if (FlickerFrame == 0)
					{
						base.Object.pRender.RenderString = "_";
					}
					else if (FlickerFrame == 1)
					{
						base.Object.pRender.RenderString = "-";
					}
					else if (FlickerFrame == 2)
					{
						base.Object.pRender.RenderString = "|";
					}
					E.ColorString = "&K";
					if (FlickerFrame == 0)
					{
						FlickerFrame = 3;
					}
					FlickerFrame--;
				}
				else
				{
					base.Object.pRender.RenderString = RenderString;
					base.Object.pRender.Tile = Tile;
				}
				if (num < 4)
				{
					base.Object.pRender.ColorString = "&Y";
					base.Object.pRender.DetailColor = "k";
				}
				else if (num < 8)
				{
					base.Object.pRender.ColorString = "&y";
					base.Object.pRender.DetailColor = "K";
				}
				else if (num < 12)
				{
					base.Object.pRender.ColorString = "&k";
					base.Object.pRender.DetailColor = "y";
				}
				else
				{
					base.Object.pRender.ColorString = "&K";
					base.Object.pRender.DetailColor = "y";
				}
				if (!Options.DisableTextAnimationEffects)
				{
					FrameOffset += Stat.Random(0, 20);
				}
				if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
				{
					base.Object.pRender.ColorString = "&K";
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

	public override bool allowCopyOnNoEffectDeepCopy()
	{
		return true;
	}
}
