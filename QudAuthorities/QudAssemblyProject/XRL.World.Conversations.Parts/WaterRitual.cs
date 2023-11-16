using System.Text;
using ConsoleLib.Console;
using Qud.API;
using XRL.Liquids;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitual : IConversationPart
{
	public const int REL_LOVE = 100;

	public const int REL_LIKE = 50;

	public const int REL_DISLIKE = -50;

	public const int REL_HATE = -100;

	public const int REL_TACTFUL = 25;

	private static BaseLiquid _Liquid;

	private static WaterRitualRecord _Record;

	private static Faction _RecordFaction;

	private static bool? _Alternative;

	public static BaseLiquid Liquid => _Liquid ?? (_Liquid = LiquidVolume.getLiquid(The.Speaker.GetWaterRitualLiquid(The.Player)));

	public static string LiquidName => Liquid.GetName();

	public static string LiquidNameStripped => ColorUtility.StripFormatting(Liquid.GetName());

	public static WaterRitualRecord Record => _Record ?? (_Record = The.Speaker.RequirePart<WaterRitualRecord>());

	public static Faction RecordFaction => _RecordFaction ?? (_RecordFaction = Factions.get(Record.faction));

	public static bool Alternative
	{
		get
		{
			bool valueOrDefault = _Alternative.GetValueOrDefault();
			if (!_Alternative.HasValue)
			{
				valueOrDefault = RecordFaction.UseAltBehavior(The.Speaker);
				_Alternative = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public static int Performance => The.Speaker.GetIntProperty("WaterRitualPerformance", 100);

	public static void Reset()
	{
		_Liquid = null;
		_Record = null;
		_RecordFaction = null;
		_Alternative = null;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != LeftElementEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == DisplayTextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(DisplayTextEvent E)
	{
		E.Text.Append("\n\n{{C|-----}}\n{{y|Your reputation with ");
		if (RecordFaction.Visible)
		{
			E.Text.Append("{{C|").Append(RecordFaction.getFormattedName()).Append("}}");
		}
		else
		{
			E.Text.Append(The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true));
		}
		E.Text.Append(" is {{C|").Append(RecordFaction.CurrentReputation).Append("}}.\n")
			.Append(RecordFaction.Visible ? The.Speaker.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) : The.Speaker.It)
			.Append(" can award an additional {{C|")
			.Append(Record.totalFactionAvailable)
			.Append("}} reputation.}}");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		Reset();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (!The.Speaker.HasIntProperty("WaterRitualed"))
		{
			PerformRitual();
		}
		if (!Record.Has("usedTactful") && The.Player.HasSkill("Customs_Tactful"))
		{
			Record.attributes.Add("usedTactful");
			The.Game.PlayerReputation.modify(RecordFaction, 25);
		}
		return base.HandleEvent(E);
	}

	public void PerformRitual()
	{
		The.Game.Systems.ForEach(delegate(IGameSystem s)
		{
			s.WaterRitualPerformed(The.Speaker);
		});
		The.Speaker.SetIntProperty("WaterRitualed", 1);
		if (!The.Speaker.HasIntProperty("SifrahWaterRitual"))
		{
			Popup.Show("You share your " + LiquidName + " with " + The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " and begin the water ritual.");
		}
		HandleAchievements();
		AddAccomplishment();
		ModifyReputation();
		NameItems();
	}

	public void HandleAchievements()
	{
		if (The.Speaker.Blueprint == "Oboroqoru")
		{
			AchievementManager.SetAchievement("ACH_WATER_RITUAL_OBOROQORU");
		}
		if (The.Speaker.Blueprint == "Mamon")
		{
			AchievementManager.SetAchievement("ACH_WATER_RITUAL_MAMON");
		}
		AchievementManager.IncrementAchievement("ACH_WATER_RITUAL_50_TIMES");
	}

	public void AddAccomplishment()
	{
		string text = The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: true, NoColor: false, Stripped: true, WithoutEpithet: false, Short: true, BaseOnly: true);
		IPronounProvider pronounProvider = The.Player.GetPronounProvider();
		JournalAPI.AddAccomplishment("In sacred ritual you shared your " + LiquidName + " with " + text + ".", "In sacred ritual =name= shared " + pronounProvider.PossessiveAdjective + " holy " + LiquidNameStripped + " with noted luminary " + text + ".", "general", JournalAccomplishment.MuralCategory.Treats, WanderSystem.WanderEnabled() ? JournalAccomplishment.MuralWeight.Low : JournalAccomplishment.MuralWeight.Medium, null, -1L);
	}

	public void ModifyReputation()
	{
		int performance = Performance;
		Record.totalFactionAvailable = Record.totalFactionAvailable * performance / 100;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num = 100;
		if (The.Player.HasSkill("Customs_Tactful"))
		{
			num += 25;
			Record.attributes.Add("usedTactful");
		}
		num = num * performance / 100;
		The.Game.PlayerReputation.modify(RecordFaction, num, null, stringBuilder);
		if (!(The.Speaker.GetPart("GivesRep") is GivesRep givesRep))
		{
			return;
		}
		givesRep.wasParleyed = true;
		foreach (string key in The.Speaker.pBrain.FactionMembership.Keys)
		{
			if (!(key == Record.faction))
			{
				Faction ifExists = Factions.getIfExists(key);
				if (ifExists != null)
				{
					The.Game.PlayerReputation.modify(ifExists, 100 * performance / 100, "because they love " + The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), stringBuilder);
				}
			}
		}
		foreach (FriendorFoe relatedFaction in givesRep.relatedFactions)
		{
			Faction ifExists2 = Factions.getIfExists(relatedFaction.faction);
			if (ifExists2 != null)
			{
				switch (relatedFaction.status)
				{
				case "friend":
					The.Game.PlayerReputation.modify(ifExists2, 100 * (100 + (performance - 100) / 10) / 100, "because they admire " + The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), stringBuilder);
					break;
				case "dislike":
					The.Game.PlayerReputation.modify(ifExists2, -50 * (100 + (performance - 100) / 10) / 100, "because they dislike " + The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), stringBuilder);
					break;
				case "hate":
					The.Game.PlayerReputation.modify(ifExists2, -100 * (100 + (performance - 100) / 10) / 100, "because they despise " + The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true), stringBuilder);
					break;
				}
			}
		}
		Popup.Show(stringBuilder.ToString());
	}

	public void NameItems()
	{
		int performance = Performance;
		ItemNaming.Opportunity(The.Speaker, null, The.Player, "WaterRitual", 7 - performance / 100, 0, 0, performance / 100);
		ItemNaming.Opportunity(The.Player, null, The.Speaker, "WaterRitual", 7 - performance / 100, 0, 0, performance / 100);
	}
}
