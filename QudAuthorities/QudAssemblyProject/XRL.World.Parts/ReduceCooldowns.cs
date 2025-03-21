using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ReduceCooldowns : IPoweredPart
{
	public string PercentageReduction;

	public string LinearReduction;

	public string IncludeAbilities;

	public string ExcludeAbilities;

	public bool UsesChargePerTurn;

	public bool UsesChargePerEffect;

	public ReduceCooldowns()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		ReduceCooldowns reduceCooldowns = p as ReduceCooldowns;
		if (reduceCooldowns.LinearReduction != LinearReduction)
		{
			return false;
		}
		if (reduceCooldowns.PercentageReduction != PercentageReduction)
		{
			return false;
		}
		if (reduceCooldowns.IncludeAbilities != IncludeAbilities)
		{
			return false;
		}
		if (reduceCooldowns.ExcludeAbilities != ExcludeAbilities)
		{
			return false;
		}
		if (reduceCooldowns.UsesChargePerTurn != UsesChargePerTurn)
		{
			return false;
		}
		if (reduceCooldowns.UsesChargePerEffect != UsesChargePerEffect)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetCooldownEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCooldownEvent E)
	{
		if (Applicable(E.Ability) && IsObjectActivePartSubject(E.Actor) && IsReady(UsesChargePerEffect, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (!string.IsNullOrEmpty(PercentageReduction))
			{
				E.PercentageReduction += PercentageReduction.RollCached();
			}
			if (!string.IsNullOrEmpty(LinearReduction))
			{
				E.LinearReduction += LinearReduction.RollCached();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if ((WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier || WorksOnImplantee) && (!string.IsNullOrEmpty(PercentageReduction) || !string.IsNullOrEmpty(LinearReduction)))
		{
			StringBuilder postfix = E.Postfix;
			postfix.Append("\n{{rules|");
			bool flag = false;
			bool flag2 = false;
			bool flag3 = (UsesChargePerTurn || UsesChargePerEffect) && ChargeUse > 0;
			if (flag3)
			{
				postfix.Append("When powered, ");
			}
			if (!string.IsNullOrEmpty(PercentageReduction))
			{
				int num = PercentageReduction.RollMin();
				int num2 = PercentageReduction.RollMax();
				if (num >= 0 && num2 > 0)
				{
					postfix.Append(flag3 ? "provides " : "Provides ").Append(num);
					if (num != num2)
					{
						postfix.Append('-').Append(num2);
					}
					postfix.Append('%');
					flag = true;
				}
				else if (num < 0 && num2 <= 0)
				{
					postfix.Append(flag3 ? "causes " : "Causes ").Append(-num2).Append('%');
					if (num != num2)
					{
						postfix.Append(" to ").Append(-num).Append('%');
					}
					flag2 = true;
				}
				else if (num != 0 || num2 != 0)
				{
					postfix.Append(flag3 ? "confers " : "Confers ").Append(num).Append('%');
					if (num != num2)
					{
						postfix.Append(" to ").Append(num2).Append('%');
					}
					flag = true;
					flag2 = true;
				}
			}
			if (!string.IsNullOrEmpty(LinearReduction))
			{
				int num3 = LinearReduction.RollMin();
				int num4 = LinearReduction.RollMax();
				if (num3 >= 0 && num4 > 0)
				{
					if (flag2)
					{
						postfix.Append(" increase");
					}
					postfix.Append((flag || flag2) ? " plus " : (flag3 ? "provides " : "Provides "));
					postfix.Append(num3);
					if (num3 != num4)
					{
						postfix.Append('-').Append(num4);
					}
					postfix.Append(' ').Append((num3 != 1 || num4 != 1) ? "points" : "point");
					if (!flag && !flag2)
					{
						postfix.Append(" of");
					}
					postfix.Append(" reduction");
					flag = true;
				}
				else if (num3 < 0 && num4 <= 0)
				{
					if (flag)
					{
						postfix.Append(" reduction");
					}
					postfix.Append((flag || flag2) ? " minus " : (flag3 ? "causes " : "Causes "));
					postfix.Append(-num4);
					if (num3 != num4)
					{
						postfix.Append('-').Append(-num3);
					}
					postfix.Append(' ').Append((num3 != -1 || num4 != -1) ? "points" : "point");
					if (!flag && !flag2)
					{
						postfix.Append(" of");
					}
					postfix.Append(" increase");
					flag2 = true;
				}
				else if (num3 != 0 || num4 != 0)
				{
					if (!flag || !flag2)
					{
						if (flag)
						{
							postfix.Append(" reduction");
						}
						if (flag2)
						{
							postfix.Append(" increase");
						}
					}
					postfix.Append((flag || flag2) ? " plus " : (flag3 ? "confers " : "Confers "));
					postfix.Append(num3);
					if (num3 != num4)
					{
						postfix.Append(" to ").Append(num4);
					}
					postfix.Append(' ').Append((num3 != num4 && num4 != 1 && num4 != -1) ? "points" : "point");
					if (!flag && !flag2)
					{
						postfix.Append(" of");
					}
					postfix.Append(" reduction/increase");
					flag = true;
					flag2 = true;
				}
			}
			else if (flag && flag2)
			{
				postfix.Append(" reduction/increase");
			}
			else
			{
				if (flag)
				{
					postfix.Append(" reduction");
				}
				if (flag2)
				{
					postfix.Append(" increase");
				}
			}
			if (flag || flag2)
			{
				postfix.Append(" in the ");
				if (!string.IsNullOrEmpty(IncludeAbilities))
				{
					List<string> list = IncludeAbilities.CachedCommaExpansion();
					postfix.Append((list.Count == 1) ? "cooldown of the ability " : "cooldowns of the abilities ").Append(ColorUtility.StripFormatting((list.Count == 1) ? list[0] : Grammar.MakeAndList(list)));
				}
				else if (!string.IsNullOrEmpty(ExcludeAbilities))
				{
					List<string> list2 = ExcludeAbilities.CachedCommaExpansion();
					postfix.Append("cooldowns of activated abilities other than ").Append(ColorUtility.StripFormatting((list2.Count == 1) ? list2[0] : Grammar.MakeAndList(list2)));
				}
				else
				{
					postfix.Append("cooldowns of activated abilities");
				}
				postfix.Append('.');
			}
			AddStatusSummary(postfix);
			postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	private bool Applicable(ActivatedAbilityEntry Ability)
	{
		if (!string.IsNullOrEmpty(IncludeAbilities) && !IncludeAbilities.CachedCommaExpansion().Contains(Ability.DisplayName))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ExcludeAbilities) && ExcludeAbilities.CachedCommaExpansion().Contains(Ability.DisplayName))
		{
			return false;
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return UsesChargePerTurn;
	}

	public override bool WantTenTurnTick()
	{
		return UsesChargePerTurn;
	}

	public override bool WantHundredTurnTick()
	{
		return UsesChargePerTurn;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (UsesChargePerTurn && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (UsesChargePerTurn && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (UsesChargePerTurn && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
		}
	}
}
