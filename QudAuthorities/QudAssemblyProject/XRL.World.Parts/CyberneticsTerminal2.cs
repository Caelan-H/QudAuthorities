using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsTerminal2 : IPoweredPart, IHackingSifrahHandler
{
	public CyberneticsTerminal2()
	{
		ChargeUse = 100;
		WorksOnSelf = true;
		NameForStatus = "BecomingNookInterface";
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
		if (E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.BaseDisplayName, null, null, null, null, null, 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CyberneticsTerminal.ShowTerminal(ParentObject, E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AddAction("Interface", "interface", "InterfaceWithBecomingNook", null, 'i', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "InterfaceWithBecomingNook" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CyberneticsTerminal.ShowTerminal(ParentObject, E.Actor);
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void HackingResultSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			int num = 1;
			while (30.in100())
			{
				num++;
			}
			ParentObject.ModIntProperty("HackLevel", num);
			if (ParentObject.GetIntProperty("SecurityAlertLevel") >= ParentObject.GetIntProperty("HackLevel"))
			{
				ParentObject.SetIntProperty("HackLevel", ParentObject.GetIntProperty("SecurityAlertLevel") + 1);
			}
		}
	}

	public void HackingResultExceptionalSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			int num = 2;
			while (50.in100())
			{
				num++;
			}
			ParentObject.ModIntProperty("HackLevel", num);
			if (ParentObject.GetIntProperty("SecurityAlertLevel") >= ParentObject.GetIntProperty("HackLevel"))
			{
				ParentObject.SetIntProperty("HackLevel", ParentObject.GetIntProperty("SecurityAlertLevel") + 1);
			}
			int num2 = 1;
			while (30.in100())
			{
				num2++;
			}
			who.ModIntProperty("CyberneticsLicenses", num2);
		}
	}

	public void HackingResultPartialSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject && who.IsPlayer())
		{
			Popup.Show("The hack fails, but you manage to cover your tracks before any security measures kick in.");
		}
	}

	public void HackingResultFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			int num = 1;
			while (30.in100())
			{
				num++;
			}
			ParentObject.ModIntProperty("SecurityAlertLevel", num);
			if (who.IsPlayer())
			{
				Popup.Show("The hack fails, and alert lights on " + ParentObject.the + ParentObject.ShortDisplayName + " begin pulsing rhythmically...");
			}
		}
	}

	public void HackingResultCriticalFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		if (who.HasPart("Dystechnia"))
		{
			FusionReactor obj2 = ParentObject.GetPart("FusionReactor") as FusionReactor;
			if (obj2 == null || !obj2.Explode())
			{
				ParentObject.Explode(10000);
			}
			game.RequestInterfaceExit();
			return;
		}
		int num = 2;
		while (50.in100())
		{
			num++;
		}
		ParentObject.ModIntProperty("SecurityAlertLevel", num);
		if (who.IsPlayer())
		{
			Popup.Show("The hack fails, and alert lights on " + ParentObject.the + ParentObject.ShortDisplayName + " begin pulsing urgently...");
		}
	}
}
