using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsInflatableAxons : IPart
{
	public string commandId = "";

	public int Bonus = 40;

	public int Duration = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		commandId = Guid.NewGuid().ToString();
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Inflate Axons", commandId, "Cybernetics", "You gain +" + Bonus + " quickness for " + Duration + " rounds, then you become sluggish for 10 rounds (-10 quickness).");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == commandId && E.Actor == ParentObject.Implantee)
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && implantee.CurrentCell != null)
			{
				int num = Duration;
				int num2 = Bonus;
				int @for = GetAvailableComputePowerEvent.GetFor(implantee);
				if (@for != 0)
				{
					num = num * (100 + @for) / 100;
					num2 = num2 * (100 + @for) / 100;
				}
				implantee.ApplyEffect(new AxonsInflated(num, num2));
				implantee.CooldownActivatedAbility(ActivatedAbilityID, 100);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
