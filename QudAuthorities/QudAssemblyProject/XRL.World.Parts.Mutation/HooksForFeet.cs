using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
[Obsolete("save compat, replace with HooksForFeet2")]
public class HooksForFeet : BaseMutation
{
	public string BodyPartType = "Feet";

	public GameObject HooksObject;

	public HooksForFeet()
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

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		BodyPart firstPart = ParentObject.Body.GetFirstPart(BodyPartType);
		if (firstPart != null)
		{
			firstPart.ForceUnequip(Silent: true);
			HooksObject = GameObjectFactory.Factory.CreateObject("Hooks");
			MeleeWeapon part = HooksObject.GetPart<MeleeWeapon>();
			Armor part2 = HooksObject.GetPart<Armor>();
			_ = HooksObject.pRender;
			part.Skill = "ShortBlades";
			part.BaseDamage = "1";
			part2.WornOn = firstPart.Type;
			part2.AV = 0;
			ParentObject.ForceEquipObject(HooksObject, firstPart, Silent: true, 0);
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		CleanUpMutationEquipment(GO, ref HooksObject);
		return base.Unmutate(GO);
	}
}
