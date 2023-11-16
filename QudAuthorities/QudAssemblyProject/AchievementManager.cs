using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using XRL;

internal static class AchievementManager
{
	public static bool Write = false;

	public static bool Enabled = true;

	public static AchievementState State = new AchievementState
	{
		{ "ACH_DIE", "Welcome to Qud" },
		{ "ACH_VIOLATE_PAULI", "The Laws of Physics Are Mere Suggestions, Vol. 1" },
		{ "ACH_BEAMSPLIT_SPACE_INVERTER", "The Laws of Physics Are Mere Suggestions, Vol. 2" },
		{ "ACH_EAT_BEAR", "Eat an Entire Bear" },
		{ "ACH_WEAR_OWN_FACE", "To Thine Own Self Be True" },
		{ "ACH_DRINK_LAVA", "On the Rocks" },
		{ "ACH_GET_FUNGAL_INFECTIONS", "Friend to Fungi" },
		{ "ACH_INHABIT_GOAT", "Goat Simulator" },
		{ "ACH_KILLED_BY_TWIN", "Your Better Half" },
		{ "ACH_FIND_ISNER", "The Spirit of Vengeance" },
		{ "ACH_FIND_STOPSVALINN", "Knights Conical" },
		{ "ACH_LOVE_SIGN", "Love at First Sign" },
		{ "ACH_INSTALL_IMPLANT", "You Are Becoming" },
		{ "ACH_INSTALL_IMPLANT_EVERY_SLOT", "You Became" },
		{ "ACH_KILL_PYRAMID", "Pyramid Scheme" },
		{ "ACH_GET_GLOTROT", "Say No More" },
		{ "ACH_GET_IRONSHANK", "Metal Pedal" },
		{ "ACH_EAT_OWN_LIMB", "You Are What You Eat" },
		{ "ACH_KILLED_BY_CHUTE_CRAB", "Shoot." },
		{ "ACH_FORESEE_DEATH", "Dark Tidings" },
		{ "ACH_LOVE_YOURSELF", "Love Thyself" },
		{ "ACH_LOVED_BY_FACTION", "A Bond Knit with Trust" },
		{ "ACH_HATED_BY_JOPPA", "The Woe of Joppa" },
		{ "ACH_LOVED_BY_NEW_BEINGS", "Peekaboo" },
		{ "ACH_CRUSHED_UNDER_SUNS", "Starry Demise" },
		{ "ACH_SPEAK_ALCHEMIST", "Quiet This Metal" },
		{ "ACH_HAVE_10_MUTATIONS", "Proteus" },
		{ "ACH_READ_10_BOOKS", "Litteratus", "STAT_BOOKS_READ", 10 },
		{ "ACH_READ_100_BOOKS", "So Powerful is the Charm of Words", "STAT_BOOKS_READ", 100 },
		{ "ACH_KILL_100_SNAPJAWS", "Jawsnapper", "STAT_SNAPJAWS_KILLED", 100 },
		{ "ACH_WATER_RITUAL_50_TIMES", "Your Thirst Is Mine, My Water Is Yours", "STAT_WATER_RITUALS_PERFORMED", 50 },
		{ "ACH_GET_MUTATION_LEVEL_15", "Advance a mutation to level 15." },
		{ "ACH_CURE_GLOTROT", "Cure glotrot." },
		{ "ACH_CURE_IRONSHANK", "Cure ironshank." },
		{ "ACH_LEARN_ONE_SULTAN_HISTORY", "Biographer" },
		{ "ACH_VIOLATE_WATER_RITUAL", "Oathbreaker" },
		{ "ACH_ACTIVATE_TIMECUBE", "Cubic and Wisest Human" },
		{ "ACH_WIELD_PRISM", "Go on. Do it." },
		{ "ACH_WATER_RITUAL_OBOROQORU", "In Contemplation of Eons" },
		{ "ACH_REGENERATE_LIMB", "Synolymb" },
		{ "ACH_DIE_BY_FALLING", "Free Falling" },
		{ "ACH_KILL_TRISLUDGE", "Three-Sludge Monte" },
		{ "ACH_KILL_PENTASLUDGE", "Five-Sludge Monte" },
		{ "ACH_KILL_DECASLUDGE", "Ten-Sludge Monte" },
		{ "ACH_GET_DECAPITATED", "Hole Like a Head" },
		{ "ACH_FIND_APPRENTICE", "What With the Disembowelment and All" },
		{ "ACH_FIND_GLOWPAD", "Psst." },
		{ "ACH_COMPLIMENT_QGIRL", "That Was Nice" },
		{ "ACH_WIELD_CAS_POL", "Gemini" },
		{ "ACH_DONATE_ITEM_200_REP", "Donation Level: Kasaphescence" },
		{ "ACH_LEARN_JUMP", "Leap, Frog." },
		{ "ACH_SIX_ARMS", "Six Arms None the Richer" },
		{ "ACH_VORTICES_ENTERED", "Six Arms None the Richer", "STAT_VORTICES_ENTERED", 25 },
		{ "ACH_100_VILLAGES", "Tourist", "STAT_VILLAGES_VISITED", 100 },
		{ "ACH_100_RECIPES", "Sultan of Salt", "STAT_RECIPES_INVENTED", 100 },
		{ "ACH_RECOVER_KINDRISH", "Close The Loop" },
		{ "ACH_KILL_OBOROQORU", "The Woe of Apes" },
		{ "ACH_EQUIP_FLUMEFLIER", "Rocket Bear" },
		{ "ACH_WATER_RITUAL_MAMON", "Feast Upon the Goat Hearts! *cheers*" },
		{ "ACH_OVERDOSE_HULKHONEY", "Aaaaaaaaargh!" },
		{ "ACH_RECRUIT_HIGH_PRIEST", "Mechanimist Reformer" },
		{ "ACH_CLONE_CTESIPHUS", "Two Cats Are Better Than One" },
		{ "ACH_10_CLONES", "Me, Myself, and I" },
		{ "ACH_30_CLONES", "Clonal Colony" },
		{ "ACH_20_GLIMMER", "Sight Older Than Sight" },
		{ "ACH_40_GLIMMER", "What Are Directions on a Space That Cannot be Ordered?" },
		{ "ACH_100_GLIMMER", "Star-Eye Esper" },
		{ "ACH_200_GLIMMER", "The Quasar Mind" },
		{ "ACH_TRUE_GLIMMER", "Glimmer of Truth" },
		{ "ACH_ABSORB_PSYCHE", "There Can Be Only One" },
		{ "ACH_ATE_SURPRISE", "Surprise!" },
		{ "ACH_COOKED_FLUX", "Absolute Unit" },
		{ "ACH_COOKED_EXTRADIMENSIONAL", "Non-Locally Sourced" },
		{ "ACH_WEAR_6_FACES", "The Narrowing Gyre", "STAT_WEAR_FACES", 6, "STAT_WEAR_FACE_1", "STAT_WEAR_FACE_2", "STAT_WEAR_FACE_3", "STAT_WEAR_FACE_4", "STAT_WEAR_FACE_5", "STAT_WEAR_FACE_6" },
		{ "ACH_TRANSMUTED_GEM", "Jeweled Dusk" },
		{ "ACH_SWALLOWED_WHOLE", "*gulp*" },
		{ "ACH_WINKED_OUT", "I-" },
		{ "ACH_TURNED_STONE", "Aetalag" },
		{ "ACH_GIFTED_10ASTERISK", "Token of Gratitude" },
		{ "ACH_GAVEALL_REPULSIVE_DEVICE", "Dayenu", "STAT_GAVE_REPULSIVE_DEVICE", 4, "STAT_GAVE_REPULSIVE_DEVICE_NACHAM", "STAT_GAVE_REPULSIVE_DEVICE_VAAM", "STAT_GAVE_REPULSIVE_DEVICE_DAGASHA", "STAT_GAVE_REPULSIVE_DEVICE_KAH" },
		{ "ACH_WEAR_OTHERPEARL", "The Recitation of the Drowning of Eudoxia by the Witches of Moonhearth" },
		{ "ACH_CROSS_BRIGHTSHEOL", "From Thyn Heres Shaken the Wet and Olde Lif" },
		{ "ACH_COMPLETE_WATERVINE", "What's Eating the Watervine?" },
		{ "ACH_HEAD_EXPLODE", "Open Your Mind" },
		{ "ACH_LEARN_SECRET_FROM_MUMBLEMOUTH", "Mumblecore" },
		{ "ACH_SIX_DAY_STILT", "May the Ground Shake But the Six Day Stilt Never Tumble" },
		{ "ACH_TATTOO_SELF", "Live and Ink" },
		{ "ACH_20_DRAMS_BRAIN_BRINE", "The Psychal Chorus", "STAT_BRAIN_BRINE_DRAMS_DRUNK", 20 },
		{ "ACH_MUTATION_FROM_GAMMAMOTH", "Lottery Winner" },
		{ "ACH_CHAOS_SPIEL", "Was It Something I Said?" },
		{ "ACH_RECOVER_RELIC", "Raisins in the Layer Cake" },
		{ "ACH_STONED_RED_ROCK", "Red Rock Hazing Ritual" },
		{ "ACH_DISAPPOINT_HEB", "tsk tsk" },
		{ "ACH_AURORAL", "Dawnglider" },
		{ "ACH_SLYNTH_HOME", "Belong, Friends" },
		{ "ACH_RECAME", "You Recame" },
		{ "ACH_TRAVEL_TZIMTZLUM", "All Those Who Wander" },
		{ "ACH_BESTOW_LIFE_20", "Become as Gods", "STAT_BESTOW_LIFE", 20 },
		{ "ACH_100_CLAMS_ENTERED", "Byevalve", "STAT_CLAMS_ENTERED", 100 },
		{ "ACH_ENTER_MAK_CLAM", "A Clammy Reception" }
	};

