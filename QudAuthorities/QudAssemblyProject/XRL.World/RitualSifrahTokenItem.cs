using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenItem : SocialSifrahTokenItem
{
	public RitualSifrahTokenItem()
	{
		Description = "offer an item";
	}

	public RitualSifrahTokenItem(string Blueprint)
		: this()
	{
		base.Blueprint = Blueprint;
		GameObject gameObject = GameObject.createSample(Blueprint);
		Description = "offer " + gameObject.an();
		gameObject.Obliterate();
	}

	public new static RitualSifrahTokenItem GetAppropriate(GameObject ContextObject)
	{
		string stringProperty = ContextObject.GetStringProperty("SignatureItemBlueprint");
		if (!string.IsNullOrEmpty(stringProperty) && !RitualSifrahTokenFood.IsFood(stringProperty))
		{
			return new RitualSifrahTokenItem(stringProperty);
		}
		int tier = ContextObject.GetTier();
		for (int i = 0; i < 30; i++)
		{
			GameObjectBlueprint randomElement = GameObjectFactory.Factory.BlueprintList.GetRandomElement();
			if (!randomElement.HasPart("Brain") && randomElement.HasPart("Physics") && randomElement.HasPart("Render") && !RitualSifrahTokenFood.IsFood(randomElement) && !randomElement.Tags.ContainsKey("NoSparkingQuest") && !randomElement.Tags.ContainsKey("BaseObject") && !randomElement.Tags.ContainsKey("ExcludeFromDynamicEncounters") && !randomElement.ResolvePartParameter("Physics", "Takeable").EqualsNoCase("false") && !randomElement.ResolvePartParameter("Physics", "IsReal").EqualsNoCase("false") && !randomElement.ResolvePartParameter("Render", "DisplayName").Contains("[") && (!randomElement.Props.ContainsKey("SparkingQuestBlueprint") || randomElement.Name == randomElement.Props["SparkingQuestBlueprint"]) && randomElement.Tier <= tier)
			{
				string text = randomElement.ResolvePartParameter("Examiner", "Complexity");
				if (string.IsNullOrEmpty(text) || text == "0")
				{
					return new RitualSifrahTokenItem(randomElement.Name);
				}
			}
		}
		return null;
	}
}
