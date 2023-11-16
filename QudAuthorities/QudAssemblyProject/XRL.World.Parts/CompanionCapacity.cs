using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class CompanionCapacity : IActivePart
{
	public int Proselytized;

	public int Beguiled;

	public CompanionCapacity()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != DroppedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Proselytized > 0)
		{
			string text = ((Proselytized == 1) ? "follower" : "followers");
			E.Postfix.Compound("{{rules|", '\n').Append("You may Proselytize " + Grammar.Cardinal(Proselytized) + " additional " + text + ".}}");
		}
		if (Beguiled > 0)
		{
			string text2 = ((Beguiled == 1) ? "follower" : "followers");
			E.Postfix.Compound("{{rules|", '\n').Append("You may Beguile " + Grammar.Cardinal(Beguiled) + " additional " + text2 + ".}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public void EvaluateAsSubject(GameObject Object)
	{
		if (IsObjectActivePartSubject(Object))
		{
			Object.RegisterPartEvent(this, "GetMaxBeguiled");
			Object.RegisterPartEvent(this, "GetMaxProselytized");
		}
		else
		{
			Object.UnregisterPartEvent(this, "GetMaxBeguiled");
			Object.UnregisterPartEvent(this, "GetMaxProselytized");
		}
	}

	public override bool FireEvent(Event E)
	{
		if (Beguiled != 0 && E.ID == "GetMaxBeguiled" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.ModParameter("Amount", Beguiled);
		}
		else if (Proselytized != 0 && E.ID == "GetMaxProselytized" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.ModParameter("Amount", Proselytized);
		}
		return base.FireEvent(E);
	}
}
