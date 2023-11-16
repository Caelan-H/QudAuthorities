using System;

namespace XRL.World.Parts;

[Serializable]
public class Daylight : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
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
		GameObject thePlayer = IComponent<GameObject>.ThePlayer;
		if (thePlayer != null && thePlayer.CurrentZone?.IsInside() == true)
		{
			return base.HandleEvent(E);
		}
		int num = Calendar.CurrentDaySegment / 500;
		int num2 = (int)((float)(Calendar.CurrentDaySegment - 500 * num) / 8.33333f);
		int num3 = 5;
		if (num < 5)
		{
			num3 = 0;
		}
		else if (num >= 5 && (num < 18 || (num == 18 && num2 < 15)))
		{
			num3 = (Calendar.CurrentDaySegment - 2500) / 10;
		}
		else
		{
			num3 = 80 - (Calendar.CurrentDaySegment - 9124) / 10;
			if (num3 < 0)
			{
				num3 = 0;
			}
		}
		foreach (IGameSystem system in The.Game.Systems)
		{
			num3 = system.GetDaylightRadius(num3);
		}
		Cell cell = IComponent<GameObject>.ThePlayer.CurrentCell;
		if (cell != null)
		{
			if (num3 > 0)
			{
				cell.ParentZone.AddLight(cell.X, cell.Y, num3, LightLevel.Light);
			}
			if (num3 < 3)
			{
				cell.ParentZone.AddExplored(cell.X, cell.Y, 3);
			}
		}
		return base.HandleEvent(E);
	}
}
