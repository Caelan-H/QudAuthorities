using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class Reputation
{
	public const int LOVED = 2;

	public const int LIKED = 1;

	public const int INDIFFERENT = 0;

	public const int DISLIKED = -1;

	public const int HATED = -2;

	private Dictionary<string, float> ReputationValues = new Dictionary<string, float>(64);

	public const float maxReputation = 1000f;

	public const int hatedRep = -600;

	public const int dislikedRep = -250;

	public const int likedRep = 250;

	public const int lovedRep = 600;

	public const int alliedFeeling = 250;

	public const int repDollar = 50;

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(ReputationValues.Keys.Count);
		foreach (string key in ReputationValues.Keys)
		{
			Writer.Write(key);
			Writer.Write(ReputationValues[key]);
		}
	}

	public static Reputation Load(SerializationReader Reader)
	{
		Reputation reputation = new Reputation();
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			float value = Reader.ReadSingle();
			reputation.ReputationValues.Add(key, value);
		}
		return reputation;
	}

	public bool any(string faction)
	{
		return ReputationValues.ContainsKey(faction);
	}

	public int get(Faction faction)
	{
		float value;
		int num = ((!ReputationValues.TryGetValue(faction.Name, out value)) ? getDefaultRep(faction.Name) : ((int)value));
		GameObject body = XRLCore.Core.Game.Player.Body;
		if (body == null)
		{
			return num;
		}
		if (faction.PartReputation != null)
		{
			foreach (KeyValuePair<string, int> item in faction.PartReputation)
			{
				if (body.HasPart(item.Key))
				{
					num += item.Value;
				}
			}
		}
		if (faction != null && faction.Visible)
		{
			num += body.GetIntProperty("AllVisibleRepModifier");
		}
		return num;
	}

	public int getLevel(string faction)
	{
		int num = get(faction);
		if (num >= 600)
		{
			return 2;
		}
		if (num >= 250)
		{
			return 1;
		}
		if (num > -250)
		{
			return 0;
		}
		if (num > -600)
		{
			return -1;
		}
		return -2;
	}

	public int get(string faction)
	{
		return get(Factions.get(faction));
	}

	public int set(Faction faction, int rep)
	{
		float old = 0f;
		if (ReputationValues.ContainsKey(faction.Name))
		{
			old = ReputationValues[faction.Name];
		}
		ReputationValues[faction.Name] = rep;
		The.Game.Systems.ForEach(delegate(IGameSystem s)
		{
			s.PlayerReputationChanged(faction.Name, (int)old, rep, null);
		});
		return rep;
	}

	public int set(string faction, int rep)
	{
		return set(Factions.get(faction), rep);
	}

	public float modify(Faction f, int delta, string because = null, StringBuilder putMessage = null, bool silent = false, bool transient = false)
	{
		string faction = f.Name;
		if (!ReputationValues.ContainsKey(faction))
		{
			ReputationValues.Add(faction, getDefaultRep(faction));
		}
		int @for = ReputationChangeEvent.GetFor(f, delta, silent, transient);
		if (@for == 0)
		{
			return get(faction);
		}
		int oldRep = get(faction);
		ReputationValues[faction] += @for;
		int newRep = get(faction);
		char color = getColor(newRep);
		bool flag = false;
		bool flag2 = false;
		bool state = false;
		bool state2 = false;
		bool state3 = false;
		string text = null;
		bool flag3 = false;
		if (@for > 0)
		{
			if (oldRep < 600 && newRep >= 600)
			{
				flag = true;
				text = "loved";
			}
			else if (oldRep < 250 && newRep >= 250)
			{
				state2 = true;
				text = "favored";
			}
			else if (oldRep <= -250 && newRep > -250)
			{
				state3 = true;
				text = "indifferent";
				flag3 = true;
			}
			else if (oldRep <= -600 && newRep > -600)
			{
				state = true;
				text = "disliked";
			}
		}
		else if (oldRep > -600 && newRep <= -600)
		{
			flag2 = true;
			text = "despised";
		}
		else if (oldRep > -250 && newRep <= -250)
		{
			state = true;
			text = "disliked";
		}
		else if (oldRep >= 250 && newRep < 250)
		{
			state3 = true;
			text = "indifferent";
			flag3 = true;
		}
		else if (oldRep >= 600 && newRep < 600)
		{
			state2 = true;
			text = "favored";
		}
		if (f.Visible)
		{
			if (!silent)
			{
				StringBuilder stringBuilder = putMessage ?? Event.NewStringBuilder();
				if (!string.IsNullOrEmpty(because))
				{
					stringBuilder.Append(ColorUtility.CapitalizeExceptFormatting(because)).Append(", your ");
				}
				else
				{
					stringBuilder.Append("Your ");
				}
				stringBuilder.Append("reputation with {{C|").Append(f.getFormattedName()).Append("}} ")
					.Append((@for >= 0) ? "increased" : "decreased")
					.Append(" by {{")
					.Append((@for >= 0) ? 'G' : 'R')
					.Append('|')
					.Append(Math.Abs(@for))
					.Append("}} to {{")
					.Append(color)
					.Append('|')
					.Append(newRep)
					.Append("}}.");
				if (text != null)
				{
					if (flag3)
					{
						stringBuilder.Append(' ').Append(ColorUtility.CapitalizeExceptFormatting(f.getFormattedName())).Append(' ')
							.Append(f.Plural ? "are" : "is")
							.Append(" now {{")
							.Append(color)
							.Append("|")
							.Append(text)
							.Append("}} to you.");
					}
					else
					{
						stringBuilder.Append(" You are now {{").Append(color).Append("|")
							.Append(text)
							.Append("}} by {{C|")
							.Append(f.getFormattedName())
							.Append("}}.");
					}
				}
				if (putMessage == null)
				{
					Popup.Show(stringBuilder.ToString());
				}
				else
				{
					putMessage.Append('\n');
				}
			}
			if (flag && The.Player != null)
			{
				JournalAPI.AddAccomplishment("You became loved among " + Faction.getFormattedName(faction) + " and were treated as one of their own.", "Deep in the wilds of " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= stumbled upon a clan of " + Faction.getFormattedName(faction) + " performing a secret ritual. Because of " + The.Player.GetPronounProvider().PossessiveAdjective + " " + HistoricStringExpander.ExpandString("<spice.elements." + IComponent<GameObject>.ThePlayerMythDomain + ".quality.!random>") + ", they accepted " + The.Player.GetPronounProvider().Objective + " into their fold and taught " + The.Player.GetPronounProvider().Objective + " their secrets.", "general", JournalAccomplishment.MuralCategory.BecomesLoved, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				AchievementManager.SetAchievement("ACH_LOVED_BY_FACTION");
				if (faction == "Newly Sentient Beings")
				{
					AchievementManager.SetAchievement("ACH_LOVED_BY_NEW_BEINGS");
				}
			}
			if (flag2 && faction == "Joppa")
			{
				AchievementManager.SetAchievement("ACH_HATED_BY_JOPPA");
			}
		}
		if (The.Player != null && The.Player.HasRegisteredEvent("ReputationChanged"))
		{
			StringBuilder stringBuilder2 = putMessage ?? Event.NewStringBuilder();
			Event @event = Event.New("ReputationChanged");
			@event.SetParameter("Actor", The.Player);
			@event.SetParameter("Faction", faction);
			@event.SetParameter("OldReputation", oldRep);
			@event.SetParameter("NewReputation", newRep);
			@event.SetParameter("Because", because);
			@event.SetParameter("Message", stringBuilder2);
			@event.SetFlag("BecameLoved", flag);
			@event.SetFlag("BecameLiked", state2);
			@event.SetFlag("BecameIndifferent", state3);
			@event.SetFlag("BecameDisliked", state);
			@event.SetFlag("BecameHated", flag2);
			The.Player.FireEvent(@event);
			if (putMessage == null && stringBuilder2.Length > 0)
			{
				Popup.Show(stringBuilder2.ToString());
			}
		}
		The.Game.Systems.ForEach(delegate(IGameSystem s)
		{
			s.PlayerReputationChanged(faction, oldRep, newRep, because);
		});
		return get(faction);
	}

	public float modify(string faction, int delta, string because = null, StringBuilder putMessage = null, bool silent = false, bool transient = false)
	{
		return modify(Factions.get(faction), delta, because, putMessage, silent, transient);
	}

	public float modify(string faction, int delta, bool displayMessage)
	{
		return modify(Factions.get(faction), delta, null, null, !displayMessage);
	}

	public bool Use(string Faction, int Amount)
	{
		if (get(Faction) < Amount)
		{
			return false;
		}
		modify(Faction, -Amount);
		return true;
	}

	public static int getAttitude(int rep)
	{
		if (rep <= -600)
		{
			return -2;
		}
		if (rep <= -250)
		{
			return -1;
		}
		if (rep < 250)
		{
			return 0;
		}
		if (rep < 600)
		{
			return 1;
		}
		return 2;
	}

	public int getAttitude(Faction faction)
	{
		return getAttitude(get(faction));
	}

	public int getAttitude(string faction)
	{
		return getAttitude(get(Factions.get(faction)));
	}

	public static int getTradePerformance(int rep)
	{
		if (rep <= -600)
		{
			return -3;
		}
		if (rep <= -250)
		{
			return -1;
		}
		if (rep < 250)
		{
			return 0;
		}
		if (rep < 600)
		{
			return 1;
		}
		return 3;
	}

	public int getTradePerformance(Faction faction)
	{
		return getTradePerformance(get(faction));
	}

	public int getTradePerformance(string faction)
	{
		return getTradePerformance(Factions.get(faction));
	}

	public static char getColor(int rep)
	{
		if (rep <= -600)
		{
			return 'R';
		}
		if (rep <= -250)
		{
			return 'r';
		}
		if (rep < 250)
		{
			return 'C';
		}
		if (rep < 600)
		{
			return 'g';
		}
		return 'G';
	}

	public char getColor(Faction faction)
	{
		return getColor(get(faction));
	}

	public char getColor(string faction)
	{
		return getColor(get(Factions.get(faction)));
	}

	public int getDefaultRep(string faction)
	{
		return 0;
	}

	public int getFeeling(string faction)
	{
		float num = get(faction);
		if (num <= -600f)
		{
			return -100;
		}
		if (num <= -250f)
		{
			return -50;
		}
		if (num < 250f)
		{
			Faction faction2 = Factions.get(faction);
			if (faction2.HolyPlaces.Count > 0 && The.Player.CurrentZone != null && faction2.HolyPlaces.Contains(The.Player.CurrentZone.ZoneID))
			{
				return -50;
			}
			return 0;
		}
		if (num < 600f)
		{
			return 50;
		}
		return 100;
	}

	public void Init()
	{
		foreach (Faction item in Factions.loop())
		{
			int initialPlayerReputation = item.InitialPlayerReputation;
			if (initialPlayerReputation != int.MinValue)
			{
				set(item.Name, initialPlayerReputation);
			}
		}
	}
}
