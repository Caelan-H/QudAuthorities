using System.Collections.Generic;
using HistoryKit;
using XRL.Language;

namespace XRL.World.Parts;

public class GenerateFriendOrFoe
{
	private static List<string> _hateReasons;

	private static List<string> _likeReasons;

	static GenerateFriendOrFoe()
	{
		_hateReasons = new List<string>(30);
		_likeReasons = new List<string>(19);
		_hateReasons.Add("insulting their $noun");
		_hateReasons.Add("stealing a cherished heirloom");
		_hateReasons.Add("slaying one of their leaders");
		_hateReasons.Add("eating one of their young");
		_hateReasons.Add("casting doubt on their beliefs");
		_hateReasons.Add("tricking them into sharing their freshwater");
		_hateReasons.Add("some reason no one remembers");
		_hateReasons.Add("sowing their fields with salt");
		_hateReasons.Add("disproving a famous theorem");
		_hateReasons.Add("digging up the remains of their ancestors");
		_hateReasons.Add("torching one of their villages");
		_hateReasons.Add("repeatedly beating them at dice");
		_hateReasons.Add("releasing snakes into one of their camps");
		_hateReasons.Add("disparaging a famous poet");
		_hateReasons.Add("poisoning their freshwater");
		_hateReasons.Add("impersonating one of their leaders");
		_hateReasons.Add("leading a raiding party on one of their camps");
		_hateReasons.Add("cooking them a rancid meal");
		_hateReasons.Add("telling bawdy jokes");
		_hateReasons.Add("lighting a beacon fire to warn their enemies");
		_hateReasons.Add("refusing them entrance to a local library");
		_hateReasons.Add("selling a map of their vaults to adventurers");
		_hateReasons.Add("eating all their fruit");
		_hateReasons.Add("eavesdropping on their secret ceremonies");
		_hateReasons.Add("reprogramming their favorite robot");
		_hateReasons.Add("ruining the festival of Ut yara Ux");
		_hateReasons.Add("burning one of their leaders in effigy");
		_hateReasons.Add("giving one of their kind an unfavorable horoscope reading");
		_hateReasons.Add("questioning the origins of the moon");
		_hateReasons.Add("worshipping a highly entropic being");
		_likeReasons.Add("praising their $noun");
		_likeReasons.Add("sharing freshwater with them");
		_likeReasons.Add("making them feel welcomed at a supper feast");
		_likeReasons.Add("saving one of their young from drowning");
		_likeReasons.Add("resembling one of their idols");
		_likeReasons.Add("respecting the sanctity of a burial site");
		_likeReasons.Add("worshipping the same deities");
		_likeReasons.Add("attending a funeral for their leader");
		_likeReasons.Add("uncovering a plot against them");
		_likeReasons.Add("penning a moving poem");
		_likeReasons.Add("telling bawdy jokes");
		_likeReasons.Add("cooking them a splendid meal");
		_likeReasons.Add("giving one of their kind a favorable horoscope reading");
		_likeReasons.Add("faithfully adapting one of their plays");
		_likeReasons.Add("pouring asphalt on one of their enemies");
		_likeReasons.Add("explaining the meaning of the Canticles Chromaic");
		_likeReasons.Add("reprogramming their least favorite robot");
		_likeReasons.Add("fervently celebrating the solstice");
		_likeReasons.Add("providing shelter during a glass storm");
	}

	public static string getHateReason()
	{
		return _hateReasons.GetRandomElement().Replace("$noun", Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>")));
	}

	public static string getLikeReason()
	{
		return _likeReasons.GetRandomElement().Replace("$noun", Grammar.Pluralize(HistoricStringExpander.ExpandString("<spice.nouns.!random>")));
	}

	public static string getRandomFaction(GameObject parent)
	{
		List<string> list = new List<string>(Factions.getFactionCount());
		foreach (Faction item in Factions.loop())
		{
			if (item.Visible && !parent.pBrain.FactionMembership.ContainsKey(item.Name))
			{
				list.Add(item.Name);
			}
		}
		return list.GetRandomElement();
	}
}
