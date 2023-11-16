using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsOnboardRecoilerTeleporter : ITeleporter
{
	public const int COOLDOWN = 50;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsOnboardRecoilerTeleporter()
	{
		ChargeUse = 0;
		WorksOnCarrier = false;
		WorksOnHolder = false;
		WorksOnImplantee = true;
		NameForStatus = "TeleportationSystem";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Recoil", "CommandActivateOnboardRecoilerTeleporter" + ParentObject.id, "Cybernetics");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if ((E.Command == "CommandActivateOnboardRecoilerTeleporter" || (E.Command != null && E.Command.StartsWith("CommandActivateOnboardRecoilerTeleporter") && E.Command == "CommandActivateOnboardRecoilerTeleporter" + ParentObject.id)) && E.Actor == ParentObject.Implantee)
		{
			ActuateTeleport(IComponent<GameObject>.ThePlayer, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice reduces this item's cooldown.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsObjectActivePartSubject(IComponent<GameObject>.ThePlayer))
		{
			E.AddAction("Activate", "activate", "ActivateRecoilerTeleporter", null, 'a', FireOnActor: false, 100);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateRecoilerTeleporter")
		{
			ActuateTeleport(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void ActuateTeleport(GameObject who, IEvent E)
	{
		if (who.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			if (AttemptTeleport(who, E))
			{
				int turns = GetAvailableComputePowerEvent.AdjustDown(who, 50);
				who.CooldownActivatedAbility(ActivatedAbilityID, turns);
				who.UseEnergy(1000, "Cybernetics Recoiler");
				E.RequestInterfaceExit();
			}
		}
		else if (who.IsPlayer())
		{
			Popup.ShowFail("You can't recoil yet.");
		}
	}
}
