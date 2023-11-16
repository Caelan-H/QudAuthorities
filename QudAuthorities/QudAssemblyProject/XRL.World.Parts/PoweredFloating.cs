using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class PoweredFloating : IPoweredPart
{
	public bool FallIntoInventory = true;

	public PoweredFloating()
	{
		WorksOnEquipper = true;
		IsBootSensitive = false;
		NameForStatus = "HoverSystems";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as PoweredFloating).FallIntoInventory != FallIntoInventory)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID)
		{
			return ID == EffectRemovedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		CheckFloating();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		CheckFloating();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
			CheckFloating();
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingEquipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped" && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: true, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			string text = ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("do") + " not seem to be able to float at this time.";
			if (E.GetIntParameter("AutoEquipTry") > 0)
			{
				E.SetParameter("FailureMessage", text);
			}
			else if (E.GetGameObjectParameter("Equipper").IsPlayer())
			{
				Popup.Show(text);
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public void CheckFloating()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null || IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		if (FallIntoInventory || equipped.OnWorldMap())
		{
			ParentObject.ForceUnequip(Silent: true);
			if (ParentObject.Equipped == null && equipped.IsPlayer())
			{
				if (FallIntoInventory)
				{
					Popup.Show(equipped.Poss(ParentObject) + ParentObject.GetVerb("cease") + " floating near you.");
					return;
				}
				Popup.Show(equipped.Poss(ParentObject) + ParentObject.GetVerb("fall") + " to the ground; you scoop " + ParentObject.them + " up.");
			}
		}
		else
		{
			if (equipped.CurrentCell == null)
			{
				return;
			}
			ParentObject.ForceUnequip(Silent: true);
			if (ParentObject.Equipped == null)
			{
				if (equipped.IsPlayer())
				{
					Popup.Show(equipped.Poss(ParentObject) + ParentObject.GetVerb("fall") + " to the ground.");
				}
				equipped.FireEvent(Event.New("CommandDropObject", "Object", ParentObject, "Forced", 1).SetSilent(Silent: true));
			}
		}
	}
}
