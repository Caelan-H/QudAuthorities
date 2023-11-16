using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Yonderbrush : IPart
{
	public bool Harvested;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != GetNavigationWeightEvent.ID || Harvested))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!Harvested && (!(ParentObject.GetPart("Hidden") is Hidden hidden) || hidden.Found))
		{
			E.MinWeight(95);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!Harvested && ParentObject.CurrentCell != null)
		{
			if (E.Object != null && E.Object.HasPart("Combat") && ParentObject.pBrain.IsHostileTowards(E.Object) && E.Object != ParentObject)
			{
				Hidden hidden = ParentObject.GetPart("Hidden") as Hidden;
				if ((hidden == null || hidden.Found) && E.Object.HasPart("CookingAndGathering_Harvestry"))
				{
					CookingAndGathering_Harvestry obj = E.Object.GetPart("CookingAndGathering_Harvestry") as CookingAndGathering_Harvestry;
					if (obj.IsMyActivatedAbilityToggledOn(obj.ActivatedAbilityID))
					{
						Harvested = true;
						return true;
					}
				}
				if (hidden != null)
				{
					hidden.Found = true;
				}
				E.Object.RandomTeleport(Swirl: true);
			}
			else if (E.Object != null && E.Object.IsPlayer() && !ParentObject.IsHostileTowards(E.Object) && ParentObject.GetPart("Hidden") is Hidden hidden2)
			{
				hidden2.Reveal();
			}
		}
		return base.HandleEvent(E);
	}
}
