using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Switch : IPart
{
	public bool Enabled = true;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetPointsOfInterestEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (Enabled && E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (FlipSwitch(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Flip", "flip", "FlipSwitch", null, 'f', FireOnActor: false, 100);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "FlipSwitch" && FlipSwitch(E.Actor))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Enable");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Enable")
		{
			if (!Enabled)
			{
				Enabled = true;
				ParentObject.pRender.SetForegroundColor('G');
				if (Visible())
				{
					Popup.Show("You hear a loud click from across the room.");
				}
				else
				{
					Popup.Show("You hear a distant click.");
				}
			}
		}
		else if (E.ID == "Disable" && Enabled)
		{
			Enabled = false;
			ParentObject.pRender.SetForegroundColor('R');
			if (Visible())
			{
				Popup.Show("You hear a loud click from across the room.");
			}
			else
			{
				Popup.Show("You hear a distant click.");
			}
		}
		return base.FireEvent(E);
	}

	public bool FlipSwitch(GameObject who)
	{
		if (!Enabled)
		{
			if (who.IsPlayer())
			{
				Popup.Show("Nothing happens.");
			}
			return false;
		}
		return ParentObject.FireEvent("SwitchActivated");
	}
}
