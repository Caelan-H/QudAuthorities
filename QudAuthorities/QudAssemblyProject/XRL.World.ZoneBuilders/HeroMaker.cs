using System;
using XRL.Names;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ZoneBuilders;

public class HeroMaker
{
	public static GameObject CreateHero(string BaseBlueprint)
	{
		return MakeHero(GameObject.create(BaseBlueprint));
	}

	public static GameObject MakeHero(GameObject BaseCreature, string AdditionalBaseTemplate = null, string AdditionalSpecializationTemplate = null, int tierOverride = -1, string SpecialType = "Hero")
	{
		string[] additionalBaseTemplates = ((AdditionalBaseTemplate != null) ? new string[1] { AdditionalBaseTemplate } : new string[0]);
		string[] additionalSpecializationTemplates = ((AdditionalSpecializationTemplate != null) ? new string[1] { AdditionalSpecializationTemplate } : new string[0]);
		return MakeHero(BaseCreature, additionalBaseTemplates, additionalSpecializationTemplates, tierOverride, SpecialType);
	}

	public static string ResolveTemplateTag(GameObject BaseCreature, string Tag, string Default, string[] AdditionalBaseTemplates, string[] AdditionalSpecializationTemplates)
	{
		string @default = Default;
		string[] array = AdditionalBaseTemplates;
		foreach (string key in array)
		{
			if (GameObjectFactory.Factory.Blueprints.ContainsKey(key))
			{
				@default = GameObjectFactory.Factory.Blueprints[key].GetTag(Tag, @default);
			}
		}
		@default = BaseCreature.GetTag(Tag, @default);
		array = AdditionalSpecializationTemplates;
		foreach (string key2 in array)
		{
			if (GameObjectFactory.Factory.Blueprints.ContainsKey(key2))
			{
				@default = GameObjectFactory.Factory.Blueprints[key2].GetTag(Tag, @default);
			}
		}
		return BaseCreature.GetStringProperty(Tag, @default);
	}

