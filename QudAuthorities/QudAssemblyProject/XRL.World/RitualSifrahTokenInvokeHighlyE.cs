using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenInvokeHighlyEntropicBeing : SifrahPrioritizableToken
{
	public string Name;

	public RitualSifrahTokenInvokeHighlyEntropicBeing()
	{
		Description = "invoke a highly entropic being";
		Tile = "Items/sw_chiral_rings.bmp";
		RenderString = "\u0015";
		ColorString = "&K";
		DetailColor = 'm';
	}

	public RitualSifrahTokenInvokeHighlyEntropicBeing(string Name)
		: this()
	{
		SetBeingName(Name);
	}

	public void SetBeingName(string Name)
	{
		this.Name = Name;
		Description = "invoke " + this.Name;
		GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(Name);
		if (blueprintIfExists != null)
		{
			Tile = blueprintIfExists.GetPartParameter("Render", "Tile", Tile);
		}
	}

	public override int GetPriority()
	{
		if (Name != null)
		{
			return Name.Length;
		}
		return 1;
	}

	public override int GetTiebreakerPriority()
	{
		if (Name != null)
		{
			return Name[0] - 65;
		}
		return 0;
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (string.IsNullOrEmpty(Name))
		{
			List<string> invokableHighlyEntropicBeings = Factions.GetInvokableHighlyEntropicBeings();
			if (invokableHighlyEntropicBeings != null && invokableHighlyEntropicBeings.Count > 0)
			{
				char[] array = new char[invokableHighlyEntropicBeings.Count];
				char c = 'a';
				int i = 0;
				for (int count = invokableHighlyEntropicBeings.Count; i < count; i++)
				{
					array[i] = ((c > 'z') ? ' ' : c);
					if (c <= 'z')
					{
						c = (char)(c + 1);
					}
				}
				int num = Popup.ShowOptionList("Invoke whom?", invokableHighlyEntropicBeings.ToArray(), array, 2, null, 60, RespectOptionNewlines: false, AllowEscape: true);
				if (num < 0)
				{
					return false;
				}
				SetBeingName(invokableHighlyEntropicBeings[num]);
			}
		}
		return true;
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
		Factions.HighlyEntropicBeingWorshipped(Name);
	}
}
