using System;

namespace XRL.World.Parts;

[Serializable]
public class Blackout : IPart
{
	public bool OnlyIfCharged;

	public bool Lit = true;

	public int Radius = 5;

	public override bool SameAs(IPart p)
	{
		LightSource lightSource = p as LightSource;
		if (lightSource.OnlyIfCharged != OnlyIfCharged)
		{
			return false;
		}
		if (lightSource.Lit != Lit)
		{
			return false;
		}
		if (lightSource.Radius != Radius)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderLateEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderLateEvent E)
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return true;
		}
		if (OnlyIfCharged && !ParentObject.TestCharge(1, LiveOnly: false, 0L))
		{
			return true;
		}
		if (Radius == 999)
		{
			cell.ParentZone.UnlightAll();
		}
		else if (Lit)
		{
			cell.ParentZone.RemoveLight(cell.X, cell.Y, Radius);
		}
		return base.HandleEvent(E);
	}
}
