using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.CharacterBuilds.Qud;

public class QudGameBootModule : AbstractEmbarkBuilderModule
{
	public static readonly string BOOTEVENT_BEGINBOOT = "BeginBoot";

	/// <summary>
	///             world progress UI is now setup and the game has an ID
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_AFTERBEGINBOOT = "AfterBeginBoot";

	public static readonly string BOOTEVENT_CACHERESET = "CacheReset";

	/// <summary>
	///             caches are now reset
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_AFTERCACHERESET = "AfterCacheReset";

	public static readonly string BOOTEVENT_GENERATESEEDS = "GenerateSeeds";

	/// <summary>
	///             game seeds are now setup
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_AFTERGENERATESEEDS = "AfterGenerateSeeds";

	public static readonly string BOOTEVENT_INITIALIZESYSTEMS = "InitializeSystems";

	/// <summary>
	///             typically used to use game.AddSystem(...) your systems
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_AFTERINITIALIZESYSTEMS = "AfterInitializeSystems";

	public static readonly string BOOTEVENT_INITIALIZEGAMESTATESINGLETONS = "InitializeGamestateSingletons";

	/// <summary>
	///             GamestateSingletons have been initialized. This will probably be handled rarely.
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_AFTERINITIALIZEGAMESTATESINGLETONS = "AfterInitializeGamestateSingletons";

	public static readonly string BOOTEVENT_BEFOREINITIALIZEHISTORY = "BeforeInitializeHistory";

	/// <summary>
	///             worlds are about to be built
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_INITIALIZEHISTORY = "InitializeHistory";

	public static readonly string BOOTEVENT_AFTERINITIALIZEHISTORY = "AfterInitializeHistory";

	/// <summary>
	///             worlds are about to be built
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_INITIALIZESULTANHISTORY = "InitializeSultanHistory";

	public static readonly string BOOTEVENT_AFTERINITIALIZESULTANHISTORY = "AfterInitializeSultanHistory";

	/// <summary>
	///             worlds are about to be built
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_INITIALIZEWORLDS = "InitializeWorlds";

	public static readonly string BOOTEVENT_AFTERINITIALIZEWORLDS = "AfterInitializeWorlds";

	/// <summary>
	///             element is a GlobalLocation that will be the player's starting cell
	///             </summary>
	public static readonly string BOOTEVENT_BOOTSTARTINGLOCATION = "BootStartingLocation";

	public static readonly string BOOTEVENT_GENERATERANDOMPLAYERNAME = "GenerateRandomPlayerName";

	/// <summary>
	///             Generate the blueprint for the player's GameObject
	///             element is a string that will be the player's body
	///             </summary>
	public static readonly string BOOTEVENT_BOOTPLAYEROBJECTBLUEPRINT = "BootPlayerObjectBlueprint";

	public static readonly string BOOTEVENT_BEFOREBOOTPLAYEROBJECT = "BeforeBootPlayerObject";

	/// <summary>
	///             create the player's GameObject
	///             element is a GameObject that will be the player's body
	///             </summary>
	public static readonly string BOOTEVENT_BOOTPLAYEROBJECT = "BootPlayerObject";

	public static readonly string BOOTEVENT_AFTERBOOTPLAYEROBJECT = "AfterBootPlayerObject";

	/// <summary>
	///             Generate the player's tile.
	///             Element is a string path to the tile texture.
	///             </summary>
	public static readonly string BOOTEVENT_BOOTPLAYERTILE = "BootPlayerTile";

	public static readonly string BOOTEVENT_BOOTPLAYERTILEFOREGROUND = "BootPlayerTileForeground";

	/// <summary>
	///             Generate the player's background color.
	///             Element is a string color character.
	///             </summary>
	public static readonly string BOOTEVENT_BOOTPLAYERTILEBACKGROUND = "BootPlayerTileBackground";

	public static readonly string BOOTEVENT_BOOTPLAYERTILEDETAIL = "BootPlayerTileDetail";

	/// <summary>
	///             the game is just about to start this is the last event before it does
	///             has no element
	///             </summary>
	public static readonly string BOOTEVENT_GAMESTARTING = "GameStarting";

	public override void InitFromSeed(string seed)
	{
		builder.info.GameSeed = seed;
	}

