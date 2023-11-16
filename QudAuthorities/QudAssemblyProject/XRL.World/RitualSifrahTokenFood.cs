using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenFood : SocialSifrahTokenItem
{
	public RitualSifrahTokenFood()
	{
		Description = "offer food";
		Tile = "Items/sw_rawmeat1.bmp";
		RenderString = "%";
		ColorString = "&r";
		DetailColor = 'w';
	}

	public RitualSifrahTokenFood(string Blueprint)
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

	public new static RitualSifrahTokenFood GetAppropriate(GameObject ContextObject)
	{
		string stringProperty = ContextObject.GetStringProperty("SignatureItemBlueprint");
		if (!string.IsNullOrEmpty(stringProperty) && IsFood(stringProperty))
		{
			return new RitualSifrahTokenFood(stringProperty);
		}
		int tier = ContextObject.GetTier();
		for (int i = 0; i < 30; i++)
		{
			GameObjectBlueprint randomElement = GameObjectFactory.Factory.BlueprintList.GetRandomElement();
			if (!randomElement.HasPart("Brain") && randomElement.HasPart("Physics") && randomElement.HasPart("Render") && IsFood(randomElement) && (!ContextObject.HasPart("Carnivorous") || randomElement.Tags.ContainsKey("Meat")) && !randomElement.Tags.ContainsKey("NoSparkingQuest") && !randomElement.Tags.ContainsKey("BaseObject") && !randomElement.Tags.ContainsKey("ExcludeFromDynamicEncounters") && !randomElement.ResolvePartParameter("Physics", "Takeable").EqualsNoCase("false") && !randomElement.ResolvePartParameter("Physics", "IsReal").EqualsNoCase("false") && !randomElement.ResolvePartParameter("Render", "DisplayName").Contains("[") && (!randomElement.Props.ContainsKey("SparkingQuestBlueprint") || randomElement.Name == randomElement.Props["SparkingQuestBlueprint"]) && randomElement.Tier <= tier)
			{
				string text = randomElement.ResolvePartParameter("Examiner", "Complexity");
				if (string.IsNullOrEmpty(text) || text == "0")
				{
					return new RitualSifrahTokenFood(randomElement.Name);
				}
			}
		}
		return null;
	}
}
