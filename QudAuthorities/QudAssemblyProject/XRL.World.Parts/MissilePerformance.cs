using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class MissilePerformance : IActivePart
{
	public int PenetrationModifier;

	public int PenetrationCapModifier;

	public int DamageDieModifier;

	public int DamageModifier;

	public string AddAttributes;

	public string RemoveAttributes;

	public bool? PenetrateCreatures;

	public MissilePerformance()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		MissilePerformance missilePerformance = p as MissilePerformance;
		if (missilePerformance.PenetrationModifier != PenetrationModifier)
		{
			return false;
		}
		if (missilePerformance.PenetrationCapModifier != PenetrationCapModifier)
		{
			return false;
		}
		if (missilePerformance.DamageDieModifier != DamageDieModifier)
		{
			return false;
		}
		if (missilePerformance.DamageModifier != DamageModifier)
		{
			return false;
		}
		if (missilePerformance.AddAttributes != AddAttributes)
		{
			return false;
		}
		if (missilePerformance.RemoveAttributes != RemoveAttributes)
		{
			return false;
		}
		if (missilePerformance.PenetrateCreatures != PenetrateCreatures)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetMissileWeaponPerformanceEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		if (IsObjectActivePartSubject(E.Subject) && IsReady(E.Active, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (PenetrationModifier != 0)
			{
				E.BasePenetration += PenetrationModifier;
				E.PenetrationCap += PenetrationModifier;
			}
			if (PenetrationCapModifier != 0)
			{
				E.PenetrationCap += PenetrationCapModifier;
			}
			if (DamageDieModifier != 0)
			{
				E.GetDamageRoll()?.AdjustDieSize(DamageDieModifier);
			}
			if (DamageModifier != 0)
			{
				E.GetDamageRoll()?.AdjustResult(DamageModifier);
			}
			List<string> list = null;
			bool flag = false;
			if (!string.IsNullOrEmpty(RemoveAttributes) && !string.IsNullOrEmpty(E.Attributes))
			{
				List<string> list2 = RemoveAttributes.CachedCommaExpansion();
				if (list == null)
				{
					list = new List<string>(E.Attributes.Split(' '));
				}
				int i = 0;
				for (int count = list2.Count; i < count; i++)
				{
					if (list.Contains(list2[i]))
					{
						list.Remove(list2[i]);
						flag = true;
					}
				}
			}
			if (!string.IsNullOrEmpty(AddAttributes))
			{
				List<string> list3 = AddAttributes.CachedCommaExpansion();
				if (list == null)
				{
					list = (string.IsNullOrEmpty(E.Attributes) ? new List<string>() : new List<string>(E.Attributes.Split(' ')));
				}
				int j = 0;
				for (int count2 = list3.Count; j < count2; j++)
				{
					if (!list.Contains(list3[j]))
					{
						list.Add(list3[j]);
						flag = true;
					}
				}
			}
			if (flag)
			{
				E.Attributes = string.Join(" ", list.ToArray());
			}
			if (PenetrateCreatures.HasValue)
			{
				E.PenetrateCreatures = PenetrateCreatures.Value;
			}
		}
		return base.HandleEvent(E);
	}

	public bool WantAddAttribute(string attr)
	{
		if (string.IsNullOrEmpty(AddAttributes))
		{
			AddAttributes = attr;
			return true;
		}
		List<string> list = AddAttributes.CachedCommaExpansion();
		if (!list.Contains(attr))
		{
			list.Add(attr);
			AddAttributes = string.Join(",", list.ToArray());
			return true;
		}
		return false;
	}

	public bool WantRemoveAttribute(string attr)
	{
		if (string.IsNullOrEmpty(RemoveAttributes))
		{
			RemoveAttributes = attr;
			return true;
		}
		List<string> list = RemoveAttributes.CachedCommaExpansion();
		if (!list.Contains(attr))
		{
			list.Remove(attr);
			RemoveAttributes = string.Join(",", list.ToArray());
			return true;
		}
		return false;
	}
}
