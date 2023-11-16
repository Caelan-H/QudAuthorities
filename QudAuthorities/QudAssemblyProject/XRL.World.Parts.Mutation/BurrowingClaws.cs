using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BurrowingClaws : BaseDefaultEquipmentMutation
{
	public GameObject ClawsObject;

	public string BodyPartType = "Hands";

	public bool CreateObject = true;

	public Guid DigUpActivatedAbilityID = Guid.Empty;

	public Guid DigDownActivatedAbilityID = Guid.Empty;

	public Guid EnableActivatedAbilityID = Guid.Empty;

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		BurrowingClaws obj = base.DeepCopy(Parent, MapInv) as BurrowingClaws;
		obj.ClawsObject = null;
		return obj;
	}

	public BurrowingClaws()
	{
		DisplayName = "Burrowing Claws";
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterGameLoadedEvent.ID)
		{
			return ID == PartSupportEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "Digging" && IsMyActivatedAbilityToggledOn(EnableActivatedAbilityID))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		NeedPartSupportEvent.Send(ParentObject, "Digging");
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandDigDown");
		Object.RegisterPartEvent(this, "CommandDigUp");
		Object.RegisterPartEvent(this, "CommandToggleBurrowingClaws");
		Object.RegisterPartEvent(this, "DealDamage");
		Object.RegisterPartEvent(this, "GetAttackerHitDice");
		base.Register(Object);
	}

	public bool CheckDig()
	{
		if (ParentObject.AreHostilesNearby())
		{
			Popup.ShowFail("You can't excavate with hostiles nearby.");
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandToggleBurrowingClaws")
		{
			ToggleMyActivatedAbility(EnableActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(EnableActivatedAbilityID))
			{
				ParentObject.RequirePart<Digging>();
			}
			else
			{
				NeedPartSupportEvent.Send(ParentObject, "Digging");
			}
			ParentObject.RemoveStringProperty("Burrowing");
		}
		else if (E.ID == "CommandDigDown")
		{
			if (CheckDig())
			{
				Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
				ParentObject.CurrentCell.AddObject("StairsDown");
				cellFromDirection.AddObject("StairsUp");
			}
		}
		else if (E.ID == "CommandDigUp")
		{
			if (ParentObject.CurrentCell.ParentZone.Z <= 10)
			{
				Popup.ShowFail("You can't excavate the sky!");
				return true;
			}
			if (CheckDig())
			{
				Cell cellFromDirection2 = ParentObject.CurrentCell.GetCellFromDirection("U", BuiltOnly: false);
				ParentObject.CurrentCell.AddObject("StairsUp");
				cellFromDirection2.AddObject("StairsDown");
			}
		}
		else if (E.ID == "GetAttackerHitDice")
		{
			if (E.GetGameObjectParameter("Defender").IsDiggable())
			{
				E.SetParameter("PenetrationBonus", E.GetIntParameter("PenetrationBonus") + 3 * base.Level);
			}
		}
		else if (E.ID == "DealDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter.IsDiggable())
			{
				Damage damage = E.GetParameter("Damage") as Damage;
				damage.Amount = (int)Math.Ceiling((float)gameObjectParameter.BaseStat("Hitpoints") / 4f);
				E.SetParameter("Damage", damage);
			}
		}
		return base.FireEvent(E);
	}

	public string GetPenetration(int Level)
	{
		return "1d6+" + 3 * Level;
	}

	public int GetAV(int Level)
	{
		if (Level < 5)
		{
			return 1;
		}
		if (Level < 9)
		{
			return 2;
		}
		return 3;
	}

	public override string GetDescription()
	{
		return "You bear spade-like claws that can burrow through the earth.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "4 successful attacks dig through a wall\n";
		text = text + "Claw penetration vs walls: {{rules|" + 3 * Level + "}}\n";
		if (Options.EnablePrereleaseContent)
		{
			text += "Can dig passages up or down when outside of combat\n";
		}
		return text + "Claws are also a short-blade class natural weapon that deal {{rules|" + GetClawsDamage(Level) + "}} base damage to non-walls.";
	}

	public string GetClawsDamage(int Level)
	{
		if (Level <= 3)
		{
			return "1d2";
		}
		if (Level <= 6)
		{
			return "1d3";
		}
		if (Level <= 9)
		{
			return "1d4";
		}
		if (Level <= 12)
		{
			return "1d6";
		}
		if (Level <= 15)
		{
			return "1d8";
		}
		if (Level <= 18)
		{
			return "1d10";
		}
		return "1d12";
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		if (CreateObject)
		{
			List<BodyPart> list = (from p in body.GetParts()
				where p.Type == "Hand"
				select p).ToList();
			for (int i = 0; i < list.Count && i < 2; i++)
			{
				list[i].DefaultBehavior = GameObject.create("Burrowing Claws Claw");
				list[i].DefaultBehavior.GetPart<MeleeWeapon>().BaseDamage = GetClawsDamage(base.Level);
			}
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (Options.EnablePrereleaseContent)
		{
			DigUpActivatedAbilityID = AddMyActivatedAbility("Excavate up", "CommandDigUp", "Physical Mutation", null, "\u0018");
			DigDownActivatedAbilityID = AddMyActivatedAbility("Excavate down", "CommandDigDown", "Physical Mutation", null, "\u0019");
		}
		EnableActivatedAbilityID = AddMyActivatedAbility("Burrowing Claws", "CommandToggleBurrowingClaws", "Physical Mutation", null, "ë", null, Toggleable: true, DefaultToggleState: true);
		if (IsMyActivatedAbilityToggledOn(EnableActivatedAbilityID))
		{
			GO.RequirePart<Digging>();
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		NeedPartSupportEvent.Send(GO, "Digging", this);
		RemoveMyActivatedAbility(ref DigUpActivatedAbilityID);
		RemoveMyActivatedAbility(ref DigDownActivatedAbilityID);
		RemoveMyActivatedAbility(ref EnableActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
