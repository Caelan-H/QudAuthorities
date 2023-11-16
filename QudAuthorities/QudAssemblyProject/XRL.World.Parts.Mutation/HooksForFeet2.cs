using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HooksForFeet2 : BaseDefaultEquipmentMutation
{
	public string BodyPartType = "Feet";

	public GameObject HooksObject;

	public HooksForFeet2()
	{
		DisplayName = "Hooks for Feet ({{r|D}})";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You have hooks for feet.\n\nYou cannot wear shoes.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override void OnRegenerateDefaultEquipment(Body Body)
	{
		if (!GameObject.validate(ref HooksObject))
		{
			HooksObject = GameObjectFactory.Factory.CreateObject("Hooks");
		}
		BodyPart bodyPart = RequireRegisteredSlot(Body, BodyPartType);
		if (bodyPart != null && bodyPart.Equipped != HooksObject && bodyPart.ForceUnequip(Silent: true))
		{
			MeleeWeapon part = HooksObject.GetPart<MeleeWeapon>();
			part.Skill = "ShortBlades";
			part.Slot = bodyPart.Type;
			part.BaseDamage = "1";
			Armor part2 = HooksObject.GetPart<Armor>();
			part2.WornOn = bodyPart.Type;
			part2.AV = 0;
			ParentObject.ForceEquipObject(HooksObject, bodyPart, Silent: true, 0);
		}
		base.OnRegenerateDefaultEquipment(Body);
	}

	public override bool Unmutate(GameObject GO)
	{
		CleanUpMutationEquipment(GO, ref HooksObject);
		return base.Unmutate(GO);
	}
}
