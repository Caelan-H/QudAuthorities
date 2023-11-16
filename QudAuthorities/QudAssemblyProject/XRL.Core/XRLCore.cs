using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Qud.API;
using Qud.UI;
using Steamworks;
using UnityEngine;
using XRL.CharacterBuilds;
using XRL.Help;
using XRL.Messages;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.Core;

[Serializable]
[HasModSensitiveStaticCache]
public class XRLCore
{
	public class SortObjectBydistanceToPlayer : Comparer<XRL.World.GameObject>
	{
		public override int Compare(XRL.World.GameObject ox, XRL.World.GameObject oy)
		{
			Cell currentCell = The.Player.CurrentCell;
			if (currentCell == null)
			{
				return 0;
			}
			Cell currentCell2 = ox.CurrentCell;
			Cell currentCell3 = oy.CurrentCell;
			if (currentCell2 == null)
			{
				return 1;
			}
			if (currentCell3 == null)
			{
				return 0;
			}
			Point point = new Point(currentCell2.X, currentCell2.Y);
			Point point2 = new Point(currentCell3.X, currentCell3.Y);
			int x = currentCell.X;
			int y = currentCell.Y;
			int num = (x - point.X) * (x - point.X) + (y - point.Y) * (y - point.Y);
			int value = (x - point2.X) * (x - point2.X) + (y - point2.Y) * (y - point2.Y);
			return num.CompareTo(value);
		}
	}

	public class SortCellBydistanceToObject : Comparer<Cell>
	{
		private XRL.World.GameObject _Target;

		public SortCellBydistanceToObject(XRL.World.GameObject Target)
		{
			_Target = Target;
		}

		public override int Compare(Cell x, Cell y)
		{
			if (_Target.pPhysics.CurrentCell == null)
			{
				return 0;
			}
			if (x == y)
			{
				return 0;
			}
			if (x.X == y.X && x.Y == y.Y)
			{
				return 0;
			}
			int x2 = _Target.pPhysics.CurrentCell.X;
			int y2 = _Target.pPhysics.CurrentCell.Y;
			int num = (x2 - x.X) * (x2 - x.X) + (y2 - x.Y) * (y2 - x.Y);
			int num2 = (x2 - y.X) * (x2 - y.X) + (y2 - y.Y) * (y2 - y.Y);
			if (num == num2)
			{
				return Stat.Random(-1, 1);
			}
			return num.CompareTo(num2);
		}
	}

	public class SortPoint
	{
		public int X;

		public int Y;

		public SortPoint(int _x, int _y)
		{
			X = _x;
			Y = _y;
		}
	}

	public class SortBydistanceToPlayer : Comparer<SortPoint>
	{
		public override int Compare(SortPoint x, SortPoint y)
		{
			if (x.Equals(y))
			{
				return 0;
			}
			if (x == y)
			{
				return 0;
			}
			Cell currentCell = The.Player.CurrentCell;
			if (currentCell == null)
			{
				return 0;
			}
			if (x == y)
			{
				return 0;
			}
			if (x.X == y.X && x.Y == y.Y)
			{
				return 0;
			}
			int x2 = currentCell.X;
			int y2 = currentCell.Y;
			int num = (x2 - x.X) * (x2 - x.X) + (y2 - x.Y) * (y2 - x.Y);
			int num2 = (x2 - y.X) * (x2 - y.X) + (y2 - y.Y) * (y2 - y.Y);
			if (num == num2)
			{
				return Stat.Random(-1, 1);
			}
			return num.CompareTo(num2);
		}
	}

	[UIView("NewGame", true, true, false, "Menu", null, false, 0, false)]
	public class EmptyCoreUIs
	{
	}

	public static Stopwatch runTime = null;

	public static XRLCore Core;

	public XRLGame Game;

	public static XRLManual Manual;

	public static TextConsole _Console;

	public static ScreenBuffer _Buffer;

	public static ParticleManager ParticleManager;

	public static int CurrentFrame;

	public static int CurrentFrameLong;

	public static int CurrentFrame10;

	public static int CurrentFrameLong10;

	public static Stopwatch FrameTimer = new Stopwatch();

	public bool EnableAnimation = true;

	public bool ShowFPS;

	public bool VisAllToggle;

	public bool CheatMaxMod;

	public bool AllowWorldMapParticles;

	public float XPMul = 1f;

	[NonSerialized]
	public bool cool;

	[NonSerialized]
	public bool IDKFA;

	[NonSerialized]
	public bool Calm;

	[NonSerialized]
	public static bool bThreadFocus = true;

	[NonSerialized]
	public string MoveConfirmDirection;

	[NonSerialized]
	public int RenderedObjects;

	public string _PlayerWalking = "";

	public SortPoint AutoexploreTarget;

	public CleanQueue<SortPoint> PlayerAvoid = new CleanQueue<SortPoint>();

	public SortPoint LastCell;

	[NonSerialized]
	public static int MemCheckTurns = 0;

	[NonSerialized]
	public const long InitialMemCheckHeadRoom = 2500000000L;

	[NonSerialized]
	public static long MemCheckHeadRoom = 2500000000L;

	[NonSerialized]
	public static bool sixArmsSet = false;

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnBeginPlayerTurnCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnEndPlayerTurnCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnEndPlayerTurnSingleCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore>> OnPassedTenPlayerTurnCallbacks = new List<Action<XRLCore>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<XRLCore, ScreenBuffer>> AfterRenderCallbacks = new List<Action<XRLCore, ScreenBuffer>>();

	[NonSerialized]
	[ModSensitiveStaticCache(true)]
	private static List<Action<string>> OnNewMessageLogEntryCallbacks = new List<Action<string>>();

	private static List<Cell> SmartUseCells = new List<Cell>();

	public static bool CludgeTargetRendered = false;

	public List<XRL.World.GameObject> CludgeCreaturesRendered;

	public static bool RenderFloorTextures = true;

	public static BeforeRenderEvent eBeforeRender = new BeforeRenderEvent();

	public static BeforeRenderLateEvent eBeforeRenderLate = new BeforeRenderLateEvent();

	[NonSerialized]
	public System.Random ConfusionRng = new System.Random();

	private static int nFrame = 0;

	public List<XRL.World.GameObject> OldHostileWalkObjects = new List<XRL.World.GameObject>();

	public List<XRL.World.GameObject> HostileWalkObjects = new List<XRL.World.GameObject>();

	private bool _isNewGameModFlow;

	public static int lastWait = 10;

	public static bool waitForSegmentOnGameThread = false;

	public static Thread CoreThread = null;

	public static string DataPath = "";

	public static string _SavePath = null;

	public static bool bStarted = false;

	public static bool IsCoreThread => CoreThread == Thread.CurrentThread;

	public static XRL.World.GameObject player
	{
		get
		{
			if (Core == null)
			{
				return null;
			}
			if (Core.Game == null)
			{
				return null;
			}
			return Core.Game.Player.Body;
		}
	}

	[Obsolete("Don't use this use player.GetConfusion() instead! Will be removed ~Q3 2021")]
	public int ConfusionLevel
	{
		get
		{
			if (Game == null)
			{
				return 0;
			}
			if (Game.Player.Body == null)
			{
				return 0;
			}
			return Game.Player.Body.GetConfusion();
		}
		set
		{
			if (Game == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a game.");
			}
			else if (Game.Player.Body == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a player body.");
			}
			else
			{
				Game.Player.Body.SetIntProperty("ConfusionLevel", value);
			}
		}
	}

	[Obsolete("Don't use this use player.GetFuriousConfusion() instead! Will be removed ~Q3 2021")]
	public int FuriousConfusion
	{
		get
		{
			if (Game == null)
			{
				return 0;
			}
			if (Game.Player.Body == null)
			{
				return 0;
			}
			return Game.Player.Body.GetFuriousConfusion();
		}
		set
		{
			if (Game == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a game.");
			}
			else if (Game.Player.Body == null)
			{
				MetricsManager.LogWarning("Trying to set confusion level without a player body.");
			}
			else
			{
				Game.Player.Body.SetIntProperty("FuriousConfusionLevel", value);
			}
		}
	}

	public static long CurrentTurn
	{
		get
		{
			if (Core.Game == null)
			{
				return 0L;
			}
			return Core.Game.TimeTicks;
		}
	}

	public string PlayerWalking
	{
		get
		{
			return _PlayerWalking;
		}
		set
		{
			_PlayerWalking = value;
			if (value == "")
			{
				Loading.SetLoadingStatus(null);
				PlayerAvoid.Clear();
				AutoexploreTarget = null;
			}
		}
	}

	public static string SavePath
	{
		get
		{
			if (_SavePath == null)
			{
				return Application.persistentDataPath;
			}
			return _SavePath;
		}
		set
		{
			_SavePath = value;
		}
	}

	public XRLCore()
	{
		if (runTime == null)
		{
			runTime = new Stopwatch();
			runTime.Start();
		}
	}

	public void Reset()
	{
		FrameTimer.Reset();
		FrameTimer.Start();
		if (Core.Game.WallTime == null)
		{
			Core.Game.WallTime = new Stopwatch();
		}
		Core.Game.WallTime.Reset();
		Core.Game.WallTime.Start();
		JournalAPI.Init();
		GameManager.Instance.gameQueue.clear();
	}

	public static int GetCurrentFrameAtFPS(int fps)
	{
		return (int)(FrameTimer.ElapsedMilliseconds / (1000 / fps));
	}

	public bool InAvoidList(SortPoint P)
	{
		for (int i = 0; i < PlayerAvoid.Items.Count; i++)
		{
			SortPoint sortPoint = PlayerAvoid.Items[i];
			if (sortPoint.X == P.X && sortPoint.Y == P.Y)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckInventoryObject(XRL.World.GameObject GO)
	{
		if (GO.CurrentCell != null)
		{
			return false;
		}
		if (GO.Equipped != null)
		{
			return false;
		}
		if (GO.InInventory != The.Player)
		{
			return false;
		}
		return true;
	}

	/// <summary>Force all mod/game sensitive static caches to be loaded</summary>
	public void LoadEverything()
	{
		if (_isNewGameModFlow)
		{
			return;
		}
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(HasGameBasedStaticCacheAttribute)).Union(ModManager.GetTypesWithAttribute(typeof(HasModSensitiveStaticCacheAttribute))))
		{
			foreach (MethodInfo item2 in from mi in item.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where mi.GetCustomAttributes(typeof(PreGameCacheInitAttribute), inherit: false).Count() > 0
				select mi)
			{
				item2.Invoke(null, new object[0]);
			}
		}
	}

	public void HotloadConfiguration(bool bGenerateCorpusData = false)
	{
		ModManager.ResetModSensitiveStaticCaches();
		LoadEverything();
		WorldFactory.Factory.Init();
		MessageQueue.AddPlayerMessage("Configuration hotloaded...");
		WriteConsoleLine("Hot Loading Books...\n");
		BookUI.InitBooks(bGenerateCorpusData);
	}

	public bool AttemptSmartUse(Cell TargetCell)
	{
		XRL.World.GameObject gameObject = SmartUse.FindPlayerSmartUseObject(TargetCell);
		if ((gameObject == null || gameObject.IsOpenLiquidVolume()) && TargetCell == The.Player.CurrentCell && TargetCell.HasObjectWithPart("Physics", SmartUse.CanPlayerTake))
		{
			return The.Player.FireEvent(XRL.World.Event.New("CommandGet", "GetOne", false, "TargetCell", TargetCell));
		}
		if (gameObject != null)
		{
			return SmartUse.PlayerPerformSmartUse(gameObject);
		}
		return false;
	}

	public void UpdateOverlay()
	{
	}

	public static void RegisterOnBeginPlayerTurnCallback(Action<XRLCore> action)
	{
		OnBeginPlayerTurnCallbacks.Add(action);
	}

	public static void RemoveOnBeginPlayerTurnCallback(Action<XRLCore> action)
	{
		OnBeginPlayerTurnCallbacks.Remove(action);
	}

	public static void RegisterOnEndPlayerTurnCallback(Action<XRLCore> action, bool Single = false)
	{
		if (Single)
		{
			OnEndPlayerTurnSingleCallbacks.Add(action);
		}
		else
		{
			OnEndPlayerTurnCallbacks.Add(action);
		}
	}

	public static void RemoveOnEndPlayerTurnCallback(Action<XRLCore> action, bool Single = false)
	{
		if (Single)
		{
			OnEndPlayerTurnSingleCallbacks.Remove(action);
		}
		else
		{
			OnEndPlayerTurnCallbacks.Remove(action);
		}
	}

	public static void RegisterOnPassedTenPlayerTurnCallback(Action<XRLCore> action)
	{
		OnPassedTenPlayerTurnCallbacks.Add(action);
	}

	public static void TenPlayerTurnsPassed()
	{
		int i = 0;
		for (int count = OnPassedTenPlayerTurnCallbacks.Count; i < count; i++)
		{
			OnPassedTenPlayerTurnCallbacks[i](Core);
		}
	}

	public static void RegisterAfterRenderCallback(Action<XRLCore, ScreenBuffer> action)
	{
		AfterRenderCallbacks.Add(action);
	}

	public static void RegisterNewMessageLogEntryCallback(Action<string> action)
	{
		if (!OnNewMessageLogEntryCallbacks.Contains(action))
		{
			OnNewMessageLogEntryCallbacks.Add(action);
		}
	}

