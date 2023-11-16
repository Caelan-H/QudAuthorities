using System;
using System.Text;
using XRL.Language;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SaveModifier : IActivePart
{
	public string Vs;

	public int Amount = 1;

	public bool ShowInShortDescription = true;

	public SaveModifier()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		SaveModifier saveModifier = p as SaveModifier;
		if (saveModifier.Vs != Vs)
		{
			return false;
		}
		if (saveModifier.Amount != Amount)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription && (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier))
		{
			E.Postfix.AppendRules(AppendRulesDescription, base.AddStatusSummary);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (IsObjectActivePartSubject(E.Defender) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && SavingThrows.Applicable(Vs, E))
		{
			E.Roll += Amount;
		}
		return base.HandleEvent(E);
	}

	private void AppendRulesDescription(StringBuilder SB)
	{
		SB.AppendSigned(Amount).Append(" on saves");
		if (!string.IsNullOrEmpty(Vs))
		{
			SB.Append(" vs. ");
			if (Vs.Contains(","))
			{
				SB.Append(Grammar.MakeAndList(SavingThrows.VsList(Vs)));
			}
			else
			{
				SB.Append(Vs);
			}
		}
		SB.Append('.');
	}
}