	public static GameObject MakeHero(GameObject BaseCreature, string[] AdditionalBaseTemplates, string[] AdditionalSpecializationTemplates, int tierOverride = -1, string SpecialType = "Hero")
	{
		try
		{
			if (BaseCreature.GetIntProperty("Hero") > 0)
			{
				return BaseCreature;
			}
			if (!BaseCreature.HasPart("Brain"))
			{
				AnimateObject.Animate(BaseCreature);
			}
			BaseCreature.SetIntProperty("Hero", 1);
			BaseCreature.SetStringProperty("Role", "Hero");
			string text = ResolveTemplateTag(BaseCreature, "HeroNameColor", "M", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text2 = ResolveTemplateTag(BaseCreature, "HeroTileColor", "&M", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text3 = ResolveTemplateTag(BaseCreature, "HeroColorString", "&M", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text4 = ResolveTemplateTag(BaseCreature, "HeroDetailColor", "same", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!string.IsNullOrEmpty(text2) && text2 != "same")
			{
				BaseCreature.pRender.TileColor = text2;
			}
			if (!string.IsNullOrEmpty(text3) && text3 != "same")
			{
				BaseCreature.pRender.ColorString = text3;
			}
			if (!string.IsNullOrEmpty(text4) && text4 != "same")
			{
				BaseCreature.pRender.DetailColor = text4;
			}
			if (BaseCreature.HasStat("Strength"))
			{
				BaseCreature.GetStat("Strength").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroStrBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Intelligence"))
			{
				BaseCreature.GetStat("Intelligence").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroIntBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Toughness"))
			{
				BaseCreature.GetStat("Toughness").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroTouBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Willpower"))
			{
				BaseCreature.GetStat("Willpower").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroWilBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Ego"))
			{
				BaseCreature.GetStat("Ego").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroEgoBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Agility"))
			{
				BaseCreature.GetStat("Agility").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroAgiBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Hitpoints"))
			{
				BaseCreature.GetStat("Hitpoints").BaseValue *= Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroHPBoost", "2", AdditionalBaseTemplates, AdditionalSpecializationTemplates));
			}
			if (BaseCreature.HasStat("Level"))
			{
				BaseCreature.GetStat("Level").BaseValue = Math.Max((int)((double)BaseCreature.GetStat("Level").Value * Convert.ToDouble(ResolveTemplateTag(BaseCreature, "HeroLevelMultiplier", "1.5", AdditionalBaseTemplates, AdditionalSpecializationTemplates))), Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroMinLevel", "0", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("XP"))
			{
				BaseCreature.GetStat("XP").BaseValue = BaseCreature.GetStat("Level").Value * Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroXPPerLevel", "250", AdditionalBaseTemplates, AdditionalSpecializationTemplates));
			}
			string text5 = ResolveTemplateTag(BaseCreature, "HeroSkills", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!string.IsNullOrEmpty(text5))
			{
				foreach (string item in text5.CachedCommaExpansion())
				{
					BaseCreature.AddSkill(item);
				}
			}
			string text6 = ResolveTemplateTag(BaseCreature, "HeroSelfPreservationThreshold", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!string.IsNullOrEmpty(text6))
			{
				BaseCreature.RequirePart<AISelfPreservation>().Threshold = text6.RollCached();
			}
			string value = ResolveTemplateTag(BaseCreature, "SimpleConversation", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text7 = ResolveTemplateTag(BaseCreature, "HeroConversation", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!string.IsNullOrEmpty(text7))
			{
				BaseCreature.RequirePart<ConversationScript>().ConversationID = text7;
			}
			else if (!string.IsNullOrEmpty(value))
			{
				BaseCreature.RequirePart<ConversationScript>();
				BaseCreature.SetStringProperty("SimpleConversation", value);
			}
			int num = ResolveTemplateTag(BaseCreature, "HeroMentalMutations", "0-2", AdditionalBaseTemplates, AdditionalSpecializationTemplates).RollCached();
			int num2 = ResolveTemplateTag(BaseCreature, "HeroPhysicalMutations", "0-2", AdditionalBaseTemplates, AdditionalSpecializationTemplates).RollCached();
			if (ResolveTemplateTag(BaseCreature, "HeroGenotype", "none", AdditionalBaseTemplates, AdditionalSpecializationTemplates) == "True Kin")
			{
				num = 0;
				num2 = 0;
			}
			if (BaseCreature.HasTag("Robot"))
			{
				num = 0;
				num2 = 0;
			}
			if (ResolveTemplateTag(BaseCreature, "HeroGenotype", "none", AdditionalBaseTemplates, AdditionalSpecializationTemplates) == "Esper")
			{
				num2 = 0;
				num++;
			}
			if (ResolveTemplateTag(BaseCreature, "HeroGenotype", "none", AdditionalBaseTemplates, AdditionalSpecializationTemplates) == "Chimera")
			{
				num = 0;
				num2++;
			}
			Mutations mutations = BaseCreature.GetPart("Mutations") as Mutations;
			for (int i = 0; i < num; i++)
			{
				BaseMutation baseMutation = null;
				do
				{
					baseMutation = MutationFactory.GetRandomMutation("Mental");
				}
				while (baseMutation != null && mutations.HasMutation(baseMutation));
				if (baseMutation != null)
				{
					mutations.AddMutation(baseMutation, "1d4".RollCached());
				}
			}
			for (int j = 0; j < num2; j++)
			{
				BaseMutation baseMutation2 = null;
				do
				{
					baseMutation2 = MutationFactory.GetRandomMutation("Physical");
				}
				while (baseMutation2 != null && mutations.HasMutation(baseMutation2));
				if (baseMutation2 != null)
				{
					mutations.AddMutation(baseMutation2, "1d4".RollCached());
				}
			}
			if (BaseCreature.GetBlueprint().Tags.TryGetValue("HeroMutationPopulation", out var value2) && !value2.IsNullOrEmpty())
			{
				BaseCreature.MutateFromPopulationTable(value2, (tierOverride == -1) ? BaseCreature.GetTier() : tierOverride);
			}
			if (!BaseCreature.HasProperName)
			{
				string text8 = NameMaker.MakeName(BaseCreature, null, null, null, null, null, null, null, null, SpecialType, null, FailureOkay: false, SpecialFaildown: true);
				if (text == "&y" || text == "y")
				{
					BaseCreature.pRender.DisplayName = text8;
				}
				else
				{
					BaseCreature.pRender.DisplayName = "{{" + text + "|" + text8 + "}}";
				}
				BaseCreature.HasProperName = true;
			}
			if (BaseCreature.HasPart("Inventory"))
			{
				int num3;
				if (tierOverride != -1)
				{
					num3 = tierOverride;
					BaseCreature.SetIntProperty("InventoryTier", num3);
				}
				else
				{
					num3 = BaseCreature.GetTier();
				}
				string MarkString = null;
				if (BaseCreature.HasTag("HeroAddMakersMark"))
				{
					MarkString = MakersMark.Generate();
					BaseCreature.SetStringProperty("MakersMark", MarkString);
				}
				string tag = BaseCreature.GetTag("HeroInventory");
				if (!string.IsNullOrEmpty(tag))
				{
					if (MarkString != null)
					{
						BaseCreature.EquipFromPopulationTable(tag, num3, delegate(GameObject obj)
						{
							if (obj.HasPart("Description") && !obj.HasTag("AlwaysStack"))
							{
								obj.GetPart<Description>().Mark = "{{R|" + MarkString + "}}{{C|: This item bears the mark of " + BaseCreature.DisplayNameOnlyStripped + ".}}";
							}
						});
					}
					else
					{
						BaseCreature.EquipFromPopulationTable(tag, num3);
					}
				}
			}
			if (BaseCreature.HasTag("HeroHasGuards"))
			{
				HasGuards hasGuards = new HasGuards();
				hasGuards.numberOfGuards = BaseCreature.GetTag("HeroHasGuards", "2-4");
				BaseCreature.AddPart(hasGuards);
			}
			if (BaseCreature.HasTag("HeroHasThralls"))
			{
				HasThralls hasThralls = new HasThralls();
				hasThralls.numberOfThralls = BaseCreature.GetTag("HeroHasThralls", "4-7");
				BaseCreature.AddPart(hasThralls);
			}
			if (!ResolveTemplateTag(BaseCreature, "HeroNoWaterRitual", "false", AdditionalBaseTemplates, AdditionalSpecializationTemplates).EqualsNoCase("true"))
			{
				BaseCreature.RemovePart("GivesRep");
				BaseCreature.AddPart(new GivesRep());
				BaseCreature.FireEvent("FactionsAdded");
			}
			string value3 = ResolveTemplateTag(BaseCreature, "HeroFactionHeirloomChance", "", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!string.IsNullOrEmpty(value3) && Convert.ToInt32(value3).in100() && BaseCreature.pBrain != null)
			{
				Faction ifExists = Factions.getIfExists(BaseCreature.pBrain.GetPrimaryFaction());
				if (ifExists != null)
				{
					BaseCreature.TakeObject(ifExists.GenerateHeirloom(), Silent: true, 0);
				}
			}
			if (BaseCreature.pRender != null)
			{
				BaseCreature.pRender.RenderLayer++;
			}
			BaseCreature.FireEvent("MadeHero");
			return BaseCreature;
		}
		catch (Exception ex)
		{
			MetricsManager.LogError("Error making heroic: " + BaseCreature.Blueprint + " -> " + ex);
			return null;
		}
	}
}
