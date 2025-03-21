using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModDisguise : IModification
{
	public string DisguiseBlueprint;

	[NonSerialized]
	private static List<GameObjectBlueprint> DisguiseBlueprints;

	public ModDisguise()
	{
	}

	public ModDisguise(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override string GetModificationDisplayName()
	{
		string disguiseAppearanceDescription = GetDisguiseAppearanceDescription();
		if (disguiseAppearanceDescription == null)
		{
			return "a disguise";
		}
		return "a disguise of " + disguiseAppearanceDescription;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!(Object.GetPart("Armor") is Armor armor))
		{
			return false;
		}
		if (armor.WornOn != "Body" && armor.WornOn != "Back")
		{
			return false;
		}
		string usesSlots = Object.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Body") && !usesSlots.Contains("Back"))
		{
			return false;
		}
		return true;
	}

	public override bool BeingAppliedBy(GameObject Object, GameObject Who)
	{
		if (Who.IsPlayer())
		{
			List<GameObjectBlueprint> disguiseBlueprints = GetDisguiseBlueprints();
			List<string[]> list = new List<string[]>(disguiseBlueprints.Count);
			List<string> list2 = new List<string>(disguiseBlueprints.Count);
			foreach (GameObjectBlueprint item in disguiseBlueprints)
			{
				if (item.HasBeenSeen())
				{
					string partParameter = item.GetPartParameter("Render", "DisplayName", "creature of some kind");
					if (!list2.Contains(partParameter))
					{
						list.Add(new string[3]
						{
							item.Name,
							partParameter,
							ColorUtility.StripFormatting(partParameter)
						});
						list2.Add(partParameter);
					}
				}
			}
			if (list.Count == 0)
			{
				Popup.Show("You aren't familiar enough with any creatures to make a disguise.");
				return false;
			}
			list.Sort((string[] a, string[] b) => a[2].CompareTo(b[2]));
			List<string> list3 = new List<string>(16);
			List<object> list4 = new List<object>(16);
			List<char> list5 = new List<char>(16);
			char c = 'a';
			foreach (string[] item2 in list)
			{
				list3.Add(item2[1]);
				list4.Add(item2[0]);
				list5.Add((c <= 'z') ? c++ : ' ');
			}
			int num = Popup.ShowOptionList("Choose a disguise to make.", list3.ToArray(), list5.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			DisguiseBlueprint = list4[num] as string;
		}
		else
		{
			string Species = Who.GetTag("Species") ?? Who.Blueprint;
			GameObjectBlueprint randomElement = (from bp in GetDisguiseBlueprints()
				where (bp.GetTag("Species") ?? bp.Name) != Species
				select bp).GetRandomElement();
			if (randomElement != null)
			{
				DisguiseBlueprint = randomElement.Name;
			}
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (string.IsNullOrEmpty(DisguiseBlueprint) || !GameObjectFactory.Factory.Blueprints.TryGetValue(DisguiseBlueprint, out var value))
		{
			value = SelectDisguiseBlueprint();
			DisguiseBlueprint = value.Name;
		}
		string partParameter = value.GetPartParameter("Brain", "Factions");
		if (partParameter != null)
		{
			foreach (string item in partParameter.CachedCommaExpansion())
			{
				string spec = item;
				int num = Brain.ExtractFactionMembership(ref spec);
				if (num <= 0)
				{
					continue;
				}
				Faction faction = Factions.get(spec);
				if (faction != null && faction.Visible)
				{
					try
					{
						AddsRep.AddModifier(Object, string.Concat(str2: (GetFactionReputationBase() * num / 100).ToString(), str0: spec, str1: ":", str3: ":hidden"));
					}
					catch
					{
					}
				}
			}
		}
		IncreaseDifficultyIfComplex(2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsWorn(E.Part))
		{
			ApplyDisguise(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		UnapplyDisguise(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			if (!string.IsNullOrEmpty(DisguiseBlueprint) && GameObjectFactory.Factory.Blueprints.ContainsKey(DisguiseBlueprint))
			{
				GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[DisguiseBlueprint];
				E.AddClause("and " + gameObjectBlueprint.GetPartParameter("Render", "DisplayName", "creature") + " disguise", -20);
			}
			else
			{
				E.AddClause("and disguise", -20);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingEquipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped")
		{
			BodyPart bodyPart = E.GetParameter("BodyPart") as BodyPart;
			if (ParentObject.IsWorn(bodyPart))
			{
				foreach (GameObject equippedObject in E.GetGameObjectParameter("EquippingObject").GetEquippedObjects())
				{
					if (equippedObject.HasPart("ModDisguise") && (bodyPart == null || bodyPart.Equipped != equippedObject))
					{
						if (!E.IsSilent() && E.GetIntParameter("AutoEquipTry") <= 1)
						{
							Popup.Show("You are already wearing a disguise.");
						}
						return false;
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	private void ApplyDisguise(GameObject who)
	{
		who.ApplyEffect(new Disguised(DisguiseBlueprint, GetDisguiseAppearanceDescription()));
	}

	private void UnapplyDisguise(GameObject who)
	{
		who.RemoveEffect("Disguised", (Effect fx) => fx is Disguised disguised && disguised.BlueprintName == DisguiseBlueprint);
	}

	public static string GetDescription(int Tier)
	{
		return "Disguise: This item changes its wearer's appearance and improves their reputation with the faction of the mimicked creature.";
	}

	public static int GetFactionReputationBase(int Tier)
	{
		return 500;
	}

	public int GetFactionReputationBase()
	{
		return GetFactionReputationBase(Tier);
	}

	public string GetDisguiseAppearanceDescription()
	{
		if (string.IsNullOrEmpty(DisguiseBlueprint) || !GameObjectFactory.Factory.Blueprints.ContainsKey(DisguiseBlueprint))
		{
			return null;
		}
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints[DisguiseBlueprint];
		string partParameter = gameObjectBlueprint.GetPartParameter("Render", "DisplayName", "creature of some kind");
		string text = gameObjectBlueprint.GetxTag("Grammar", "iArticle");
		if (HasPropertyOrTag("OverrideIArticle"))
		{
			text = GetPropertyOrTag("OverrideIArticle");
		}
		if (text == null)
		{
			return Grammar.A(partParameter);
		}
		return text + " " + partParameter;
	}

	public string GetInstanceDescription()
	{
		string disguiseAppearanceDescription = GetDisguiseAppearanceDescription();
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (disguiseAppearanceDescription != null)
		{
			stringBuilder.Append("Disguise: This item makes its wearer appear to be ").Append(disguiseAppearanceDescription).Append('.');
		}
		else
		{
			stringBuilder.Append(GetDescription(Tier));
		}
		if (!string.IsNullOrEmpty(DisguiseBlueprint) && GameObjectFactory.Factory.Blueprints.ContainsKey(DisguiseBlueprint))
		{
			string partParameter = GameObjectFactory.Factory.Blueprints[DisguiseBlueprint].GetPartParameter("Brain", "Factions");
			if (partParameter != null)
			{
				foreach (string item in partParameter.CachedCommaExpansion())
				{
					string spec = item;
					int num = Brain.ExtractFactionMembership(ref spec);
					if (num <= 0)
					{
						continue;
					}
					Faction faction = Factions.get(spec);
					if (faction != null && faction.Visible)
					{
						try
						{
							int value = GetFactionReputationBase() * num / 100;
							stringBuilder.Append("\n+").Append(value).Append(" reputation with ")
								.Append(faction.getFormattedName());
						}
						catch
						{
						}
					}
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static bool IsBlueprintUsableForDisguise(GameObjectBlueprint BP)
	{
		if (!BP.DescendsFrom("Creature"))
		{
			return false;
		}
		if (!BP.HasPart("Combat"))
		{
			return false;
		}
		if (!EncountersAPI.IsEligibleForDynamicEncounters(BP))
		{
			return false;
		}
		if (BP.HasTag("NoDisguise"))
		{
			return false;
		}
		if (BP.GetxTag("Grammar", "Proper", "false").EqualsNoCase("true"))
		{
			return false;
		}
		string partParameter = BP.GetPartParameter("Render", "DisplayName");
		if (string.IsNullOrEmpty(partParameter) || partParameter.Contains("[") || partParameter.Contains("]"))
		{
			return false;
		}
		return true;
	}

	public static bool IsBlueprintUsableForDisguise(string BP)
	{
		return IsBlueprintUsableForDisguise(GameObjectFactory.Factory.Blueprints[BP]);
	}

	private static List<GameObjectBlueprint> GetDisguiseBlueprints()
	{
		if (DisguiseBlueprints == null)
		{
			DisguiseBlueprints = new List<GameObjectBlueprint>(256);
			foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
			{
				if (IsBlueprintUsableForDisguise(value))
				{
					DisguiseBlueprints.Add(value);
				}
			}
		}
		return DisguiseBlueprints;
	}

	private static GameObjectBlueprint SelectDisguiseBlueprint()
	{
		return EncountersAPI.GetACreatureBlueprintModel(IsBlueprintUsableForDisguise);
	}
}
