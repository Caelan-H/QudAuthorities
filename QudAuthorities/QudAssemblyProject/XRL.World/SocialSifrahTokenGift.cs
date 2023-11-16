using System;
using Qud.API;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenGift : SifrahPrioritizableToken
{
	public string Blueprint;

	public SocialSifrahTokenGift()
	{
		Description = "gift an item";
		Tile = "Items/sw_gadget.bmp";
		RenderString = "\n";
		ColorString = "&M";
		DetailColor = 'W';
	}

	public SocialSifrahTokenGift(string Blueprint)
		: this()
	{
		this.Blueprint = Blueprint;
		GameObject gameObject = GameObject.createSample(Blueprint);
		Description = "gift " + gameObject.an();
		gameObject.Obliterate();
	}

	public static SocialSifrahTokenGift GetAppropriate(GameObject ContextObject)
	{
		string stringProperty = ContextObject.GetStringProperty("SignatureItemBlueprint");
		if (!string.IsNullOrEmpty(stringProperty))
		{
			return new SocialSifrahTokenGift(stringProperty);
		}
		int tier = ContextObject.GetTier();
		for (int i = 0; i < 10; i++)
		{
			GameObjectBlueprint anObjectBlueprintModel = EncountersAPI.GetAnObjectBlueprintModel((GameObjectBlueprint pbp) => pbp.HasTagOrProperty("Gift") && !pbp.HasPart("Brain") && !pbp.ResolvePartParameter("Physics", "Takeable").EqualsNoCase("false") && !pbp.ResolvePartParameter("Render", "DisplayName").Contains("[") && (!pbp.Props.ContainsKey("SparkingQuestBlueprint") || pbp.Name == pbp.Props["SparkingQuestBlueprint"]) && (!pbp.HasTagOrProperty("GiftTrueKinOnly") || ContextObject.IsTrueKin()) && pbp.Tier <= tier);
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
					return new SocialSifrahTokenGift(anObjectBlueprintModel.Name);
				}
			}
		}
		return null;
	}

	public override int GetPriority()
	{
		return GetNumberAvailable();
	}

	public override int GetTiebreakerPriority()
	{
		return 0;
	}

	public int GetNumberAvailable(int Chosen = 0)
	{
		int num = -Chosen;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.Blueprint == Blueprint || item.GetBlueprint().DescendsFrom(Blueprint))
			{
				num++;
			}
		}
		return num;
	}

	public bool IsAvailable(int Chosen = 0)
	{
		int num = 0;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.Blueprint == Blueprint || item.GetBlueprint().DescendsFrom(Blueprint))
			{
				num++;
				if (num > Chosen)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return Description + " [have {{C|" + GetNumberAvailable(Game.GetTimesChosen(this, Slot)) + "}}]";
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable(Game.GetTimesChosen(this, Slot)))
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		int timesChosen = Game.GetTimesChosen(this);
		if (!IsAvailable(timesChosen))
		{
			GameObject gameObject = GameObject.createSample(Blueprint);
			if (gameObject == null)
			{
				Popup.ShowFail("You do not have any more of that kind of item.");
			}
			else if (timesChosen > 0)
			{
				Popup.ShowFail("You do not have any more " + gameObject.GetPluralName() + ".");
			}
			else
			{
				Popup.ShowFail("You do not have " + gameObject.an() + ".");
			}
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override int GetPowerup(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (Slot.CurrentMove == Slot.Token)
		{
			return 1;
		}
		return base.GetPowerup(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.Blueprint == Blueprint || item.GetBlueprint().DescendsFrom(Blueprint))
			{
				item.Destroy();
				break;
			}
		}
		base.UseToken(Game, Slot, ContextObject);
	}
}
