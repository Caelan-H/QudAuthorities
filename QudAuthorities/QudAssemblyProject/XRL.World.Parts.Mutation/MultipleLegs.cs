using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MultipleLegs : BaseMutation, IRankedMutation
{
	public int Rank = 1;

	public int Bonus;

	public string AdditionsManagerID => ParentObject.id + "::MultipleLegs::Add";

	public string ChangesManagerID => ParentObject.id + "::MultipleLegs::Change";

	public MultipleLegs()
	{
		DisplayName = "Multiple Legs";
	}

	public override bool AffectsBodyParts()
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID)
		{
			return ID == GetMaxCarriedWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		E.AdjustWeight((1.0 + (double)GetCarryCapacityBonus(base.Level) / 100.0) * (double)Rank);
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You have an extra set of legs.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("+{{rules|" + GetMoveSpeedBonus(Level) + "}} move speed\n", "+{{rules|", GetCarryCapacityBonus(Level).ToString(), "%}} carry capacity");
	}

	public int GetMoveSpeedBonus(int Level)
	{
		return Level * 20;
	}

	public int GetCarryCapacityBonus(int Level)
	{
		return Level + 5;
	}

	public int GetRank()
	{
		return Rank;
	}

	public int AdjustRank(int amount)
	{
		Rank += amount;
		return Rank;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		base.StatShifter.SetStatShift(ParentObject, "MoveSpeed", -GetMoveSpeedBonus(NewLevel), baseValue: true);
		CarryingCapacityChangedEvent.Send(ParentObject);
		return base.ChangeLevel(NewLevel);
	}

	public void AddMoreLegs(GameObject GO)
	{
		if (GO == null)
		{
			return;
		}
		Body body = GO.Body;
		if (body == null)
		{
			return;
		}
		BodyPart body2 = body.GetBody();
		BodyPart firstAttachedPart = body2.GetFirstAttachedPart("Feet", 0, body, EvenIfDismembered: true);
		if (firstAttachedPart != null)
		{
			if (firstAttachedPart.Manager != null || !firstAttachedPart.IsLateralitySafeToChange(0, body))
			{
				body2.AddPartAt(firstAttachedPart, "Feet", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				return;
			}
			body2.AddPartAt(firstAttachedPart, "Feet", 64, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
			firstAttachedPart.ChangeLaterality(16);
			firstAttachedPart.Manager = ChangesManagerID;
		}
		else
		{
			body2.AddPartAt("Feet", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Feet", OrInsertBefore: new string[3] { "Roots", "Tail", "Thrown Weapon" });
		}
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		AddMoreLegs(GO);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.RemoveBodyPartsByManager(AdditionsManagerID, EvenIfDismembered: true);
		foreach (BodyPart item in GO.GetBodyPartsByManager(ChangesManagerID, EvenIfDismembered: true))
		{
			if (item.Laterality == 16 && item.IsLateralityConsistent())
			{
				item.ChangeLaterality(item.Laterality & -17);
			}
		}
		base.StatShifter.RemoveStatShifts();
		CarryingCapacityChangedEvent.Send(ParentObject);
		return base.Unmutate(GO);
	}
}
