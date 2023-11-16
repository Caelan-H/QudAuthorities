using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SaveModifiers : IActivePart
{
	public Dictionary<string, int> Tracking = new Dictionary<string, int>();

	public string Modifiers
	{
		set
		{
			string[] array = value.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(':');
				if (array2.Length == 2)
				{
					try
					{
						Tracking[array2[0]] = Convert.ToInt32(array2[1]);
					}
					catch
					{
					}
				}
			}
		}
	}

	public SaveModifiers()
	{
		WorksOnEquipper = true;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		SaveModifiers obj = base.DeepCopy(Parent) as SaveModifiers;
		obj.Tracking = new Dictionary<string, int>(Tracking);
		return obj;
	}

	public void AddModifier(string Vs, int Level)
	{
		if (Tracking.TryGetValue(Vs, out var value))
		{
			Tracking[Vs] = value + Level;
		}
		else
		{
			Tracking.Add(Vs, Level);
		}
	}

	public override bool SameAs(IPart p)
	{
		if (!(p as SaveModifiers).Tracking.SameAs(Tracking))
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
		if (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier)
		{
			List<string> list = new List<string>(Tracking.Keys);
			list.Sort();
			foreach (string item in list)
			{
				E.Postfix.Append("\n{{rules|");
				int num = Tracking[item];
				if (num < 0)
				{
					E.Postfix.Append(num);
				}
				else
				{
					E.Postfix.Append('+').Append(num);
				}
				E.Postfix.Append(" on saves");
				if (!string.IsNullOrEmpty(item))
				{
					if (item.Contains(","))
					{
						E.Postfix.Append(" vs. ").Append(Grammar.MakeAndList(SavingThrows.VsList(item)));
					}
					else
					{
						E.Postfix.Append(" vs. ").Append(item);
					}
				}
				E.Postfix.Append('.');
				AddStatusSummary(E.Postfix);
				E.Postfix.Append("}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (IsObjectActivePartSubject(E.Defender) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			foreach (KeyValuePair<string, int> item in Tracking)
			{
				if (SavingThrows.Applicable(item.Key, E))
				{
					E.Roll += item.Value;
				}
			}
		}
		return base.HandleEvent(E);
	}
}
