using System;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class SenseRobotEffect : Effect
{
	public bool Identified;

	public int Level = 1;

	public GameObject Listener;

	public GameObject Device;

	public SenseRobotEffect()
	{
		base.Duration = 1;
	}

	public SenseRobotEffect(int Level = 1, GameObject Listener = null, GameObject Device = null)
		: this()
	{
		this.Level = Level;
		this.Listener = Listener;
		this.Device = Device;
	}

	public override int GetEffectType()
	{
		return 16777280;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return null;
	}

	private bool InvalidListen()
	{
		Listener = null;
		base.Object.RemoveEffect(this);
		return true;
	}

	public bool CheckListen()
	{
		if (!GameObject.validate(ref Listener) || !Listener.IsPlayer() || Listener.IsNowhere())
		{
			return InvalidListen();
		}
		int num = base.Object.DistanceTo(Listener);
		if (num > Level)
		{
			return InvalidListen();
		}
		if (!CyberneticsElectromagneticSensor.WillSense(base.Object))
		{
			return InvalidListen();
		}
		if (!GameObject.validate(ref Device) || Device.Implantee != Listener)
		{
			return InvalidListen();
		}
		if (Identified)
		{
			return true;
		}
		if (base.Object.CurrentCell == null)
		{
			return true;
		}
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

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteringCellEvent E)
	{
		if (base.Object != null)
		{
			CheckListen();
		}
		return base.HandleEvent(E);
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (base.Object == null)
		{
			return true;
		}
		Cell cell = base.Object.CurrentCell;
		if (cell != null && !cell.IsGraveyard() && (!cell.IsLit() || !cell.IsExplored() || !cell.IsVisible()))
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
				E.RenderString = "?";
				E.ColorString = "&C";
				E.Tile = null;
				E.CustomDraw = true;
			}
			return false;
		}
		return true;
	}
}
