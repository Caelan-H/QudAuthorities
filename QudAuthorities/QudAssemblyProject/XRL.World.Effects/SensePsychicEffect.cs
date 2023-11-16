using System;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class SensePsychicEffect : Effect
{
	public bool Identified;

	public int Level = 1;

	public GameObject Listener;

	private int LastDistance;

	public SensePsychicEffect()
	{
		base.Duration = 1;
	}

	public SensePsychicEffect(int Level, GameObject Listener)
		: this()
	{
		this.Level = Level;
		this.Listener = Listener;
	}

	public override int GetEffectType()
	{
		return 32770;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return null;
	}

	private bool BadListener()
	{
		Listener = null;
		base.Object.RemoveEffect(this);
		return true;
	}

	public bool CheckListen()
	{
		if (!GameObject.validate(ref Listener) || !Listener.IsPlayer())
		{
			return BadListener();
		}
		if (!(Listener.GetPart("SensePsychic") is SensePsychic sensePsychic) || sensePsychic.Level <= 0)
		{
			return BadListener();
		}
		int num = base.Object.DistanceTo(Listener);
		if (num > sensePsychic.Radius)
		{
			return BadListener();
		}
		if (Identified)
		{
			return true;
		}
		if (base.Object.CurrentCell == null)
		{
			return true;
		}
		if (LastDistance > 0 && num >= LastDistance)
		{
			return true;
		}
		LastDistance = num;
		if (((int)((double)(100 + 10 * Level) / Math.Pow(num + 9, 2.0) * 100.0)).in100())
		{
			Identified = true;
			if (Listener.IsPlayer())
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
		CheckListen();
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

	public bool HeardAndNotSeen(GameObject obj)
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
		if (HeardAndNotSeen(base.Object) && base.Object.CanHypersensesDetect())
		{
			if (Identified)
			{
				E.HighestLayer = 0;
				base.Object.Render(E);
				E.Tile = base.Object.pRender.Tile;
				E.CustomDraw = true;
			}
			else
			{
				E.RenderString = "&B?";
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
			CheckListen();
		}
		return base.FireEvent(E);
	}
}
