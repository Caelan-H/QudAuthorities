using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenLeverageBeingFavored : SifrahToken
{
	private string UseFaction;

	public SocialSifrahTokenLeverageBeingFavored()
	{
		Description = "leverage being favored";
		Tile = "Items/ms_happy_face.png";
		RenderString = "\u0001";
		ColorString = "&M";
		DetailColor = 'Y';
	}

	public SocialSifrahTokenLeverageBeingFavored(GameObject Representative)
		: this()
	{
		UseFaction = Representative.GetPrimaryFaction();
	}

	public SocialSifrahTokenLeverageBeingFavored(string Faction)
		: this()
	{
		UseFaction = Faction;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!string.IsNullOrEmpty(UseFaction))
		{
			return Description + " by " + Faction.getFormattedName(UseFaction);
		}
		return Description;
	}
}
/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenLeverageBeingLoved : SifrahToken
{
	private string UseFaction;

	public SocialSifrahTokenLeverageBeingLoved()
	{
		Description = "leverage being loved";
		Tile = "Items/ms_heart.png";
		RenderString = "\u0003";
		ColorString = "&R";
		DetailColor = 'Y';
	}

	public SocialSifrahTokenLeverageBeingLoved(GameObject Representative)
		: this()
	{
		UseFaction = Representative.GetPrimaryFaction();
	}

	public SocialSifrahTokenLeverageBeingLoved(string Faction)
		: this()
	{
		UseFaction = Faction;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!string.IsNullOrEmpty(UseFaction))
		{
			return Description + " by " + Faction.getFormattedName(UseFaction);
		}
		return Description;
	}
}
/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenLeverageBeingTrueKin : SifrahToken
{
	public SocialSifrahTokenLeverageBeingTrueKin()
	{
		Description = "leverage being True Kin";
		Tile = "Items/ms_happy_face.png";
		RenderString = "\u0002";
		ColorString = "&Y";
		DetailColor = 'B';
	}
}
