using System;
using Qud.API;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenGift : SocialSifrahTokenGift
{
	public RitualSifrahTokenGift()
	{
		Description = "offer an item";
	}

	public RitualSifrahTokenGift(string Blueprint)
		: this()
	{
		base.Blueprint = Blueprint;
		GameObject gameObject = GameObject.createSample(Blueprint);
		Description = "offer " + gameObject.an();
		gameObject.Obliterate();
	}

	public static bool IsFood(string Blueprint)
	{
		return IsFood(GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint));
	}

	public static bool IsFood(GameObjectBlueprint BP)
	{
		if (BP != null && BP.HasPart("Food"))
		{
			return BP.GetPartParameter("Food", "Gross") != "true";
		}
		return false;
	}

	public new static RitualSifrahTokenGift GetAppropriate(GameObject ContextObject)
	{
		string stringProperty = ContextObject.GetStringProperty("SignatureItemBlueprint");
		if (!string.IsNullOrEmpty(stringProperty))
		{
			return new RitualSifrahTokenGift(stringProperty);
		}
		int tier = ContextObject.GetTier();
		for (int i = 0; i < 10; i++)
		{
			GameObjectBlueprint anObjectBlueprintModel = EncountersAPI.GetAnObjectBlueprintModel((GameObjectBlueprint pbp) => pbp.HasTagOrProperty("Gift") && !IsFood(pbp) && !pbp.HasPart("Brain") && !pbp.ResolvePartParameter("Physics", "Takeable").EqualsNoCase("false") && !pbp.ResolvePartParameter("Render", "DisplayName").Contains("[") && (!pbp.Props.ContainsKey("SparkingQuestBlueprint") || pbp.Name == pbp.Props["SparkingQuestBlueprint"]) && (!pbp.HasTagOrProperty("GiftTrueKinOnly") || ContextObject.IsTrueKin()) && pbp.Tier <= tier);
			if (anObjectBlueprintModel == null)
			{
				continue;
			}
			string propertyOrTag = anObjectBlueprintModel.GetPropertyOrTag("GiftSkillRestriction");
			if (string.IsNullOrEmpty(propertyOrTag) || ContextObject.HasSkill(propertyOrTag))
			{
				string text = anObjectBlueprintModel.ResolvePartParameter("Examiner", "Complexity");
				if (string.IsNullOrEmpty(text) || text == "0")
				{
					return new RitualSifrahTokenGift(anObjectBlueprintModel.Name);
				}
			}
		}
		return null;
	}
}
