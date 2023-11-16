using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Wish;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class CorruptObject : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Destroy", "{{R|destroy}}", "Destroy", null, 'd', FireOnActor: false, 1000);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Destroy")
		{
			ParentObject.Obliterate(null, Silent: true);
		}
		return base.HandleEvent(E);
	}

	[WishCommand("clearcorrupt", null)]
	public static void ClearAll()
	{
		foreach (Zone value2 in The.ZoneManager.CachedZones.Values)
		{
			for (int i = 0; i < value2.Width; i++)
			{
				for (int j = 0; j < value2.Height; j++)
				{
					Cell cell = value2.Map[i][j];
					cell.ClearObjectsWithPart("CorruptObject");
					int k = 0;
					for (int count = cell.Objects.Count; k < count; k++)
					{
						List<GameObject> contents = cell.Objects[k].GetContents();
						int l = 0;
						for (int count2 = contents.Count; l < count2; l++)
						{
							if (contents[l].HasPart("CorruptObject"))
							{
								contents[l].RemoveFromContext();
							}
						}
					}
				}
			}
		}
		string[] array = The.ZoneManager.CachedObjects.Keys.ToArray();
		for (int m = 0; m < array.Length; m++)
		{
			if (The.ZoneManager.CachedObjects.TryGetValue(array[m], out var value) && value.HasPart("CorruptObject"))
			{
				The.ZoneManager.CachedObjects.Remove(array[m]);
			}
		}
	}
}
