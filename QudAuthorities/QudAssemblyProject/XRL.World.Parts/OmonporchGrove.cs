using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class OmonporchGrove : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		try
		{
			Zone currentZone = ParentObject.CurrentZone;
			int num = 0;
			for (int i = 0; i < currentZone.Width; i++)
			{
				for (int j = 0; j < currentZone.Height; j++)
				{
					foreach (GameObject item in currentZone.GetCell(i, j).GetObjectsWithPart("PlantProperties"))
					{
						num++;
						item.RequirePart<RipePlant>();
					}
				}
			}
			for (int k = 0; k < 200 - num; k++)
			{
				int num2 = 0;
				while (++num2 <= 100000)
				{
					int x = Stat.Random(0, 79);
					int y = Stat.Random(0, 24);
					Cell cell = currentZone.GetCell(x, y);
					if (cell != null && cell.IsEmpty() && cell.IsReachable())
					{
						GameObject gameObject = GameObject.create("Red Death Dacca");
						gameObject.RequirePart<RipePlant>();
						cell.AddObject(gameObject);
						break;
					}
				}
			}
		}
		catch
		{
		}
		return base.HandleEvent(E);
	}
}
