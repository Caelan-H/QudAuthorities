using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Book : IPart
{
	public string ID = "";

	public override bool SameAs(IPart p)
	{
		if ((p as Book).ID != ID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != HasBeenReadEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (E.Actor == The.Player && GetHasBeenRead())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, GetHasBeenRead() ? 1 : 100);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read" && E.Actor.IsPlayer())
		{
			BookUI.ShowBook(ID);
			if (!GetHasBeenRead())
			{
				SetHasBeenRead(flag: true);
				JournalAPI.AddAccomplishment("You read " + ParentObject.a + ParentObject.pRender.DisplayName + ".", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + ", =name= penned the influential book, " + ParentObject.a + ParentObject.pRender.DisplayName + ".", "general", JournalAccomplishment.MuralCategory.CreatesSomething, JournalAccomplishment.MuralWeight.VeryLow, null, -1L);
			}
		}
		return base.HandleEvent(E);
	}

	public string GetBookKey()
	{
		return "AlreadyRead_" + ID;
	}

	public bool GetHasBeenRead()
	{
		return The.Game.GetStringGameState(GetBookKey()) == "Yes";
	}

	public void SetHasBeenRead(bool flag)
	{
		if (flag)
		{
			The.Game.SetStringGameState(GetBookKey(), "Yes");
		}
		else
		{
			The.Game.SetStringGameState(GetBookKey(), "");
		}
	}
}
