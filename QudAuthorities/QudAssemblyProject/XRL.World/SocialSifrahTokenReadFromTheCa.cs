using System;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenReadFromTheCanticlesChromaic : SifrahPrioritizableToken
{
	public SocialSifrahTokenReadFromTheCanticlesChromaic()
	{
		Description = "read from the Canticles Chromaic";
		Tile = "Items/sw_book2.bmp";
		RenderString = "ë";
		ColorString = "&c";
		DetailColor = 'W';
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
		return The.Player.HasObjectInInventory("Canticles3");
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
			Popup.ShowFail("You do not have a copy of the Canticles Chromaic.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		string text = BookUI.Books["Preacher1"].GetRandomElement().FullText.Replace("\n", " ").Replace("  ", " ").Trim();
		The.Player.ParticleText("{{W|'" + text + "'}}");
		base.UseToken(Game, Slot, ContextObject);
	}
}