	private static void AddItem(string Blueprint, GameObject player)
	{
		try
		{
			AddItem(GameObject.create(Blueprint), player);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception creating " + Blueprint, x);
		}
	}

	private static void AddItem(string Blueprint, int Number, GameObject player)
	{
		for (int i = 0; i < Number; i++)
		{
			AddItem(Blueprint, player);
		}
	}

	private static void AddItem(GameObject GO, GameObject player)
	{
		GO.Seen();
		GO.MakeUnderstood();
		GO.GetPart<EnergyCellSocket>()?.Cell?.MakeUnderstood();
		player.ReceiveObject(GO);
	}

	private static void AddSkill(string Class, GameObject player)
	{
		object obj = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts.Skill." + Class));
		(player.GetPart("Skills") as Skills).AddSkill(obj as BaseSkill);
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == BOOTEVENT_BOOTPLAYEROBJECT)
		{
			GameObject obj = element as GameObject;
			obj.pBrain.Goals.Clear();
			obj.DisplayName = The.Game.PlayerName;
			obj.HasProperName = true;
			obj.RequirePart<Description>().Short = "It's you.";
			obj.AddPart(new OpeningStory());
			return obj;
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public override void Init()
	{
		builder.info.GameSeed = Guid.NewGuid().ToString();
	}

	public override void bootGame(XRLGame game, EmbarkInfo info)
	{
		MetricsManager.LogInfo("Beginning world build for seed: " + info.GameSeed);
		XRLCore core = The.Core;
		info.fireBootEvent(BOOTEVENT_BEGINBOOT, game);
		GameManager.Instance.PushGameView("WorldCreationProgress");
		Loading.SetHideLoadStatus(hidden: true);
		WorldCreationProgress.Begin(7);
		WorldCreationProgress.NextStep("Initialize game environment", 2);
		WorldCreationProgress.StepProgress("Generate game ID");
		game.GameID = Guid.NewGuid().ToString();
		info.fireBootEvent(BOOTEVENT_AFTERBEGINBOOT, game);
		info.fireBootEvent(BOOTEVENT_CACHERESET, game);
		WorldCreationProgress.StepProgress("Initialize local cache");
		game.GetCacheDirectory();
		game.bZoned = false;
		The.Core.ResetGameBasedStaticCaches();
		info.fireBootEvent(BOOTEVENT_AFTERCACHERESET, game);
		info.fireBootEvent(BOOTEVENT_GENERATESEEDS, game);
		game.SetStringGameState("OriginalWorldSeed", info.GameSeed);
		game.SetBooleanGameState("WorldSeedReady", Value: true);
		Stat.ReseedFrom(info.GameSeed, includeLifetimeSeeds: true);
		game.RemoveIntGameState("WorldSeed");
		game.GetWorldSeed();
		info.fireBootEvent(BOOTEVENT_AFTERGENERATESEEDS, game);
		info.fireBootEvent(BOOTEVENT_INITIALIZESYSTEMS, game);
		info.fireBootEvent(BOOTEVENT_AFTERINITIALIZESYSTEMS, game);
		info.fireBootEvent(BOOTEVENT_INITIALIZEGAMESTATESINGLETONS, game);
		List<IGamestateSingleton> list = new List<IGamestateSingleton>();
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(GamestateSingleton)))
		{
			Stat.ReseedFrom("GAMESTATE" + item.Name);
			object obj = Activator.CreateInstance(item);
			GamestateSingleton gamestateSingleton = item.GetCustomAttributes(typeof(GamestateSingleton), inherit: true)[0] as GamestateSingleton;
			(obj as IGamestateSingleton)?.init();
			game.SetObjectGameState(gamestateSingleton.id, obj);
			list.Add(obj as IGamestateSingleton);
		}
		info.fireBootEvent(BOOTEVENT_AFTERINITIALIZEGAMESTATESINGLETONS, game);
		Stat.ReseedFrom("HISTORYINIT");
		info.fireBootEvent(BOOTEVENT_BEFOREINITIALIZEHISTORY, game);
		info.fireBootEvent(BOOTEVENT_INITIALIZEHISTORY, game);
		info.fireBootEvent(BOOTEVENT_AFTERINITIALIZEHISTORY, game);
		info.fireBootEvent(BOOTEVENT_INITIALIZEWORLDS, game);
		WorldCreationProgress.NextStep("Building world name map", WorldFactory.Factory.countWorlds() * 40);
		Stat.ReseedFrom("NAMEMAP");
		WorldFactory.Factory.BuildZoneNameMap();
		Stat.ReseedFrom("BUILDWORLDS");
		WorldFactory.Factory.BuildWorlds();
		foreach (IGamestateSingleton item2 in list)
		{
			Stat.ReseedFrom("GAMESTATE" + item2.GetType().Name);
			item2?.worldBuild();
		}
		info.fireBootEvent(BOOTEVENT_AFTERINITIALIZEWORLDS, game);
		WorldCreationProgress.NextStep("Game startup", 2);
		game.Running = true;
		WorldCreationProgress.StepProgress("Adding player to world");
		GlobalLocation globalLocation = info.fireBootEvent(BOOTEVENT_BOOTSTARTINGLOCATION, game, new GlobalLocation());
		if (string.IsNullOrEmpty(game.PlayerName?.Trim()))
		{
			game.PlayerName = info.fireBootEvent(BOOTEVENT_GENERATERANDOMPLAYERNAME, game, core.GenerateRandomPlayerName(info.getModule<QudSubtypeModule>().data.Subtype));
		}
		GameObject element = info.fireBootEvent<GameObject>(BOOTEVENT_BEFOREBOOTPLAYEROBJECT, game, null);
		element = info.fireBootEvent(BOOTEVENT_BOOTPLAYEROBJECT, game, element);
		Render pRender = element.pRender;
		string tile = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILE, game, null) ?? pRender.Tile;
		string text = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILEFOREGROUND, game, null) ?? pRender.GetForegroundColor();
		string text2 = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILEBACKGROUND, game, null) ?? pRender.GetBackgroundColor();
		string detailColor = info.fireBootEvent<string>(BOOTEVENT_BOOTPLAYERTILEDETAIL, game, null) ?? pRender.DetailColor;
		pRender.Tile = tile;
		pRender.ColorString = "&" + text + "^" + text2;
		pRender.DetailColor = detailColor;
		foreach (Type item3 in ModManager.GetTypesWithAttribute(typeof(PlayerMutator)))
		{
			Stat.ReseedFrom("PLAYERMUTATOR" + item3.Name);
			(Activator.CreateInstance(item3) as IPlayerMutator)?.mutate(element);
		}
		info.fireBootEvent(BOOTEVENT_AFTERBOOTPLAYEROBJECT, game, element);
		MetricsManager.SendTelemetryWithPayload("game_start", "funnel.stages", new Dictionary<string, string> { { "GameMode", game.gameMode } });
		WorldCreationProgress.StepProgress("Starting game!", Last: true);
		Stat.ReseedFrom("InitialSeeds");
		game.SetIntGameState("RandomSeed", Stat.Rnd.Next());
		Stat.Rnd = new Random(game.GetIntGameState("RandomSeed"));
		game.SetIntGameState("RandomSeed2", Stat.Rnd.Next());
		Stat.Rnd2 = new Random(game.GetIntGameState("RandomSeed2"));
		game.SetIntGameState("RandomSeed3", Stat.Rnd.Next());
		game.SetIntGameState("RandomSeed4", Stat.Rnd.Next());
		Stat.Rnd4 = new Random(game.GetIntGameState("RandomSeed4"));
		Loading.SetHideLoadStatus(hidden: false);
		MetricsManager.LogInfo("Cached objects: " + game.ZoneManager.CachedObjects.Count);
		MemoryHelper.GCCollect();
		MetricsManager.LogEditorInfo("Starting at: " + globalLocation.ToString());
		Cell cell = globalLocation.ResolveCell();
		if (!cell.IsReachable())
		{
			cell = cell.getClosestReachableCell();
		}
		cell.AddObject(element);
		game.Player.Body = element;
		element.UpdateVisibleStatusColor();
		Keyboard.ClearInput();
		info.fireBootEvent(BOOTEVENT_GAMESTARTING, game);
		Stat.ReseedFrom("GameStart");
	}
}
