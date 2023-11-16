using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsNocturnalApex : IPart
{
	public bool Used;

	public string commandId = "";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != CommandEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		commandId = Guid.NewGuid().ToString();
		E.Implantee.RegisterPartEvent(this, commandId);
		E.Implantee.RegisterPartEvent(this, "Regenerating2");
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Prowl", commandId, "Cybernetics", "You gain +6 agility and +10 movespeed for 100 turns. Can only be activated at night.");
		E.Implantee.DisableActivatedAbility(ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, commandId);
		E.Implantee.UnregisterPartEvent(this, "Regenerating2");
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == commandId && E.Actor == ParentObject.Implantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee?.CurrentCell != null)
			{
				int num = 100;
				int num2 = -10;
				int num3 = 6;
				int @for = GetAvailableComputePowerEvent.GetFor(implantee);
				if (@for != 0)
				{
					num = num * (100 + @for) / 100;
					num2 = num2 * (100 + @for) / 100;
					num3 = num3 * (100 + @for) / 100;
				}
				implantee.ApplyEffect(new NocturnalApexed(num, num2, num3));
				Used = true;
				implantee.DisableActivatedAbility(ActivatedAbilityID);
				implantee.CooldownActivatedAbility(ActivatedAbilityID, 1200);
				implantee.SetActivatedAbilityDisabledMessage(ActivatedAbilityID, "You've already prowled tonight.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		GameObject implantee = ParentObject.Implantee;
		if (E.Object == implantee)
		{
			if (!Used && IsNight())
			{
				implantee?.EnableActivatedAbility(ActivatedAbilityID);
			}
			else
			{
				if (Used && IsDay())
				{
					Used = false;
				}
				if (implantee != null)
				{
					implantee.DisableActivatedAbility(ActivatedAbilityID);
					if (!Used)
					{
						implantee.SetActivatedAbilityDisabledMessage(ActivatedAbilityID, "You can only prowl at night.");
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating2" && IsDay())
		{
			float num = GetAvailableComputePowerEvent.AdjustUp(ParentObject.Implantee, 1.1f);
			E.SetParameter("Amount", (int)((float)E.GetIntParameter("Amount") * num));
		}
		return base.FireEvent(E);
	}
}
