using System;

namespace XRL.World.Parts;

[Serializable]
public class Flame : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != GetMatterPhaseEvent.ID)
		{
			return ID == RespiresEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetMatterPhaseEvent E)
	{
		E.MinMatterPhase(4);
		return false;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && !ParentObject.HasTag("Creature"))
		{
			if (E.Damage.IsHeatDamage() || E.Damage.IsAcidDamage() || E.Damage.HasAttribute("Poison"))
			{
				E.Damage.Amount = 0;
			}
			else if (E.Damage.IsColdDamage())
			{
				E.Damage.Amount /= 2;
			}
		}
		return base.HandleEvent(E);
	}
}
