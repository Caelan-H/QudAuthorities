using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class TinkeringSifrahTokenAdvancedToolkit : SifrahPrioritizableToken
{
	public TinkeringSifrahTokenAdvancedToolkit()
	{
		Description = "apply an advanced toolkit";
		Tile = "Items/sw_toolbox_large.bmp";
		RenderString = "\b";
		ColorString = "&c";
		DetailColor = 'C';
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			return 0;
		}
		return int.MaxValue;
	}

	public override int GetTiebreakerPriority()
	{
		return int.MaxValue;
	}

	public bool IsAvailable()
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.GetPart("Toolbox") is Toolbox toolbox && toolbox.TrackAsToolbox && toolbox.PoweredDisassembleBonus >= 15 && toolbox.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsUnusable()
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.GetPart("Toolbox") is Toolbox toolbox && toolbox.TrackAsToolbox && toolbox.PoweredDisassembleBonus >= 15 && !toolbox.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return true;
			}
		}
		return false;
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			if (IsUnusable())
			{
				Popup.ShowFail("You do not have a usable advanced toolkit.");
			}
			else
			{
				Popup.ShowFail("You do not have an advanced toolkit.");
			}
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.GetPart("Toolbox") is Toolbox toolbox && toolbox.TrackAsToolbox && toolbox.PoweredInspectBonus >= 5 && toolbox.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				toolbox.ConsumeChargeIfOperational();
				break;
			}
		}
		base.UseToken(Game, Slot, ContextObject);
	}
}
