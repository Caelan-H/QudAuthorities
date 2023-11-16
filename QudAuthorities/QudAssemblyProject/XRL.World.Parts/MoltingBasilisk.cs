using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class MoltingBasilisk : IPart
{
	public bool Created;

	public int Puffed;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != BeginTakeActionEvent.ID || Created) && ID != EnteredCellEvent.ID)
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		SyncState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		SyncState();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Created)
		{
			Created = true;
			Zone currentZone = ParentObject.CurrentZone;
			if (currentZone != null && !currentZone.Built)
			{
				int num = Stat.Random(7, 11);
				for (int i = 0; i < num; i++)
				{
					int num2 = 20;
					while (num2-- > 0)
					{
						int x = Stat.Random(0, currentZone.Width - 1);
						int y = Stat.Random(0, currentZone.Height - 1);
						Cell cell = currentZone.GetCell(x, y);
						if (cell.IsEmpty())
						{
							cell.AddObject("Molting Basilisk Husk");
							break;
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AICombatStart");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AICombatStart")
		{
			SyncState();
		}
		return base.FireEvent(E);
	}

	public void SyncState()
	{
		Description description = ParentObject.GetPart("Description") as Description;
		if (ParentObject.Target == null)
		{
			description.Short = "The stony, sloughed skin of a basilisk.";
			ParentObject.DisplayName = "molting basilisk husk";
			ParentObject.SetIntProperty("HideCon", 1);
		}
		else
		{
			description.Short = "The basilisk is nature's statue; its scaled skin is the color of dull quartz and it strikes as still a pose as an artist's mould.";
			ParentObject.DisplayName = "molting basilisk";
			ParentObject.RemoveIntProperty("HideCon");
		}
	}
}
