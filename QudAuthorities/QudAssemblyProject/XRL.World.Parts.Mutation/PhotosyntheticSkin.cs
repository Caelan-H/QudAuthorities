using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class PhotosyntheticSkin : BaseMutation
{
	public Guid BaskActivatedAbilityID = Guid.Empty;

	public string oldBleedLiquid;

	public string oldBleedColor;

	public string oldBleedPrefix;

	public int SoakCounter;

	private bool MutationColor = Options.MutationColor && !Options.HPColor;

	public PhotosyntheticSkin()
	{
		DisplayName = "Photosynthetic Skin";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandBask");
		Object.RegisterPartEvent(this, "VisibleStatusColor");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You replenish yourself by absorbing sunlight through your hearty green skin.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text = text + "You can bask in the sunlight instead of eating a meal to gain a special metabolizing effect for {{rules|" + GetBonusDurationString(Level) + "}}: +{{rules|" + GetBonusRegeneration(Level) + "%}} to natural healing rate and +{{rules|" + GetBonusQuickness(Level) + "}} Quickness\n";
		text = text + "While in the sunlight, you accrue starch and lignin that you can use as ingredients in meals you cook (max {{rules|" + GetStarchServings(Level) + "}} of each).\n";
		return text + "+200 reputation with {{w|roots}}, {{w|trees}}, {{w|vines}}, and {{w|the Consortium of Phyta}}";
	}

	public static int GetBonusRegeneration(int Level)
	{
		return 20 + Level * 10;
	}

	public static int GetBonusQuickness(int Level)
	{
		return 13 + Level * 2;
	}

	public static string GetStarchServings(int Level)
	{
		int num = (Level - 1) / 4 + 1;
		if (num != 1)
		{
			return num + " servings";
		}
		return num + " serving";
	}

	public static int GetBonusDuration(int Level)
	{
		return (Level - 1) / 4 + 1;
	}

	public static string GetBonusDurationString(int Level)
	{
		int bonusDuration = GetBonusDuration(Level);
		if (bonusDuration != 1)
		{
			return bonusDuration + " days";
		}
		return bonusDuration + " day";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (ParentObject.IsUnderSky() && IsDay() && !ParentObject.IsTemporary)
			{
				SoakCounter++;
				if (SoakCounter > 300)
				{
					SoakCounter = 0;
					int num = (base.Level - 1) / 4 + 1;
					Inventory inventory = ParentObject.Inventory;
					if (inventory.Count("Starch") < num)
					{
						inventory.AddObject("Starch", bSilent: true);
					}
					if (inventory.Count("Lignin") < num)
					{
						inventory.AddObject("Lignin", bSilent: true);
					}
				}
			}
		}
		else if (E.ID == "VisibleStatusColor")
		{
			if (E.GetStringParameter("Color") == "&Y" && (!ParentObject.IsPlayer() || Options.MutationColor) && ParentObject.GetIntProperty("DontOverrideColor") < 1)
			{
				E.SetParameter("Color", "&g");
			}
		}
		else if (E.ID == "CommandBask")
		{
			if (ParentObject.AreHostilesNearby())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You can't bask with hostiles nearby.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities("Bask", ShowMessage: true))
			{
				return false;
			}
			if (IsDay())
			{
				if (ParentObject.CurrentCell.ParentZone.IsWorldMap() || ParentObject.CurrentCell.ConsideredOutside())
				{
					ProceduralCookingEffect proceduralCookingEffect = ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainPhotosyntheticSkin_RegenerationUnit", "CookingDomainPhotosyntheticSkin_UnitQuickness", "CookingDomainPhotosyntheticSkin_SatedUnit" });
					ParentObject.FireEvent("ClearFoodEffects");
					ParentObject.CleanEffects();
					proceduralCookingEffect.Init(ParentObject);
					proceduralCookingEffect.Duration = 1200 * GetBonusDuration(base.Level);
					ParentObject.ApplyEffect(proceduralCookingEffect);
					ParentObject.GetPart<Stomach>().HungerLevel = 0;
					ParentObject.GetPart<Stomach>().CookingCounter = 0;
					ParentObject.RemoveEffect("Famished");
					Popup.Show("You bask in the sunlight and absorb the nourishing rays.");
					Popup.Show("You start to metabolize the meal, gaining the following effect for the rest of the day:\n\n&W" + proceduralCookingEffect.GetDetails());
				}
				else
				{
					Popup.Show("You need sunlight to bask in.");
				}
			}
			else
			{
				Popup.Show("You need sunlight to bask in.");
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.GetIntProperty("DontOverrideColor") >= 1)
		{
			return true;
		}
		if (ParentObject.IsPlayerControlled())
		{
			if ((XRLCore.FrameTimer.ElapsedMilliseconds & 0x7F) == 0L)
			{
				MutationColor = Options.MutationColor && !Options.HPColor;
			}
			if (!MutationColor)
			{
				return true;
			}
		}
		E.ColorString = "&g";
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		BaskActivatedAbilityID = AddMyActivatedAbility("Bask", "CommandBask", "Physical Mutation", "Bask in the sunlight and absorb the sun's nourishing rays.\n\n+20% + (Photosynthetic Skin level * 10)% to natural healing rate\n\n+13 + (Photosynthetic Skin level * 2) Quickness", "\u000f");
		oldBleedLiquid = ParentObject.GetPropertyOrTag("BleedLiquid", "blood-1000");
		oldBleedLiquid = ParentObject.GetPropertyOrTag("BleedPrefix", "{{r|bloody}}");
		oldBleedLiquid = ParentObject.GetPropertyOrTag("BleedColor", "&r");
		ParentObject.SetStringProperty("BleedLiquid", "blood-500,sap-500");
		ParentObject.SetStringProperty("BleedPrefix", "{{r|bloody}} and {{Y|sugary}}");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref BaskActivatedAbilityID);
		ParentObject.SetStringProperty("BleedLiquid", oldBleedLiquid);
		ParentObject.SetStringProperty("BleedPrefix", oldBleedPrefix);
		ParentObject.SetStringProperty("BleedColor", oldBleedColor);
		return base.Unmutate(GO);
	}
}
