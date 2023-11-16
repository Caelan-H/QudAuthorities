using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class Shield : IPart
{
	public string WornOn = "Arm";

	public int AV;

	public int DV;

	public int SpeedPenalty;

	[NonSerialized]
	public int Blocks;

	public override bool SameAs(IPart p)
	{
		Shield shield = p as Shield;
		if (shield.WornOn != WornOn)
		{
			return false;
		}
		if (shield.AV != AV)
		{
			return false;
		}
		if (shield.DV != DV)
		{
			return false;
		}
		if (shield.SpeedPenalty != SpeedPenalty)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShieldBlockPreferenceEvent.ID && ID != GetShortDescriptionEvent.ID && ID != QueryEquippableListEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShieldBlockPreferenceEvent E)
	{
		E.Preference += AV * 100;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (!E.List.Contains(ParentObject) && E.SlotType == WornOn)
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		base.StatShifter.SetStatShift(E.Actor, "DV", DV);
		base.StatShifter.SetStatShift(E.Actor, "Speed", -SpeedPenalty);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		base.StatShifter.RemoveStatShifts(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("{{b|").Append('\u0004').Append("}}")
				.Append(AV)
				.Append(" {{K|")
				.Append('\t')
				.Append("}}")
				.Append(DV);
			E.AddTag(stringBuilder.ToString(), -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Shields only grant their AV when you successfully block an attack.");
		return base.HandleEvent(E);
	}
}