	public static void Awake()
	{
		Load();
	}

	public static void Update()
	{
		if (Write)
		{
			Write = false;
			Task.Run((Action)Save);
		}
	}

	public static void Load()
	{
		string text = DataManager.SavePath("Achievements.json");
		if (File.Exists(text))
		{
			try
			{
				AchievementState achievementState = new JsonSerializer
				{
					DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
				}.Deserialize<AchievementState>(text);
				foreach (KeyValuePair<string, AchievementInfo> achievement in achievementState.Achievements)
				{
					if (State.Achievements.TryGetValue(achievement.Key, out var value))
					{
						value.Achieved = achievement.Value.Achieved;
					}
				}
				foreach (KeyValuePair<string, StatInfo> stat in achievementState.Stats)
				{
					if (State.Stats.TryGetValue(stat.Key, out var value2))
					{
						value2.Value = stat.Value.Value;
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error reading local achievement data", x);
			}
		}
		Write |= LoadIntState();
		if (!SteamManager.Initialized)
		{
			return;
		}
		foreach (AchievementInfo value3 in State.Achievements.Values)
		{
			if (!value3.SteamID.IsNullOrEmpty())
			{
				Write |= SteamManager.UpdateAchievement(value3.SteamID, ref value3.Achieved);
			}
		}
		foreach (StatInfo value4 in State.Stats.Values)
		{
			if (!value4.SteamID.IsNullOrEmpty())
			{
				Write |= SteamManager.UpdateStat(value4.SteamID, ref value4.Value);
			}
		}
	}

	[Obsolete("save compat")]
	public static bool LoadIntState()
	{
		Dictionary<string, int> elements = GlobalState.instance.intState.elements;
		StatInfo statInfo = State.Stats["STAT_BOOKS_READ"];
		bool flag = true;
		if (elements.TryGetValue("ReadAchievementCount", out var value))
		{
			statInfo.Value = Math.Max(statInfo.Value, value);
		}
		if (elements.TryGetValue("KillAchievementCount_Snapjaws", out value))
		{
			statInfo = State.Stats["STAT_SNAPJAWS_KILLED"];
			statInfo.Value = Math.Max(statInfo.Value, value);
		}
		if (elements.TryGetValue("WaterRitualCount", out value))
		{
			statInfo = State.Stats["STAT_WATER_RITUALS_PERFORMED"];
			statInfo.Value = Math.Max(statInfo.Value, value);
		}
		if (elements.TryGetValue("SpaceTimeVortexEntryCount", out value))
		{
			statInfo = State.Stats["STAT_VORTICES_ENTERED"];
			statInfo.Value = Math.Max(statInfo.Value, value);
		}
		if (flag)
		{
			GlobalState.instance.save();
		}
		return flag;
	}

	public static void Save()
	{
		string file = DataManager.SavePath("Achievements.json");
		lock (State)
		{
			try
			{
				new JsonSerializer
				{
					Formatting = Formatting.Indented,
					DefaultValueHandling = DefaultValueHandling.Ignore
				}.Serialize(file, State);
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error writing local achievement data", x);
			}
		}
	}

	public static void Reset()
	{
		SteamManager.ResetAchievements();
		foreach (AchievementInfo value in State.Achievements.Values)
		{
			value.Achieved = false;
		}
		foreach (StatInfo value2 in State.Stats.Values)
		{
			value2.Value = 0;
			SteamManager.SetStat(value2.SteamID, 0);
		}
	}

	public static bool GetAchievement(string ID)
	{
		if (State.Achievements.TryGetValue(ID, out var value))
		{
			return value.Achieved;
		}
		return false;
	}

	public static void SetAchievement(string ID, float Percent = 100f)
	{
		if (!State.Achievements.TryGetValue(ID, out var value))
		{
			Debug.LogWarning("Unknown achievement id: " + ID);
		}
		else if (!value.Achieved && Enabled)
		{
			lock (State)
			{
				value.Achieved = true;
				Write = true;
			}
			SteamManager.SetAchievement(value.SteamID);
		}
	}

	public static void IndicateAchievementProgress(string ID)
	{
		if (!State.Achievements.TryGetValue(ID, out var value))
		{
			Debug.LogWarning("Unknown achievement id: " + ID);
		}
		else
		{
			IndicateAchievementProgress(value);
		}
	}

	public static void IndicateAchievementProgress(AchievementInfo Ach)
	{
		if (!Ach.Achieved && Enabled)
		{
			SteamManager.IndicateAchievementProgress(Ach.SteamID, (uint)Ach.Progress.Value, (uint)Ach.AchievedAt);
		}
	}

	public static void IncrementAchievement(string ID, int Value = 1)
	{
		if (!State.Achievements.TryGetValue(ID, out var value))
		{
			Debug.LogWarning("Unknown achievement id: " + ID);
		}
		else
		{
			if (value.Achieved || !Enabled)
			{
				return;
			}
			if (value.Progress == null)
			{
				SetAchievement(ID);
				return;
			}
			Value = Math.Min(value.Progress.Value + Value, value.Progress.MaxValue);
			if (value.Progress.Value != Value)
			{
				bool flag = Value >= value.AchievedAt;
				lock (State)
				{
					value.Progress.Value = Value;
					value.Achieved |= flag;
					Write = true;
				}
				SteamManager.SetStat(value.Progress.SteamID, Value);
				if (flag)
				{
					SteamManager.SetAchievement(value.SteamID);
				}
				else if (value.AchievedAt < 100 || Value % 10 == 0)
				{
					IndicateAchievementProgress(value);
				}
			}
		}
	}

	public static void IncrementAchievement(string ID, string StatID, int Value = 1)
	{
		if (!State.Achievements.TryGetValue(ID, out var value))
		{
			Debug.LogWarning("Unknown achievement id: " + ID);
		}
		else
		{
			if (value.Achieved || !Enabled)
			{
				return;
			}
			if (!State.Stats.TryGetValue(StatID, out var value2))
			{
				Debug.LogWarning("Unknown stat id: " + ID);
				return;
			}
			Value = Math.Min(value2.Value + Value, value2.MaxValue);
			if (value2.Value == Value)
			{
				return;
			}
			bool flag = Value >= value2.MaxValue;
			int num = 0;
			foreach (StatInfo stat in value.Stats)
			{
				if (stat == value2)
				{
					num += (flag ? 1 : 0);
				}
				else if (stat.Value >= stat.MaxValue)
				{
					num++;
				}
			}
			bool flag2 = num >= value.AchievedAt;
			lock (State)
			{
				value2.Value = Value;
				value.Progress.Value = num;
				value.Achieved |= flag2;
				Write = true;
			}
			SteamManager.SetStat(value2.SteamID, Value);
			SteamManager.SetStat(value.Progress.SteamID, num);
			if (flag2)
			{
				SteamManager.SetAchievement(value.SteamID);
			}
			else if (flag)
			{
				IndicateAchievementProgress(value);
			}
		}
	}
}
