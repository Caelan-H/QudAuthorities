#define NLOG_ALL
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.UI;

[HasModSensitiveStaticCache]
public class Options
{
	public static NameValueBag Bag;

	public static GameOptions Map;

	public static Dictionary<string, List<GameOption>> OptionsByCategory;

	public static Dictionary<string, GameOption> OptionsByID;

	public static List<OptionValueCacheEntry> ValueCache = new List<OptionValueCacheEntry>();

	private static readonly Dictionary<string, Action<XmlDataHelper>> _Nodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "options", HandleNodes },
		{ "option", HandleOptionNode }
	};

	public static bool DisableTextAnimationEffects;

	public static int MasterVolume => Convert.ToInt32(GetOption("OptionMasterVolume", "0"));

	public static bool Sound => GetOption("OptionSound").EqualsNoCase("Yes");

	public static int SoundVolume => Convert.ToInt32(GetOption("OptionSoundVolume", "0"));

	public static bool Music => GetOption("OptionMusic").EqualsNoCase("Yes");

	public static int MusicVolume => Convert.ToInt32(GetOption("OptionMusicVolume", "0"));

	public static bool MusicBackground => GetOption("OptionMusicBackground").EqualsNoCase("Yes");

	[Obsolete("Option no longer exists, Options.ModernUI instead.")]
	public static bool OverlayPrereleaseStage => ModernUI;

	public static bool ModernUI => GetOption("OptionModernUI").EqualsNoCase("Yes");

	public static string StageViewID => "Stage";

	public static bool UseTiles => Globals.RenderMode == RenderModeType.Tiles;

	public static bool ShowErrorPopups => GetOption("OptionShowErrorPopups").EqualsNoCase("Yes");

	public static bool UseCombatSounds => GetOption("OptionUseCombatSounds").EqualsNoCase("Yes");

	public static bool DisplayVignette => GetOption("OptionDisplayVignette").EqualsNoCase("Yes");

	public static bool DisplayScanlines => GetOption("OptionDisplayScanlines").EqualsNoCase("Yes");

	public static bool ShowModSelectionNewGame => GetOption("OptionShowModSelectionNewGame").EqualsNoCase("Yes");

	public static int DisplayBrightness => Convert.ToInt32(GetOption("OptionDisplayBrightness", "0"));

	public static int DisplayContrast => Convert.ToInt32(GetOption("OptionDisplayContrast", "0"));

	public static string FullscreenResolution => GetOption("OptionDisplayResolution");

	public static bool DisplayFullscreen => GetOption("OptionDisplayFullscreen").EqualsNoCase("Yes");

	public static string DisplayFramerate => GetOption("OptionDisplayFramerate");

	public static double StageScale
	{
		get
		{
			try
			{
				string option = GetOption("OptionPrereleaseStageScale");
				if (option.StartsWith("auto"))
				{
					double num = 1.0;
					if (option == "auto x1.25")
					{
						num = 1.25;
					}
					if (option == "auto x1.5")
					{
						num = 1.5;
					}
					if (Screen.width > Screen.height)
					{
						return (double)Screen.width * num / 1920.0;
					}
					return (double)Screen.height * num / 1080.0;
				}
				return Convert.ToDouble(GetOption("OptionPrereleaseStageScale", "1.0"), CultureInfo.InvariantCulture);
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Error parsing Stage Scale", x);
				return 1.0;
			}
		}
	}

	public static bool PrereleaseInputManager => GetOption("OptionsPrereleaseInputManager").EqualsNoCase("Yes");

	public static int KeyRepeatDelay => Convert.ToInt32(GetOption("OptionKeyRepeatDelay", "0"));

	public static int KeyRepeatRate => Convert.ToInt32(GetOption("OptionKeyRepeatRate", "0"));

	public static bool OverlayUI => GetOption("OptionOverlayUI").EqualsNoCase("Yes");

	public static bool MouseInput => GetOption("OptionMouseInput").EqualsNoCase("Yes");

	public static bool MouseMovement => GetOption("OptionMouseMovement").EqualsNoCase("Yes");

	public static int OverlayScale2 => Convert.ToInt32(GetOption("OptionOverlayScale2", "50"));

	public static bool PannableDisplay => GetOption("OptionPannableDisplay").EqualsNoCase("Yes");

	public static int MinimapScale => Convert.ToInt32(GetOption("OptionMinimapScale", "0"));

	public static bool OverlayMinimap
	{
		get
		{
			return GetOption("OptionOverlayMinimap").EqualsNoCase("Yes");
		}
		set
		{
			SetOption("OptionOverlayMinimap", value);
		}
	}

	public static bool OverlayNearbyObjects
	{
		get
		{
			return GetOption("OptionOverlayNearbyObjects").EqualsNoCase("Yes");
		}
		set
		{
			SetOption("OptionOverlayNearbyObjects", value);
		}
	}

	public static bool OverlayNearbyObjectsPools => GetOption("OptionOverlayNearbyObjectsPools").EqualsNoCase("Yes");

	public static bool OverlayNearbyObjectsPlants => GetOption("OptionOverlayNearbyObjectsPlants").EqualsNoCase("Yes");

	public static bool OverlayTooltips => GetOption("OptionOverlayTooltips").EqualsNoCase("Yes");

	public static bool OverlayPrereleaseInventory => GetOption("OptionOverlayPrereleaseInventory").EqualsNoCase("Yes");

	public static bool OverlayPrereleaseTrade => GetOption("OptionOverlayPrereleaseTrade").EqualsNoCase("Yes");

	public static bool OverlayNoShaders => GetOption("OptionOverlayNoShaders").EqualsNoCase("Yes");

	public static bool UseOverlayCombatEffects => GetOption("OptionUseOverlayCombatEffects").EqualsNoCase("Yes");

	public static bool AutoSip => GetOption("OptionAutoSip").EqualsNoCase("Yes");

	public static string AutoSipLevel => GetOption("OptionAutoSipLevel", "Thirsty");

	public static int AutosaveInterval
	{
		get
		{
			if (int.TryParse(GetOption("OptionAutosaveInterval", "5"), out var result))
			{
				return result;
			}
			return int.MaxValue;
		}
	}

	public static bool AutoTorch => GetOption("OptionAutoTorch").EqualsNoCase("Yes");

	public static bool AutoDisassembleScrap2 => GetOption("OptionAutoDisassembleScrap2").EqualsNoCase("Yes");

	public static bool ShowScavengeItemAsMessage => GetOption("OptionShowScavengeItemAsMessage").EqualsNoCase("Yes");

	public static bool AutogetPrimitiveAmmo => GetOption("OptionAutogetPrimitiveAmmo").EqualsNoCase("Yes");

	public static bool AutogetAmmo => GetOption("OptionAutogetAmmo").EqualsNoCase("Yes");

	public static bool AutogetNuggets => GetOption("OptionAutogetNuggets").EqualsNoCase("Yes");

	public static bool AutogetTradeGoods => GetOption("OptionAutogetTradeGoods").EqualsNoCase("Yes");

	public static bool AutogetFreshWater => GetOption("OptionAutogetFreshWater").EqualsNoCase("Yes");

	public static bool AutogetArtifacts => GetOption("OptionAutogetArtifacts").EqualsNoCase("Yes");

	public static bool AutogetSpecialItems => GetOption("OptionAutogetSpecialItems").EqualsNoCase("Yes");

	public static bool AutogetScrap => GetOption("OptionAutogetScrap").EqualsNoCase("Yes");

	public static bool AutogetFood => GetOption("OptionAutogetFood").EqualsNoCase("Yes");

	public static bool AutogetBooks => GetOption("OptionAutogetBooks").EqualsNoCase("Yes");

	public static bool AutogetZeroWeight => GetOption("OptionAutogetZeroWeight").EqualsNoCase("Yes");

	public static bool AutogetIfHostiles => GetOption("OptionAutogetIfHostiles").EqualsNoCase("Yes");

	public static bool AutogetFromNearby => GetOption("OptionAutogetFromNearby").EqualsNoCase("Yes");

	public static bool TakeallCorpses => GetOption("OptionTakeallCorpses").EqualsNoCase("Yes");

	public static int AutoexploreRate
	{
		get
		{
			if (int.TryParse(GetOption("OptionAutoexploreRate", "10"), out var result))
			{
				return result;
			}
			return 0;
		}
	}

	public static int AutoexploreIgnoreEasyEnemies => DifficultyEvaluation.GetDifficultyFromDescription(GetOption("OptionAutoexploreIgnoreEasyEnemies"));

	public static int AutoexploreIgnoreDistantEnemies
	{
		get
		{
			if (int.TryParse(GetOption("OptionAutoexploreIgnoreDistantEnemies", "None"), out var result))
			{
				return result;
			}
			return int.MaxValue;
		}
	}

	public static bool AutoexploreChests => GetOption("OptionAutoexploreChests").EqualsNoCase("Yes");

	public static bool AutoexploreAutopickups => GetOption("OptionAutoexploreAutopickups").EqualsNoCase("Yes");

	public static bool AutoexploreBookshelves => GetOption("OptionAutoexploreBookshelves").EqualsNoCase("Yes");

	public static bool AskForWorldmap => GetOption("OptionAskForWorldmap").EqualsNoCase("Yes");

	public static bool AskForOneItem => GetOption("OptionAskForOneItem").EqualsNoCase("Yes");

	public static bool AskAutostair => GetOption("OptionAskAutostair").EqualsNoCase("Yes");

	public static bool ConfirmSwimming => GetOption("OptionConfirmSwimming").EqualsNoCase("Yes");

	public static bool ConfirmDangerousLiquid => GetOption("OptionConfirmDangerousLiquid").EqualsNoCase("Yes");

	public static bool AlwaysHPColor => GetOption("Option@AlwaysHPColor").EqualsNoCase("Yes");

	public static bool HPColor => GetOption("Option@HPColor").EqualsNoCase("Yes");

	public static bool MutationColor => GetOption("Option@MutationColor").EqualsNoCase("Yes");

	public static bool F1TakeAll => GetOption("OptionF1TakeAll").EqualsNoCase("Yes");

	public static bool ShowSidebarAbilities => GetOption("OptionShowSidebarAbilities").EqualsNoCase("Yes");

	public static bool ShowCurrentCellPopup => GetOption("OptionShowCurrentCellPopup").EqualsNoCase("Yes");

	public static bool ShowDetailedWeaponStats => GetOption("OptionShowDetailedWeaponStats").EqualsNoCase("Yes");

	public static bool ShowMonsterHPHearts => GetOption("OptionShowMonsterHPHearts").EqualsNoCase("Yes");

	public static bool ShiftHidesSidebar => GetOption("OptionShiftHidesSidebar").EqualsNoCase("Yes");

	public static bool ShowNumberOfItems => GetOption("OptionShowNumberOfItems").EqualsNoCase("Yes");

	public static bool DisableFloorTextures => GetOption("OptionDisableFloorTextures").EqualsNoCase("Yes");

	public static bool HighlightStairs => GetOption("OptionHighlightStairs").EqualsNoCase("Yes");

	public static bool BackgroundImage => GetOption("OptionBackgroundImage").EqualsNoCase("Yes");

	public static bool LocationIntseadOfName => GetOption("OptionLocationIntseadOfName").EqualsNoCase("Yes");

	public static bool AlphanumericBits => GetOption("OptionAlphanumericBits").EqualsNoCase("Yes");

	public static bool DisableFullscreenColorEffects => GetOption("OptionDisableFullscreenColorEffects").EqualsNoCase("Yes");

	public static bool LowContrast => GetOption("OptionLowContrast").EqualsNoCase("Yes");

	public static bool LookLocked
	{
		get
		{
			return GetOption("LookLocked", "Yes").EqualsNoCase("Yes");
		}
		set
		{
			SetOption("LookLocked", value ? "Yes" : "No");
		}
	}

	public static bool PickTargetLocked
	{
		get
		{
			return GetOption("PickTargetLocked", "Yes").EqualsNoCase("Yes");
		}
		set
		{
			SetOption("PickTargetLocked", value ? "Yes" : "No");
		}
	}

	public static bool MapDirectionsToKeypad => GetOption("OptionMapDirectionsToKeypad").EqualsNoCase("Yes");

	public static bool MapShiftDirectionToPage => GetOption("OptionMapShiftDirectionToPage").EqualsNoCase("Yes");

	public static bool CapInputBuffer => GetOption("OptionCapInputBuffer").EqualsNoCase("Yes");

	public static bool LogTurnSeparator => GetOption("OptionLogTurnSeparator").EqualsNoCase("Yes");

	public static bool IndentBodyParts => GetOption("OptionIndentBodyParts").EqualsNoCase("Yes");

	public static bool AbilityCooldownWarningAsMessage => GetOption("OptionAbilityCooldownWarningAsMessage").EqualsNoCase("Yes");

	public static bool EnableMods => GetOption("OptionEnableMods").EqualsNoCase("Yes");

	public static bool AllowCSMods => GetOption("OptionAllowCSMods").EqualsNoCase("Yes");

	public static bool HarmonyDebug => GetOption("OptionHarmonyDebug").EqualsNoCase("Yes");

	public static bool ApproveCSMods => GetOption("OptionApproveCSMods").EqualsNoCase("Yes");

	public static bool OutputModAssembly => GetOption("OptionOutputModAssembly").EqualsNoCase("Yes");

	public static bool DisableCacheCompression => GetOption("OptionDisableCacheCompression").EqualsNoCase("Yes");

	public static bool CacheEarly => GetOption("OptionCacheEarly").EqualsNoCase("Yes");

	public static bool CollectEarly => GetOption("OptionCollectEarly").EqualsNoCase("Yes");

	public static bool DisableFloorTextureObjects => GetOption("OptionDisableFloorTextureObjects").EqualsNoCase("Yes");

	public static bool ThrottleAnimation => GetOption("OptionThrottleAnimation").EqualsNoCase("Yes");

	public static bool Analytics => GetOption("OptionAnalytics").EqualsNoCase("Yes");

	public static bool DisableBloodsplatter => GetOption("OptionDisableBloodsplatter").EqualsNoCase("Yes");

	public static bool DisableSmoke => GetOption("OptionDisableSmoke").EqualsNoCase("Yes");

	public static bool DisableImposters => GetOption("OptionDisableImposters").EqualsNoCase("Yes");

	public static bool DisableAchievements => GetOption("OptionDisableAchievements").EqualsNoCase("Yes");

	public static bool DebugZoneCaching => GetOption("OptionDebugZoneCaching").EqualsNoCase("Yes");

	public static bool CheckMemory => GetOption("OptionCheckMemory").EqualsNoCase("Yes");

	public static bool DrawPopulationHintMaps => GetOption("OptionDrawPopulationHintMaps").EqualsNoCase("Yes");

	public static bool DrawInfluenceMaps => GetOption("OptionDrawInfluenceMaps").EqualsNoCase("Yes");

	public static bool DrawPathfinder => GetOption("OptionDrawPathfinder").EqualsNoCase("Yes");

	public static bool DrawPathfinderHalt => GetOption("OptionDrawPathfinderHalt").EqualsNoCase("Yes");

	public static bool DrawNavigationWeightMaps => GetOption("OptionDrawNavigationWeightMaps").EqualsNoCase("Yes");

	public static bool DrawCASystems => GetOption("OptionDrawCASystems").EqualsNoCase("Yes");

	public static bool DrawFloodVis => GetOption("OptionDrawFloodVis").EqualsNoCase("Yes");

	public static bool DrawFloodAud => GetOption("OptionDrawFloodAud").EqualsNoCase("Yes");

	public static bool DrawFloodOlf => GetOption("OptionDrawFloodOlf").EqualsNoCase("Yes");

	public static bool DrawArcs => GetOption("OptionDrawArcs").EqualsNoCase("Yes");

	public static bool DisablePlayerbrain => GetOption("OptionDisablePlayerbrain").EqualsNoCase("Yes");

	public static bool DisableZoneCaching2 => GetOption("OptionDisableZoneCaching2").EqualsNoCase("Yes");

	public static bool SynchronousZoneCaching => GetOption("OptionSynchronousZoneCaching").EqualsNoCase("Yes");

	public static bool DebugShowConversationNode => GetOption("OptionDebugShowConversationNode").EqualsNoCase("Yes");

	public static bool DebugShowFullZoneDuringBuild => GetOption("OptionDebugShowFullZoneDuringBuild").EqualsNoCase("Yes");

	public static bool DebugDamagePenetrations => GetOption("OptionDebugDamagePenetrations").EqualsNoCase("Yes");

	public static bool DebugSavingThrows => GetOption("OptionDebugSavingThrows").EqualsNoCase("Yes");

	public static bool DebugGetLostChance => GetOption("OptionDebugGetLostChance").EqualsNoCase("Yes");

	public static bool DebugStatShift => GetOption("OptionDebugStatShift").EqualsNoCase("Yes");

	public static bool DebugEncounterChance => GetOption("OptionDebugEncounterChance").EqualsNoCase("Yes");

	public static bool DebugTravelSpeed => GetOption("OptionDebugTravelSpeed").EqualsNoCase("Yes");

	public static bool DebugInternals => GetOption("OptionDebugInternals").EqualsNoCase("Yes");

	public static bool InventoryConsistencyCheck => GetOption("OptionInventoryConsistencyCheck").EqualsNoCase("Yes");

	public static bool ShowReachable => GetOption("OptionShowReachable").EqualsNoCase("Yes");

	public static bool ShowOverlandEncounters => GetOption("OptionShowOverlandEncounters").EqualsNoCase("Yes");

	public static bool ShowOverlandRegions => GetOption("OptionShowOverlandRegions").EqualsNoCase("Yes");

	public static bool ShowQuickstartOption => GetOption("OptionShowQuickstart").EqualsNoCase("Yes");

	public static bool AllowReallydie => GetOption("OptionAllowReallydie").EqualsNoCase("Yes");

	public static bool AllowSaveLoad => GetOption("OptionAllowSaveLoad").EqualsNoCase("Yes");

	public static bool DisablePermadeath => GetOption("OptionDisablePermadeath").EqualsNoCase("Yes");

	public static bool EnablePrereleaseContent => GetOption("OptionEnablePrereleaseContent").EqualsNoCase("Yes");

	public static bool EnableWishRegionNames => GetOption("OptionEnableWishRegionNames").EqualsNoCase("Yes");

	public static bool DisableTryLimit => GetOption("OptionDisableTryLimit").EqualsNoCase("Yes");

	public static bool DisableDefectLimit => GetOption("OptionDisableDefectLimit").EqualsNoCase("Yes");

	public static bool SifrahExamine => GetOption("OptionSifrahExamine").EqualsNoCase("Yes");

	public static string SifrahExamineAuto
	{
		get
		{
			string text = GetOption("OptionSifrahExamineAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || string.IsNullOrEmpty(text))
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahRepair => GetOption("OptionSifrahRepair").EqualsNoCase("Yes");

	public static string SifrahRepairAuto
	{
		get
		{
			string text = GetOption("OptionSifrahRepairAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || string.IsNullOrEmpty(text))
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahReverseEngineer => GetOption("OptionSifrahReverseEngineer").EqualsNoCase("Yes");

	public static bool SifrahDisarming => GetOption("OptionSifrahDisarming").EqualsNoCase("Yes");

	public static string SifrahDisarmingAuto
	{
		get
		{
			string text = GetOption("OptionSifrahDisarmingAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || string.IsNullOrEmpty(text))
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahHaggling => GetOption("OptionSifrahHaggling").EqualsNoCase("Yes");

	public static bool SifrahRecruitment => GetOption("OptionSifrahRecruitment").EqualsNoCase("Yes");

	public static string SifrahRecruitmentAuto
	{
		get
		{
			string text = GetOption("OptionSifrahRecruitmentAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || string.IsNullOrEmpty(text))
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahHacking => GetOption("OptionSifrahHacking").EqualsNoCase("Yes");

	public static string SifrahHackingAuto
	{
		get
		{
			string text = GetOption("OptionSifrahHackingAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || string.IsNullOrEmpty(text))
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahItemNaming => GetOption("OptionSifrahItemNaming").EqualsNoCase("Yes");

	public static string SifrahItemModding
	{
		get
		{
			string text = GetOption("OptionSifrahItemModding");
			if (string.IsNullOrEmpty(text))
			{
				text = "Never";
			}
			return text;
		}
	}

	public static bool SifrahRealityDistortion => GetOption("OptionSifrahRealityDistortion").EqualsNoCase("Yes");

	public static string SifrahRealityDistortionAuto
	{
		get
		{
			string text = GetOption("OptionSifrahRealityDistortionAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || string.IsNullOrEmpty(text))
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static bool SifrahPsychicCombat => GetOption("OptionSifrahPsychicCombat").EqualsNoCase("Yes");

	public static string SifrahPsychicCombatAuto
	{
		get
		{
			string text = GetOption("OptionSifrahPsychicCombatAuto");
			if (text.EqualsNoCase("Yes"))
			{
				text = "Always";
			}
			else if (text.EqualsNoCase("No") || string.IsNullOrEmpty(text))
			{
				text = "Ask";
			}
			return text;
		}
	}

	public static string SifrahWaterRitual
	{
		get
		{
			string text = GetOption("OptionSifrahWaterRitual");
			if (string.IsNullOrEmpty(text))
			{
				text = "Never";
			}
			return text;
		}
	}

	public static bool SifrahBaetylOfferings => GetOption("OptionSifrahBaetylOfferings").EqualsNoCase("Yes");

	public static bool AnySifrah
	{
		get
		{
			if (!SifrahExamine && !SifrahRepair && !SifrahReverseEngineer && !SifrahDisarming && !SifrahHaggling && !SifrahRecruitment && !SifrahHacking && !SifrahItemNaming && !(SifrahItemModding != "Never") && !SifrahRealityDistortion && !SifrahPsychicCombat && !(SifrahWaterRitual != "Never"))
			{
				return SifrahBaetylOfferings;
			}
			return true;
		}
	}

	public static void SetOption(string ID, bool Value)
	{
		SetOption(ID, Value ? "Yes" : "No");
	}

	public static void SetOption(string ID, string Value)
	{
		lock (ValueCache)
		{
			Bag.SetValue(ID, Value);
			UpdateValueCacheEntry(ID, Value);
			UpdateFlags();
		}
	}

	public static void AddValueCache(string ID, string Value)
	{
		lock (ValueCache)
		{
			ValueCache.Add(new OptionValueCacheEntry(ID, Value));
		}
	}

	public static void UpdateValueCacheEntry(string ID, string Value)
	{
		for (int i = 0; i < ValueCache.Count; i++)
		{
			if (ValueCache[i].Key == ID)
			{
				ValueCache[i].Value = Value;
				return;
			}
		}
		AddValueCache(ID, Value);
	}

	public static bool HasOption(string ID)
	{
		lock (ValueCache)
		{
			for (int i = 0; i < ValueCache.Count; i++)
			{
				if (ValueCache[i].Key == ID)
				{
					return true;
				}
			}
			if (OptionsByID.TryGetValue(ID, out var _))
			{
				return true;
			}
			return false;
		}
	}

	public static string GetOption(string ID, string Default = "")
	{
		if (Bag == null)
		{
			Debug.LogWarning("accessign options pre-init: " + ID);
			return Default;
		}
		lock (ValueCache)
		{
			for (int i = 0; i < ValueCache.Count; i++)
			{
				if (ValueCache[i].Key == ID)
				{
					return ValueCache[i].Value;
				}
			}
			string value = Bag.GetValue(ID);
			if (value != null)
			{
				UpdateValueCacheEntry(ID, value);
				return value;
			}
			if (OptionsByID.TryGetValue(ID, out var value2))
			{
				UpdateValueCacheEntry(ID, value2.Default);
				return value2.Default;
			}
			UpdateValueCacheEntry(ID, Default);
			return Default;
		}
	}

	public static void UpdateFlags()
	{
		ObjectFinder.instance?.ReadOptions();
		if (CapInputBuffer)
		{
			GameManager.bCapInputBuffer = true;
		}
		else
		{
			GameManager.bCapInputBuffer = false;
		}
		if (OverlayUI)
		{
			GameManager.Instance.OverlayUIEnabled = true;
		}
		else
		{
			GameManager.Instance.OverlayUIEnabled = false;
		}
		if (GetOption("OptionUseTiles").EqualsNoCase("Yes"))
		{
			Globals.RenderMode = RenderModeType.Tiles;
		}
		else
		{
			Globals.RenderMode = RenderModeType.Text;
		}
		if (Analytics)
		{
			Globals.EnableMetrics = true;
		}
		else
		{
			Globals.EnableMetrics = false;
		}
		if (Sound)
		{
			Globals.EnableSound = true;
		}
		else
		{
			Globals.EnableSound = false;
		}
		if (Music)
		{
			Globals.EnableMusic = true;
		}
		else
		{
			Globals.EnableMusic = false;
		}
		if (MouseInput)
		{
			GameManager.Instance.MouseInput = true;
		}
		else
		{
			GameManager.Instance.MouseInput = false;
		}
		if (PannableDisplay)
		{
			GameManager.Instance.bAllowPanning = true;
		}
		else
		{
			GameManager.Instance.bAllowPanning = false;
		}
		if (PrereleaseInputManager)
		{
			GameManager.Instance.PrereleaseInput = true;
		}
		else
		{
			GameManager.Instance.PrereleaseInput = false;
		}
		AchievementManager.Enabled = !DisableAchievements;
		int masterVolume = MasterVolume;
		int musicVolume = MusicVolume;
		int soundVolume = SoundVolume;
		GameManager.Instance.compassScale = (float)Convert.ToInt32(GetOption("OptionOverlayCompassScale", "100")) / 100f;
		GameManager.Instance.nearbyObjectsListScale = (float)Convert.ToInt32(GetOption("OptionOverlayNearbyObjectsScale", "100")) / 100f;
		GameManager.Instance.minimapScale = (float)Convert.ToInt32(GetOption("OptionMinimapScale", "100")) / 100f;
		SoundManager.MasterVolume = (float)masterVolume / 100f;
		SoundManager.MusicVolume = (float)musicVolume / 100f;
		SoundManager.SoundVolume = (float)soundVolume / 100f;
		float num = (float)Convert.ToInt32(GetOption("OptionKeyRepeatDelay")) / 100f;
		float num2 = (float)Convert.ToInt32(GetOption("OptionKeyRepeatRate")) / 100f;
		RewiredExtensions.delaytime = 0.1f + 2f * num;
		RewiredExtensions.repeattime = 0f + 0.2f * (1f - num2);
		ControlManager.delaytime = 0.1f + 2f * num;
		ControlManager.repeattime = 0f + 0.2f * (1f - num2);
		ZoneManager.ZoneTransitionSaveInterval = AutosaveInterval;
		Leveler.PlayerLedPrompt = GetOption("OptionDisplayLedLevelUp").EqualsNoCase("Yes");
		IBaseJournalEntry.NotedPrompt = !GetOption("OptionPopupJournalNote").EqualsNoCase("No");
		DisableTextAnimationEffects = GetOption("OptionDisableTextAnimationEffects").EqualsNoCase("Yes");
		foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeof(OptionFlagUpdate), typeof(HasOptionFlagUpdate)))
		{
			try
			{
				item.Invoke(null, new object[0]);
			}
			catch (Exception arg)
			{
				MetricsManager.LogAssemblyError(item, $"Error invoking {item.DeclaringType.FullName}.{item.Name}: {arg}");
			}
		}
		GameManager.Instance.uiQueue.queueTask(delegate
		{
			UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_AnalogTV>().enabled = DisplayScanlines;
			UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_FastVignette>().enabled = DisplayVignette;
			UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = Math.Max(-70, DisplayBrightness);
			UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().contrast = Math.Max(-70, DisplayContrast);
			if (DisplayFullscreen)
			{
				string text = FullscreenResolution;
				if (text == "*Max")
				{
					Resolution resolution = GameManager.resolutions.Last();
					text = resolution.width + "x" + resolution.height;
				}
				if (text == "Screen")
				{
					text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
				}
				if (text == "Unset")
				{
					Screen.fullScreen = true;
				}
				else
				{
					string[] array = text.Split('x');
					int width = Convert.ToInt32(array[0]);
					int height = Convert.ToInt32(array[0]);
					Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
					Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
				}
			}
			else
			{
				Screen.fullScreen = false;
			}
			if (OverlayNoShaders)
			{
				GameManager.Instance.MainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
			}
			else
			{
				GameManager.Instance.MainCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
			}
			if (MusicBackground)
			{
				Application.runInBackground = true;
				if (SoundManager.MusicSource != null && !SoundManager.MusicSource.GetComponent<AudioSource>().ignoreListenerPause)
				{
					SoundManager.MusicSource.GetComponent<AudioSource>().ignoreListenerPause = true;
				}
			}
			else
			{
				Application.runInBackground = false;
				if (SoundManager.MusicSource != null && SoundManager.MusicSource.GetComponent<AudioSource>().ignoreListenerPause)
				{
					SoundManager.MusicSource.GetComponent<AudioSource>().ignoreListenerPause = false;
				}
			}
			string displayFramerate = DisplayFramerate;
			if (displayFramerate == "Unlimited")
			{
				QualitySettings.vSyncCount = 0;
				Application.targetFrameRate = 0;
			}
			else if (displayFramerate == "VSync")
			{
				QualitySettings.vSyncCount = 1;
				Application.targetFrameRate = 60;
			}
			else
			{
				try
				{
					Application.targetFrameRate = Convert.ToInt16(displayFramerate);
					QualitySettings.vSyncCount = 0;
				}
				catch
				{
					Application.targetFrameRate = 60;
					QualitySettings.vSyncCount = 0;
				}
			}
			GameManager.Instance.ControlPanelFloatScale = 3f / (1f + (float)StageScale);
		});
	}

	[ModSensitiveCacheInit]
	public static void LoadAllOptions()
	{
		LoadOptions();
		LoadModOptions();
		UpdateFlags();
	}

	public static void LoadOptions()
	{
		OptionsByCategory = new Dictionary<string, List<GameOption>>();
		OptionsByID = new Dictionary<string, GameOption>();
		Bag = new NameValueBag(DataManager.SavePath("PlayerOptions.json"));
		try
		{
			using (XmlDataHelper xmlDataHelper = DataManager.GetXMLStream("Options.xml", null))
			{
				HandleNodes(xmlDataHelper);
				xmlDataHelper.Close();
			}
			Bag.Load();
			lock (ValueCache)
			{
				ValueCache.Clear();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error loading options: " + ex.ToString());
		}
	}

	private static void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(_Nodes);
	}

	public static void LoadModOptions()
	{
		try
		{
			ModManager.ForEachFile("Options.xml", delegate(string path, ModInfo ModInfo)
			{
				using XmlDataHelper xmlDataHelper = DataManager.GetXMLStream(path, ModInfo);
				HandleNodes(xmlDataHelper);
				xmlDataHelper.Close();
			});
			lock (ValueCache)
			{
				ValueCache.Clear();
			}
		}
		catch (Exception ex)
		{
			Logger.log.Error("Error loading Options.xml from mod");
			Logger.Exception(ex);
		}
		SifrahGame.Installed = OptionsByID.ContainsKey("OptionSifrahExamine");
	}

	private static void HandleOptionNode(XmlDataHelper xml)
	{
		GameOption gameOption = LoadOptionNode(xml);
		xml.DoneWithElement();
		if (!OptionsByCategory.ContainsKey(gameOption.Category))
		{
			OptionsByCategory.Add(gameOption.Category, new List<GameOption>());
		}
		OptionsByCategory[gameOption.Category].Add(gameOption);
		OptionsByID.Add(gameOption.ID, gameOption);
	}

	private static GameOption LoadOptionNode(XmlDataHelper Reader)
	{
		GameOption gameOption = new GameOption();
		gameOption.ID = Reader.GetAttribute("ID");
		gameOption.DisplayText = Reader.GetAttribute("DisplayText");
		gameOption.Category = Reader.GetAttribute("Category");
		gameOption.Type = Reader.GetAttribute("Type");
		gameOption.Default = CapabilityManager.instance.GetDefaultOptionOverrideForCapabilities(gameOption.ID, Reader.GetAttribute("Default"));
		if (!string.IsNullOrEmpty(Reader.GetAttribute("Values")))
		{
			if (Reader.GetAttribute("Values") == "*Resolution")
			{
				gameOption.Values = new List<string>();
				HashSet<string> hashSet = new HashSet<string>();
				foreach (Resolution resolution in GameManager.resolutions)
				{
					string item = resolution.width + "x" + resolution.height;
					if (!hashSet.Contains(item))
					{
						gameOption.Values.Add(item);
						hashSet.Add(item);
					}
				}
				gameOption.Values.Add("Screen");
				gameOption.Values.Add("Unset");
			}
			else
			{
				gameOption.Values = new List<string>(Reader.GetAttribute("Values").Split(','));
			}
		}
		gameOption.Min = Reader.GetAttributeInt("Min", gameOption.Min);
		gameOption.Max = Reader.GetAttributeInt("Max", gameOption.Max);
		gameOption.Increment = Reader.GetAttributeInt("Increment", gameOption.Increment);
		return gameOption;
	}
}
