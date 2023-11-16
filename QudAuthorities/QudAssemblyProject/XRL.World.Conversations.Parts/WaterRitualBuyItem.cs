using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitualBuyItem : IWaterRitualPart
{
	public const int TYPE_FACTION = 1;

	public const int TYPE_VALUED = 2;

	public GameObject Item;

	public int Type;

	public string Source
	{
		set
		{
			Type = value.ToLowerInvariant() switch
			{
				"faction" => 1, 
				"valued" => 2, 
				_ => 0, 
			};
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != PrepareTextEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public bool SetFactionItem()
	{
		string text = (WaterRitual.Alternative ? WaterRitual.RecordFaction.WaterRitualItemBlueprint : WaterRitual.RecordFaction.WaterRitualAltItemBlueprint);
		if (text.IsNullOrEmpty() || WaterRitual.Record.numItems < 1)
		{
			return false;
		}
		Item = The.Speaker.HasItemWithBlueprint(text);
		if (Item == null && WaterRitual.Record.canGenerateItem)
		{
			The.Speaker.TakeObject(text, Silent: true, 0);
			Item = The.Speaker.HasItemWithBlueprint(text);
		}
		Reputation = (WaterRitual.Alternative ? WaterRitual.RecordFaction.WaterRitualAltItemCost : WaterRitual.RecordFaction.WaterRitualItemCost);
		WaterRitual.Record.canGenerateItem = false;
		return Item != null;
	}

	public override void Awake()
	{
		if (WaterRitual.Record.numGifts < 1 || !WaterRitual.RecordFaction.WaterRitualBuyMostValuableItem)
		{
			return;
		}
		if (Type == 1)
		{
			if (!SetFactionItem())
			{
				return;
			}
		}
		else if (Type == 2)
		{
			Item = The.Speaker.GetMostValuableItem();
			if (!GameObject.validate(ref Item))
			{
				return;
			}
		}
		if (Reputation <= 0)
		{
			Reputation = 5;
			if (Item.GetPart("Commerce") is Commerce commerce)
			{
				Reputation = Math.Max(5, (int)(commerce.Value / 4.0));
			}
		}
		Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Item", Reputation);
		Visible = true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Object = Item;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (UseReputation())
		{
			Item.UnequipAndRemove();
			Popup.Show(The.Speaker.Does("gift", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you " + Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "!");
			The.Player.Inventory.AddObject(Item);
			if (Type == 1)
			{
				WaterRitual.Record.numItems--;
			}
			else if (Type == 2)
			{
				WaterRitual.Record.numGifts--;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|-" + Reputation + "}} reputation]}}";
		return false;
	}
}
