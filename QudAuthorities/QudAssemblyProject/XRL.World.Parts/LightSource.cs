using System;

namespace XRL.World.Parts;

[Serializable]
public class LightSource : IPart
{
	public bool Darkvision;

	public bool OnlyIfCharged;

	public bool Lit = true;

	public int Radius = 5;

	public override bool SameAs(IPart p)
	{
		LightSource lightSource = p as LightSource;
		if (lightSource.Darkvision != Darkvision)
		{
			return false;
		}
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
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return base.HandleEvent(E);
		}
		if (OnlyIfCharged && (IsBroken() || IsRusted() || !ParentObject.TestCharge(1, LiveOnly: false, 0L)))
		{
			return base.HandleEvent(E);
		}
		if (Darkvision)
		{
			if (ParentObject.Equipped != null && ParentObject.Equipped.IsPlayer() && Lit)
			{
				if (Radius == 999)
				{
					cell.ParentZone.LightAll();
				}
				else
				{
					cell.ParentZone.AddLight(cell.X, cell.Y, Radius, LightLevel.Darkvision);
				}
			}
		}
		else if (Lit)
		{
			if (Radius == 999)
			{
				cell.ParentZone.LightAll();
			}
			else
			{
				cell.ParentZone.AddLight(cell.X, cell.Y, Radius);
				if (IsNight() && cell.ParentZone.Z <= 10 && ParentObject.Equipped != null && ParentObject.Equipped.IsPlayer())
				{
					cell.ParentZone.AddExplored(cell.X, cell.Y, Radius * 2);
				}
			}
		}
		return base.HandleEvent(E);
	}
}
