using System;

namespace XRL.World.Parts;

[Serializable]
public class Hookah : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != GetInventoryActionsEvent.ID && ID != IdleQueryEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Smoke", "smoke", "SmokeHookah", null, 's', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "SmokeHookah" && SmokeHookah(E.Actor, !E.Auto))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		return !SmokeHookah(E.Actor, FromDialog: false);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (E.Actor.HasPart("Brain") && !E.Actor.HasPart("Robot") && E.Actor.DistanceTo(ParentObject) <= 1 && 10.in100() && SmokeHookah(E.Actor, FromDialog: false))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool CanPuff()
	{
		return ParentObject.LiquidVolume?.IsPureLiquid("water") ?? false;
	}

	public bool SmokeHookah(GameObject who, bool FromDialog)
	{
		IComponent<GameObject>.XDidYToZ(who, "take", "a puff on", ParentObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog);
		who.UseEnergy(1000, "Item");
		if (CanPuff())
		{
			for (int i = 2; i < 5; i++)
			{
				ParentObject.Smoke(150, 180);
			}
			who.FireEvent(Event.New("Smoked", "Object", ParentObject));
		}
		return true;
	}
}
