using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenEffectNosebleed : SifrahPrioritizableToken
{
	public int Chance = 100;

	public PsionicSifrahTokenEffectNosebleed()
	{
		Description = "accept a {{r|nosebleed}} starting";
		Tile = "Items/ms_droplet.png";
		RenderString = "­";
		ColorString = "&r";
		DetailColor = 'W';
	}

	public PsionicSifrahTokenEffectNosebleed(int Chance)
		: this()
	{
		if (Chance < 100)
		{
			this.Chance = Math.Max(Chance, 1);
			Description = "accept " + Grammar.A(this.Chance) + "% chance of a {{r|nosebleed}} starting";
		}
		else
		{
			Chance = 100;
		}
	}

	public override int GetPriority()
	{
		return 200 - Chance;
	}

	public override int GetTiebreakerPriority()
	{
		return 350 - Chance;
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		Game.AddEffectChance("Nosebleed", Chance, null, null, Stack: true, Force: false, new Nosebleed("1d2"));
	}
}
