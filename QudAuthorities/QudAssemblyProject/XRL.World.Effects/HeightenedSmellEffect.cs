using System;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class HeightenedSmellEffect : Effect
{
	public bool Identified;

	public int Level = 1;

	public GameObject Smeller;

	public HeightenedSmellEffect()
	{
		base.Duration = 1;
	}

	public HeightenedSmellEffect(int Level, GameObject Smeller)
		: this()
	{
		this.Level = Level;
		this.Smeller = Smeller;
	}

	public override int GetEffectType()
	{
		return 512;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return null;
	}

	private bool BadSmeller()
	{
		Smeller = null;
		base.Object.RemoveEffect(this);
		return true;
	}

	public bool CheckSmell()
	{
		if (!GameObject.validate(ref Smeller) || !Smeller.IsPlayer())
		{
			return BadSmeller();
		}
		if (!(Smeller.GetPart("HeightenedSmell") is HeightenedSmell heightenedSmell) || heightenedSmell.Level <= 0)
		{
			return BadSmeller();
		}
		int num = base.Object.DistanceTo(Smeller);
		if (num > heightenedSmell.GetRadius())
		{
			return BadSmeller();
		}
		if (!base.Object.IsSmellable(Smeller))
		{
			return BadSmeller();
		}
		if (Identified)
		{
			return true;
		}
		if (base.Object.CurrentCell == null)
		{
			return true;
		}
		if (((int)((double)(100 + 20 * Level) / Math.Pow(num + 9, 2.0) * 100.0)).in100())
		{
			Identified = true;
			if (Smeller.IsPlayer())
			{
				AutoAct.CheckHostileInterrupt();
			}
		}
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.pBrain != null)
		{
			Object.pBrain.Hibernating = false;
		}
		CheckSmell();
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	public bool NotSeen(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!obj.IsVisible())
		{
			return true;
		}
		Cell cell = obj.CurrentCell;
		if (cell != null && (!cell.IsLit() || !cell.IsExplored()))
		{
			return true;
		}
		return false;
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (NotSeen(base.Object) && base.Object.CanHypersensesDetect())
		{
			if (Identified)
			{
				E.HighestLayer = 0;
				base.Object.Render(E);
				E.ColorString = "&K";
				E.DetailColor = "K";
				E.RenderString = base.Object.pRender.RenderString;
				if (Options.UseTiles)
				{
					E.Tile = base.Object.pRender.Tile;
				}
				else
				{
					E.Tile = null;
				}
				E.CustomDraw = true;
			}
			else
			{
				E.RenderString = "&K?";
				E.Tile = null;
				E.CustomDraw = true;
			}
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && base.Object != null)
		{
			CheckSmell();
		}
		return base.FireEvent(E);
	}
}
