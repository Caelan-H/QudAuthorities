using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPhaseHarmonicModulator : IPart
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Phase Harmonic Modulator", "CommandTogglePhaseHarmonicModulator", "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true);
		E.Implantee.ForceApplyEffect(new Omniphase(base.Name));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveEffect("Omniphase", OurEffect);
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandTogglePhaseHarmonicModulator" && E.Actor == ParentObject.Implantee)
		{
			E.Actor.ToggleActivatedAbility(ActivatedAbilityID);
			if (E.Actor.IsActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				if (!E.Actor.HasEffect("Omniphase", OurEffect))
				{
					E.Actor.ForceApplyEffect(new Omniphase(base.Name));
				}
			}
			else
			{
				E.Actor.RemoveEffect("Omniphase", OurEffect);
			}
		}
		return base.HandleEvent(E);
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Omniphase omniphase)
		{
			return omniphase.SourceKey == base.Name;
		}
		return false;
	}
}
