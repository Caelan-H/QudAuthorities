using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Beak : BaseDefaultEquipmentMutation
{
	public GameObject BeakObject;

	public string BodyPartType = "Face";

	public string BeakName = "beak";

	[NonSerialized]
	private List<string> variants = new List<string> { "Beak", "Bill", "Rostrum", "Frill", "Proboscis" };

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Beak obj = base.DeepCopy(Parent, MapInv) as Beak;
		obj.BeakObject = null;
		return obj;
	}

	public Beak()
	{
		DisplayName = "Beak";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		if (BeakName == "")
		{
			return "Your face bears a sightly beak.\n\n+1 Ego\nYou occasionally peck at your opponents.\n+200 reputation with {{w|birds}}";
		}
		return "Your face bears a sightly " + BeakName + ".\n\n+1 Ego\nYou occasionally peck at your opponents.\n+200 reputation with {{w|birds}}";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		BodyPart bodyPart;
		if (!HasRegisteredSlot(BodyPartType))
		{
			bodyPart = body.GetFirstPart(BodyPartType);
			if (bodyPart != null)
			{
				RegisterSlot(BodyPartType, bodyPart);
			}
		}
		else
		{
			bodyPart = GetRegisteredSlot(BodyPartType, evenIfDismembered: false);
		}
		if (bodyPart != null)
		{
			BeakObject = GameObjectFactory.Factory.CreateObject("Beak");
			MeleeWeapon part = BeakObject.GetPart<MeleeWeapon>();
			Armor part2 = BeakObject.GetPart<Armor>();
			BeakObject.pRender.DisplayName = BeakName;
			part.Skill = "ShortBlades";
			part.BaseDamage = "1";
			part.Slot = bodyPart.Type;
			part2.WornOn = bodyPart.Type;
			part2.AV = 0;
			bodyPart.DefaultBehavior = BeakObject;
		}
		base.OnRegenerateDefaultEquipment(body);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override List<string> GetVariants()
	{
		return variants;
	}

	public override void SetVariant(int n)
	{
		BeakName = variants[n].ToLower();
		DisplayName = char.ToUpper(BeakName[0]) + BeakName.Substring(1);
		base.SetVariant(n);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		base.StatShifter.SetStatShift(ParentObject, "Ego", 1, baseValue: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		base.StatShifter.RemoveStatShifts(ParentObject);
		CleanUpMutationEquipment(GO, ref BeakObject);
		return base.Unmutate(GO);
	}
}
