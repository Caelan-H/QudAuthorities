using System;

namespace XRL.World.Parts;

[Serializable]
public class GlobalModChanceFactorOnPlayerEquip : IPart
{
	public string Mod;

	public float Factor;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetModRarityWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetModRarityWeightEvent E)
	{
		if ((string.IsNullOrEmpty(Mod) || E.Mod.Part == Mod) && (ParentObject.Equipped == The.Player || ParentObject.Implantee == The.Player))
		{
			E.FactorAdjustment *= Factor;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
