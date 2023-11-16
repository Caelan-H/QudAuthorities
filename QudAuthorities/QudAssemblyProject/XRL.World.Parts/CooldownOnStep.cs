using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CooldownOnStep : IPart
{
	public string ClusterSize = "1";

	public string CooldownDamage = "1d10";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (ClusterSize != "1")
		{
			List<Cell> list = new List<Cell>(ParentObject.CurrentCell.GetLocalEmptyAdjacentCells()).ShuffleInPlace();
			int num = ClusterSize.RollCached();
			for (int i = 0; i < num && i < list.Count; i++)
			{
				if (15.in100())
				{
					list[i].AddObject(ParentObject.Blueprint);
					continue;
				}
				GameObject gameObject = GameObject.create(ParentObject.Blueprint);
				gameObject.GetPart<CooldownOnStep>().ClusterSize = "1";
				list[i].AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object?.ActivatedAbilities != null && ParentObject.IsHostileTowards(E.Object) && ParentObject.PhaseAndFlightMatches(E.Object))
		{
			Hidden hidden = ParentObject.GetPart("Hidden") as Hidden;
			if (!hidden.Found)
			{
				hidden.Found = true;
				DidXToY("prick", E.Object, "with " + ParentObject.its + " neuronal thorns", null, null, null, E.Object);
				ActivatedAbilities activatedAbilities = E.Object.ActivatedAbilities;
				if (activatedAbilities?.AbilityByGuid != null)
				{
					int num = Stat.Roll(CooldownDamage);
					foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in activatedAbilities.AbilityByGuid)
					{
						item.Value.Cooldown += num;
					}
				}
				E.Object.Splatter("^B!");
			}
		}
		else if (E.Object != null && E.Object.IsPlayer() && !ParentObject.IsHostileTowards(E.Object) && ParentObject.GetPart("Hidden") is Hidden hidden2)
		{
			hidden2.Reveal();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
