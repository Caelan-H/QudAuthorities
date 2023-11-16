using System;

namespace XRL.World.Parts;

[Serializable]
public class Inorganic : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == CanApplyEffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && E.Damage.HasAttribute("Poison"))
		{
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

	private bool Check(IEffectCheckEvent E)
	{
		if ((E.Name == "Sleep" && !ParentObject.HasTag("Robot")) || E.Name == "Bleeding" || E.Name == "Disease" || E.Name == "DiseaseOnset" || E.Name == "PoisonGasPoison" || E.Name == "StunGasStun")
		{
			return false;
		}
		return true;
	}
}
