using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TwoHeaded : BaseMutation
{
	private static List<Effect> targetEffects = new List<Effect>();

	public string AdditionsManagerID => ParentObject.id + "::TwoHeaded::Add";

	public string ChangesManagerID => ParentObject.id + "::TwoHeaded::Change";

	public TwoHeaded()
	{
		DisplayName = "Two-Headed";
	}

	public override bool AffectsBodyParts()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != EndTurnEvent.ID)
		{
			return ID == GetEnergyCostEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type != null && E.Type.Contains("Mental"))
		{
			E.PercentageReduction += GetReducedMentalActionCost(base.Level);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (AffectEffect(E.Effect) && GetShakeOff(base.Level).in100())
		{
			BodyPart bodyPart = FindExtraHead();
			if (bodyPart != null && ParentObject.IsPlayer())
			{
				if (E.Effect.ClassName == E.Effect.DisplayName)
				{
					IComponent<GameObject>.AddPlayerMessage("Your " + bodyPart.GetOrdinalName() + " " + (bodyPart.Plural ? "help" : "helps") + " shake off the effect!", 'g');
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("Your " + bodyPart.GetOrdinalName() + " " + (bodyPart.Plural ? "help" : "helps") + " shake off being " + E.Effect.DisplayName + "!", 'g');
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ParentObject.Effects != null && GetShakeOff(base.Level).in100())
		{
			targetEffects.Clear();
			int i = 0;
			for (int count = ParentObject.Effects.Count; i < count; i++)
			{
				if (AffectEffect(ParentObject.Effects[i]))
				{
					targetEffects.Add(ParentObject.Effects[i]);
				}
			}
			if (targetEffects.Count > 0)
			{
				BodyPart bodyPart = FindExtraHead();
				if (bodyPart != null)
				{
					Effect randomElement = targetEffects.GetRandomElement();
					if (randomElement != null)
					{
						if (ParentObject.IsPlayer())
						{
							if (randomElement.DisplayName == randomElement.ClassName)
							{
								IComponent<GameObject>.AddPlayerMessage("Your " + bodyPart.GetOrdinalName() + " " + (bodyPart.Plural ? "help" : "helps") + " you shake off a mental state!", 'g');
							}
							else
							{
								IComponent<GameObject>.AddPlayerMessage("Your " + bodyPart.GetOrdinalName() + " " + (bodyPart.Plural ? "help" : "helps") + " you shake off being " + randomElement.DisplayName + "!", 'g');
							}
						}
						ParentObject.RemoveEffect(randomElement);
					}
				}
			}
			targetEffects.Clear();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You have two heads.";
	}

	public int GetReducedMentalActionCost(int Level)
	{
		return 15 + 5 * Level;
	}

	public int GetShakeOff(int Level)
	{
		return 5 + 2 * Level;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("Mental actions have {{rules|" + GetReducedMentalActionCost(Level) + "%}} lower action costs\n", "{{rules|", GetShakeOff(Level).ToString(), "%}} chance initially and each round to shake off a negative mental status effect");
	}

	public BodyPart FindExtraHead()
	{
		BodyPart bodyPartByManager = ParentObject.GetBodyPartByManager(AdditionsManagerID, "Head");
		if (bodyPartByManager == null)
		{
			return null;
		}
		Body body = ParentObject.Body;
		if (body == null)
		{
			return null;
		}
		if (body.GetPartCount("Head") < 2)
		{
			return null;
		}
		return bodyPartByManager;
	}

	private bool AffectEffect(Effect FX)
	{
		if (FX.IsOfTypes(100663298))
		{
			return !FX.IsOfType(134217728);
		}
		return false;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Body body = GO.Body;
		if (body != null)
		{
			BodyPart body2 = body.GetBody();
			BodyPart firstAttachedPart = body2.GetFirstAttachedPart("Head", 0, body, EvenIfDismembered: true);
			BodyPart bodyPart = firstAttachedPart?.GetFirstAttachedPart("Face", 0, body, EvenIfDismembered: true);
			if (firstAttachedPart == null || firstAttachedPart.Manager != null || bodyPart == null || bodyPart.Manager != null || !firstAttachedPart.IsLateralitySafeToChange(0, body, bodyPart))
			{
				BodyPart bodyPart2 = ((firstAttachedPart == null) ? body2.AddPartAt("Head", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Head", OrInsertBefore: new string[8] { "Back", "Arm", "Leg", "Foot", "Hands", "Feet", "Roots", "Thrown Weapon" }) : body2.AddPartAt(firstAttachedPart, "Head", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID));
				bodyPart2.AddPart("Face", 0, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
			}
			else
			{
				BodyPart bodyPart2 = body2.AddPartAt(firstAttachedPart, "Head", 1, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				bodyPart2.AddPart("Face", 1, null, null, null, null, Category: body2.Category, Manager: AdditionsManagerID);
				firstAttachedPart.ChangeLaterality(2);
				bodyPart.ChangeLaterality(2);
				firstAttachedPart.Manager = ChangesManagerID;
				bodyPart.Manager = ChangesManagerID;
			}
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.RemoveBodyPartsByManager(AdditionsManagerID, EvenIfDismembered: true);
		foreach (BodyPart item in GO.GetBodyPartsByManager(ChangesManagerID))
		{
			if (item.Laterality == 2 && item.IsLateralityConsistent())
			{
				item.ChangeLaterality(0);
			}
		}
		return base.Unmutate(GO);
	}
}
