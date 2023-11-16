using System.Collections.Generic;
using HistoryKit;
using Qud.API;

namespace XRL.World.Parts;

public static class Gossip
{
	public static string GenerateGossip_OneFaction(string faction)
	{
		History sultanHistory = The.Game.sultanHistory;
		if (50.in100())
		{
			return GenerateGossip_TwoFactions(faction, HistoricStringExpander.ExpandString("some <spice.commonPhrases.group.!random>", null, sultanHistory));
		}
		return GenerateGossip_TwoFactions(HistoricStringExpander.ExpandString("<spice.commonPhrases.someone.!random>", null, sultanHistory), faction);
	}

	public static string GenerateGossip_TwoFactions(string actor, string actee)
	{
		History sultanHistory = The.Game.sultanHistory;
		string text = HistoricStringExpander.ExpandString("<spice.gossip.twoFaction.!random>", null, sultanHistory);
		if (text.Contains("@item"))
		{
			GameObject gameObject = GameObject.create(EncountersAPI.GetARandomDescendentOf("Item"));
			text = text.Replace("@item.a", gameObject.a).Replace("@item.name", gameObject.DisplayNameOnlyStripped);
		}
		List<string> factionNames = Factions.getFactionNames();
		if (factionNames.Contains(actor))
		{
			if (40.in100())
			{
				List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(actor);
				if (factionMembers.Count > 0)
				{
					GameObject gameObject2 = GameObjectFactory.Factory.CreateObject(factionMembers.GetRandomElement().Name);
					actor = gameObject2.a + gameObject2.DisplayNameOnlyStripped;
				}
				else
				{
					actor = ((!string.IsNullOrEmpty(Faction.getFormattedName(actor))) ? Faction.getFormattedName(actor) : actor);
				}
			}
			else
			{
				string formattedName = Faction.getFormattedName(actor);
				actor = ((!string.IsNullOrEmpty(formattedName)) ? formattedName : actor);
			}
		}
		if (factionNames.Contains(actee))
		{
			if (40.in100())
			{
				List<GameObjectBlueprint> factionMembers2 = GameObjectFactory.Factory.GetFactionMembers(actee);
				if (factionMembers2.Count > 0)
				{
					GameObject gameObject3 = GameObjectFactory.Factory.CreateObject(factionMembers2.GetRandomElement().Name);
					actee = gameObject3.a + gameObject3.DisplayNameOnlyStripped;
				}
				else
				{
					actee = ((!string.IsNullOrEmpty(Faction.getFormattedName(actee))) ? Faction.getFormattedName(actee) : actee);
				}
			}
			else
			{
				actee = ((!string.IsNullOrEmpty(Faction.getFormattedName(actee))) ? Faction.getFormattedName(actee) : actee);
			}
		}
		return text.Replace("*f1*", actor).Replace("*f2*", actee);
	}
}
