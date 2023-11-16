using System;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Roboticized : IPart
{
	public const string PREFIX_NAME = "{{c|mechanical}}";

	public const string POSTFIX_DESC = "There is a low, persistent hum emanating outward.";

	public int ChanceOneIn = 10000;

	public string NamePrefix = "{{c|mechanical}}";

	public string DescriptionPostfix = "There is a low, persistent hum emanating outward.";

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Stat.Random(1, ChanceOneIn) == 1)
		{
			Roboticize(ParentObject, NamePrefix, DescriptionPostfix);
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public static void Roboticize(GameObject Object, string NamePrefix = "{{c|mechanical}}", string DescriptionPostfix = "There is a low, persistent hum emanating outward.")
	{
		if (Object.HasPart("Robot"))
		{
			return;
		}
		Object.AddPart(new Robot());
		Object.RequirePart<MentalShield>();
		Object.RequirePart<Metal>();
		if (!Object.HasPart("DarkVision"))
		{
			Object.GetPart<Mutations>().AddMutation(new DarkVision(), 12);
		}
		if (Object.GetPart("Corpse") is Corpse corpse)
		{
			corpse.CorpseChance = 0;
			corpse.BurntCorpseChance = 0;
			corpse.VaporizedCorpseChance = 0;
		}
		Object.GetStat("ElectricResistance").BaseValue = -50;
		Object.GetStat("HeatResistance").BaseValue = 25;
		Object.GetStat("ColdResistance").BaseValue = 25;
		Object.SetIntProperty("Inorganic", 1);
		Object.SetIntProperty("Bleeds", 1);
		Object.SetStringProperty("SeveredLimbBlueprint", "RobotLimb");
		Object.SetStringProperty("SeveredHeadBlueprint", "RobotHead1");
		Object.SetStringProperty("SeveredFaceBlueprint", "RobotFace");
		Object.SetStringProperty("SeveredArmBlueprint", "RobotArm");
		Object.SetStringProperty("SeveredHandBlueprint", "RobotHand");
		Object.SetStringProperty("SeveredLegBlueprint", "RobotLeg");
		Object.SetStringProperty("SeveredFootBlueprint", "RobotFoot");
		Object.SetStringProperty("SeveredFeetBlueprint", "RobotFeet");
		Object.SetStringProperty("SeveredTailBlueprint", "RobotTail");
		Object.SetStringProperty("SeveredRootsBlueprint", "RobotRoots");
		Object.SetStringProperty("SeveredFinBlueprint", "RobotFin");
		Object.SetStringProperty("BleedLiquid", "oil-1000");
		Object.SetStringProperty("BleedColor", "&K");
		Object.SetStringProperty("BleedPrefix", "&Koily");
		Render pRender = Object.pRender;
		string text = (pRender.TileColor.IsNullOrEmpty() ? pRender.ColorString : pRender.TileColor);
		pRender.ColorString = "&c";
		pRender.TileColor = "&c";
		if (pRender.DetailColor == "c")
		{
			pRender.DetailColor = ColorUtility.FindLastForeground(text)?.ToString() ?? Crayons.GetRandomColor();
		}
		if (!string.IsNullOrEmpty(NamePrefix))
		{
			pRender.DisplayName = NamePrefix + " " + pRender.DisplayName;
		}
		Object.Body?.CategorizeAll(7);
		if (!string.IsNullOrEmpty(DescriptionPostfix) && Object.HasPart("Description"))
		{
			if (Object.HasTag("VerseDescription"))
			{
				Description part = Object.GetPart<Description>();
				part._Short = part._Short + "\n\n" + DescriptionPostfix;
			}
			else
			{
				Description part2 = Object.GetPart<Description>();
				part2._Short = part2._Short + " " + DescriptionPostfix;
			}
		}
	}
}
