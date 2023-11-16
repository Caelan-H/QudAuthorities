using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Horns : BaseMutation
{
	public string BodyPartType = "Head";

	public GameObject HornsObject;

	public string HornsName;

	[NonSerialized]
	private List<string> variants = new List<string> { "horns", "horn", "antlers", "casque" };

	public Horns()
	{
		DisplayName = "Horns";
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
		if (Variant == null)
		{
			return "Horns jut out of your head.";
		}
		if (Variant == "casque" || Variant == "horn")
		{
			return "A " + Variant + " juts out of your head.";
		}
		return Grammar.InitCap(Variant) + " jut out of your head.";
	}

	public int GetAV(int Level)
	{
		return 1 + (Level - 1) / 3;
	}

	public string GetBaseDamage(int Level)
	{
		return "2d" + (3 + Level / 2);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Horns obj = base.DeepCopy(Parent, MapInv) as Horns;
		obj.HornsObject = null;
		return obj;
	}

	public override string GetLevelText(int Level)
	{
		string baseDamage = GetBaseDamage(Level);
		int aV = GetAV(Level);
		string text = "20% chance on melee attack to gore your opponent\n";
		text = text + "Damage increment: {{rules|" + baseDamage + "}}\n";
		text = ((Level != base.Level) ? (text + "{{rules|Increased bleeding save difficulty and intensity}}\n") : (text + "Goring attacks may cause bleeding\n"));
		text = ((Variant == null) ? (text + "Horns are a short-blade class natural weapon.\n") : ((!(Variant == "casque") && !(Variant == "horn")) ? (text + Grammar.InitCap(Variant) + " are a short-blade class natural weapon.\n") : (text + "A " + Variant + " is a short-blade class natural weapon.\n")));
		text = text + "+{{rules|" + aV + " AV}}\n";
		text += "Cannot wear helmets\n";
		return text + "+100 reputation with {{w|antelopes}} and {{w|goatfolk}}";
	}

	public override bool ChangeLevel(int NewLevel)
	{
		Body body = ParentObject.Body;
		if (body != null)
		{
			BodyPart firstPart = body.GetFirstPart(BodyPartType);
			if (firstPart != null)
			{
				firstPart.ForceUnequip(Silent: true);
				if (HornsObject == null)
				{
					HornsObject = GameObject.create("Horns");
				}
				MeleeWeapon obj = HornsObject.GetPart("MeleeWeapon") as MeleeWeapon;
				Armor armor = HornsObject.GetPart("Armor") as Armor;
				if (string.IsNullOrEmpty(HornsName))
				{
					HornsName = DisplayName.ToLower();
				}
				HornsObject.pRender.DisplayName = HornsName;
				obj.MaxStrengthBonus = 100;
				armor.WornOn = firstPart.Type;
				obj.BaseDamage = GetBaseDamage(base.Level);
				armor.AV = GetAV(base.Level);
				ParentObject.ForceEquipObject(HornsObject, firstPart, Silent: true, 0);
			}
		}
		return base.ChangeLevel(NewLevel);
	}

	public override List<string> GetVariants()
	{
		return variants;
	}

	public override void SetVariant(int n)
	{
		HornsName = variants[n];
		DisplayName = char.ToUpper(HornsName[0]) + HornsName.Substring(1);
		base.SetVariant(n);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.Blueprint.Contains("Goat"))
		{
			DisplayName = "Horns";
			HornsName = "horns";
		}
		else if (GO.Blueprint.Contains("Rhino"))
		{
			DisplayName = "Horn";
			HornsName = "horn";
		}
		else if (string.IsNullOrEmpty(HornsName))
		{
			int num = Stat.Random(1, 100);
			if (num <= 35)
			{
				HornsName = "horns";
			}
			else if (num <= 65)
			{
				HornsName = "antlers";
			}
			else
			{
				HornsName = "horn";
			}
		}
		if (HornsName != null)
		{
			DisplayName = char.ToUpper(HornsName[0]) + HornsName.Substring(1);
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		CleanUpMutationEquipment(GO, ref HornsObject);
		return base.Unmutate(GO);
	}
}
