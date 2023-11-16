using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsOnboardRecoilerImprinting : IProgrammableRecoiler
{
	public const int COOLDOWN = 100;

	[NonSerialized]
	private int NameTick;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsOnboardRecoilerImprinting()
	{
		ChargeUse = 0;
		WorksOnCarrier = false;
		WorksOnHolder = false;
		WorksOnImplantee = true;
		NameForStatus = "GeospatialCore";
		Reprogrammable = true;
	}

	public override void ProgrammedForLocation(Zone Z, Cell C)
	{
		NameTick = 0;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != EndTurnEvent.ID && ID != GetInventoryActionsEvent.ID && ID != ImplantedEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Imprint with current location", "CommandActivateOnboardRecoilerImprinting" + ParentObject.id, "Cybernetics");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if ((E.Command == "CommandActivateOnboardRecoilerImprinting" || (E.Command != null && E.Command.StartsWith("CommandActivateOnboardRecoilerImprinting") && E.Command == "CommandActivateOnboardRecoilerImprinting" + ParentObject.id)) && E.Actor == ParentObject.Implantee && ProgramRecoiler(E.Actor, E))
		{
			E.Actor.CooldownActivatedAbility(ActivatedAbilityID, 100);
			E.Actor.UseEnergy(1000, "Cybernetics Recoiler Imprint");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (The.Game.ZoneManager != null && The.ZoneManager.NameUpdateTick > NameTick)
		{
			CyberneticsOnboardRecoilerTeleporter part = ParentObject.GetPart<CyberneticsOnboardRecoilerTeleporter>();
			if (part != null)
			{
				string destinationZone = part.DestinationZone;
				if (!string.IsNullOrEmpty(destinationZone))
				{
					string zoneDisplayName = The.ZoneManager.GetZoneDisplayName(destinationZone, WithIndefiniteArticle: true);
					if (string.IsNullOrEmpty(zoneDisplayName))
					{
						ParentObject.Implantee.GetActivatedAbility(part.ActivatedAbilityID).DisplayName = "Recoil";
					}
					else
					{
						ParentObject.Implantee.GetActivatedAbility(part.ActivatedAbilityID).DisplayName = "Recoil to " + zoneDisplayName;
					}
				}
			}
			NameTick = The.ZoneManager.NameUpdateTick;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsObjectActivePartSubject(IComponent<GameObject>.ThePlayer))
		{
			E.AddAction("Imprint", "imprint", "ImprintRecoiler", null, 'i', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ImprintRecoiler")
		{
			if (!E.Actor.IsActivatedAbilityUsable(ActivatedAbilityID))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("You can't imprint yet.");
				}
			}
			else if (ProgramRecoiler(E.Actor, E))
			{
				E.Actor.CooldownActivatedAbility(ActivatedAbilityID, 100);
				E.Actor.UseEnergy(1000, "Cybernetics Recoiler Imprint");
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
