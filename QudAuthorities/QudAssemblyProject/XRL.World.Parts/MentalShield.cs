using System;

namespace XRL.World.Parts;

[Serializable]
public class MentalShield : IPart
{
	public static bool Disabled;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != BeforeApplyDamageEvent.ID && ID != CanApplyEffectEvent.ID && ID != CanReceiveEmpathyEvent.ID && ID != CanReceiveTelepathyEvent.ID)
		{
			return ID == BeginMentalDefendEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (!Disabled && E.Object == ParentObject && E.Damage.HasAttribute("Mental"))
		{
			if (!E.Indirect && E.Actor != null && E.Actor.IsPlayer())
			{
				if (E.WillUseOutcomeMessageFragment)
				{
					E.OutcomeMessageFragment = ", but your mental attack has no effect";
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("Your mental attack does not affect " + ParentObject.the + ParentObject.ShortDisplayName + ".");
				}
			}
			E.Damage.Amount = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanReceiveTelepathyEvent E)
	{
		if (!Disabled)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanReceiveEmpathyEvent E)
	{
		if (!Disabled)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginMentalDefendEvent E)
	{
		if (!Disabled && E.Psionic && E.Defender == ParentObject)
		{
			if (E.Attacker.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your mental attack does not affect " + ParentObject.the + ParentObject.DisplayName + "&y.");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	private bool Check(IEffectCheckEvent E)
	{
		if (!Disabled && (E.Name == "Beguile" || E.Name == "Confusion" || E.Name == "Domination" || E.Name == "Shaken" || E.Name == "Shamed" || E.Name == "ShatterMentalArmor" || E.Name == "StunGasStun" || E.Name == "Terrified"))
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
