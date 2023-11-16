using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenScourging : SifrahPrioritizableToken
{
	public RitualSifrahTokenScourging()
	{
		Description = "scourge myself with a leather whip";
		Tile = "Items/sw_whip_1.bmp";
		RenderString = "\u00a8";
		ColorString = "&w";
		DetailColor = 'K';
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			return 0;
		}
		return The.Player.Stat("Hitpoints");
	}

	public override int GetTiebreakerPriority()
	{
		return 0;
	}

	public bool IsAvailable()
	{
		return The.Player.HasObjectInInventory("Leather Whip");
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
			Popup.ShowFail("You do not have a leather whip.");
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
		SoundManager.PlaySound("Hit_Default");
		The.Player.TakeDamage(Stat.Random(1, 4), "from scourging yourself.", null, "You scourged yourself to death.", null, The.Player);
		base.UseToken(Game, Slot, ContextObject);
	}
}