	public static void CallNewMessageLogEntryCallbacks(string log)
	{
		try
		{
			if (OnNewMessageLogEntryCallbacks != null)
			{
				for (int i = 0; i < OnNewMessageLogEntryCallbacks.Count; i++)
				{
					OnNewMessageLogEntryCallbacks[i](log);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void CallBeginPlayerTurnCallbacks()
	{
		for (int i = 0; i < OnBeginPlayerTurnCallbacks.Count; i++)
		{
			OnBeginPlayerTurnCallbacks[i](Core);
		}
	}

	public void PlayerTurn()
	{
		CallBeginPlayerTurnCallbacks();
		for (int i = 0; i < Game.Systems.Count; i++)
		{
			Game.Systems[i].BeginPlayerTurn();
		}
		bool flag = false;
		if (The.Player != null && !The.Player.HasEffect("Confused") && !The.Player.HasEffect("HulkHoney_Tonic"))
		{
			ConfusionLevel = 0;
			FuriousConfusion = 0;
		}
		_ = FrameTimer.Elapsed.TotalMilliseconds;
		Game.Player.Messages.BeginPlayerTurn();
		if (!sixArmsSet && (The.Player?.GetBodyPartCount("Arm") ?? 0) >= 6)
		{
			AchievementManager.SetAchievement("ACH_SIX_ARMS");
			sixArmsSet = true;
		}
		if (Options.InventoryConsistencyCheck)
		{
			Inventory inventory = The.Player?.Inventory;
			if (inventory != null)
			{
				int j = 0;
				for (int count = inventory.Objects.Count; j < count; j++)
				{
					XRL.World.GameObject gameObject = inventory.Objects[j];
					if (!CheckInventoryObject(gameObject))
					{
						MessageQueue.AddPlayerMessage("Invalid inventory object: " + gameObject, 'R');
					}
				}
			}
		}
		if (Options.CheckMemory)
		{
			MemCheckTurns++;
			if (MemCheckTurns > 100)
			{
				MemCheckTurns = 0;
				long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
				if (totalMemory >= MemCheckHeadRoom)
				{
					MetricsManager.SendTelemetry("mem_warning", totalMemory.ToString());
					Popup.ShowSpace("WARNING: Caves of Qud is using a lot of memory. You should probably save the game, exit the game entirely, then relaunch and continue your save to avoid a 'too many heap' crash. You'll be warned again after an additional 500 MB of RAM is used. Currently using " + $"{totalMemory:n0}" + " bytes. At or before 3,500,000,000 bytes the game WILL crash. please send a saved game with this issue to support@freeholdgames.com!");
					if (MemCheckHeadRoom == 2500000000u)
					{
						MetricsManager.LogEvent("Memwarning");
					}
					MemCheckHeadRoom += 500000000L;
				}
			}
		}
		Sidebar.UpdateState();
		Sidebar.Update();
		Keyboard.ClearMouseEvents();
		while (The.Player.Energy.Value >= 1000 && Game.Running)
		{
			while (!GameManager.focused)
			{
				GameManager.focusedEvent.WaitOne(1000);
			}
			GameManager.Instance.CurrentGameView = Options.StageViewID;
			XRL.World.Event.ResetPool();
			int value = The.Player.Energy.Value;
			if (The.Player.CurrentZone != null)
			{
				The.Player.CurrentZone.SetActive();
			}
			Cell cell;
			Cell cell2;
			string text4;
			if (Keyboard.kbhit() || Keyboard.HasMouseEvent() || !EnableAnimation || Core.PlayerWalking == "ReopenMissileUI")
			{
				string text = null;
				string text2 = "CmdNone";
				if (!The.Game.Running)
				{
					return;
				}
				CombatJuice.startTurn();
				if (Keyboard.HasMouseEvent(filterMetaCommands: true))
				{
					for (Keyboard.MouseEvent mouseEvent = Keyboard.PopMouseEvent(); mouseEvent != null && The.Player.CurrentCell != null; mouseEvent = ((!Keyboard.HasMouseEvent(filterMetaCommands: true)) ? null : Keyboard.PopMouseEvent()))
					{
						if (mouseEvent.Event == "RightClick")
						{
							Look.ShowLooker(0, mouseEvent.x, mouseEvent.y);
						}
						if (mouseEvent.Event.StartsWith("Command:"))
						{
							text2 = mouseEvent.Event.Split(':')[1];
							break;
						}
						if (mouseEvent.Event == "LeftClick" && Options.MouseMovement)
						{
							Cell currentCell = The.Player.CurrentCell;
							cell = currentCell.ParentZone.GetCell(mouseEvent.x, mouseEvent.y);
							if (cell == currentCell)
							{
								text2 = "CmdWait";
								continue;
							}
							if (cell.PathDistanceTo(currentCell) == 1)
							{
								text = currentCell.GetDirectionFromCell(cell);
								XRL.World.GameObject gameObject2 = SmartUse.FindPlayerSmartUseObject(cell);
								if (cell.HasObjectWithPart("Brain", (XRL.World.GameObject GO) => GO.pBrain.IsHostileTowards(The.Player)))
								{
									text2 = "CmdAttack" + text;
								}
								else if (gameObject2 != null && !gameObject2.IsOpenLiquidVolume())
								{
									text2 = "CmdNone";
									SmartUse.PlayerPerformSmartUse(gameObject2);
								}
								else
								{
									text2 = "CmdMove" + text;
								}
								continue;
							}
							goto IL_03f0;
						}
						text2 = mouseEvent.Event;
						break;
					}
				}
				else if (AutoAct.Setting == "ReopenMissileUI")
				{
					AutoAct.Interrupt();
					text2 = "CmdFire";
				}
				else if (Keyboard.kbhit() || !EnableAnimation)
				{
					text2 = LegacyKeyMapping.GetNextCommand();
					string text3 = AbilityManager.MapKeyToCommand(Keyboard.MetaKey);
					if (!string.IsNullOrEmpty(text3))
					{
						ActivatedAbilities activatedAbilities = The.Player.ActivatedAbilities;
						if (activatedAbilities != null)
						{
							ActivatedAbilityEntry[] array = new ActivatedAbilityEntry[activatedAbilities.AbilityByGuid.Values.Count];
							activatedAbilities.AbilityByGuid.Values.CopyTo(array, 0);
							ActivatedAbilityEntry[] array2 = array;
							foreach (ActivatedAbilityEntry activatedAbilityEntry in array2)
							{
								if (!(activatedAbilityEntry.Command == text3))
								{
									continue;
								}
								text2 = "CmdNone";
								if (!activatedAbilityEntry.Enabled)
								{
									Popup.ShowFail(activatedAbilityEntry.DisabledMessage ?? "That ability is not enabled!");
								}
								else if (activatedAbilityEntry.Cooldown > 0)
								{
									if (Options.GetOption("OptionAbilityCooldownWarningAsMessage").ToUpper() == "YES")
									{
										MessageQueue.AddPlayerMessage("You must wait {{C|" + activatedAbilityEntry.CooldownDescription + "}} to use that ability again!");
									}
									else
									{
										Popup.ShowFail("You must wait {{C|" + activatedAbilityEntry.CooldownDescription + "}} to use that ability again!");
									}
								}
								else
								{
									CommandEvent.Send(The.Player, text3);
								}
							}
						}
					}
				}
				if (text2 == "CmdWaitMenu")
				{
					string[] options = new string[6] { "Wait 1 Turn", "Wait N Turns", "Wait 20 Turns", "Wait 100 Turns", "Wait Until Healed", "Wait Until Morning" };
					int num = Popup.ShowOptionList("Select Wait Style", options, null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
					if (num > 0)
					{
						if (num == 0)
						{
							text2 = "CmdWait";
						}
						if (num == 1)
						{
							text2 = "CmdWaitN";
						}
						if (num == 2)
						{
							text2 = "CmdWait20";
						}
						if (num == 3)
						{
							text2 = "CmdWait100";
						}
						if (num == 4)
						{
							text2 = "CmdWaitUntilHealed";
						}
						if (num == 5)
						{
							text2 = "CmdWaitUntilMorning";
						}
					}
				}
				switch (text2)
				{
				case "CmdSystemMenu":
				{
					int num5;
					int num6;
					while (true)
					{
						num5 = 0;
						num6 = 0;
						bool flag7 = CheckpointingSystem.IsCheckpointingEnabled();
						string text12 = (flag7 ? "Quit Without Saving" : "Abandon Character");
						string text13 = (flag7 ? "QUIT" : "ABANDON");
						if (flag7)
						{
							num5 = 2;
							num6 = ((!CheckpointingSystem.IsPlayerInCheckpoint()) ? Popup.ShowOptionList("", new string[7] { "&KSet Checkpoint", "Restore Checkpoint", "Key Mapping", "Options", "Game Info", "Save and Quit", text12 }, new char[7] { 'k', 'r', 'c', 'o', 'g', 's', 'q' }, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true) : Popup.ShowOptionList("", new string[7] { "Set Checkpoint", "&KRestore Checkpoint", "Key Mapping", "Options", "Game Info", "Save and Quit", text12 }, new char[7] { 'k', 'r', 'c', 'o', 'g', 's', 'q' }, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true));
						}
						else
						{
							num6 = Popup.ShowOptionList("", new string[5] { "Key Mapping", "Options", "Game Info", "Save and Quit", "Abandon Character" }, new char[5] { 'k', 'o', 'g', 's', 'a' }, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
						}
						if (num6 == 4 + num5)
						{
							bool flag8 = true;
							if (!Options.DisablePermadeath)
							{
								flag8 = false;
								string text14 = (flag7 ? Popup.AskString("If you quit without saving, you will lose all your unsaved progress. Are you sure you want to QUIT and LOSE YOUR PROGRESS? Type '" + text13 + "' to confirm.", "", text13.Length) : Popup.AskString("If you quit without saving, you will lose all your progress and your character will be lost. Are you sure you want to QUIT and LOSE YOUR PROGRESS? Type '" + text13 + "' to confirm.", "", text13.Length));
								if (!string.IsNullOrEmpty(text14) && text14.ToUpper() == text13)
								{
									flag8 = true;
								}
							}
							if (flag8)
							{
								if (flag7)
								{
									Game.DeathReason = "<nodeath>";
									Game.forceNoDeath = true;
								}
								else
								{
									Game.DeathReason = "You abandoned all hope.";
									JournalAPI.AddAccomplishment("On the " + XRL.World.Calendar.getDay() + " of " + XRL.World.Calendar.getMonth() + ", you abandoned all hope.", null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Nil, null, -1L);
								}
								Game.Running = false;
								The.Player.Energy.BaseValue = 0;
								return;
							}
						}
						if (num6 == 0 && flag7)
						{
							if (CheckpointingSystem.IsPlayerInCheckpoint())
							{
								CheckpointingSystem.DoCheckpoint();
							}
							else
							{
								Popup.Show("You can only set your checkpoint in settlements.");
							}
						}
						if (num6 == 1 && flag7)
						{
							if (CheckpointingSystem.IsPlayerInCheckpoint())
							{
								Popup.Show("You can only restore your checkpoint outside settlements.");
							}
							else if (Popup.ShowYesNoCancel("Are you sure you want to restore your checkpoint?") == DialogResult.Yes && The.Game.RestoreCheckpoint())
							{
								Sidebar.UpdateState();
								Popup.Show("Checkpoint restored!");
							}
						}
						if (num6 != 2 + num5)
						{
							break;
						}
						if (!Game.HasStringGameState("OriginalWorldSeed"))
						{
							Popup.ShowFail("This saved game predates world seed info.");
							continue;
						}
						Popup.ShowBlockWithCopy("\n\n           " + The.Game.GetStringGameState("GameMode") + " mode.\n\n           Turn " + The.Game.Turns + "\n\n          World seed: " + Game.GetStringGameState("OriginalWorldSeed") + "     \n\n\n   ", " {{W|C}} - Copy seed  {{W|ESC}} - Exit ", "", Game.GetStringGameState("OriginalWorldSeed"));
					}
					if (num6 == 3 + num5)
					{
						SaveGame("Primary.sav");
						Popup.Show("Game saved!");
						Game.DeathReason = "<nodeath>";
						Game.forceNoDeath = true;
						Game.Running = false;
						return;
					}
					if (num6 == num5)
					{
						KeyMappingUI.Show();
					}
					if (num6 == 1 + num5)
					{
						OptionsUI.Show();
					}
					goto IL_372e;
				}
				case "CmdMoveW":
					The.Player.Move("W");
					goto IL_372e;
				case "CmdMoveE":
					The.Player.Move("E");
					goto IL_372e;
				case "CmdMoveN":
					The.Player.Move("N");
					goto IL_372e;
				case "CmdMoveS":
					The.Player.Move("S");
					goto IL_372e;
				case "CmdMoveNW":
					The.Player.Move("NW");
					goto IL_372e;
				case "CmdMoveNE":
					The.Player.Move("NE");
					goto IL_372e;
				case "CmdMoveSW":
					The.Player.Move("SW");
					goto IL_372e;
				case "CmdMoveSE":
					The.Player.Move("SE");
					goto IL_372e;
				case "CmdMoveTo":
				{
					Cell currentCell2 = The.Player.CurrentCell;
					if (currentCell2 != null)
					{
						cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 9999999, currentCell2.X, currentCell2.Y, Locked: false, AllowVis.OnlyExplored, null, null, null, null, "Move where?");
						if (cell2 != null && cell2 != currentCell2)
						{
							if (!cell2.IsAdjacentTo(currentCell2))
							{
								goto IL_1a0b;
							}
							string directionFromCell = currentCell2.GetDirectionFromCell(cell2);
							if (!string.IsNullOrEmpty(directionFromCell) && directionFromCell != "." && directionFromCell != "?")
							{
								The.Player.Move(directionFromCell);
							}
						}
					}
					goto IL_372e;
				}
				case "CmdMoveToEdge":
					text4 = PickDirection.ShowPicker("Move to which edge? (\u0018\u0019\u001a\u001b)");
					switch (text4)
					{
					case "N":
					case "S":
					case "E":
					case "W":
						break;
					default:
						goto IL_372e;
					}
					goto IL_1a83;
				case "CmdMoveToPointOfInterest":
				{
					List<PointOfInterest> @for = GetPointsOfInterestEvent.GetFor(The.Player);
					if (@for == null || @for.Count <= 0)
					{
						Popup.ShowFail("You haven't found any points of interest nearby.");
						return;
					}
					@for.Sort(PointOfInterest.Compare);
					string[] array3 = new string[@for.Count];
					char[] array4 = new char[@for.Count];
					IRenderable[] array5 = new IRenderable[@for.Count];
					char c = 'a';
					int n = 0;
					for (int count4 = @for.Count; n < count4; n++)
					{
						PointOfInterest pointOfInterest = @for[n];
						array3[n] = pointOfInterest.GetDisplayName(The.Player);
						array4[n] = ((c <= 'z') ? c++ : ' ');
						array5[n] = pointOfInterest.GetIcon();
					}
					int num4 = Popup.ShowOptionList("Go to which point of interest?", array3, array4, 1, null, 78, RespectOptionNewlines: true, AllowEscape: true, 0, "", null, null, array5);
					if (num4 >= 0)
					{
						@for[num4].NavigateTo(The.Player);
					}
					goto IL_372e;
				}
				case "CmdAttackW":
					The.Player.AttackDirection("W");
					goto IL_372e;
				case "CmdAttackE":
					The.Player.AttackDirection("E");
					goto IL_372e;
				case "CmdAttackN":
					The.Player.AttackDirection("N");
					goto IL_372e;
				case "CmdAttackS":
					The.Player.AttackDirection("S");
					goto IL_372e;
				case "CmdAttackNW":
					The.Player.AttackDirection("NW");
					goto IL_372e;
				case "CmdAttackNE":
					The.Player.AttackDirection("NE");
					goto IL_372e;
				case "CmdAttackSW":
					The.Player.AttackDirection("SW");
					goto IL_372e;
				case "CmdAttackSE":
					The.Player.AttackDirection("SE");
					goto IL_372e;
				case "CmdAttackU":
					if (The.Player.CurrentCell.HasObjectWithPart("StairsUp"))
					{
						The.Player.AttackDirection("U");
					}
					goto IL_372e;
				case "CmdAttackD":
					if (The.Player.CurrentCell.HasObjectWithPart("StairsDown"))
					{
						The.Player.AttackDirection("D");
					}
					goto IL_372e;
				case "CmdAttackDirection":
				{
					string text6 = PickDirection.ShowPicker("Attack in what direction?");
					if (!string.IsNullOrEmpty(text6) && text6 != "." && text6 != "?")
					{
						The.Player.AttackDirection(text6);
					}
					goto IL_372e;
				}
				case "CmdAttackCell":
				{
					Cell currentCell6 = The.Player.CurrentCell;
					if (currentCell6 != null)
					{
						Cell cell3 = PickTarget.ShowPicker(PickTarget.PickStyle.EmptyCell, 1, 1, currentCell6.X, currentCell6.Y, Locked: false, AllowVis.OnlyExplored, null, null, null, null, "Attack where?", EnforceRange: true);
						if (cell3 != null && cell3 != currentCell6 && cell3.IsAdjacentTo(currentCell6))
						{
							string directionFromCell2 = currentCell6.GetDirectionFromCell(cell3);
							if (!string.IsNullOrEmpty(directionFromCell2) && directionFromCell2 != "." && directionFromCell2 != "?")
							{
								The.Player.AttackDirection(directionFromCell2);
							}
						}
					}
					goto IL_372e;
				}
				case "CmdMoveU":
					if (The.Player.CurrentCell.FireEvent(XRL.World.Event.New("ClimbUp", "GO", The.Player)))
					{
						Cell currentCell7 = The.Player.CurrentCell;
						if (currentCell7 != null && !string.IsNullOrEmpty(currentCell7.ParentZone.SpecialUpMessage()))
						{
							Popup.Show(currentCell7.ParentZone.SpecialUpMessage());
						}
						else if (currentCell7 != null && !currentCell7.ParentZone.IsWorldMap() && currentCell7.ParentZone.Z <= 10 && !currentCell7.ParentZone.IsInside())
						{
							if (The.Player.HasPart("Stomach") && The.Player.GetPart<Stomach>().IsFamished() && !The.Core.IDKFA)
							{
								Popup.ShowFail("You're too famished to travel long distances.");
							}
							else if (The.Player.HasEffect("Lost"))
							{
								Popup.ShowFail("You are lost!");
							}
							else if (The.Player.HasEffect("Burrowed"))
							{
								Popup.ShowFail("You cannot travel long distances while burrowed.");
							}
							else if (The.Player.AreHostilesNearby() && !The.Player.IsFlying)
							{
								Popup.ShowFail("There are hostiles nearby!");
							}
							else if (!Options.AskForWorldmap || Popup.ShowYesNoCancel("Are you sure you want to go to the world map?") == DialogResult.Yes)
							{
								try
								{
									string address = currentCell7.GetAddress();
									if (address.Contains("."))
									{
										Game.SetStringGameState("LastLocationOnSurface", address);
									}
								}
								catch (Exception ex)
								{
									LogError(ex);
								}
								string zoneWorld = Game.ZoneManager.ActiveZone.GetZoneWorld();
								int zonewX = Game.ZoneManager.ActiveZone.GetZonewX();
								int zonewY = Game.ZoneManager.ActiveZone.GetZonewY();
								Zone zone = Game.ZoneManager.GetZone(zoneWorld);
								The.Player.SystemMoveTo(zone.GetCell(zonewX, zonewY));
								Cell currentCell8 = The.Player.CurrentCell;
								if (currentCell8 != null && currentCell8.ParentZone == zone)
								{
									The.ZoneManager.SetActiveZone(zone.ZoneID);
								}
								The.ZoneManager.ProcessGoToPartyLeader();
							}
						}
						else if (!The.Player.CurrentCell.ParentZone.IsWorldMap() && The.Player.GetTotalConfusion() <= 0 && (!Options.AskAutostair || Popup.ShowYesNoCancel("Would you like to walk to the nearest stairway up?") == DialogResult.Yes))
						{
							goto IL_203d;
						}
					}
					goto IL_372e;
				case "CmdMoveD":
				{
					Cell currentCell5 = The.Player.CurrentCell;
					if (currentCell5 != null && currentCell5.ParentZone.IsWorldMap())
					{
						The.Player.PullDown(AllowAlternate: true);
					}
					else if (currentCell5.FireEvent(XRL.World.Event.New("ClimbDown", "GO", The.Player)) && !currentCell5.ParentZone.IsWorldMap() && The.Player.GetTotalConfusion() <= 0 && (!Options.AskAutostair || Popup.ShowYesNoCancel("Would you like to walk to the nearest stairway down?") == DialogResult.Yes))
					{
						goto IL_20d1;
					}
					goto IL_372e;
				}
				case "CmdWait":
				{
					int num3 = 1000;
					if (The.Player.IsAflame())
					{
						if (Firefighting.AttemptFirefighting(The.Player, The.Player, 1000, Automatic: true))
						{
							num3 = 0;
						}
					}
					else if (The.Player.IsFrozen())
					{
						The.Player.TemperatureChange(20);
					}
					else
					{
						The.Player.pPhysics.Search();
					}
					The.Player.FireEvent("PassedTurn");
					if (num3 > 0)
					{
						The.Player.UseEnergy(num3, "Pass");
					}
					goto IL_372e;
				}
				case "CmdWait20":
					if (!AutoAct.ShouldHostilesInterrupt(".", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = ".20";
						The.Player.UseEnergy(1000, "Pass");
						Loading.SetLoadingStatus("Waiting...");
					}
					goto IL_372e;
				case "CmdWaitN":
					try
					{
						if (!AutoAct.ShouldHostilesInterrupt(".", null, logSpot: false, popSpot: true))
						{
							int highestActivatedAbilityCooldownTurns = The.Player.GetHighestActivatedAbilityCooldownTurns();
							int start = Math.Max(lastWait, highestActivatedAbilityCooldownTurns);
							int? num2 = Popup.AskNumber("How many turns would you like to wait?", start);
							if (num2.HasValue)
							{
								int value2 = num2.Value;
								if (value2 > 0)
								{
									if (value2 != highestActivatedAbilityCooldownTurns)
									{
										lastWait = value2;
									}
									AutoAct.Setting = "." + value2;
									Game.Player.Body.UseEnergy(1000, "Pass");
									The.Player.UseEnergy(1000, "Pass");
									Loading.SetLoadingStatus(XRL.World.Event.NewStringBuilder().Clear().Append("Waiting for ")
										.Append(value2.Things("turn"))
										.Append("...")
										.ToString());
								}
								else
								{
									MessageQueue.AddPlayerMessage(value2 + " is not a valid number of turns to wait.", 'K');
								}
							}
						}
					}
					catch (Exception x)
					{
						MetricsManager.LogError("Encountered exception inside CmdWaitN.", x);
					}
					goto IL_372e;
				case "CmdWait100":
					if (!AutoAct.ShouldHostilesInterrupt(".", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = ".100";
						The.Player.UseEnergy(1000, "Pass");
						Loading.SetLoadingStatus("Waiting 100 turns...");
					}
					goto IL_372e;
				case "CmdWaitUntilHealed":
					if (!AutoAct.ShouldHostilesInterrupt("r", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = "r";
						Core.Game.ActionManager.RestingUntilHealed = true;
						Core.Game.ActionManager.RestingUntilHealedCount = 0;
						The.Player.UseEnergy(1000, "Pass");
						Loading.SetLoadingStatus("Resting until healed...");
					}
					goto IL_372e;
				case "CmdWaitUntilMorning":
					if (!AutoAct.ShouldHostilesInterrupt("z", null, logSpot: false, popSpot: true))
					{
						AutoAct.Setting = "z";
						The.Player.UseEnergy(1000, "Pass");
						Loading.SetLoadingStatus("Resting until morning...");
					}
					goto IL_372e;
				case "CmdShowFPS":
					ShowFPS = !ShowFPS;
					goto IL_372e;
				case "CmdShowReachability":
				{
					ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
					for (int num7 = 0; num7 < 80; num7++)
					{
						for (int num8 = 0; num8 < 25; num8++)
						{
							scrapBuffer.Goto(num7, num8);
							if (Core.Game.ZoneManager.ActiveZone.IsReachable(num7, num8))
							{
								scrapBuffer.Write(".");
							}
							else
							{
								scrapBuffer.Write("#");
							}
						}
					}
					Popup._TextConsole.DrawBuffer(scrapBuffer);
					Keyboard.getch();
					goto IL_372e;
				}
				case "CmdUse":
				{
					Cell currentCell4 = The.Player.CurrentCell;
					List<Cell> list = null;
					if (text == null)
					{
						list = currentCell4.GetAdjacentCells();
						list.Add(currentCell4);
					}
					else
					{
						SmartUseCells.Clear();
						list = SmartUseCells;
						Cell cellFromDirection4 = currentCell4.GetCellFromDirection(text);
						if (cellFromDirection4 != null)
						{
							list.Add(cellFromDirection4);
						}
						list.Add(currentCell4);
					}
					XRL.World.GameObject gameObject3 = null;
					bool flag3 = false;
					int l = 0;
					for (int count2 = list.Count; l < count2; l++)
					{
						XRL.World.GameObject gameObject4 = SmartUse.FindPlayerSmartUseObject(list[l]);
						if (gameObject4 != null)
						{
							if (gameObject3 != null)
							{
								flag3 = true;
							}
							else
							{
								gameObject3 = gameObject4;
							}
						}
					}
					bool flag4 = false;
					if (list.Contains(currentCell4))
					{
						flag4 = currentCell4.HasObjectWithPart("Physics", SmartUse.CanPlayerTake);
					}
					SmartUseCells.Clear();
					if (gameObject3 != null)
					{
						if (!flag3 && !flag4)
						{
							SmartUse.PlayerPerformSmartUse(gameObject3);
						}
						else
						{
							string text8 = PickDirection.ShowPicker();
							if (text8 == null)
							{
								return;
							}
							Cell cellFromDirection5 = currentCell4.GetCellFromDirection(text8);
							if (cellFromDirection5 != null)
							{
								AttemptSmartUse(cellFromDirection5);
							}
						}
					}
					else
					{
						The.Player.FireEvent(XRL.World.Event.New("CommandGet", "GetOne", false));
					}
					goto IL_372e;
				}
				case "CmdLook":
					if (Game.Player.Body.GetTotalConfusion() > 0)
					{
						if (Game.Player.Body.GetFuriousConfusion() > 0)
						{
							Popup.ShowFail("You cannot examine things while you are enraged.");
						}
						else
						{
							Popup.ShowFail("You cannot examine things while you are confused.");
						}
					}
					else
					{
						Cell currentCell3 = The.Player.CurrentCell;
						Look.ShowLooker(0, currentCell3.X, currentCell3.Y);
					}
					goto IL_372e;
				case "CmdHelp":
					Manual.ShowHelp("");
					goto IL_372e;
				case "CmdToggleAnimation":
					EnableAnimation = !EnableAnimation;
					goto IL_372e;
				case "CmdJournal":
					Screens.CurrentScreen = 6;
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdTinkering":
					Screens.CurrentScreen = 7;
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdExplore":
					The.Player.CurrentCell.ParentZone.ExploreAll();
					Core.VisAllToggle = !Core.VisAllToggle;
					goto IL_372e;
				case "CmdSaveAndQuit":
					if (Popup.ShowYesNoCancel("Are you sure you want to save and quit?") == DialogResult.Yes)
					{
						SaveGame("Primary.sav");
						Game.Running = false;
						return;
					}
					goto IL_372e;
				case "CmdQuit":
					if (Popup.ShowYesNoCancel("Are you sure you want to quit?") == DialogResult.Yes)
					{
						switch (Popup.ShowYesNoCancel("Do you want to save first?"))
						{
						case DialogResult.Yes:
							SaveGame("Primary.sav");
							Game.DeathReason = "<nodeath>";
							Game.forceNoDeath = true;
							Game.Running = false;
							return;
						case DialogResult.No:
						{
							bool flag2 = true;
							if (!Options.DisablePermadeath && !CheckpointingSystem.IsCheckpointingEnabled())
							{
								flag2 = false;
								string text5 = Popup.AskString("If you quit without saving, you will lose all your unsaved progress. Are you sure you want to QUIT and LOSE YOUR PROGRESS? Type 'ABANDON' to confirm.", "", 7);
								if (!string.IsNullOrEmpty(text5) && text5.ToUpper() == "ABANDON")
								{
									flag2 = true;
								}
							}
							if (flag2)
							{
								if (CheckpointingSystem.IsCheckpointingEnabled())
								{
									Game.DeathReason = "<nodeath>";
									Game.forceNoDeath = true;
								}
								else
								{
									Game.DeathReason = "You abandoned all hope.";
									JournalAPI.AddAccomplishment("On the " + XRL.World.Calendar.getDay() + " of " + XRL.World.Calendar.getMonth() + ", you abandoned all hope.", null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Nil, null, -1L);
								}
								Game.Running = false;
								The.Player.Energy.BaseValue = 0;
								return;
							}
							break;
						}
						}
					}
					goto IL_372e;
				case "CmdSave":
					if (Options.AllowSaveLoad && Popup.ShowYesNoCancel("Quick save the game?") == DialogResult.Yes)
					{
						The.Game.QuickSave();
						Popup.Show("Game saved!");
					}
					goto IL_372e;
				case "CmdLoad":
					if (Options.AllowSaveLoad && Popup.ShowYesNoCancel("Load your last quick save?") == DialogResult.Yes && The.Game.QuickLoad())
					{
						Sidebar.UpdateState();
						Popup.Show("Game loaded!");
					}
					goto IL_372e;
				case "CmdAbilities":
				{
					The.Player.ModIntProperty("HasAccessedAbilities", 1);
					string text15 = AbilityManager.Show(The.Player);
					if (!string.IsNullOrEmpty(text15))
					{
						CommandEvent.Send(The.Player, text15);
					}
					goto IL_372e;
				}
				case "CmdTarget":
				{
					Cell currentCell9 = The.Player.CurrentCell;
					Cell cell4 = PickTarget.ShowPicker(PickTarget.PickStyle.Burst, 0, 999, currentCell9.X, currentCell9.Y, Locked: true, AllowVis.OnlyVisible);
					if (cell4 != null)
					{
						Sidebar.CurrentTarget = cell4.GetCombatTarget(The.Player, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
					}
					goto IL_372e;
				}
				case "CmdThrow":
				{
					Body body2 = The.Player.Body;
					if (body2 != null)
					{
						BodyPart firstPart = body2.GetFirstPart("Thrown Weapon");
						if (firstPart == null || firstPart.Equipped == null)
						{
							Popup.ShowFail("You do not have a thrown weapon equipped!");
						}
						else
						{
							The.Player.FireEvent(XRL.World.Event.New("CommandThrowWeapon"));
						}
					}
					goto IL_372e;
				}
				case "CmdFire":
				{
					bool flag5 = false;
					bool flag6 = false;
					string text11 = null;
					List<XRL.World.GameObject> missileWeapons = The.Player.GetMissileWeapons();
					if (missileWeapons != null && missileWeapons.Count > 0)
					{
						int m = 0;
						for (int count3 = missileWeapons.Count; m < count3; m++)
						{
							if (missileWeapons[m].GetPart("MissileWeapon") is MissileWeapon missileWeapon)
							{
								flag6 = true;
								if (missileWeapon.ReadyToFire())
								{
									flag5 = true;
									break;
								}
								if (text11 == null)
								{
									text11 = missileWeapon.GetNotReadyToFireMessage();
								}
							}
						}
					}
					if (!flag6)
					{
						Popup.ShowFail("You do not have a missile weapon equipped!");
					}
					else if (!flag5)
					{
						Popup.ShowFail(text11 ?? ("You need to reload! (" + ControlManager.getCommandInputDescription("CmdReload", Options.ModernUI) + ")"));
					}
					else
					{
						The.Player.FireEvent(XRL.World.Event.New("CommandFireMissileWeapon"));
					}
					goto IL_372e;
				}
				case "CmdWish":
				{
					string text10 = Popup.AskString("Your wish is my command!", "", 999);
					if (!string.IsNullOrEmpty(text10))
					{
						try
						{
							MetricsManager.LogEvent("Wish:" + text10);
							Wishing.HandleWish(The.Player, text10);
						}
						catch
						{
						}
					}
					goto IL_372e;
				}
				case "CmdFactions":
					Screens.ShowPopup("Factions", The.Player);
					goto IL_372e;
				case "CmdQuests":
					Screens.CurrentScreen = 5;
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdActiveEffects":
					The.Player.ShowActiveEffects();
					goto IL_372e;
				case "CmdLastStatusPage":
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdCharacter":
					Screens.CurrentScreen = 1;
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdInventory":
					Screens.CurrentScreen = 2;
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdEquipment":
					Screens.CurrentScreen = 3;
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdSkillsPowers":
					Screens.CurrentScreen = 0;
					Screens.Show(The.Player);
					goto IL_372e;
				case "CmdAutoExplore":
					break;
				case "CmdAutoAttack":
					goto IL_2be9;
				case "CmdAttackNearest":
					goto IL_2c42;
				case "CmdWalk":
					if (Core.Game.ZoneManager.ActiveZone != null && !Core.Game.ZoneManager.ActiveZone.IsWorldMap())
					{
						goto IL_3213;
					}
					goto IL_372e;
				case "CmdGet":
					The.Player.FireEvent("CommandGet");
					goto IL_372e;
				case "CmdGetFrom":
					The.Player.FireEvent("CommandGetFrom");
					goto IL_372e;
				case "CmdOpen":
				{
					string text9 = PickDirection.ShowPicker();
					if (text9 != null)
					{
						Cell cellFromDirection6 = The.Player.CurrentCell.GetCellFromDirection(text9);
						if (cellFromDirection6 != null)
						{
							List<XRL.World.GameObject> objectsWithRegisteredEvent = cellFromDirection6.GetObjectsWithRegisteredEvent("Open");
							if (objectsWithRegisteredEvent.Count == 1)
							{
								objectsWithRegisteredEvent[0].FireEvent(XRL.World.Event.New("Open", "Opener", The.Player));
							}
							else if (objectsWithRegisteredEvent.Count > 1)
							{
								Popup.PickGameObject("Open", objectsWithRegisteredEvent, AllowEscape: true)?.FireEvent(XRL.World.Event.New("Open", "Opener", The.Player));
							}
						}
					}
					goto IL_372e;
				}
				case "CmdXP":
				{
					XRL.World.GameObject gameObject5 = The.Player;
					Game.Player.Body = null;
					Popup.bSuppressPopups = true;
					gameObject5.AwardXP(250000);
					Popup.bSuppressPopups = false;
					ParticleManager.Banners.Clear();
					Game.Player.Body = gameObject5;
					goto IL_372e;
				}
				case "CmdReload":
					CommandReloadEvent.Execute(The.Player);
					goto IL_372e;
				case "CmdTalk":
				{
					string text7 = PickDirection.ShowPicker();
					if (text7 != null)
					{
						Cell cellFromDirection3 = The.Player.CurrentCell.GetCellFromDirection(text7);
						if (cellFromDirection3 != null)
						{
							XRL.World.GameObject firstObjectWithPart = cellFromDirection3.GetFirstObjectWithPart("ConversationScript", delegate(XRL.World.GameObject GO)
							{
								if (GO.IsPlayer())
								{
									return false;
								}
								return (GO.pRender == null || GO.pRender.Visible) ? true : false;
							});
							if (firstObjectWithPart == null || !firstObjectWithPart.GetPart<ConversationScript>().AttemptConversation(Silent: true))
							{
								XRL.World.GameObject firstObjectWithRegisteredEvent = cellFromDirection3.GetFirstObjectWithRegisteredEvent("ObjectTalking", (XRL.World.GameObject GO) => !GO.IsPlayer());
								if (firstObjectWithRegisteredEvent == null || !firstObjectWithRegisteredEvent.FireEvent("ObjectTalking"))
								{
									firstObjectWithPart?.GetPart<ConversationScript>().AttemptConversation();
								}
							}
						}
					}
					goto IL_372e;
				}
				case "CmdToggleMessageVerbosity":
					Game.Player.Messages.Terse = !Game.Player.Messages.Terse;
					if (Game.Player.Messages.Terse)
					{
						MessageQueue.AddPlayerMessage("Set Terse messages");
					}
					if (!Game.Player.Messages.Terse)
					{
						MessageQueue.AddPlayerMessage("Set Verbose messages");
					}
					goto IL_372e;
				case "CmdZoomIn":
					GameManager.Instance.uiQueue.queueTask(delegate
					{
						GameManager.Instance.OnScroll(new Vector2(0f, 1f));
					});
					goto IL_372e;
				case "CmdZoomOut":
					GameManager.Instance.uiQueue.queueTask(delegate
					{
						GameManager.Instance.OnScroll(new Vector2(0f, -1f));
					});
					goto IL_372e;
				case "CmdShowSidebar":
					Sidebar.Hidden = !Sidebar.Hidden;
					goto IL_372e;
				case "CmdShowSidebarMessages":
					Sidebar.SidebarState++;
					if (Sidebar.SidebarState >= 4)
					{
						Sidebar.SidebarState = 0;
					}
					goto IL_372e;
				case "CmdNoclipU":
				{
					Cell cellFromDirection2 = The.Player.CurrentCell.GetCellFromDirection("U", BuiltOnly: false);
					if (cellFromDirection2 != null)
					{
						The.Player.SystemMoveTo(cellFromDirection2);
						The.ZoneManager.SetActiveZone(cellFromDirection2.ParentZone.ZoneID);
						The.ZoneManager.ProcessGoToPartyLeader();
					}
					goto IL_372e;
				}
				case "CmdNoclipD":
				{
					Cell cellFromDirection = The.Player.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
					if (cellFromDirection != null)
					{
						The.Player.SystemMoveTo(cellFromDirection);
						The.ZoneManager.SetActiveZone(cellFromDirection.ParentZone.ZoneID);
						The.ZoneManager.ProcessGoToPartyLeader();
					}
					goto IL_372e;
				}
				case "CmdDismemberLimb":
				{
					Body body = The.Player.Body;
					BodyPart dismemberableBodyPart = Axe_Dismember.GetDismemberableBodyPart(The.Player);
					if (dismemberableBodyPart != null)
					{
						body.Dismember(dismemberableBodyPart);
					}
					goto IL_372e;
				}
				case "CmdRegenerateLimb":
					GenericNotifyEvent.Send(The.Player, "RegenerateLimb");
					goto IL_372e;
				case "CmdMessageHistory":
					The.Game.Player.Messages.Show();
					goto IL_372e;
				default:
					if (The.Player != null)
					{
						CommandEvent.Send(The.Player, text2);
					}
					goto IL_372e;
				case "CmdNone":
				case "CmdShowBrainlog":
				case "CmdUnknown":
					goto IL_372e;
					IL_372e:
					Sidebar.UpdateState();
					Sidebar.Update();
					flag = true;
					UpdateOverlay();
					if (Thread.CurrentThread != CoreThread)
					{
						MetricsManager.LogEditorWarning("Ending player turn on game thread (1)...");
						GameManager.runPlayerTurnOnUIThread = false;
					}
					goto IL_377a;
				}
				if (!The.Player.CurrentCell.ParentZone.IsWorldMap())
				{
					The.Player.CurrentCell.ParentZone.ClearNavigationCaches();
					AutoAct.Setting = "?";
				}
				break;
			}
			if (Thread.CurrentThread != CoreThread)
			{
				MetricsManager.LogEditorWarning("Ending player turn on game thread (2)...");
				GameManager.runPlayerTurnOnUIThread = false;
			}
			goto IL_377a;
			IL_377a:
			int num9 = 0;
			for (int count5 = OnEndPlayerTurnCallbacks.Count; num9 < count5; num9++)
			{
				OnEndPlayerTurnCallbacks[num9](this);
			}
			_ = FrameTimer.Elapsed.TotalMilliseconds;
			if (The.Player.Energy.Value > 0)
			{
				if (value != The.Player.Energy.Value)
				{
					value = The.Player.Energy.Value;
					Game.Player.Messages.EndPlayerTurn();
				}
				RenderBase(UpdateSidebar: false);
			}
			else
			{
				Game.Player.Messages.LastMessage = Game.Player.Messages.Messages.Count;
				RenderBase(UpdateSidebar: false);
			}
			if (flag)
			{
				Core.Game.ActionManager.UpdateMinimap();
				int num10 = 0;
				for (int count6 = OnEndPlayerTurnSingleCallbacks.Count; num10 < count6; num10++)
				{
					OnEndPlayerTurnSingleCallbacks[num10](this);
				}
				flag = false;
			}
			if (The.Player.Energy.Value >= 1000)
			{
				Keyboard.IdleWait();
				if (ActionManager.SkipPlayerTurn)
				{
					ActionManager.SkipPlayerTurn = false;
					break;
				}
			}
			if (Thread.CurrentThread != CoreThread)
			{
				MetricsManager.LogEditorWarning("Ending player turn on game thread (3)...");
				GameManager.runPlayerTurnOnUIThread = false;
				break;
			}
			continue;
			IL_03f0:
			XRL.World.GameObject gameObject6 = SmartUse.FindPlayerSmartUseObject(cell);
			if (gameObject6 != null && !gameObject6.IsOpenLiquidVolume())
			{
				AutoAct.Setting = "U" + cell.X + "," + cell.Y;
			}
			else
			{
				AutoAct.Setting = "M" + cell.X + "," + cell.Y;
			}
			break;
			IL_1a83:
			AutoAct.Setting = "G" + text4;
			break;
			IL_2c42:
			if (The.Player.GetTotalConfusion() > 0)
			{
				if (The.Player.GetFuriousConfusion() > 0)
				{
					The.Player.FireEvent(XRL.World.Event.New("CmdMove" + Directions.GetRandomDirection()));
				}
				else
				{
					Popup.ShowFail("You cannot autoattack while you are confused.");
				}
				break;
			}
			Cell currentCell10 = The.Player.CurrentCell;
			XRL.World.GameObject gameObject7 = The.Player.Target;
			if (The.Player.GetEffect("Engulfed") is Engulfed engulfed && XRL.World.GameObject.validate(engulfed.EngulfedBy) && engulfed.EngulfedBy.IsHostileTowards(The.Player))
			{
				gameObject7 = engulfed.EngulfedBy;
			}
			if (gameObject7 != null && gameObject7.pBrain != null && !gameObject7.IsHostileTowards(The.Player))
			{
				Popup.ShowFail("You do not autoattack " + gameObject7.the + gameObject7.ShortDisplayName + " because " + gameObject7.itis + " not hostile to you.");
				break;
			}
			bool flag9 = false;
			int num11 = ((gameObject7 == null) ? 9999999 : currentCell10.DistanceTo(gameObject7));
			if (num11 == 1)
			{
				string directionFromCell3 = currentCell10.GetDirectionFromCell(gameObject7.CurrentCell);
				if (!string.IsNullOrEmpty(directionFromCell3) && directionFromCell3 != ".")
				{
					The.Player.AttackDirection(directionFromCell3);
					break;
				}
			}
			else if (num11 == 0)
			{
				Cell randomElement = currentCell10.GetLocalNavigableAdjacentCells(The.Player).GetRandomElement();
				if (randomElement != null)
				{
					string directionFromCell4 = currentCell10.GetDirectionFromCell(randomElement);
					if (!string.IsNullOrEmpty(directionFromCell4) && directionFromCell4 != ".")
					{
						int count7 = Core.Game.Player.Messages.Messages.Count;
						if (The.Player.Move(directionFromCell4) || count7 < Core.Game.Player.Messages.Messages.Count)
						{
							break;
						}
					}
				}
				flag9 = true;
			}
			if (gameObject7 != null && !gameObject7.IsVisible())
			{
				Popup.ShowFail("You cannot see your target.");
				break;
			}
			List<Cell> localAdjacentCells = currentCell10.GetLocalAdjacentCells();
			int? num12 = null;
			int num13 = 0;
			for (int count8 = localAdjacentCells.Count; num13 < count8; num13++)
			{
				XRL.World.GameObject combatTarget = localAdjacentCells[num13].GetCombatTarget(The.Player);
				if (combatTarget != null && combatTarget.IsHostileTowards(The.Player) && combatTarget.IsVisible())
				{
					int? num14 = The.Player.Con(combatTarget);
					if (num14.HasValue && (!num12.HasValue || num14 < num12 || (num14 == num12 && combatTarget.Health() < gameObject7.Health())))
					{
						gameObject7 = combatTarget;
					}
				}
			}
			if (gameObject7 != null)
			{
				string directionFromCell5 = currentCell10.GetDirectionFromCell(gameObject7.CurrentCell);
				if (!string.IsNullOrEmpty(directionFromCell5) && directionFromCell5 != ".")
				{
					The.Player.AttackDirection(directionFromCell5);
					break;
				}
			}
			num11 = int.MaxValue;
			List<XRL.World.GameObject> list2 = currentCell10.ParentZone.FastSquareVisibility(currentCell10.X, currentCell10.Y, 80, "Brain", The.Player, VisibleToPlayerOnly: true, IncludeWalls: true);
			int num15 = 0;
			for (int count9 = list2.Count; num15 < count9; num15++)
			{
				XRL.World.GameObject gameObject8 = list2[num15];
				if (gameObject8.IsHostileTowards(The.Player))
				{
					int num16 = currentCell10.DistanceTo(gameObject8);
					bool flag10 = false;
					if (num16 < num11)
					{
						flag10 = true;
					}
					else if (num16 == num11 && The.Player.Con(gameObject8) < The.Player.Con(gameObject7))
					{
						flag10 = true;
					}
					if (flag10 && gameObject8.CurrentCell.GetCombatTarget(The.Player) == gameObject8)
					{
						gameObject7 = gameObject8;
						num11 = num16;
					}
				}
			}
			if (gameObject7 != null)
			{
				FindPath findPath = new FindPath(currentCell10, gameObject7.CurrentCell, PathGlobal: false, PathUnlimited: true, The.Player, 20, ExploredOnly: true);
				if (!findPath.bFound || findPath.Steps.Count == 0)
				{
					if (flag9)
					{
						Popup.ShowFail("You can't find a way to flee from " + gameObject7.the + gameObject7.ShortDisplayName + ".");
					}
					else
					{
						Popup.ShowFail("You can't find a way to reach " + gameObject7.the + gameObject7.ShortDisplayName + ".");
					}
				}
				else
				{
					The.Player.Move(findPath.Directions[0]);
				}
			}
			else if (flag9)
			{
				gameObject7 = The.Player.Target;
				Popup.ShowFail("You can't find a way to flee from " + gameObject7.the + gameObject7.ShortDisplayName + ".");
			}
			else
			{
				MessageQueue.AddPlayerMessage("You don't see any hostiles nearby.");
			}
			break;
			IL_2be9:
			if (The.Player.Target == null)
			{
				Popup.ShowFail("You don't have a target.");
			}
			else if (The.Player.GetConfusion() > 0 && The.Player.GetFuriousConfusion() <= 0)
			{
				Popup.ShowFail("You cannot autoattack while you are confused.");
			}
			else
			{
				AutoAct.Setting = "a";
			}
			break;
			IL_3213:
			string text16 = PickDirection.ShowPicker();
			if (text16 == null)
			{
				break;
			}
			Cell currentCell11 = The.Player.CurrentCell;
			if (currentCell11 != null)
			{
				Cell cellFromDirection7 = currentCell11.GetCellFromDirection(text16);
				if (cellFromDirection7 != null)
				{
					if (cellFromDirection7.IsEmpty())
					{
						AutoAct.Setting = text16;
					}
					else
					{
						if (cellFromDirection7.HasObjectWithPart("Combat", (XRL.World.GameObject GO) => GO.pBrain != null && GO.pBrain.IsHostileTowards(The.Player)))
						{
							Popup.ShowFail("You may not {{Y|w}}alk into a hostile creature!");
							goto IL_377a;
						}
						AutoAct.Setting = text16;
					}
				}
			}
			if (AutoAct.Setting == ".")
			{
				AutoAct.Setting = ".20";
			}
			break;
			IL_203d:
			AutoAct.Setting = "<";
			break;
			IL_20d1:
			AutoAct.Setting = ">";
			break;
			IL_1a0b:
			AutoAct.Setting = "M" + cell2.X + "," + cell2.Y;
			break;
		}
		for (int num17 = 0; num17 < Game.Systems.Count; num17++)
		{
			Game.Systems[num17].EndPlayerTurn();
		}
	}

	public void RenderMapToBuffer(ScreenBuffer Buffer)
	{
		_ = FrameTimer.Elapsed.TotalMilliseconds;
		Game.ZoneManager.ActiveZone.Render(Buffer);
		_ = FrameTimer.Elapsed.TotalMilliseconds;
		TimeSpan elapsed = FrameTimer.Elapsed;
		CurrentFrame = (int)(elapsed.TotalMilliseconds % 1000.0) / 16;
		CurrentFrameLong = (int)(elapsed.TotalMilliseconds % 1000.0);
		CurrentFrame10 = (int)(elapsed.TotalMilliseconds % 10000.0) / 16;
		CurrentFrameLong10 = (int)(elapsed.TotalMilliseconds % 10000.0);
		if (Options.DisableTextAnimationEffects)
		{
			CurrentFrame = 8;
			CurrentFrameLong = 8;
			CurrentFrame10 = 8;
			CurrentFrameLong10 = 8;
		}
		if (Confusion.CurrentConfusionLevel > 0)
		{
			ConfusionShuffle(Buffer);
		}
	}

	public void RenderBaseToBuffer(ScreenBuffer Buffer)
	{
		Sidebar.UpdateState();
		Zone.IncrementLOSCacheValue();
		CludgeTargetRendered = false;
		RenderFloorTextures = !Options.DisableFloorTextures;
		if (GameManager.bDraw == 7)
		{
			return;
		}
		if (Game.ZoneManager.ActiveZone.IsWorldMap())
		{
			if (Game == null)
			{
				LogError("Game is NULL!");
			}
			if (Game.ZoneManager == null)
			{
				LogError("ZoneManager is NULL!");
			}
			if (Game.ZoneManager.ActiveZone == null)
			{
				LogError("ActiveZone is NULL!");
			}
			XRL.World.Parts.Physics pPhysics = The.Player.pPhysics;
			Game.ZoneManager.ActiveZone.HandleEvent(eBeforeRender);
			Game.ZoneManager.ActiveZone.HandleEvent(eBeforeRenderLate);
			if (pPhysics != null)
			{
				Game.ZoneManager.ActiveZone.ExploreAll();
				Game.ZoneManager.ActiveZone.LightAll();
				Game.ZoneManager.ActiveZone.VisAll();
			}
			Game.ZoneManager.ActiveZone.Render(Buffer);
			int i = 0;
			for (int count = AfterRenderCallbacks.Count; i < count; i++)
			{
				AfterRenderCallbacks[i](this, _Buffer);
			}
			if (!CludgeTargetRendered)
			{
				if (XRL.World.GameObject.validate(Sidebar.CurrentTarget) && !Sidebar.CurrentTarget.IsInGraveyard())
				{
					MessageQueue.AddPlayerMessage("You have lost sight of " + Sidebar.CurrentTarget.the + Sidebar.CurrentTarget.ShortDisplayName + ".");
				}
				Sidebar.CurrentTarget = null;
			}
			Sidebar.Render(Buffer);
			if (AllowWorldMapParticles)
			{
				ParticleManager.Render(Buffer);
			}
			else
			{
				ParticleManager.Particles.Clear();
			}
			return;
		}
		Game.ZoneManager.ActiveZone.ClearLightMap();
		Game.ZoneManager.ActiveZone.ClearVisiblityMap();
		Game.ZoneManager.ActiveZone.HandleEvent(eBeforeRender);
		Game.ZoneManager.ActiveZone.HandleEvent(eBeforeRenderLate);
		Cell currentCell = The.Player.CurrentCell;
		if (currentCell != null && Game.ZoneManager.ActiveZone != null)
		{
			Game.ZoneManager.ActiveZone.AddVisibility(currentCell.X, currentCell.Y, The.Player.GetVisibilityRadius());
		}
		if (Core.VisAllToggle)
		{
			Game.ZoneManager.ActiveZone.VisAll();
			Game.ZoneManager.ActiveZone.ExploreAll();
			Game.ZoneManager.ActiveZone.LightAll();
		}
		if (GameManager.bDraw == 11)
		{
			return;
		}
		Game.ZoneManager.ActiveZone.Render(Buffer);
		for (int j = 0; j < AfterRenderCallbacks.Count; j++)
		{
			AfterRenderCallbacks[j](this, _Buffer);
		}
		if ((GameManager.bDraw > 0 && GameManager.bDraw <= 20) || GameManager.bDraw == 21)
		{
			return;
		}
		if (!CludgeTargetRendered)
		{
			if (XRL.World.GameObject.validate(Sidebar.CurrentTarget) && !Sidebar.CurrentTarget.IsInGraveyard())
			{
				MessageQueue.AddPlayerMessage("You have lost sight of " + Sidebar.CurrentTarget.the + Sidebar.CurrentTarget.ShortDisplayName + ".");
			}
			Sidebar.CurrentTarget = null;
		}
		ParticleManager.Render(Buffer);
		if (Confusion.CurrentConfusionLevel > 0)
		{
			ConfusionShuffle(Buffer);
		}
		if (GameManager.bDraw != 22)
		{
			Sidebar.Render(Buffer);
		}
	}

	public void ConfusionShuffle(ScreenBuffer Buffer)
	{
		ConfusionRng = new System.Random((int)Core.Game.Turns);
		for (int i = 0; i < Buffer.Width; i++)
		{
			for (int j = 0; j < Buffer.Height; j++)
			{
				if (The.Player != null && The.Player.CurrentCell != null && (i != The.Player.CurrentCell.X || j != The.Player.CurrentCell.Y))
				{
					int num = i + ConfusionRng.Next(-1, 1);
					int num2 = j + ConfusionRng.Next(-1, 1);
					if ((num != The.Player.pPhysics.CurrentCell.X || num2 != The.Player.pPhysics.CurrentCell.Y) && num >= 0 && num < Buffer.Width - 1 && num2 >= 0 && num2 < Buffer.Height - 1)
					{
						ConsoleChar value = Buffer[i, j];
						Buffer[i, j] = Buffer[num, num2];
						Buffer[num, num2] = value;
						if (Game.Player.Body != null && Game.Player.Body.GetFuriousConfusion() > 0)
						{
							Buffer[i, j].Attributes = ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Red, TextColor.Black);
							Buffer[num, num2].Attributes = ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Red, TextColor.Black);
						}
					}
				}
				if (Game.Player.Body != null && Game.Player.Body.GetFuriousConfusion() > 0)
				{
					Buffer[i, j].Attributes = ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Red, TextColor.Black);
				}
			}
		}
	}

	public void RenderDelay(int Milliseconds, bool Interruptible = true)
	{
		long num = FrameTimer.ElapsedMilliseconds + Milliseconds;
		while (FrameTimer.ElapsedMilliseconds < num && (!Interruptible || !Keyboard.kbhit()))
		{
			RenderBase();
		}
		while (Keyboard.kbhit())
		{
			Keyboard.getch();
		}
	}

	public string GenerateRandomPlayerName(string Type = "")
	{
		return NameMaker.MakeName(null, null, Type);
	}

	public string GenerateRandomPlayerName(XRL.World.GameObject Player)
	{
		return NameMaker.MakeName(Player);
	}

	public void RenderBase(bool UpdateSidebar = true, bool DuringRestOkay = false)
	{
		if (Thread.CurrentThread == CoreThread)
		{
			GameManager.Instance.gameQueue.executeTasks();
		}
		if (GameManager.bDraw == 1)
		{
			return;
		}
		if (UpdateSidebar)
		{
			Sidebar.Update();
		}
		string setting = AutoAct.Setting;
		if (!DuringRestOkay && (setting == "r" || setting == "z" || (setting.Length > 0 && setting[0] == '.')))
		{
			return;
		}
		if (HostileWalkObjects.Count > 0)
		{
			HostileWalkObjects.Clear();
		}
		if (GameManager.bDraw == 2)
		{
			return;
		}
		ParticleManager.Frame();
		TimeSpan elapsed = FrameTimer.Elapsed;
		CurrentFrame = (int)(elapsed.TotalMilliseconds % 1000.0) / 16;
		CurrentFrameLong = (int)(elapsed.TotalMilliseconds % 1000.0);
		CurrentFrame10 = (int)(elapsed.TotalMilliseconds % 10000.0) / 16;
		CurrentFrameLong10 = (int)(elapsed.TotalMilliseconds % 10000.0);
		nFrame++;
		if (Options.DisableTextAnimationEffects)
		{
			CurrentFrame = 8;
			CurrentFrameLong = 8;
			CurrentFrame10 = 8;
			CurrentFrameLong10 = 8;
		}
		if (GameManager.bDraw == 3)
		{
			return;
		}
		RenderBaseToBuffer(_Buffer);
		if (GameManager.bDraw > 0 && GameManager.bDraw <= 15)
		{
			return;
		}
		if (ShowFPS)
		{
			_Buffer.Goto(0, 0);
			_Buffer.Write("Frame: " + nFrame + " GC:" + GC.CollectionCount(0) + " M:" + GC.GetTotalMemory(forceFullCollection: false));
			_Buffer.Goto(0, 1);
			_Buffer.Write("Objects Created: " + GameObjectFactory.Factory.ObjectsCreated);
			_Buffer.Goto(0, 2);
			_Buffer.Write("Rendered Objects: " + Core.RenderedObjects);
			_Buffer.Goto(0, 3);
			_Buffer.Write("FPS: " + nFrame / (FrameTimer.ElapsedMilliseconds / 1000));
		}
		if (GameManager.bDraw == 22)
		{
			return;
		}
		_Console.DrawBuffer(_Buffer, ImposterManager.getImposterUpdateFrame());
		if (GameManager.bDraw == 23 || !(setting != ""))
		{
			return;
		}
		if (OldHostileWalkObjects != null && HostileWalkObjects != null)
		{
			int i = 0;
			for (int count = HostileWalkObjects.Count; i < count; i++)
			{
				XRL.World.GameObject gameObject = HostileWalkObjects[i];
				if (!OldHostileWalkObjects.Contains(gameObject))
				{
					AutoAct.Interrupt(gameObject);
					OldHostileWalkObjects.Clear();
					OldHostileWalkObjects.AddRange(HostileWalkObjects);
					HostileWalkObjects.Clear();
					break;
				}
			}
		}
		if (setting != "")
		{
			OldHostileWalkObjects.Clear();
			OldHostileWalkObjects.AddRange(HostileWalkObjects);
			HostileWalkObjects.Clear();
		}
	}

	public void ResetGameBasedStaticCaches()
	{
		Type typeFromHandle = typeof(HasGameBasedStaticCacheAttribute);
		Type typeFromHandle2 = typeof(GameBasedStaticCacheAttribute);
		foreach (FieldInfo item in ModManager.GetFieldsWithAttribute(typeFromHandle2, typeFromHandle))
		{
			try
			{
				if (item.IsStatic)
				{
					object value = ((((GameBasedStaticCacheAttribute)item.GetCustomAttributes(typeFromHandle2, inherit: false)[0]).CreateInstance || item.FieldType.IsValueType) ? Activator.CreateInstance(item.FieldType) : null);
					item.SetValue(null, value);
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("resetting field " + item?.Name, x);
			}
		}
		foreach (Type item2 in ModManager.GetTypesWithAttribute(typeFromHandle))
		{
			MethodInfo method = item2.GetMethod("Reset", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			try
			{
				if (method != null)
				{
					method.Invoke(null, new object[0]);
				}
				foreach (MethodInfo item3 in from mi in item2.GetMethods()
					where mi.GetCustomAttributes(typeof(GameBasedCacheInitAttribute), inherit: false).Count() > 0
					select mi)
				{
					item3.Invoke(null, new object[0]);
				}
			}
			catch (Exception x2)
			{
				MetricsManager.LogException("calling Reset on type " + item2.Name, x2);
			}
		}
	}

	public XRLGame NewGame()
	{
		GameManager.Instance.gameQueue.clear();
		if (Game != null)
		{
			Game.Release();
		}
		List<ModInfo> mods = ModManager.Mods;
		if (Options.ShowModSelectionNewGame && mods.Count > 0)
		{
			_isNewGameModFlow = true;
			bool done = false;
			ModManagerUI mm = UIManager.getWindow("ModManager") as ModManagerUI;
			GameManager.Instance.uiQueue.awaitTask(delegate
			{
				UIManager.pushWindow("ModManager");
				mm.SetBackButtonText("Continue");
				mm.nextHideCallback.AddListener(delegate
				{
					done = true;
				});
			});
			while (!done)
			{
				GameManager.Instance.gameQueue.executeTasks();
				Thread.Sleep(50);
			}
			_isNewGameModFlow = false;
		}
		LoadEverything();
		ResetGameBasedStaticCaches();
		MemoryHelper.GCCollectMax();
		Game = new XRLGame(_Console, _Buffer);
		Game.CreateNewGame();
		Reset();
		EmbarkInfo result = EmbarkBuilder.Begin().Result;
		if (result == null)
		{
			return null;
		}
		result.bootGame(Game);
		GameManager.Instance.PopGameView();
		return Game;
	}

	public void UpdateGlobalChoices()
	{
		if (Game != null && !Game.StringGameState.ContainsKey("SeekerEnemyFaction"))
		{
			for (int i = 0; i < 1000; i++)
			{
				Faction randomFaction = Factions.GetRandomFaction("Seekers");
				if (randomFaction != null)
				{
					Game.SetStringGameState("SeekerEnemyFaction", randomFaction.DisplayName);
					Factions.get("Seekers").setFactionFeeling(randomFaction.Name, -100);
					break;
				}
			}
		}
		Factions.RequireCachedHeirlooms();
		BookUI.InitBooks();
	}

	public void CreateMarkOfDeath()
	{
		char[] list = new char[11]
		{
			'!', '@', '#', '$', '%', '*', '(', ')', '>', '<',
			'/'
		};
		string text = "";
		for (int i = 0; i < 3; i++)
		{
			text += list.GetRandomElement();
		}
		for (int num = 2; num >= 0; num--)
		{
			text += text[num];
		}
		Game.SetStringGameState("MarkOfDeath", text);
		JournalAPI.AddObservation("The lost Mark of Death from the late sultanate was " + text + ".", "MarkOfDeathSecret", "Gossip", "MarkOfDeathSecret", new string[2] { "gossip", "old" }, revealed: false, -1L, "{{W|You recover the Mark of Death of the late sultanate.}}\n\n");
	}

	public void CreateCures()
	{
		List<string> list = new List<string>();
		list.Add("asphalt");
		list.Add("oil");
		list.Add("honey");
		list.Add("blood");
		list.Add("wine");
		list.Add("salt");
		list.Add("cider");
		list.Add("sap");
		List<string> list2 = new List<string>();
		list2.Add("wine");
		list2.Add("honey");
		list2.Add("water");
		list2.Add("cider");
		list2.Add("sap");
		int num = 0;
		for (int i = 1; i <= 2; i++)
		{
			num = Stat.Random(0, list.Count - 1);
			Game.SetStringGameState("GlotrotCure" + i, list[num]);
			if (list2.CleanContains(list[num]))
			{
				list2.Remove(list[num]);
			}
			list.RemoveAt(num);
		}
		num = Stat.Random(0, list2.Count - 1);
		Game.SetStringGameState("GlotrotCure3", list2[num]);
		List<string> list3 = new List<string>();
		list3.Add("blood");
		list3.Add("honey");
		list3.Add("wine");
		list3.Add("oil");
		list3.Add("asphalt");
		list3.Add("sap");
		Game.SetStringGameState("IronshankCure", list3.GetRandomElement());
		List<string> list4 = new List<string>();
		list4.Add("salt");
		list4.Add("cider");
		list4.Add("ink");
		Game.SetStringGameState("MonochromeCure", list4.GetRandomElement());
		GenerateFungalCure();
	}

	public void GenerateFungalCure()
	{
		string randomElement = new List<string>
		{
			"cider", "honey", "wine", "oil", "asphalt", "blood", "slime", "acid", "putrid", "convalessence",
			"proteangunk", "sap"
		}.GetRandomElement();
		Game.SetStringGameState("FungalCureLiquid", randomElement);
		Game.SetStringGameState("FungalCureLiquidDisplay", ConsoleLib.Console.ColorUtility.StripFormatting(LiquidVolume.getLiquid(randomElement).GetName(null)));
		List<string> list = new List<string>();
		for (int i = 0; i < GameObjectFactory.Factory.BlueprintList.Count; i++)
		{
			GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.BlueprintList[i];
			if (!gameObjectBlueprint.DescendsFrom("BaseWorm") || gameObjectBlueprint.Tags.ContainsKey("NoCure") || !gameObjectBlueprint.HasPart("Corpse"))
			{
				continue;
			}
			string partParameter = gameObjectBlueprint.GetPartParameter("Corpse", "CorpseChance");
			if (string.IsNullOrEmpty(partParameter) || Convert.ToInt32(partParameter) <= 0)
			{
				continue;
			}
			string partParameter2 = gameObjectBlueprint.GetPartParameter("Corpse", "CorpseBlueprint");
			if (GameObjectFactory.Factory.GetBlueprint(partParameter2).DescendsFrom("Corpse"))
			{
				list.Add(partParameter2);
				if (ConsoleLib.Console.ColorUtility.StripFormatting(XRL.World.GameObject.createSample(partParameter2).DisplayName).Contains("object"))
				{
					MetricsManager.LogError("Invalid fungal worm selected: " + partParameter2);
				}
			}
		}
		string randomElement2 = list.GetRandomElement();
		Game.SetStringGameState("FungalCureWorm", randomElement2);
		Game.SetStringGameState("FungalCureWormDisplay", ConsoleLib.Console.ColorUtility.StripFormatting(GameObjectFactory.Factory.CreateObject(randomElement2).DisplayName));
	}

	public void RunGame()
	{
		Sidebar.WaitingForHPWarning = true;
		The.ParticleManager.reset();
		RenderBase();
		UpdateGlobalChoices();
		while (Game.Running)
		{
			try
			{
				if (GameManager.runWholeTurnOnUIThread && Thread.CurrentThread == CoreThread)
				{
					waitForSegmentOnGameThread = true;
					GameManager.Instance.uiQueue.queueTask(delegate
					{
						Game.ActionManager.RunSegment();
						waitForSegmentOnGameThread = false;
					}, 1);
					while (waitForSegmentOnGameThread)
					{
					}
				}
				else
				{
					Game.ActionManager.RunSegment();
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("TurnError", x);
			}
		}
		The.ParticleManager.reset();
		ImposterManager.qudClearImposters();
		FungalVisionary.VisionLevel = 0;
		GameManager.Instance.GreyscaleLevel = 0;
		Sidebar.MaxHP = 0;
		if (!(Game.DeathReason != "<nodeath>") || Game.forceNoDeath)
		{
			return;
		}
		BuildScore();
		if (!Options.DisablePermadeath)
		{
			string cacheDirectory = Game.GetCacheDirectory();
			if (cacheDirectory != null)
			{
				Directory.Delete(cacheDirectory, recursive: true);
			}
		}
	}

	public void BuildScore(bool bReal = true, string fakeDeathReason = null)
	{
		int num = 0;
		XRLGame game = The.Game;
		XRL.World.GameObject gameObject = The.Player;
		num += (int)((double)gameObject.Stat("XP") * 0.334);
		XRL.World.GameObject gameObject2 = null;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		foreach (XRL.World.GameObject content in gameObject.GetContents())
		{
			if (content.HasTag("ExcludeFromGameScore"))
			{
				continue;
			}
			num += content.Count;
			if (!(content.GetPart("Examiner") is Examiner examiner))
			{
				continue;
			}
			int techTier = content.GetTechTier();
			int tier = content.GetTier();
			int num7 = techTier + tier + examiner.Complexity + examiner.Difficulty;
			bool flag = false;
			if (num7 > num6)
			{
				flag = true;
			}
			else if (num7 == num6)
			{
				if (techTier > num5)
				{
					flag = true;
				}
				else if (techTier == num5)
				{
					if (tier > num4)
					{
						flag = true;
					}
					else if (tier == num4)
					{
						if (examiner.Complexity > num2)
						{
							flag = true;
						}
						else if (examiner.Complexity == num2 && examiner.Difficulty > num3)
						{
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				gameObject2 = content;
				num5 = techTier;
				num4 = tier;
				num2 = examiner.Complexity;
				num3 = examiner.Difficulty;
				num6 = num7;
			}
			num += Math.Max(techTier, examiner.Complexity) * 250 * content.Count;
		}
		num += game.FinishedQuests.Keys.Count * 733;
		num += game.Quests.Keys.Count * 133;
		num += (int)((double)game.Turns * 0.01);
		num += game.ZoneManager.VisitedTime.Keys.Count * 35;
		num += 1211 * game.GetIntGameState("ArtifactsGenerated");
		int intGameState = game.GetIntGameState("LairsFound");
		num += intGameState * 75;
		num -= 387;
		num -= 35;
		char value = 'ê';
		if (game.GetStringGameState("VictoryCondition") == "Brightsheol")
		{
			num += 100000;
			value = 'í';
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{{C|").Append(value).Append("}} Game summary for {{W|")
			.Append(game.PlayerName)
			.Append("}} {{C|")
			.Append(value)
			.Append("}}\n\n");
		stringBuilder.Append("This game ended on ").Append(DateTime.Now.ToLocalTime().ToLongDateString()).Append(" at ")
			.Append(DateTime.Now.ToLocalTime().ToLongTimeString())
			.Append(".\n");
		if (!string.IsNullOrEmpty(fakeDeathReason))
		{
			stringBuilder.Append(fakeDeathReason).Append('\n');
		}
		else
		{
			stringBuilder.Append(game.DeathReason.Replace("!", ".")).Append('\n');
		}
		stringBuilder.Append("You were level {{C|" + gameObject.Stat("Level") + "}}.\n");
		stringBuilder.Append("You scored {{C|").Append(num).Append("}} ")
			.Append((num == 1) ? "point" : "points")
			.Append(".\n");
		MetricsManager.LogEvent("Death:Score", num);
		stringBuilder.Append("You survived for {{C|").Append(game.Turns).Append("}} ")
			.Append((game.Turns == 1) ? "turn" : "turns")
			.Append(".\n");
		MetricsManager.LogEvent("Death:Turns", game.Turns);
		MetricsManager.LogEvent("Death:Zones", game.ZoneManager.VisitedTime.Keys.Count);
		if (intGameState > 0)
		{
			stringBuilder.Append("You found {{C|").Append(intGameState).Append("}} ")
				.Append((intGameState == 1) ? "lair" : "lairs")
				.Append(".\n");
			MetricsManager.LogEvent("Death:Lairs", intGameState);
		}
		int intGameState2 = game.GetIntGameState("PlayerItemNamingDone");
		if (intGameState2 > 0)
		{
			stringBuilder.Append("You named {{C|").Append(intGameState2).Append("}} ")
				.Append((intGameState2 == 1) ? "item" : "items")
				.Append(".\n");
			MetricsManager.LogEvent("Death:ItemsNamed", intGameState2);
		}
		if (game.HasIntGameState("ArtifactsGenerated"))
		{
			int intGameState3 = game.GetIntGameState("ArtifactsGenerated");
			stringBuilder.Append("You generated {{C|").Append(intGameState3).Append("}} storied ")
				.Append((intGameState3 == 1) ? "item" : "items")
				.Append(".\n");
			MetricsManager.LogEvent("Death:Artifacts", intGameState3);
		}
		game.HasIntGameState("MetempsychosisCount");
		if (gameObject2 != null)
		{
			string text = gameObject2.an(int.MaxValue, null, null, AsIfKnown: true, Single: true, NoConfusion: true);
			stringBuilder.Append("The most advanced artifact in your possession was " + text + ".\n");
			MetricsManager.LogEvent("Death:Artifact:" + text + " [" + gameObject2.Blueprint + "]");
		}
		stringBuilder.Append("This game was played in ").Append(game.gameMode).Append(" mode.\n");
		stringBuilder.Append("\n\n");
		stringBuilder.Append("{{C|").Append(value).Append("}} Chronology for {{W|")
			.Append(game.PlayerName)
			.Append("}} {{C|")
			.Append(value)
			.Append("}}\n\n");
		foreach (JournalAccomplishment accomplishment in JournalAPI.Accomplishments)
		{
			stringBuilder.Append("{{C|").Append('ú').Append("}} ")
				.Append(accomplishment.GetDisplayText())
				.Append("\n");
		}
		stringBuilder.Append("\n\n");
		stringBuilder.Append("{{C|").Append(value).Append("}} Final messages for {{W|")
			.Append(game.PlayerName)
			.Append("}} {{C|")
			.Append(value)
			.Append("}}\n\n");
		int num8 = 0;
		foreach (string lines in The.Game.Player.Messages.GetLinesList(0, 30))
		{
			if (num8 != 0)
			{
				stringBuilder.Append("\n");
			}
			num8++;
			stringBuilder.Append(lines);
		}
		string text2 = stringBuilder.ToString();
		MetricsManager.LogEvent("Death:Score:" + num);
		string leaderboardID = null;
		if (bReal)
		{
			try
			{
				if (game.HasStringGameState("leaderboardMode") && !string.IsNullOrEmpty(game.GetStringGameState("leaderboardMode")))
				{
					string name = "leaderboardresult_" + game.GetStringGameState("leaderboardMode");
					if (!Prefs.HasString(name))
					{
						leaderboardID = LeaderboardManager.SubmitResult(game.GetStringGameState("leaderboardMode"), num, delegate(LeaderboardScoresDownloaded_t result)
						{
							StringBuilder stringBuilder2 = new StringBuilder();
							int num9 = Math.Min(result.m_cEntryCount, 10);
							for (int i = 0; i < num9; i++)
							{
								LeaderboardEntry_t pLeaderboardEntry = default(LeaderboardEntry_t);
								SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out pLeaderboardEntry, null, 0);
								SteamFriends.RequestUserInformation(pLeaderboardEntry.m_steamIDUser, bRequireNameOnly: true);
								stringBuilder2.Append(pLeaderboardEntry.m_nGlobalRank + ": {{Y|" + SteamFriends.GetFriendPersonaName(pLeaderboardEntry.m_steamIDUser) + "}} ({{W|" + pLeaderboardEntry.m_nScore + "}})\n");
							}
							if (!LeaderboardManager.leaderboardresults.ContainsKey(leaderboardID))
							{
								LeaderboardManager.leaderboardresults.Add(leaderboardID, "");
							}
							LeaderboardManager.leaderboardresults[leaderboardID] = stringBuilder2.ToString();
							UnityEngine.Debug.Log(stringBuilder2.ToString());
							Keyboard.PushMouseEvent("LeaderboardResultsUpdated");
						});
						Prefs.SetString(name, num.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				leaderboardID = null;
				LogError(ex);
			}
			Scores.Scoreboard.Add(num, text2, game.Turns, game.GameID, game.GetStringGameState("GameMode"), CheckpointingSystem.IsCheckpointingEnabled(), The.Player.Level, The.Player.DisplayNameOnly);
			if (leaderboardID != null)
			{
				text2 = "<%leaderboard%>\n\n" + text2;
			}
		}
		GameSummaryUI.Show(num, text2, game.PlayerName, leaderboardID, bReal);
	}

	public void SaveGame(string GameName)
	{
		Game.SaveGame(GameName);
	}

	[Obsolete("ShowThinker does nothing and will be removed in a future version")]
	public static void ShowThinker()
	{
	}

	public static void SetClipboard(string Msg)
	{
		UnityEngine.Debug.LogError(Msg);
	}

	public static void LogError(string Error)
	{
		UnityEngine.Debug.LogError("ERROR:" + Error);
		if (!Keyboard.Closed)
		{
			MetricsManager.LogException("Unknown", new Exception(Error));
		}
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void Log(string Info)
	{
		UnityEngine.Debug.Log(Info);
	}

	public static void LogError(Exception ex)
	{
		UnityEngine.Debug.LogError("ERROR:" + ex.ToString());
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void LogError(string Category, Exception ex)
	{
		UnityEngine.Debug.LogError("ERROR:" + Category + ":" + ex.ToString());
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void LogError(string Category, string error)
	{
		UnityEngine.Debug.LogError("ERROR:" + Category + ":" + error);
		if (Keyboard.Closed)
		{
			Thread.CurrentThread.Abort();
			throw new Exception("Stopping game thread with an exception!");
		}
	}

	public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
	{
		LogError("FATAL thread exception:" + e.ToString());
	}

	public bool RestoreModsLoaded(List<string> Enabled)
	{
		return RestoreModsLoadedAsync(Enabled).Result;
	}

	public async Task<bool> RestoreModsLoadedAsync(List<string> Enabled)
	{
		List<string> list = new List<string>(Enabled.Except(ModManager.GetAvailableMods()));
		List<string> extraLoaded = new List<string>(ModManager.GetRunningMods().Except(Enabled));
		List<string> notLoaded = new List<string>(Enabled.Except(ModManager.GetRunningMods()).Except(list));
		if (list.Count > 0 && (await Popup.NewPopupMessageAsync(XRL.World.Event.NewStringBuilder("One or more mods enabled in this save are {{red|not available}}:{{red|").Compound(list.Select(ModManager.GetModTitle), "\n{{y|:}} ").Compound("}}Do you still wish to try to load this save?", "\n\n")
			.ToString(), PopupMessage.YesNoButton, null, "Incomplete Mod Configuration")).command != PopupMessage.YesNoButton[0].command)
		{
			return false;
		}
		if (extraLoaded.Count > 0 || notLoaded.Count > 0)
		{
			StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
			if (extraLoaded.Count > 0)
			{
				stringBuilder.Compound("These mods are {{red|disabled}} in the save:{{red|", '\n').Compound(extraLoaded.Select(ModManager.GetModTitle), "\n{{y|:}} ").Append("}}")
					.AppendLine();
			}
			if (notLoaded.Count > 0)
			{
				stringBuilder.Compound("These mods are {{green|enabled}} in the save:{{green|", '\n').Compound(notLoaded.Select(ModManager.GetModTitle), "\n{{y|:}} ").Append("}}")
					.AppendLine();
			}
			stringBuilder.AppendLine();
			string[] options = new string[2] { "Load using save game's mod configuration", "Load keeping current mod configuration" };
			switch (await Popup.ShowOptionListAsync("Mod Configuration Differs", options, null, 0, stringBuilder.ToString(), 60, RespectOptionNewlines: false, AllowEscape: true))
			{
			case -1:
				return false;
			case 0:
				foreach (string mod2 in extraLoaded)
				{
					ModManager.Mods.Find((ModInfo m) => m.ID == mod2).IsEnabled = false;
				}
				foreach (string mod in notLoaded)
				{
					ModManager.Mods.Find((ModInfo m) => m.ID == mod).IsEnabled = true;
				}
				ModManager.Init(Reload: true);
				ModManager.BuildScriptMods();
				HotloadConfiguration();
				break;
			}
		}
		return true;
	}

	public void WriteConsoleLine(string s)
	{
		try
		{
			UnityEngine.Debug.Log(s);
		}
		catch
		{
		}
	}

	public static void Stop()
	{
		UnityEngine.Debug.Log("Stopping...");
		GameManager.bDraw = 0;
		Keyboard.Closed = true;
		CoreThread.Interrupt();
		CoreThread.Abort();
	}

	public static Thread Start()
	{
		DataPath = Path.Combine(Application.streamingAssetsPath, "Base");
		SavePath = Application.persistentDataPath;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs != null && commandLineArgs.Length != 0)
		{
			for (int i = 0; i < commandLineArgs.Length - 1; i++)
			{
				if (commandLineArgs[i].ToUpper() == "-SAVEPATH")
				{
					SavePath = commandLineArgs[i + 1];
					SavePath = Path.GetFullPath(SavePath);
					int length = SavePath.Length;
					if (length == 0 || !SavePath[length - 1].IsDirectorySeparator())
					{
						SavePath += Path.DirectorySeparatorChar;
					}
					if (!Directory.Exists(SavePath))
					{
						Directory.CreateDirectory(SavePath);
					}
					break;
				}
			}
		}
		Log("Data path: " + DataPath);
		Log("Save path: " + SavePath);
		if (!Directory.Exists(XRLGame.GetSaveDirectory()))
		{
			Directory.CreateDirectory(XRLGame.GetSaveDirectory());
		}
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		UnityEngine.Debug.Log("Starting core...");
		CoreThread = new Thread(_ThreadStart);
		CoreThread.CurrentCulture = CultureInfo.InvariantCulture;
		CoreThread.Priority = System.Threading.ThreadPriority.Highest;
		CoreThread.Start();
		bStarted = true;
		UnityEngine.Debug.Log("Started!");
		return CoreThread;
	}

	public static void _ThreadStart()
	{
		try
		{
			Core = new XRLCore();
			Core._Start();
			GameManager.Instance.gameThreadSynchronizationContext = SynchronizationContext.Current;
		}
		catch (ThreadInterruptedException)
		{
			UnityEngine.Debug.Log("Core thread shut down...");
		}
		catch (Exception message)
		{
			UnityEngine.Debug.LogError(message);
		}
	}

	public void ReloadUIViews()
	{
		WriteConsoleLine("Init Console UIs...\n");
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(UIView)))
		{
			if (typeof(IWantsTextConsoleInit).IsAssignableFrom(item) && Activator.CreateInstance(item) is IWantsTextConsoleInit wantsTextConsoleInit)
			{
				wantsTextConsoleInit.Init(_Console, _Buffer);
			}
		}
	}

	public void _Start()
	{
		Core = this;
		if (Options.LowContrast)
		{
			ScreenBuffer.bLowContrast = true;
		}
		if (TextConsole.bExtended)
		{
			_Buffer = ScreenBuffer.create(80, 33);
			if (TextConsole.Mode == TerminalMode.OpenGL)
			{
			}
		}
		else
		{
			if (Options.LowContrast)
			{
				ScreenBuffer.bLowContrast = true;
			}
			_Buffer = ScreenBuffer.create(80, 25);
			_ = TextConsole.Mode;
			_ = 1;
		}
		_Console = new TextConsole();
		_Console.ShowCursor(bShow: false);
		WriteConsoleLine("Cleaning up...\n");
		WriteConsoleLine("Starting up XRL...\n");
		WriteConsoleLine("Initialize Genders and Pronoun Sets...\n");
		Gender.Init();
		PronounSet.Init();
		WriteConsoleLine("Initialize Name Styles...\n");
		NameStyles.CheckInit();
		WriteConsoleLine("Loading World Blueprints...\n");
		WorldFactory.Factory.Init();
		WriteConsoleLine("Loading Help...\n");
		Manual = new XRLManual(_Console);
		WriteConsoleLine("Init Pathfinder...\n");
		FindPath.Initalize();
		ReloadUIViews();
		ParticleManager = new ParticleManager();
		ParticleManager.Init(_Console, _Buffer);
		if (TextConsole.Mode == TerminalMode.OpenGL)
		{
			_Console.Hide();
			_Console.FocusUI();
		}
		if (Options.LowContrast)
		{
			ScreenBuffer.bLowContrast = true;
		}
		int num = 0;
		string s = "{{K|" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "}}";
		LoadEverything();
		WriteConsoleLine("Starting Game...");
		SoundManager.PlayMusic("PilgrimsPath");
		bool flag = SavesAPI.GetSavedGameInfo().Count > 0;
		while (true)
		{
			if (GameManager.Instance.CurrentGameView != "MainMenu")
			{
				GameManager.Instance.PushGameView("MainMenu");
			}
			GameManager.Instance.ClearRegions();
			XRL.World.Event.ResetPool();
			_Buffer.Clear();
			_Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			_Buffer.Goto(35, 9);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|N}}ew game" : "New game");
			if (num == 0)
			{
				_Buffer.WriteAt(33, 9, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|New game}}");
			}
			GameManager.Instance.AddRegion(35, 9, 43, 9, "NewGame", null, "Select:0");
			_Buffer.Goto(35, 10);
			if (flag)
			{
				_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|C}}ontinue" : "Continue");
			}
			else
			{
				_Buffer.Write("{{K|Continue}}");
			}
			GameManager.Instance.AddRegion(35, 10, 43, 10, "Continue", null, "Select:1");
			if (num == 1)
			{
				_Buffer.WriteAt(33, 10, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Continue}}");
			}
			_Buffer.Goto(35, 12);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|O}}ptions" : "Options");
			if (num == 2)
			{
				_Buffer.WriteAt(33, 12, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Options}}");
			}
			GameManager.Instance.AddRegion(35, 12, 43, 12, "Options", null, "Select:2");
			_Buffer.Goto(35, 13);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|H}}igh Scores" : "High Scores");
			if (num == 3)
			{
				_Buffer.WriteAt(33, 13, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|High Scores}}");
			}
			GameManager.Instance.AddRegion(35, 13, 43, 13, "HighScores", null, "Select:3");
			_Buffer.Goto(35, 14);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "[{{W|?}}] Help" : "Help");
			if (num == 4)
			{
				_Buffer.WriteAt(33, 14, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Help}}");
			}
			GameManager.Instance.AddRegion(35, 14, 43, 14, "Help", null, "Select:4");
			_Buffer.Goto(35, 15);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|Q}}uit" : "Quit");
			if (num == 5)
			{
				_Buffer.WriteAt(33, 15, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Quit}}");
			}
			GameManager.Instance.AddRegion(35, 15, 43, 15, "Quit", null, "Select:5");
			_Buffer.Goto(35, 18);
			_Buffer.Write(CapabilityManager.AllowKeyboardHotkeys ? "{{W|R}}edeem Code" : "Redeem Code");
			if (num == 6)
			{
				_Buffer.WriteAt(33, 18, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Redeem Code}}");
			}
			GameManager.Instance.AddRegion(35, 18, 43, 18, "RedeemCode", null, "Select:6");
			_Buffer.Goto(35, 17);
			_Buffer.Write("{{g|Dromad Edition}}");
			if (ModManager.Mods != null && ModManager.Mods.Count > 0)
			{
				_Buffer.WriteAt(35, 21, CapabilityManager.AllowKeyboardHotkeys ? "{{W|M}}ods" : "Mods");
				if (num == 7)
				{
					_Buffer.WriteAt(33, 21, CapabilityManager.AllowKeyboardHotkeys ? "{{Y|>}}" : "{{Y|>}} {{W|Mods}}");
				}
				GameManager.Instance.AddRegion(35, 21, 43, 21, "ModManager", null, "Select:7");
			}
			if (ModManager.AreAnyModsFailed())
			{
				_Buffer.WriteAt(40, 21, "{{y|-}} {{R|You have mods with errors.}}");
			}
			else if (ModManager.AreAnyModsUnapproved())
			{
				_Buffer.WriteAt(40, 21, "{{y|-}} {{R|You have unapproved scripting mods.}}");
			}
			_Buffer.WriteAt(32, 0, "  {{C|Caves of Qud}}  ");
			_Buffer.WriteAt(55, 0, s);
			_Buffer.WriteAt(27, 24, " {{Y|Copyright ({{w|c}}) Freehold Games({{w|tm}})}} ");
			_Console.DrawBuffer(_Buffer);
			Keys keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			int num2 = 6;
			if (ModManager.Mods != null && ModManager.Mods.Count > 0)
			{
				num2 = 7;
			}
			if (keys == Keys.Enter)
			{
				keys = Keys.Space;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.NumPad2 && num < num2)
			{
				num++;
			}
			if (keys == Keys.A)
			{
				NameEditorUI.Show();
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Select:"))
			{
				num = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
			}
			if (keys == Keys.M || (keys == Keys.Space && num == 7) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "ModManager"))
			{
				UIManager.pushWindow("ModManager");
				SingletonWindowBase<ModManagerUI>.instance.nextHideCallback.AddListener(delegate
				{
					Keyboard.PushMouseEvent("Refresh");
				});
			}
			if (keys == Keys.R || (keys == Keys.Space && num == 6) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RedeemCode"))
			{
				string text = Popup.AskString("Redeem a Code", "", 32);
				if (!string.IsNullOrEmpty(text))
				{
					CodeRedemptionManager.redeem(text);
				}
			}
			if (keys == Keys.Q || (keys == Keys.Space && num == 5) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Quit"))
			{
				break;
			}
			XRLGame xRLGame = null;
			if (keys == Keys.N || (keys == Keys.Space && num == 0) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "NewGame"))
			{
				xRLGame = NewGame();
			}
			if (keys == Keys.C || (keys == Keys.Space && num == 1) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Continue"))
			{
				xRLGame = ((!Options.ModernUI) ? SaveManagement() : SingletonWindowBase<Qud.UI.SaveManagement>.instance.ContinueMenu().Result);
				flag = SavesAPI.GetSavedGameInfo().Count > 0;
			}
			if (xRLGame != null)
			{
				RunGame();
				flag = SavesAPI.GetSavedGameInfo().Count > 0;
				continue;
			}
			if (keys == Keys.O || (keys == Keys.Space && num == 2) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Options"))
			{
				OptionsUI.Show();
			}
			if (keys == Keys.H || (keys == Keys.Space && num == 3) || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "HighScores"))
			{
				Scores.Show();
			}
			if (keys != Keys.OemQuestion && (keys & Keys.OemQuestion) != Keys.OemQuestion && (keys != Keys.Space || num != 4))
			{
				switch (keys)
				{
				default:
					continue;
				case Keys.MouseEvent:
					if (!(Keyboard.CurrentMouseEvent.Event == "Help"))
					{
						continue;
					}
					break;
				case Keys.F1:
					break;
				}
			}
			Manual.ShowHelp("");
		}
		Popup._TextConsole.Close();
	}

	public XRLGame SaveManagement()
	{
		GameManager.Instance.PushGameView("LegacySaveManagement");
		bool flag = true;
		List<SaveGameInfo> Info;
		while (true)
		{
			int num = 0;
			int num2 = 0;
			if (Directory.GetDirectories(XRLGame.GetSaveDirectory()).Length == 0)
			{
				break;
			}
			flag = false;
			Info = null;
			Loading.LoadTask("Indexing saved games...", delegate
			{
				Info = SavesAPI.GetSavedGameInfo();
			});
			Keys keys;
			do
			{
				XRL.World.Event.ResetPool();
				_Buffer.Clear();
				_Buffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				for (int i = num2; i < num2 + 4 && i < Info.Count; i++)
				{
					int num3 = (i - num2) * 5 + 3;
					if (num == i)
					{
						_Buffer.Goto(3, num3);
						_Buffer.Write("{{Y|>}} {{W|" + Info[i].Name + ", " + Info[i].Description + "}}");
					}
					else
					{
						_Buffer.Goto(5, num3);
						_Buffer.Write("{{w|" + Info[i].Name + ", " + Info[i].Description + "}}");
					}
					_Buffer.Goto(5, num3 + 1);
					_Buffer.Write(Info[i].Info);
					_Buffer.Goto(5, num3 + 2);
					if (!string.IsNullOrEmpty(Info[i].SaveTime))
					{
						_Buffer.Write(Info[i].SaveTime);
					}
					else
					{
						_Buffer.Write("{{K|{" + Info[i].ID + "} }}");
					}
					_Buffer.Goto(5, num3 + 3);
					_Buffer.Write("{{K|" + Info[i].Size + " {" + Info[i].ID + "} }}");
				}
				_Buffer.Goto(5, 24);
				_Buffer.Write(" {{W|Space}} - Load Game {{W|Delete}} or {{W|D}} - Delete Game ");
				_Console.DrawBuffer(_Buffer, null, bSkipIfOverlay: true);
				keys = Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
				switch (keys)
				{
				case Keys.Escape:
					GameManager.Instance.PopGameView();
					return null;
				case Keys.NumPad8:
					if (num > 0)
					{
						num--;
						if (num < num2)
						{
							num2--;
						}
					}
					break;
				}
				if (keys == Keys.NumPad2 && num < Info.Count - 1)
				{
					num++;
				}
				if (num - num2 >= 4)
				{
					num2++;
				}
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Play:"))
				{
					Info = SavesAPI.GetSavedGameInfo();
					num = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
					keys = Keys.Space;
				}
				if (Info.Count == 0)
				{
					GameManager.Instance.PopGameView();
					return null;
				}
				if (num >= Info.Count)
				{
					num = Info.Count - 1;
				}
				if (keys != Keys.Space && keys != Keys.Enter)
				{
					continue;
				}
				try
				{
					if (Info[num].TryRestoreModsAndLoadAsync().Result)
					{
						GameManager.Instance.PopGameView();
						return The.Game;
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError(ex.ToString());
				}
			}
			while ((keys != Keys.Delete && keys != Keys.D) || Popup.ShowYesNoCancel("Are you sure you want to delete this saved game?") != 0);
			Info[num].Delete();
		}
		if (flag)
		{
			Popup.ShowFail("You have no existing saved games.");
		}
		GameManager.Instance.PopGameView();
		return null;
	}

	public static bool WantEvent(int ID, int cascade)
	{
		if (Core == null)
		{
			return false;
		}
		if (Core.Game == null)
		{
			return false;
		}
		return Core.Game.WantEvent(ID, cascade);
	}

	public static bool HandleEvent<T>(T E) where T : MinEvent
	{
		if (Core == null)
		{
			return false;
		}
		if (Core.Game == null)
		{
			return false;
		}
		return Core.Game.HandleEvent(E);
	}

	public static bool HandleEvent<T>(T E, IEvent ParentEvent) where T : MinEvent
	{
		bool result = HandleEvent(E);
		ParentEvent.ProcessChildEvent(E);
		return result;
	}
}
