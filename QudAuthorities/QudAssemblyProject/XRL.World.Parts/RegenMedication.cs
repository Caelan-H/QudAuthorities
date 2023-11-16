using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class RegenMedication : IPart
{
	public string Message = "The tube hisses against your skin, and a hot tingling infuses your whole body.";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			E.Actor.FireEvent(Event.New("Regenera", "Level", 10, "Source", ParentObject));
			if (E.Actor.IsTrueKin())
			{
				GenericNotifyEvent.Send(E.Actor, "RegenerateAllLimbs", null, ParentObject);
				E.Actor.ApplyEffect(new Refresh(20));
			}
			else
			{
				GenericNotifyEvent.Send(E.Actor, "RegenerateWholeLimb", null, ParentObject);
			}
			if (E.Actor.IsPlayer())
			{
				Popup.Show(Message);
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
