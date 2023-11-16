#define NLOG_ALL
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using HarmonyLib;
using Kobold;
using ModelShark;
using Qud.API;
using Qud.UI;
using QupKit;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

public class GameManager : BaseGameController<GameManager>
{
	public enum PreferredSidebarPosition
	{
		None,
		Left,
		Right
	}

	public class ViewInfo
	{
		public string NavCategory;

		public string UICanvas;

		public int UICanvasHost;

		public bool WantsTileOver;

		public bool ForceFullscreen;

		public bool ForceFullscreenInLegacy;

		public bool IgnoreForceFullscreen;

		public bool ExecuteActions;

		public int OverlayMode;

		public bool TakesScroll;

		public ViewInfo(bool WantsTileOver, bool ForceFullscreen, string NavCategory = null, string UICanvas = null, int OverlayMode = 0, bool ExecuteActions = false, bool TakesScroll = false, int UICanvasHost = 0, bool IgnoreForceFullscreen = false, bool ForceFullscreenInLegacy = false)
		{
			this.WantsTileOver = WantsTileOver;
			this.ForceFullscreen = ForceFullscreen;
			this.ForceFullscreenInLegacy = ForceFullscreenInLegacy;
			this.IgnoreForceFullscreen = IgnoreForceFullscreen;
			this.UICanvas = UICanvas;
			this.UICanvasHost = UICanvasHost;
			this.OverlayMode = OverlayMode;
			this.ExecuteActions = ExecuteActions;
			this.NavCategory = NavCategory;
			this.TakesScroll = TakesScroll;
		}

		public bool IsSame(ViewInfo other)
		{
			if (WantsTileOver == other.WantsTileOver && ForceFullscreen == other.ForceFullscreen && ForceFullscreenInLegacy == other.ForceFullscreenInLegacy && UICanvas == other.UICanvas && UICanvasHost == other.UICanvasHost && OverlayMode == other.OverlayMode && ExecuteActions == other.ExecuteActions && NavCategory == other.NavCategory)
			{
				return TakesScroll == other.TakesScroll;
			}
			return false;
		}
	}

	public class ClickRegion
	{
		public int x1;

		public int y1;

		public int x2;

		public int y2;

		public string OverCommand;

		public string LeftClickCommand;

		public string RightClickCommand;

		public bool Contains(int x, int y)
		{
			if (x >= x1 && x <= x2 && y >= y1)
			{
				return y <= y2;
			}
			return false;
		}
	}

	public static bool _focused = true;

	public static float mouseDisable = 0f;

	public static UnityEngine.GameObject MainCamera;

	public static Camera cameraMainCamera;

	public static LetterboxCamera MainCameraLetterbox;

	public UnityEngine.GameObject Backdrop;

	public UnityEngine.GameObject TileRoot;

	public UnityEngine.GameObject OverlayRoot;

	public static GameManager Instance;

	public UnityEngine.GameObject Minimap;

	public static ManualResetEvent focusedEvent = new ManualResetEvent(initialState: true);

	public ThreadTaskQueue uiQueue = new ThreadTaskQueue();

	public ThreadTaskQueue gameQueue = new ThreadTaskQueue();

	public static bool bCapInputBuffer = false;

	public int tileWidth = 16;

	public int tileHeight = 24;

	private Point2D CurrentPlayerCell = new Point2D(-1, -1);

	public static List<Action<PreferredSidebarPosition>> OnPreferredSidebarPositionUpdatedCallbacks = new List<Action<PreferredSidebarPosition>>();

	public PreferredSidebarPosition currentSidebarPosition;

	public Dictionary<string, UnityEngine.GameObject> findCache = new Dictionary<string, UnityEngine.GameObject>();

	private QudItemList currentSidebarList;

	public UIManager uiManager;

	public Player player;

	public static List<Resolution> resolutions;

	public SynchronizationContext uiSynchronizationContext;

	public SynchronizationContext gameThreadSynchronizationContext;

	public Texture2D minimapTexture;

	public static Color32[] minimapColors = new Color32[4000];

	public bool _bAlt;

	public exTextureInfo[] CharInfos = new exTextureInfo[256];

	public ex3DSprite2[,] ConsoleCharacter = new ex3DSprite2[80, 25];

	public ex3DSprite2[,] OverlayCharacter = new ex3DSprite2[82, 27];

	public int[,] CurrentShadermode = new int[80, 25];

	public string[,] CurrentTile = new string[80, 25];

	public bool MouseInput;

	private Dictionary<string, ViewInfo> _ViewData = new Dictionary<string, ViewInfo> { 
	{
		"*Default",
		new ViewInfo(WantsTileOver: false, ForceFullscreen: true, "Menu")
	} };

	public Point2D LastTileOver = new Point2D(-1, -1);

	public float TargetZoomFactor = 1f;

	public Vector3 TargetCameraLocation = new Vector3(0f, 0f, -10f);

	private bool _overlayOptionsUpdated;

	private bool _overlayUIEnabled;

	private float TargetIntensity;

	private CC_AnalogTV TV;

	public string _ActiveGameView;

	public string __CurrentGameView = "MainMenu";

	private Stack<string> GameViewStack = new Stack<string>();

	public bool skipAnInput;

	private List<ClickRegion> Regions = new List<ClickRegion>();

	private int nRegions;

	public bool bViewUpdated;

	public TooltipTrigger generalTooltip;

	public TooltipTrigger lookerTooltip;

	public TooltipTrigger tileTooltip;

	public Vector2i lastHover = new Vector2i(-1, -1);

	public List<string> LeftGameViews = new List<string>();

	public string LastGameView = "";

	public ViewInfo currentUIView;

	private List<string> HiddenNongameViews = new List<string>();

	public static bool capslock = false;

	public static int bDraw = 0;

	public bool bInitComplete;

	public bool bInitStarted;

	public bool bEditorMode;

	public bool bEditorInit;

	public bool FadeSplash;

	public UnityEngine.GameObject SplashCanvas;

	public float minimapScale = 1f;

	public float nearbyObjectsListScale = 1f;

	public float compassScale = 1f;

	private bool bFirstUpdate = true;

	public bool bAllowPanning;

	public bool PrereleaseInput;

	public int LastWidth;

	public int LastHeight;

	public bool Hallucinating;

	public bool Greyscaling;

	public int GreyscaleLevel;

	public float _spacefoldingT;

	public bool _spacefolding;

	public float _fuzzingT;

	public bool _fuzzing;

	public int _selectedAbility = -1;

	public TextMeshProUGUI selectedAbilityText;

	public ActivatedAbilityEntry currentlySelectedAbility;

	public UnityEngine.GameObject playerTrackerPrefab;

	public PlayerTracker playerTracker;

	public MonoBehaviour InputManagerModuleNew;

	public MonoBehaviour InputManagerModuleOld;

	public string currentNavCategory;

	public string currentNavDirection;

	public StringBuilder selectedAbilitybuilder = new StringBuilder();

	public float lookRepeat;

	public UnityEngine.UI.Text currentNavDirectionDisplay;

	public static bool _runPlayerTurnOnUIThread = false;

	public bool _fadingToBlack;

	public int fadeToBlackStage;

	public float originalBrightness = float.MinValue;

	public float fadingToBlackTimer;

	public static bool runWholeTurnOnUIThread = false;

	public Vector3 lastCameraPosition = Vector3.zero;

	private bool lastFullscreen = true;

	public static bool focused
	{
		get
		{
			return _focused;
		}
		set
		{
			if (!_focused && value)
			{
				ConsoleLib.Console.Keyboard.ClearInput();
				ConsoleLib.Console.Keyboard.ClearMouseEvents();
				mouseDisable = 0.25f;
			}
			_focused = value;
			if (value)
			{
				focusedEvent.Set();
			}
			else
			{
				focusedEvent.Reset();
			}
		}
	}

	public bool bAlt
	{
		get
		{
			return _bAlt;
		}
		set
		{
			_bAlt = value;
			if (_bAlt)
			{
				GameObjectFind("AltButton").GetComponent<Image>().color = Color.red;
			}
			if (!_bAlt)
			{
				GameObjectFind("AltButton").GetComponent<Image>().color = Color.white;
			}
		}
	}

	[Obsolete("The ViewData property will soon be removed, update to using XRL.UI.UIView on a class.")]
	public Dictionary<string, ViewInfo> ViewData => _ViewData;

	public int ControlPanelScale
	{
		set
		{
			float num2 = (ControlPanelFloatScale = (float)value / 100f + 0.75f);
		}
	}

	public float ControlPanelFloatScale
	{
		set
		{
			UnityEngine.GameObject.Find("Legacy Main Canvas").GetComponent<CanvasScaler>().referenceResolution = new Vector2(1248f * value, 768f * value);
		}
	}

	public bool OverlayUIEnabled
	{
		get
		{
			return _overlayUIEnabled;
		}
		set
		{
			if (_overlayUIEnabled != value)
			{
				_overlayUIEnabled = value;
				_overlayOptionsUpdated = true;
			}
		}
	}

	public string _CurrentGameView
	{
		get
		{
			return __CurrentGameView;
		}
		set
		{
			if (value != __CurrentGameView)
			{
				__CurrentGameView = value;
			}
		}
	}

	public string CurrentGameView
	{
		get
		{
			return _CurrentGameView;
		}
		set
		{
			if (!(_CurrentGameView == value))
			{
				_CurrentGameView = value;
				ClearRegions();
			}
		}
	}

	public bool Spacefolding
	{
		get
		{
			return _spacefolding;
		}
		set
		{
			_spacefolding = value;
			if (value)
			{
				_spacefoldingT = 0f;
			}
		}
	}

	public bool Fuzzing
	{
		get
		{
			return _fuzzing;
		}
		set
		{
			_fuzzing = value;
			if (value)
			{
				_fuzzingT = 0f;
			}
		}
	}

	public int selectedAbility
	{
		get
		{
			return _selectedAbility;
		}
		set
		{
			_selectedAbility = value;
		}
	}

	public static bool runPlayerTurnOnUIThread
	{
		get
		{
			return _runPlayerTurnOnUIThread;
		}
		set
		{
			_runPlayerTurnOnUIThread = value;
			Debug.LogWarning("runPlayerTurnSet:" + _runPlayerTurnOnUIThread);
		}
	}

	public bool fadeToBlack
	{
		get
		{
			return _fadingToBlack;
		}
		set
		{
			if (value)
			{
				originalBrightness = float.MinValue;
				fadeToBlackStage = 0;
				fadingToBlackTimer = 0f;
			}
			_fadingToBlack = value;
		}
	}

	public static Camera MapCamera => MainCamera.GetComponent<Camera>();

	private void SetPlayerCell(Point2D C)
	{
		playerTracker.transform.position = getTileCenter(C.x, C.y, 20);
		if (CurrentPlayerCell != C)
		{
			TargetCameraLocation = CellToWorldspace(C);
			CurrentPlayerCell = C;
		}
		UpdatePreferredSidebarPosition();
	}

	public Vector3 CellToWorldspace(Point2D C)
	{
		return new Vector3(40 * -tileWidth + C.x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(C.y * tileHeight) - 12f, -10f);
	}

	public Vector2 WorldspaceToCanvasSpace(Vector3 WorldSpace)
	{
		return UIManager.instance.WorldspaceToCanvasSpace(WorldSpace, cameraMainCamera);
	}

	public void UpdatePreferredSidebarPosition()
	{
		Vector2 vector = WorldspaceToCanvasSpace(CellToWorldspace(CurrentPlayerCell));
		float num = 0.45f;
		float num2 = 0.55f;
		PreferredSidebarPosition preferredSidebarPosition = currentSidebarPosition;
		float num3 = vector.x / (float)Screen.width;
		if (currentSidebarPosition == PreferredSidebarPosition.None)
		{
			preferredSidebarPosition = PreferredSidebarPosition.Left;
		}
		else if (currentSidebarPosition == PreferredSidebarPosition.Right)
		{
			if (num3 > num2)
			{
				preferredSidebarPosition = PreferredSidebarPosition.Left;
			}
		}
		else if (currentSidebarPosition == PreferredSidebarPosition.Left && num3 < num)
		{
			preferredSidebarPosition = PreferredSidebarPosition.Right;
		}
		if (preferredSidebarPosition != currentSidebarPosition)
		{
			if (preferredSidebarPosition == PreferredSidebarPosition.Left)
			{
				Debug.Log("Sidebar swap to left");
			}
			if (preferredSidebarPosition == PreferredSidebarPosition.Right)
			{
				Debug.Log("Sidebar swap to right");
			}
			currentSidebarPosition = preferredSidebarPosition;
			for (int i = 0; i < OnPreferredSidebarPositionUpdatedCallbacks.Count; i++)
			{
				OnPreferredSidebarPositionUpdatedCallbacks[i](preferredSidebarPosition);
			}
		}
	}

	private UnityEngine.GameObject GameObjectFind(string id)
	{
		if (findCache.ContainsKey(id))
		{
			if (!(findCache[id] == null))
			{
				return findCache[id];
			}
			findCache.Remove(id);
		}
		UnityEngine.GameObject gameObject = UnityEngine.GameObject.Find(id);
		if (gameObject != null)
		{
			findCache.Add(id, gameObject);
		}
		_ = gameObject == null;
		return gameObject;
	}

	private void SetPlayerCell(int x, int y)
	{
		TargetCameraLocation = new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, -10f);
	}

	public bool ListsDiffer(QudItemList l1, QudItemList l2)
	{
		if (l1 == null || l2 == null)
		{
			return true;
		}
		if (l1.objects.Count != l2.objects.Count)
		{
			return true;
		}
		for (int i = 0; i < l1.objects.Count; i++)
		{
			if (l1.objects[i].go != l2.objects[i].go)
			{
				return true;
			}
		}
		return false;
	}

	public void ProcessBufferExtra(IScreenBufferExtra extra)
	{
		QudScreenBufferExtra qudScreenBufferExtra = (QudScreenBufferExtra)extra;
		foreach (KeyValuePair<long, ImposterState> imposterUpdate in qudScreenBufferExtra.imposterUpdates)
		{
			ImposterManager.unityProcessImposterUpdate(imposterUpdate.Value);
		}
		if (qudScreenBufferExtra.playerPosition != Point2D.invalid)
		{
			SetPlayerCell(qudScreenBufferExtra.playerPosition);
			RefreshLayout();
		}
		extra.Free();
	}

	public void LoadCommandLine()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs == null || commandLineArgs.Length == 0)
		{
			return;
		}
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (commandLineArgs[i].ToUpper().Contains("NOMETRICS"))
			{
				Globals.ForceMetricsOff = true;
				break;
			}
		}
	}

	private void Awake()
	{
		resolutions = new List<Resolution>(Screen.resolutions);
		uiQueue.threadContext = Thread.CurrentThread;
		uiSynchronizationContext = SynchronizationContext.Current;
		LoadCommandLine();
		Logger.gameLog.Info("Starting up logger...");
		Logger.gameLog.Info("Getting player 0...");
		player = ReInput.players.GetPlayer(0);
		Logger.gameLog.Info("Got player 0...");
		originalBrightness = float.MinValue;
		try
		{
			Debug.Log("Startup time: " + DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString());
			Debug.Log("Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
			Debug.Log("Platform: " + Application.platform);
			Debug.Log("System Language: " + Application.systemLanguage);
			Debug.Log("Unity Version: " + Application.unityVersion);
			Debug.Log("Unity Reported Version: " + Application.version);
		}
		catch (Exception ex)
		{
			Debug.Log("Error with log header: " + ex.ToString());
		}
		Instance = this;
		Application.targetFrameRate = 60;
		for (int i = 0; i < 50; i++)
		{
			Regions.Add(new ClickRegion());
		}
		FingerGestures.OnPinchMove += FingerGesturesOnOnPinchMove;
		SplashCanvas.SetActive(value: true);
		XRLCore.DataPath = Path.Combine(Application.streamingAssetsPath, "Base");
		XRLCore.SavePath = Application.persistentDataPath;
		SteamManager.Awake();
		GetComponent<CapabilityManager>().Init();
		Options.LoadOptions();
		Options.UpdateFlags();
		GetComponent<ControlManager>().Init();
		AchievementManager.Awake();
		uiManager.Init();
		FileLog.logPath = DataManager.SavePath("harmony.log.txt");
		ModManager.Init(Reload: true);
		ModManager.BuildScriptMods();
		LegacyKeyMapping.LoadCommands();
		minimapTexture = new Texture2D(80, 50, TextureFormat.ARGB32, mipChain: false);
		minimapTexture.filterMode = UnityEngine.FilterMode.Point;
		Minimap.GetComponent<Image>().sprite = Sprite.Create(minimapTexture, new Rect(0f, 0f, 80f, 50f), new Vector2(0f, 0f));
		for (int j = 0; j < 80; j++)
		{
			for (int k = 0; k < 50; k++)
			{
				minimapTexture.SetPixel(j, k, new Color(0f, 0f, 0f, 0f));
			}
		}
		minimapTexture.Apply();
		_overlayOptionsUpdated = false;
		mouseDisable = 0f;
	}

	public void DirtyControlPanel()
	{
		_overlayOptionsUpdated = true;
	}

	public void UpdateMinimap()
	{
		lock (minimapColors)
		{
			minimapTexture.SetPixels32(minimapColors);
			minimapTexture.Apply();
		}
	}

	private void FingerGesturesOnOnPinchMove(Vector2 fingerPos1, Vector2 fingerPos2, float delta)
	{
	}

	public void Quit()
	{
		OnDestroy();
		Application.Quit();
	}

	private void OnApplicationQuit()
	{
		OnDestroy();
	}

	private void OnDestroy()
	{
		try
		{
			XRLCore.Stop();
			if (XRLCore.CoreThread != null)
			{
				XRLCore.CoreThread.Interrupt();
				XRLCore.CoreThread.Abort();
			}
		}
		catch
		{
		}
		SteamManager.Shutdown();
	}

	public bool HasViewData(string V)
	{
		if (V == "Inventory" && !Options.OverlayPrereleaseInventory)
		{
			return false;
		}
		if (V == "Popup:Item" && !Options.OverlayPrereleaseInventory)
		{
			return false;
		}
		if (V == "Popup:TradeAmount" && !Options.OverlayPrereleaseTrade)
		{
			return false;
		}
		return _ViewData.ContainsKey(V);
	}

	public ViewInfo GetViewData(string V)
	{
		if (V == "Inventory" && !Options.OverlayPrereleaseInventory)
		{
			return _ViewData["*Default"];
		}
		if (V == "Popup:Item" && !Options.OverlayPrereleaseInventory)
		{
			return _ViewData["*Default"];
		}
		if (V == "Trade" && Options.OverlayPrereleaseTrade)
		{
			return _ViewData["TradePrerelease"];
		}
		if (V == "Popup:TradeAmount" && !Options.OverlayPrereleaseTrade)
		{
			return _ViewData["*Default"];
		}
		if (_ViewData.ContainsKey(V))
		{
			return _ViewData[V];
		}
		return _ViewData["*Default"];
	}

	public void OnDrag(Vector2 Delta)
	{
		if (XRLCore.bThreadFocus)
		{
			TargetCameraLocation -= new Vector3(Delta.x / TargetZoomFactor, Delta.y / TargetZoomFactor, 0f);
		}
	}

	public void OnTileOver(int x, int y)
	{
		if (mouseDisable > 0f || !GetViewData(CurrentGameView).WantsTileOver || (LastTileOver.x == x && LastTileOver.y == y))
		{
			return;
		}
		LastTileOver = new Point2D(x, y);
		lock (Regions)
		{
			for (int i = 0; i < nRegions; i++)
			{
				if (Regions[i].OverCommand != null && Regions[i].Contains(x, y))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent(Regions[i].OverCommand, x, y);
					return;
				}
			}
		}
		ConsoleLib.Console.Keyboard.PushMouseEvent("PointerOver", x, y);
	}

	public void OnTileClicked(string Event, int x, int y)
	{
		if (mouseDisable > 0f)
		{
			return;
		}
		lock (Regions)
		{
			for (int i = 0; i < nRegions; i++)
			{
				if (Event == "LeftClick" && Regions[i].LeftClickCommand != null && Regions[i].Contains(x, y))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent(Regions[i].LeftClickCommand, x, y);
					return;
				}
				if (Event == "RightClick" && Regions[i].RightClickCommand != null && Regions[i].Contains(x, y))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent(Regions[i].RightClickCommand, x, y);
					return;
				}
			}
		}
		if (Event == "RightClick" && CurrentGameView == Options.StageViewID && Options.OverlayTooltips)
		{
			ShowTooltipForTile(x, y);
		}
		else
		{
			ConsoleLib.Console.Keyboard.PushMouseEvent(Event, x, y);
		}
	}

	public void ZoomIn()
	{
		TargetZoomFactor = Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor + 1f);
	}

	public void ZoomOut()
	{
		TargetZoomFactor = Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor - 1f);
	}

	public void OnScroll(Vector2 Amount)
	{
		if (LegacyViewManager.Instance.ActiveView != null && LegacyViewManager.Instance.ActiveView.Name == "MapEditor")
		{
			MainCameraLetterbox.DesiredZoomFactor = TargetZoomFactor;
			TargetZoomFactor = Math.Max(0.25f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor + Amount.y * 0.1f);
		}
		else if (XRLCore.bThreadFocus && (currentUIView == null || !(currentUIView.UICanvas != "Looker") || !(currentUIView.UICanvas != "Keybinds") || !(currentUIView.UICanvas != Options.StageViewID)))
		{
			ViewInfo viewData = GetViewData(CurrentGameView);
			if ((!viewData.ForceFullscreenInLegacy && !viewData.ForceFullscreen && !viewData.TakesScroll) || _ActiveGameView == Options.StageViewID)
			{
				TargetZoomFactor = Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor + Amount.y * 0.25f);
			}
			else if (Amount.y > 0f)
			{
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad8, '8'));
			}
			else
			{
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad2, '2'));
			}
		}
	}

	public void OnViewCommand(string ID)
	{
		Views.OnCommand(ID);
	}

	public void OnControlPanelButton(string ID)
	{
		if (ID == "CmdZoomIn")
		{
			TargetZoomFactor = Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor + 1f);
		}
		if (ID == "CmdZoomOut")
		{
			TargetZoomFactor = Mathf.Max(1f, Camera.main.GetComponent<LetterboxCamera>().DesiredZoomFactor - 1f);
		}
		if (ID == "CmdAlt")
		{
			bAlt = !bAlt;
			return;
		}
		if (CurrentGameView != Options.StageViewID)
		{
			switch (ID)
			{
			case "CmdMoveU":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Space, 'y'));
				return;
			case "CmdMoveD":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.N, 'n'));
				return;
			case "CmdWait":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Space, ' '));
				return;
			case "CmdMoveN":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad8));
				return;
			case "CmdMoveS":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad2));
				return;
			case "CmdMoveE":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad6));
				return;
			case "CmdMoveW":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad4));
				return;
			case "CmdMoveNW":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad7));
				return;
			case "CmdMoveNE":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad9));
				return;
			case "CmdMoveSW":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad1));
				return;
			case "CmdMoveSE":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad3));
				return;
			case "CmdFire":
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad3));
				return;
			}
		}
		else
		{
			if (ID.StartsWith("CmdMove") && bAlt)
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent(ID.Replace("CmdMove", "CmdAttack"), -1, -1);
				return;
			}
			if (ID == "CmdWaitUntilHealed" && bAlt)
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent(ID.Replace("CmdAutoExplore", "CmdAttack"), -1, -1);
				return;
			}
		}
		if (ID == "CmdEscape")
		{
			ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		else if (ID == "CmdReturn")
		{
			ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Return, '\r'));
		}
		else
		{
			ConsoleLib.Console.Keyboard.PushMouseEvent(ID, -1, -1);
		}
	}

	public void ClearRegions()
	{
		lock (Regions)
		{
			nRegions = 0;
		}
	}

	public void AddRegion(int x1, int y1, int x2, int y2, string LeftCommand = null, string RightCommand = null, string OverCommand = null)
	{
		lock (Regions)
		{
			if (nRegions >= Regions.Count)
			{
				Debug.Log("Add region");
				Regions.Add(new ClickRegion());
			}
			Regions[nRegions].x1 = x1;
			Regions[nRegions].y1 = y1;
			Regions[nRegions].x2 = x2;
			Regions[nRegions].y2 = y2;
			Regions[nRegions].LeftClickCommand = LeftCommand;
			Regions[nRegions].RightClickCommand = RightCommand;
			Regions[nRegions].OverCommand = OverCommand;
			nRegions++;
		}
	}

	public void ShowTooltipForTile(int x, int y, string mode = "rightclick")
	{
		if (OverlayUIEnabled && ((LegacyViewManager.Instance.ActiveView != null && LegacyViewManager.Instance.ActiveView.Name == "Looker") || (UIManager.instance.currentWindow != null && UIManager.instance.currentWindow.name == "Stage")))
		{
			tileTooltip.ForceHideTooltip();
			gameQueue.queueSingletonTask("TileHover", delegate
			{
				Look.QueueLookerTooltip(x, y, mode);
			});
		}
	}

	public void TileHover(int x, int y)
	{
	}

	public bool InGameView()
	{
		lock (GameViewStack)
		{
			return GameViewStack.Count > 0;
		}
	}

	public void PushGameView(string NewView, bool bHard = true)
	{
		ControlManager.ResetInput();
		lock (GameViewStack)
		{
			if (LeftGameViews.Contains(NewView))
			{
				LeftGameViews.Remove(NewView);
			}
			GameViewStack.Push(CurrentGameView);
			CurrentGameView = NewView;
			bViewUpdated = true;
			if (bHard)
			{
				TextConsole.BufferUpdated = true;
			}
		}
	}

	public void ForceGameView()
	{
		lock (GameViewStack)
		{
			GameViewStack.Clear();
			CurrentGameView = Options.StageViewID;
			TextConsole.BufferUpdated = true;
			bViewUpdated = true;
		}
	}

	public void PopGameView(bool bHard = false)
	{
		ControlManager.ResetInput();
		lock (GameViewStack)
		{
			if (bHard)
			{
				TextConsole.BufferUpdated = true;
			}
			if (GameViewStack.Count > 0)
			{
				if (CurrentGameView != null && !LeftGameViews.Contains(CurrentGameView))
				{
					LeftGameViews.Add(CurrentGameView);
				}
				CurrentGameView = GameViewStack.Pop();
			}
			bViewUpdated = true;
		}
	}

	private void UpdateView()
	{
		ShowNongameViews();
		for (int num = LeftGameViews.Count - 1; num >= 0; num--)
		{
			if (num < LeftGameViews.Count && num > 0 && LegacyViewManager.Instance.Views.TryGetValue(LeftGameViews[num], out var value))
			{
				value.Leave();
			}
		}
		LeftGameViews.Clear();
		bool bForceEnter = false;
		if (_ActiveGameView != _CurrentGameView)
		{
			bForceEnter = true;
			skipAnInput = true;
			Input.ResetInputAxes();
			ConsoleLib.Console.Keyboard.ClearInput();
		}
		_ActiveGameView = _CurrentGameView;
		if (_ActiveGameView == Options.StageViewID)
		{
			currentUIView = null;
			HideNongameViews(forShowingLater: false);
			MetricsManager.LogEditorInfo("Show new stage: " + _ActiveGameView);
			currentUIView = GetViewData(_ActiveGameView);
			if (!Options.ModernUI)
			{
				MetricsManager.LogEditorInfo("  overlay stage is disabled");
				Views.SetActiveView(null);
			}
			else if (currentUIView.UICanvasHost == 0)
			{
				UIManager.showWindow(null);
				if (OverlayUIEnabled)
				{
					Views.SetActiveView(currentUIView.UICanvas, bHideOldView: true, bForceEnter);
				}
				else
				{
					Views.SetActiveView(null);
				}
			}
			else if (currentUIView.UICanvasHost == 1)
			{
				Views.SetActiveView(null);
				UIManager.showWindow(currentUIView.UICanvas, aggressive: true);
			}
			ImposterManager.enableImposters(Globals.RenderMode == RenderModeType.Tiles && !Input.GetKey(UnityEngine.KeyCode.LeftAlt) && !Input.GetKey(UnityEngine.KeyCode.RightAlt));
			return;
		}
		MetricsManager.LogEditorInfo("Show legacy view: " + _ActiveGameView);
		currentUIView = GetViewData(_ActiveGameView);
		if (_ActiveGameView == null || currentUIView == null)
		{
			MetricsManager.LogEditorInfo("  passthrough");
			Views.SetActiveView(null);
			UIManager.showWindow("PassthroughDefault", aggressive: true);
		}
		else if (currentUIView.UICanvasHost == 0)
		{
			UIManager.showWindow("PassthroughDefault", aggressive: true);
			if (!OverlayUIEnabled)
			{
				Views.SetActiveView(null);
			}
			else if (currentUIView.UICanvas == null)
			{
				MetricsManager.LogEditorInfo("  null canvas");
				Views.SetActiveView(null);
			}
			else if (currentUIView.UICanvas.StartsWith("Popup:"))
			{
				MetricsManager.LogError("Uhoh old popup! " + currentUIView.UICanvas);
				Views.SetActiveView(currentUIView.UICanvas, bHideOldView: false, bForceEnter: true);
			}
			else
			{
				MetricsManager.LogEditorInfo("  " + currentUIView.UICanvas);
				Views.SetActiveView(currentUIView.UICanvas, bHideOldView: true, bForceEnter);
			}
		}
		else if (currentUIView.UICanvasHost == 1)
		{
			MetricsManager.LogEditorInfo("Show new view: " + _ActiveGameView);
			Views.SetActiveView(null);
			UIManager.showWindow(currentUIView.UICanvas);
		}
		else
		{
			MetricsManager.LogError("Bad view canvas host: " + currentUIView.UICanvasHost);
		}
		ImposterManager.enableImposters(bEnable: false);
	}

	private void HideNongameViews(bool forShowingLater = true)
	{
		foreach (KeyValuePair<string, BaseView> view in LegacyViewManager.Instance.Views)
		{
			if (view.Value.rootObject != null && view.Value.Name != Options.StageViewID && view.Value.Name != "Splashscreen" && view.Value.rootObject.activeInHierarchy)
			{
				if (forShowingLater)
				{
					view.Value.rootObject.SetActive(value: false);
					HiddenNongameViews.Add(view.Value.Name);
				}
				else
				{
					view.Value.Leave();
				}
			}
		}
		if (!forShowingLater)
		{
			lock (GameViewStack)
			{
				GameViewStack.Clear();
			}
		}
	}

	private void ShowNongameViews()
	{
		foreach (string hiddenNongameView in HiddenNongameViews)
		{
			if (hiddenNongameView != Options.StageViewID && LegacyViewManager.Instance.Views.ContainsKey(hiddenNongameView))
			{
				LegacyViewManager.Instance.Views[hiddenNongameView].rootObject.SetActive(value: true);
			}
		}
		HiddenNongameViews.Clear();
	}

	public ex3DSprite2 getTile(int x, int y)
	{
		return ConsoleCharacter[x, y];
	}

	public Vector3 getTileCenter(int x, int y, int z = 0)
	{
		return new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, 100 - z);
	}

	private void StartGameThread()
	{
		Debug.Log("Starting game thread...");
		base.useGUILayout = false;
		MainCamera = GameObjectFind("Main Camera");
		cameraMainCamera = MainCamera.GetComponent<Camera>();
		MainCameraLetterbox = MainCamera.GetComponent<LetterboxCamera>();
		WaveCollapseTools.LoadTemplates();
		SoundManager.Init();
		Thread threadContext = XRLCore.Start();
		gameQueue.threadContext = threadContext;
		TV = MainCamera.GetComponent<CC_AnalogTV>();
		TargetIntensity = TV.noiseIntensity;
		TV.noiseIntensity = 1f;
		TileRoot = new UnityEngine.GameObject();
		TileRoot.name = "_tileroot";
		TileRoot.AddComponent<CameraShake>();
		playerTracker = UnityEngine.Object.Instantiate(playerTrackerPrefab).GetComponent<PlayerTracker>();
		playerTracker.transform.SetParent(TileRoot.transform, worldPositionStays: false);
		OverlayRoot = new UnityEngine.GameObject();
		OverlayRoot.name = "_overlayroot";
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				CurrentTile[i, j] = null;
			}
		}
		for (int k = 0; k < 80; k++)
		{
			for (int l = 0; l < 25; l++)
			{
				CurrentShadermode[k, l] = 0;
				ConsoleCharacter[k, l] = SpriteManager.CreateSprite("assets_content_textures_text_1.bmp").GetComponent<ex3DSprite2>();
				ConsoleCharacter[k, l].gameObject.transform.SetParent(TileRoot.transform);
				ConsoleCharacter[k, l].gameObject.transform.position = getTileCenter(k, l);
				ConsoleCharacter[k, l].gameObject.AddComponent<TileBehavior>().SetPosition(k, l);
			}
		}
		Backdrop = SpriteManager.CreateSprite("assets_content_textures_text_0.bmp");
		Backdrop.transform.SetParent(TileRoot.transform);
		Backdrop.transform.position = new Vector3(0f, 0f, 1000f);
		Backdrop.GetComponent<ex3DSprite2>().width = 1280f;
		Backdrop.GetComponent<ex3DSprite2>().height = 600f;
		Backdrop.GetComponent<ex3DSprite2>().color = ConsoleLib.Console.ColorUtility.ColorMap['k'];
		Backdrop.GetComponent<ex3DSprite2>().detailcolor = ConsoleLib.Console.ColorUtility.ColorMap['k'];
		Backdrop.GetComponent<ex3DSprite2>().backcolor = ConsoleLib.Console.ColorUtility.ColorMap['k'];
		for (int m = 0; m < 82; m++)
		{
			for (int n = 0; n < 27; n++)
			{
				if (m == 0 || m == 81 || n == 0 || n == 26)
				{
					OverlayCharacter[m, n] = SpriteManager.CreateSprite("assets_content_textures_text_1.bmp").GetComponent<ex3DSprite2>();
					OverlayCharacter[m, n].gameObject.transform.parent = OverlayRoot.transform;
					OverlayCharacter[m, n].gameObject.transform.position = getTileCenter(m - 1, n - 1) + new Vector3(0f, 0f, -10f);
					OverlayCharacter[m, n].enabled = true;
					if (m == 0)
					{
						SpriteManager.SetSprite(OverlayCharacter[m, n].gameObject, "assets_content_textures_text_27.bmp");
					}
					if (m == 81)
					{
						SpriteManager.SetSprite(OverlayCharacter[m, n].gameObject, "assets_content_textures_text_26.bmp");
					}
					if (n == 0)
					{
						SpriteManager.SetSprite(OverlayCharacter[m, n].gameObject, "assets_content_textures_text_24.bmp");
					}
					if (n == 26)
					{
						SpriteManager.SetSprite(OverlayCharacter[m, n].gameObject, "assets_content_textures_text_25.bmp");
					}
					OverlayCharacter[m, n].backcolor = Color.white;
					OverlayCharacter[m, n].detailcolor = Color.white;
					OverlayCharacter[m, n].color = new Color(0.1f, 0.1f, 0.1f, 1f);
					if (m == 0)
					{
						OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveW");
					}
					if (m == 81)
					{
						OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveE");
					}
					if (n == 0)
					{
						OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveN");
					}
					if (n == 26)
					{
						OverlayCharacter[m, n].gameObject.AddComponent<BorderTileBehavior>().SetDirection("CmdMoveS");
					}
					if (m == 0 && n == 0)
					{
						OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveNW");
					}
					if (m == 81 && n == 0)
					{
						OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveNE");
					}
					if (m == 0 && n == 26)
					{
						OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveSW");
					}
					if (m == 81 && n == 26)
					{
						OverlayCharacter[m, n].GetComponent<BorderTileBehavior>().SetDirection("CmdMoveSE");
					}
				}
			}
		}
		for (int num = 0; num < 256; num++)
		{
			CharInfos[num] = SpriteManager.GetTextureInfo("assets_content_textures_text_" + num + ".bmp");
		}
		Debug.Log("Initilization complete...");
		bInitComplete = true;
	}

	public void OnGUI()
	{
		if (!XRLCore.bThreadFocus || UIManager.instance == null || !UIManager.instance.AllowPassthroughInput())
		{
			return;
		}
		if (Instance.PrereleaseInput)
		{
			if (!OverlayUIEnabled && currentUIView != null && (currentUIView.UICanvas == "Popup:Text" || currentUIView.UICanvas == "Popup:AskString"))
			{
				pushKeyEvents();
			}
			else
			{
				pushKeyEvents();
			}
			return;
		}
		if (OverlayUIEnabled && currentUIView != null && currentUIView.UICanvas != null && !(currentUIView.UICanvas == "Stage") && !(currentUIView.UICanvas == "Popup:Text"))
		{
			if (currentUIView.UICanvas.StartsWith("Popup:") || currentNavCategory.Contains("Conversation"))
			{
				return;
			}
			if (currentUIView.UICanvas == "Looker")
			{
				if (UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.Escape)
				{
					return;
				}
			}
			else if (!(currentUIView.UICanvas == "Keybinds"))
			{
				return;
			}
		}
		pushKeyEvents();
	}

	public static void pushKeyEvents()
	{
		capslock = UnityEngine.Event.current.capsLock;
		ConsoleLib.Console.Keyboard._bAlt = Input.GetKey(UnityEngine.KeyCode.RightAlt) || Input.GetKey(UnityEngine.KeyCode.LeftAlt) || (Instance.PrereleaseInput && (Instance.player.GetButton("Highlight") || (Instance.player.GetButton("Alternate") && Instance.player.GetButton("AltHighlight"))));
		ConsoleLib.Console.Keyboard._bCtrl = Input.GetKey(UnityEngine.KeyCode.RightControl) || Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightCommand) || Input.GetKey(UnityEngine.KeyCode.LeftCommand);
		ConsoleLib.Console.Keyboard._bShift = Input.GetKey(UnityEngine.KeyCode.RightShift) || Input.GetKey(UnityEngine.KeyCode.LeftShift);
		if (!UnityEngine.Event.current.isKey || UnityEngine.Event.current.type != EventType.KeyDown || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightCommand || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftCommand || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftControl || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightControl || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftAlt || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightAlt || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.LeftShift || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.RightShift || UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.None)
		{
			return;
		}
		if (ControlManager.PrereleaseInput)
		{
			if (!ControlManager.isKeyMapped(UnityEngine.Event.current.keyCode))
			{
				if (UnityEngine.Event.current.keyCode == UnityEngine.KeyCode.KeypadEnter)
				{
					UnityEngine.Event.current.keyCode = UnityEngine.KeyCode.Return;
				}
				ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.Event.current), bAllowMap: true);
			}
		}
		else
		{
			ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.Event.current), bAllowMap: true);
		}
	}

	public void OnGUIExternal()
	{
		if (Views != null)
		{
			Views.OnGUI();
		}
	}

	private void EditorInit()
	{
	}

	private void OnApplicationFocus(bool focus)
	{
		XRLCore.bThreadFocus = focus;
		focused = focus;
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		focused = !pauseStatus;
	}

	public bool ColorMatch(Color C1, Color C2)
	{
		if (C1.r == C2.r && C1.g == C2.g)
		{
			return C1.b == C2.b;
		}
		return false;
	}

	public override void RegisterViews()
	{
		foreach (Type item in ModManager.GetTypesWithAttribute(typeof(UIView)))
		{
			string text = null;
			object[] customAttributes = item.GetCustomAttributes(typeof(UIView), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				UIView uIView = (UIView)customAttributes[i];
				text = text ?? uIView.UICanvas ?? uIView.ID;
				if (_ViewData.TryGetValue(uIView.ID, out var value))
				{
					if (!value.IsSame(uIView.AsGameManagerViewInfo()))
					{
						Debug.LogError("Found a second UIView with ID " + uIView.ID + " in " + item.ToString() + " with different parameters from the last one.");
					}
				}
				else
				{
					_ViewData[uIView.ID] = uIView.AsGameManagerViewInfo();
				}
			}
			if (item.IsSubclassOf(typeof(BaseView)) && Activator.CreateInstance(item) is BaseView v)
			{
				RegisterView(text, v);
			}
		}
		RegisterView("SteamWorkshopUploader", new SteamWorkshopUploaderView());
		Views.AddView("PickFile", new PickFileView());
	}

	public void SetActiveView(string id)
	{
	}

	public override void OnStart()
	{
		UIManager.showWindow("PassthroughDefault");
		Views.GetView("Splashscreen").ShowClickthroughOverlay();
		base.gameObject.AddComponent<CombatJuiceManager>().gameManager = this;
	}

	public Vector3 GetCellCenter(int x, int y, int z = -10)
	{
		return new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, z);
	}

	public Vector3 CenterOnCell(int x, int y)
	{
		if (TargetZoomFactor <= 1f)
		{
			return GetCellCenter(x, y);
		}
		MainCameraLetterbox.SetPositionImmediately(new Vector3(40 * -tileWidth + x * tileWidth + 8, 12.5f * (float)tileHeight - (float)(y * tileHeight) - 12f, -10f));
		TargetCameraLocation = MainCameraLetterbox.DesiredPosition;
		return new Vector3(TargetCameraLocation.x, TargetCameraLocation.y, 0f);
	}

	public void RefreshLayout()
	{
		if (!(MainCameraLetterbox != null) || !(CurrentGameView != "MapEditor"))
		{
			return;
		}
		if (CurrentGameView == null || !GetViewData(CurrentGameView).IgnoreForceFullscreen)
		{
			if (CurrentGameView != null && CurrentGameView.StartsWith("Popup:"))
			{
				if (CurrentGameView.StartsWith("Popup:Item") && (GetViewData(CurrentGameView).ForceFullscreen || (!Options.ModernUI && GetViewData(CurrentGameView).ForceFullscreenInLegacy)))
				{
					if (MainCameraLetterbox.DesiredPosition != new Vector3(0f, 0f, -10f))
					{
						MainCameraLetterbox.UpdateDelay = 2;
					}
					MainCameraLetterbox.DesiredPosition = new Vector3(0f, 0f, -10f);
					MainCameraLetterbox.DesiredZoomFactor = 1f;
				}
			}
			else if (!GetViewData(CurrentGameView).ForceFullscreen && (Options.ModernUI || !GetViewData(CurrentGameView).ForceFullscreenInLegacy))
			{
				if (bAllowPanning)
				{
					if (MainCameraLetterbox.DesiredZoomFactor != TargetZoomFactor)
					{
						UpdatePreferredSidebarPosition();
					}
					MainCameraLetterbox.DesiredZoomFactor = TargetZoomFactor;
					if (TargetZoomFactor <= 1f)
					{
						MainCameraLetterbox.DesiredPosition = new Vector3(0f, 0f, -10f);
					}
					else
					{
						MainCameraLetterbox.SetPositionImmediately(TargetCameraLocation);
					}
				}
				else
				{
					MainCameraLetterbox.DesiredZoomFactor = 1f;
					MainCameraLetterbox.DesiredPosition = new Vector3(0f, 0f, -10f);
				}
			}
			else
			{
				MainCameraLetterbox.DesiredPosition = new Vector3(0f, 0f, -10f);
				MainCameraLetterbox.DesiredZoomFactor = 1f;
			}
		}
		if (!(CurrentGameView == Options.StageViewID) && !Options.ModernUI && CurrentGameView != "PopupMessage")
		{
			MainCameraLetterbox.DesiredPosition = new Vector3(0f, 0f, -10f);
			MainCameraLetterbox.DesiredZoomFactor = 1f;
		}
	}

	public void UpdateSelectedAbility()
	{
		if (_selectedAbility == -1)
		{
			return;
		}
		selectedAbilitybuilder.Length = 0;
		currentlySelectedAbility = AbilityAPI.GetAbility(selectedAbility);
		if (currentlySelectedAbility == null)
		{
			selectedAbilitybuilder.Append("<color=#666666>");
			selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Previous Ability"));
			selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Next Ability"));
			selectedAbilitybuilder.Append("</color>");
			selectedAbilitybuilder.Append(" <none>");
			selectedAbilityText.text = selectedAbilitybuilder.ToString();
			return;
		}
		selectedAbilitybuilder.Append("<color=#666666>");
		selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Previous Ability"));
		selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Next Ability"));
		selectedAbilitybuilder.Append("</color>");
		selectedAbilitybuilder.Append(" ");
		if (currentlySelectedAbility.Cooldown > 0 || !currentlySelectedAbility.Enabled)
		{
			selectedAbilitybuilder.Append("<color=#999999>");
		}
		else
		{
			selectedAbilitybuilder.Append("<color=#FFFFFF>");
		}
		selectedAbilitybuilder.Append("<color=#FFFF00>");
		selectedAbilitybuilder.Append(ControlManager.getCommandInputDescription("Use Ability"));
		selectedAbilitybuilder.Append("</color>");
		selectedAbilitybuilder.Append(" ");
		selectedAbilitybuilder.Append(currentlySelectedAbility.DisplayName);
		if (currentlySelectedAbility.Cooldown > 0)
		{
			selectedAbilitybuilder.Append(" [");
			selectedAbilitybuilder.Append(currentlySelectedAbility.Cooldown / 10 + 1);
			selectedAbilitybuilder.Append(" turns]");
		}
		selectedAbilitybuilder.Append("</color>");
		selectedAbilityText.text = selectedAbilitybuilder.ToString();
	}

	public string LongDirectionToShortDirection(string dir)
	{
		switch (dir)
		{
		case "North":
		case "N":
			return "N";
		case "South":
		case "S":
			return "S";
		case "East":
		case "E":
			return "E";
		case "West":
		case "W":
			return "W";
		case "Northeast":
		case "NE":
			return "NE";
		case "Northwest":
		case "NW":
			return "NW";
		case "Southeast":
		case "SE":
			return "SE";
		case "Southwest":
		case "SW":
			return "SW";
		default:
			return ".";
		}
	}

	public void UpdateInput()
	{
		if (!XRLCore.bThreadFocus)
		{
			return;
		}
		if (skipAnInput)
		{
			skipAnInput = false;
			return;
		}
		if (!Instance.PrereleaseInput)
		{
			if (InputManagerModuleNew.enabled)
			{
				InputManagerModuleNew.enabled = false;
			}
			if (!InputManagerModuleOld.enabled)
			{
				InputManagerModuleOld.enabled = true;
			}
			if (currentNavDirectionDisplay != null && currentNavDirectionDisplay.text != "")
			{
				currentNavDirectionDisplay.text = "";
			}
			return;
		}
		if (!InputManagerModuleNew.enabled)
		{
			ControlManager.ResetInput();
			InputManagerModuleNew.enabled = true;
			ControlManager.ResetInput();
		}
		if (InputManagerModuleOld.enabled)
		{
			InputManagerModuleOld.enabled = false;
			ControlManager.ResetInput();
		}
		string text = GetViewData(CurrentGameView).NavCategory;
		if (text == null || text == "StringInput")
		{
			text = "Menu";
		}
		if (currentNavCategory != text)
		{
			currentNavCategory = text;
			ControlManager.DisableAllLayers();
			ControlManager.EnableLayer("Default");
			if (currentNavCategory.Contains("Adventure") || currentNavCategory.Contains("Looker"))
			{
				ControlManager.EnableLayer("Adventure");
				ControlManager.EnableLayer("AltAdventure");
			}
			if (currentNavCategory.Contains("Cancel"))
			{
				ControlManager.EnableLayer("Menus");
			}
			if (currentNavCategory.Contains("Menu"))
			{
				ControlManager.EnableLayer("Menus");
				ControlManager.EnableLayer("Character Sheet");
				ControlManager.EnableLayer("Containers");
			}
			if (currentNavCategory.Contains("Charactersheet"))
			{
				ControlManager.EnableLayer("Character Sheet");
			}
			if (currentNavCategory.Contains("Trade"))
			{
				ControlManager.EnableLayer("Trade");
			}
			if (currentNavCategory.Contains("Conversation"))
			{
				ControlManager.EnableLayer("Conversation");
			}
			if (currentNavCategory.Contains("Targeting"))
			{
				ControlManager.EnableLayer("Targeting");
				ControlManager.EnableLayer("Adventure");
				ControlManager.EnableLayer("AltAdventure");
			}
			skipAnInput = true;
		}
		else
		{
			if (!UIManager.instance.AllowPassthroughInput())
			{
				return;
			}
			if (currentNavCategory != null)
			{
				if (Options.OverlayUI && currentNavCategory.Contains("Conversation"))
				{
					if (player.GetButtonDown("Cancel"))
					{
						ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
					}
					if (player.GetButtonDown("Trade"))
					{
						ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Tab);
					}
					return;
				}
				if (CurrentGameView == "Inventory" && Options.OverlayUI && Options.OverlayPrereleaseInventory)
				{
					if (player.GetButtonDown("Previous Page"))
					{
						ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Keypad7);
					}
					if (player.GetButtonDown("Next Page"))
					{
						ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Keypad9);
					}
					if (player.GetButtonDown("Cancel"))
					{
						ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
					}
					return;
				}
			}
			if (currentUIView != null && !(currentUIView.NavCategory == "Menu") && !(currentUIView.UICanvas == "Stage") && !(currentUIView.UICanvas == "Popup:Text") && !(currentUIView.UICanvas == "Looker"))
			{
				if (text == "StringInput")
				{
					if (player.GetButtonDown("Accept"))
					{
						ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Return));
					}
					if (player.GetButtonDown("Cancel"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Cancel");
					}
					return;
				}
				if (currentNavCategory.Contains("Conversation") && player.GetButtonDown("Trade"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Tab);
				}
				if (text.Contains("Charactersheet"))
				{
					if (player.GetButtonDown("Previous Page"))
					{
						ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Keypad7);
					}
					if (player.GetButtonDown("Next Page"))
					{
						ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Keypad9);
					}
				}
				if ((text.Contains("Menu") || text.Contains("Cancel")) && !text.Contains("Nocancelescape") && (!text.Contains("Nomoderncancelescape") || !Options.OverlayUI) && player.GetButtonDown("Cancel"))
				{
					ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
				}
			}
			if (player.GetButtonDown("Accept"))
			{
				if (CurrentGameView == "AbilityManager")
				{
					ConsoleLib.Console.Keyboard.PushKey(new ConsoleLib.Console.Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Space, ' '));
				}
				else
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Accept");
				}
			}
			if (text != null && !text.Contains("Nocancelescape") && player.GetButtonDown("Cancel"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Cancel");
			}
			if (text.Contains("Menu") && !text.Contains("Targeting"))
			{
				if (player.GetButtonDownRepeating("Navigate Vertical") || player.GetNegativeButtonDownRepeating("Navigate Vertical"))
				{
					if (player.GetAxis("Navigate Vertical") > 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetAxis("Navigate Vertical") < 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
				}
				else if (player.GetButtonDownRepeating("Navigate Horizontal") || player.GetNegativeButtonDownRepeating("Navigate Horizontal"))
				{
					if (player.GetAxis("Navigate Horizontal") > 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetAxis("Navigate Horizontal") < 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
				}
			}
			if (text.Contains("Menu") && !text.Contains("Looker") && !text.Contains("Targeting"))
			{
				if (player.GetButtonDown("Previous Page"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Keypad7);
				}
				if (player.GetButtonDown("Next Page"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Keypad9);
				}
				if (player.GetButtonDown("Take All"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:Take All");
				}
				if (player.GetButtonDown("Store Items"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:Store Items");
				}
				if (player.GetButtonDown("Set Primary Limb"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:Set Primary Limb");
				}
				if (player.GetButtonDown("New Map Pin"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:New Map Pin");
				}
				if (player.GetButtonDown("Add Journal Entry"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:Add Journal Entry");
				}
				if (player.GetButtonDown("Delete Journal Entry"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:Delete Journal Entry");
				}
			}
			if (text.Contains("Conversation") && player.GetButtonDown("Trade"))
			{
				ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Tab);
			}
			if (text.Contains("Trade"))
			{
				if (player.GetButtonDown("Add one item"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.KeypadPlus);
				}
				if (player.GetButtonDown("Remove one item"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.KeypadMinus);
				}
				if (player.GetButtonDown("All items"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Space);
				}
				if (player.GetButtonDown("Offer"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.O);
				}
				if (player.GetButtonDown("Vendor Actions"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:Vendor Actions");
				}
				if (player.GetButtonDown("Cancel"))
				{
					ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Escape);
				}
			}
			if (text.Contains("Targeting"))
			{
				if (player.GetButtonDown("CmdMissileWeaponMenu"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:CmdMissileWeaponMenu");
				}
				if (player.GetButtonDown("Fire"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdFire");
				}
				if (player.GetButton("Alternate") && player.GetButtonDown("AltFire"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdFire");
				}
				if (player.GetButtonDownRepeating("Navigate Vertical") || player.GetNegativeButtonDownRepeating("Navigate Vertical"))
				{
					if (player.GetAxis("Navigate Vertical") > 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetAxis("Navigate Vertical") < 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
				}
				else if (player.GetButtonDownRepeating("Navigate Horizontal") || player.GetNegativeButtonDownRepeating("Navigate Horizontal"))
				{
					if (player.GetAxis("Navigate Horizontal") > 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetAxis("Navigate Horizontal") < 0f)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
				}
				else if (player.GetButton("Alternate"))
				{
					if (player.GetButtonDownRepeating("AltMove North"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetButtonDownRepeating("AltMove South"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
					if (player.GetButtonDownRepeating("AltMove East"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetButtonDownRepeating("AltMove West"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
					if (player.GetButtonDownRepeating("AltMove Northwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorthwest");
					}
					if (player.GetButtonDownRepeating("AltMove Northeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNortheast");
					}
					if (player.GetButtonDownRepeating("AltMove Southwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouthwest");
					}
					if (player.GetButtonDownRepeating("AltMove Southeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSoutheast");
					}
				}
				else
				{
					if (player.GetButtonDownRepeating("Move North"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetButtonDownRepeating("Move South"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
					if (player.GetButtonDownRepeating("Move East"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetButtonDownRepeating("Move West"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
					if (player.GetButtonDownRepeating("Move Northwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorthwest");
					}
					if (player.GetButtonDownRepeating("Move Northeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNortheast");
					}
					if (player.GetButtonDownRepeating("Move Southwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouthwest");
					}
					if (player.GetButtonDownRepeating("Move Southeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSoutheast");
					}
				}
			}
			if (text.Contains("Adventure"))
			{
				if (player.GetButtonDown("System"))
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:System");
					Input.ResetInputAxes();
				}
				if (player.GetButton("Alternate"))
				{
					if (player.GetButtonDown("AltNext Ability"))
					{
						selectedAbility++;
						if (selectedAbility >= AbilityAPI.abilityCount)
						{
							selectedAbility = 0;
						}
						UpdateSelectedAbility();
					}
					if (player.GetButtonDown("AltPrevious Ability"))
					{
						selectedAbility--;
						if (selectedAbility < 0)
						{
							selectedAbility = AbilityAPI.abilityCount - 1;
						}
						UpdateSelectedAbility();
					}
					if (player.GetButtonDownRepeating("AltMove North"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetButtonDownRepeating("AltMove South"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
					if (player.GetButtonDownRepeating("AltMove East"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetButtonDownRepeating("AltMove West"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
					if (player.GetButtonDownRepeating("AltMove Northwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorthwest");
					}
					if (player.GetButtonDownRepeating("AltMove Northeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNortheast");
					}
					if (player.GetButtonDownRepeating("AltMove Southwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouthwest");
					}
					if (player.GetButtonDownRepeating("AltMove Southeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSoutheast");
					}
					if (player.GetButtonDown("AltMove Up"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateUp");
					}
					if (player.GetButtonDown("AltMove Down"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateDown");
					}
					if (player.GetButtonDownRepeating("AltAttack North"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackN");
					}
					if (player.GetButtonDownRepeating("AltAttack South"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackS");
					}
					if (player.GetButtonDownRepeating("AltAttack East"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackE");
					}
					if (player.GetButtonDownRepeating("AltAttack West"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackW");
					}
					if (player.GetButtonDownRepeating("AltAttack Northwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackNW");
					}
					if (player.GetButtonDownRepeating("AltAttack Northeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackNE");
					}
					if (player.GetButtonDownRepeating("AltAttack Southwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackSW");
					}
					if (player.GetButtonDownRepeating("AltAttack Southeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackSE");
					}
					if (player.GetButtonDown("AltInteract"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Use");
					}
					if (player.GetButtonDown("AltFire"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdFire");
					}
					if (player.GetButtonDown("AltReload"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdReload");
					}
					if (player.GetButtonDown("AltAbilities"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAbilities");
					}
					if (player.GetButtonDown("AltUse Ability") && currentlySelectedAbility != null)
					{
						gameQueue.queueTask(delegate
						{
							if (currentlySelectedAbility != null && currentlySelectedAbility.Cooldown <= 0 && currentlySelectedAbility.Enabled)
							{
								CommandEvent.Send(The.Player, currentlySelectedAbility.Command);
							}
							else if (currentlySelectedAbility.Cooldown > 0)
							{
								gameQueue.queueTask(delegate
								{
									MessageQueue.AddPlayerMessage("You must wait " + (currentlySelectedAbility.Cooldown / 10 + 1) + " turns before using that ability.");
								});
							}
							else
							{
								gameQueue.queueTask(delegate
								{
									MessageQueue.AddPlayerMessage("That ability can't be used at this time.");
								});
							}
						});
					}
					if (player.GetButtonDown("AltCmdMoveToPointOfInterest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdMoveToPointOfInterest");
					}
					if (player.GetButtonDown("AltCharacter Sheet"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:CharacterSheet");
					}
					if (player.GetButtonDown("AltWalk"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Walk");
					}
					if (player.GetButtonDown("AltLook"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdLook");
					}
					if (player.GetButtonDown("AltZoomIn"))
					{
						ZoomIn();
					}
					if (player.GetButtonDown("AltZoomOut"))
					{
						ZoomOut();
					}
					if (player.GetButtonDown("AltGet"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Get");
					}
					if (player.GetButtonDown("AltGetNearby"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:GetNearby");
					}
					if (player.GetButtonDown("AltInteractNearby"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdGetFrom");
					}
					if (player.GetButtonDown("AltAutoexplore"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Autoexplore");
					}
					if (player.GetButtonDown("AltWait"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdWait");
					}
					if (player.GetButtonDown("AltWait Menu"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdWaitMenu");
					}
					if (player.GetButtonDown("AltRest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Rest");
					}
					if (player.GetButtonDown("AltThrow"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Throw");
					}
					if (player.GetButtonDown("AltWish"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdWish");
					}
					if (player.GetButtonDown("AltQuests"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdQuests");
					}
					if (player.GetButtonDown("AltSave"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdSave");
					}
					if (player.GetButtonDown("AltLoad"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdLoad");
					}
					if (player.GetButtonDown("AltAutoAttackNearest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackNearest");
					}
					if (player.GetButtonDownRepeating("AltForceAttack") && !(currentNavDirection == ".") && currentNavDirection != null)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttack" + LongDirectionToShortDirection(currentNavDirection));
					}
				}
				else
				{
					if (player.GetButtonDown("Next Ability"))
					{
						selectedAbility++;
						if (selectedAbility >= AbilityAPI.abilityCount)
						{
							selectedAbility = 0;
						}
						UpdateSelectedAbility();
					}
					if (player.GetButtonDown("Previous Ability"))
					{
						selectedAbility--;
						if (selectedAbility < 0)
						{
							selectedAbility = AbilityAPI.abilityCount - 1;
						}
						UpdateSelectedAbility();
					}
					if (player.GetButtonDownRepeating("Move North"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorth");
					}
					if (player.GetButtonDownRepeating("Move South"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouth");
					}
					if (player.GetButtonDownRepeating("Move East"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateEast");
					}
					if (player.GetButtonDownRepeating("Move West"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateWest");
					}
					if (player.GetButtonDownRepeating("Move Northwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorthwest");
					}
					if (player.GetButtonDownRepeating("Move Northeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNortheast");
					}
					if (player.GetButtonDownRepeating("Move Southwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouthwest");
					}
					if (player.GetButtonDownRepeating("Move Southeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSoutheast");
					}
					if (player.GetButtonDown("Move Up"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateUp");
					}
					if (player.GetButtonDown("Move Down"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateDown");
					}
					if (player.GetButtonDownRepeating("Attack North"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackN");
					}
					if (player.GetButtonDownRepeating("Attack South"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackS");
					}
					if (player.GetButtonDownRepeating("Attack East"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackE");
					}
					if (player.GetButtonDownRepeating("Attack West"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackW");
					}
					if (player.GetButtonDownRepeating("Attack Northwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackNW");
					}
					if (player.GetButtonDownRepeating("Attack Northeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackNE");
					}
					if (player.GetButtonDownRepeating("Attack Southwest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackSW");
					}
					if (player.GetButtonDownRepeating("Attack Southeast"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackSE");
					}
					if (player.GetButtonDown("Interact"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Use");
					}
					if (player.GetButtonDown("Fire"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdFire");
					}
					if (player.GetButtonDown("Reload"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdReload");
					}
					if (player.GetButtonDown("Abilities"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAbilities");
					}
					if (player.GetButtonDown("Use Ability") && currentlySelectedAbility != null)
					{
						gameQueue.queueTask(delegate
						{
							if (currentlySelectedAbility != null && currentlySelectedAbility.Cooldown <= 0 && currentlySelectedAbility.Enabled)
							{
								CommandEvent.Send(The.Player, currentlySelectedAbility.Command);
							}
							else if (currentlySelectedAbility.Cooldown > 0)
							{
								gameQueue.queueTask(delegate
								{
									MessageQueue.AddPlayerMessage("You must wait " + (currentlySelectedAbility.Cooldown / 10 + 1) + " turns before using that ability.");
								});
							}
							else
							{
								gameQueue.queueTask(delegate
								{
									MessageQueue.AddPlayerMessage("That ability can't be used at this time.");
								});
							}
						});
					}
					if (player.GetButtonDown("CmdMoveToPointOfInterest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdMoveToPointOfInterest");
					}
					if (player.GetButtonDown("Character Sheet"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:CharacterSheet");
					}
					if (player.GetButtonDown("Walk"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Walk");
					}
					if (player.GetButtonDown("Look"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdLook");
					}
					if (player.GetButtonDown("ZoomIn"))
					{
						ZoomIn();
					}
					if (player.GetButtonDown("ZoomOut"))
					{
						ZoomOut();
					}
					if (player.GetButtonDown("Get"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Get");
					}
					if (player.GetButtonDown("GetNearby"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:GetNearby");
					}
					if (player.GetButtonDown("InteractNearby"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdGetFrom");
					}
					if (player.GetButtonDown("Autoexplore"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Autoexplore");
					}
					if (player.GetButtonDown("Wait"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdWait");
					}
					if (player.GetButtonDown("Wait Menu"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdWaitMenu");
					}
					if (player.GetButtonDown("Rest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Rest");
					}
					if (player.GetButtonDown("Throw"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Throw");
					}
					if (player.GetButtonDown("Wish"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdWish");
					}
					if (player.GetButtonDown("Quests"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdQuests");
					}
					if (player.GetButtonDown("Save"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdSave");
					}
					if (player.GetButtonDown("Load"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdLoad");
					}
					if (player.GetButtonDown("AutoAttackNearest"))
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttackNearest");
					}
					currentNavDirection = ResolveMovementDirection("Direct Horizontal", "Direct Vertical");
					if (currentNavDirectionDisplay == null)
					{
						UnityEngine.GameObject gameObject = UnityEngine.GameObject.Find("CurrentNavDirection");
						if (gameObject != null)
						{
							currentNavDirectionDisplay = gameObject.GetComponent<UnityEngine.UI.Text>();
						}
					}
					playerTracker.setActiveDirection(currentNavDirection);
					if (currentNavDirectionDisplay != null)
					{
						if (currentNavDirection != null)
						{
							currentNavDirectionDisplay.text = currentNavDirection;
						}
						else
						{
							currentNavDirectionDisplay.text = "";
						}
					}
					if (player.GetButtonDownRepeating("ForceAttack") && !(currentNavDirection == ".") && currentNavDirection != null)
					{
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttack" + LongDirectionToShortDirection(currentNavDirection));
					}
					if (player.GetButtonDownRepeating("Take A Step"))
					{
						if (currentNavDirection == "." || currentNavDirection == null)
						{
							ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Wait");
						}
						else if (player.GetButton("Alternate"))
						{
							ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdAttack" + LongDirectionToShortDirection(currentNavDirection));
						}
						else
						{
							ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Navigate" + currentNavDirection);
						}
					}
					if (ResolveMovementDirection("Look Horizontal", "Look Vertical") != null)
					{
						lookRepeat = 0f;
						Look.bLocked = false;
						ConsoleLib.Console.Keyboard.PushMouseEvent("Command:CmdLook");
					}
				}
			}
			else
			{
				playerTracker.setActiveDirection(null);
			}
			if (!text.Contains("Looker"))
			{
				return;
			}
			if (player.GetButtonDown("Accept"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Passthrough:Interact");
			}
			if (player.GetButtonDownRepeating("Move North"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorth");
			}
			if (player.GetButtonDownRepeating("Move South"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouth");
			}
			if (player.GetButtonDownRepeating("Move East"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateEast");
			}
			if (player.GetButtonDownRepeating("Move West"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateWest");
			}
			if (player.GetButtonDownRepeating("Move Northwest"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNorthwest");
			}
			if (player.GetButtonDownRepeating("Move Northeast"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateNortheast");
			}
			if (player.GetButtonDownRepeating("Move Southwest"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSouthwest");
			}
			if (player.GetButtonDownRepeating("Move Southeast"))
			{
				ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:NavigateSoutheast");
			}
			if (player.GetButtonDown("Alternate"))
			{
				Look.bLocked = !Look.bLocked;
			}
			string text2 = ResolveMovementDirection("Look Horizontal", "Look Vertical");
			if (text2 != null)
			{
				lookRepeat -= Time.deltaTime;
				if (lookRepeat <= 0f)
				{
					ConsoleLib.Console.Keyboard.PushMouseEvent("Meta:Navigate" + text2);
					lookRepeat = 0.15f;
				}
			}
			currentNavDirection = ResolveMovementDirection("Direct Horizontal", "Direct Vertical");
			if (currentNavDirection != null)
			{
				ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Escape);
			}
		}
	}

	public string ResolveMovementDirection(string AxisX, string AxisY)
	{
		float axis = player.GetAxis(AxisX);
		float axis2 = player.GetAxis(AxisY);
		float num = 0.4f;
		if (axis > num && axis2 > num)
		{
			return "NE";
		}
		if (axis > num && axis2 < 0f - num)
		{
			return "SE";
		}
		if (axis < 0f - num && axis2 > num)
		{
			return "NW";
		}
		if (axis < 0f - num && axis2 < 0f - num)
		{
			return "SW";
		}
		if (axis < 0f - num)
		{
			return "W";
		}
		if (axis > num)
		{
			return "E";
		}
		if (axis2 < 0f - num)
		{
			return "S";
		}
		if (axis2 > num)
		{
			return "N";
		}
		return null;
	}

	public static bool IsOnUIContext()
	{
		return SynchronizationContext.Current == The.UiContext;
	}

	public static bool IsOnGameContext()
	{
		return SynchronizationContext.Current == The.GameContext;
	}

	public override void OnUpdate()
	{
		if (fadeToBlack)
		{
			if (originalBrightness == float.MinValue)
			{
				originalBrightness = UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness;
			}
			if (fadeToBlackStage == 0)
			{
				fadingToBlackTimer += Time.deltaTime;
				if (fadingToBlackTimer < 2f)
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = Mathf.Lerp(originalBrightness, -100f, fadingToBlackTimer / 2f);
				}
				else if (fadingToBlackTimer < 3f)
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = -100f;
					fadeToBlackStage = 1;
				}
			}
			if (fadeToBlackStage == 3)
			{
				fadingToBlackTimer += Time.deltaTime;
				if (fadingToBlackTimer < 5f)
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = Mathf.Lerp(-100f, originalBrightness, (fadingToBlackTimer - 3f) / 2f);
				}
				else
				{
					UnityEngine.GameObject.Find("Main Camera").GetComponent<CC_BrightnessContrastGamma>().brightness = originalBrightness;
					fadeToBlack = false;
					fadingToBlackTimer = 0f;
					originalBrightness = float.MinValue;
					gameQueue.queueTask(delegate
					{
						Thread.Sleep(2000);
						XRLCore.Core.Game.FinishQuestStep("Tomb of the Eaters", "Ascend the Tomb and Cross into Brightsheol");
					});
				}
			}
		}
		if (mouseDisable > 0f)
		{
			mouseDisable -= Time.deltaTime;
		}
		UpdateInput();
		uiQueue.executeTasks();
		if (lastFullscreen != Screen.fullScreen)
		{
			if (Screen.fullScreen != Options.DisplayFullscreen)
			{
				if (Options.HasOption("OptionDisplayFullscreen"))
				{
					Options.SetOption("OptionDisplayFullscreen", Screen.fullScreen ? "Yes" : "No");
				}
				else
				{
					Options.SetOption("OptionDisplayFullscreen", "Yes");
					Screen.fullScreen = true;
				}
			}
			lastFullscreen = Screen.fullScreen;
		}
		if (Spacefolding)
		{
			MainCamera.gameObject.GetComponent<CC_RadialBlur>().enabled = true;
			_spacefoldingT += Time.deltaTime;
			MainCamera.gameObject.GetComponent<CC_RadialBlur>().amount = 1f - Easing.BounceEaseInOut(_spacefoldingT / 2f);
			if (_spacefoldingT > 2f)
			{
				MainCamera.gameObject.GetComponent<CC_RadialBlur>().enabled = false;
				_spacefolding = false;
			}
		}
		if (Fuzzing)
		{
			CC_AnalogTV component = MainCamera.gameObject.GetComponent<CC_AnalogTV>();
			component.enabled = true;
			component.noiseIntensity = (0.5f - _fuzzingT) * 2f;
			_fuzzingT += Time.deltaTime;
			if ((double)_fuzzingT > 0.5)
			{
				component.enabled = Options.DisplayScanlines;
				component.noiseIntensity = 0.045f;
				Fuzzing = false;
			}
		}
		if (!Greyscaling && GreyscaleLevel > 0)
		{
			Greyscaling = true;
			MainCamera.gameObject.GetComponent<CC_Grayscale>().amount = 0f;
			MainCamera.gameObject.GetComponent<CC_Grayscale>().enabled = true;
		}
		else if (Greyscaling && GreyscaleLevel <= 0)
		{
			if (MainCamera.gameObject.GetComponent<CC_Grayscale>().amount > 0f)
			{
				MainCamera.gameObject.GetComponent<CC_Grayscale>().amount -= Time.deltaTime * 0.5f;
			}
			else
			{
				Greyscaling = false;
				MainCamera.gameObject.GetComponent<CC_Grayscale>().amount = 0f;
				MainCamera.gameObject.GetComponent<CC_Grayscale>().enabled = false;
			}
		}
		if (Greyscaling && MainCamera.gameObject.GetComponent<CC_Grayscale>().amount < 1f)
		{
			MainCamera.gameObject.GetComponent<CC_Grayscale>().amount += Time.deltaTime * 0.2f;
		}
		if (Options.GetOption("OptionDisableFullscreenWarpEffects") == "Yes")
		{
			if (Hallucinating)
			{
				Hallucinating = false;
				MainCamera.gameObject.GetComponent<CC_Wiggle>().enabled = false;
			}
		}
		else if (!Hallucinating && FungalVisionary.VisionLevel > 0)
		{
			Hallucinating = true;
			MainCamera.gameObject.GetComponent<CC_Wiggle>().enabled = true;
		}
		else if (Hallucinating && FungalVisionary.VisionLevel <= 0)
		{
			Hallucinating = false;
			MainCamera.gameObject.GetComponent<CC_Wiggle>().enabled = false;
		}
		if (Globals.RenderMode == RenderModeType.Text || Options.DisableImposters)
		{
			ImposterManager.enableImposters(bEnable: false);
		}
		_ = Hallucinating;
		if (!XRLCore.bThreadFocus)
		{
			if (Application.runInBackground)
			{
				SoundManager.Update();
			}
			else
			{
				Thread.Sleep(250);
			}
			return;
		}
		if (bFirstUpdate)
		{
			Instance = this;
			MainCamera = GameObjectFind("Main Camera");
			MainCanvas = GameObjectFind("Legacy Main Canvas").GetComponent<Canvas>();
			bFirstUpdate = false;
		}
		ClipboardHelper.UpdateFromMainThread();
		SoundManager.Update();
		if (Screen.width != LastWidth || Screen.height != LastHeight)
		{
			Sidebar.bOverlayUpdated = true;
			LastWidth = Screen.width;
			LastHeight = Screen.height;
			UpdatePreferredSidebarPosition();
		}
		if (_overlayOptionsUpdated)
		{
			_overlayOptionsUpdated = false;
			UpdateView();
		}
		RefreshLayout();
		if (!string.IsNullOrEmpty(Social.TweetThis))
		{
			string tweetThis = Social.TweetThis;
			Social.TweetThis = null;
			Application.OpenURL("http://www.twitter.com/intent/tweet?text=" + Uri.EscapeDataString(tweetThis) + "&via=cavesofqud&hashtags=cavesofqud");
		}
		_ = bEditorMode;
		if (!bInitStarted)
		{
			StartGameThread();
			bInitStarted = true;
		}
		if (TV.noiseIntensity != TargetIntensity)
		{
			TV.noiseIntensity = Mathf.MoveTowards(TV.noiseIntensity, TargetIntensity, Time.deltaTime * 1f);
		}
		if (!bInitComplete)
		{
			return;
		}
		if ((XRLCore.bStarted && XRLCore.CoreThread.ThreadState == ThreadState.Stopped) || !XRLCore.bStarted)
		{
			Debug.Log("Exiting main thread due to stopped game thread." + XRLCore.CoreThread.ThreadState);
			OnDestroy();
			Application.Quit();
		}
		if (MetricsManager.ManagerObject == null && Globals.EnableMetrics)
		{
			MetricsManager.Init();
		}
		MetricsManager.Update();
		if (TextConsole.BufferUpdated)
		{
			lock (TextConsole.BufferCS)
			{
				if (_ActiveGameView != CurrentGameView || bViewUpdated)
				{
					ControlManager.ResetInput();
					UpdateView();
					bViewUpdated = false;
				}
				if (!FadeSplash)
				{
					UnityEngine.GameObject gameObject = UnityEngine.GameObject.Find("Splash");
					if (gameObject != null)
					{
						FadeSplash = true;
						LeanTween.alpha(gameObject, 0f, 2f);
						gameObject.AddComponent<Temporary>().Duration = 2f;
						gameObject.GetComponent<Temporary>().BeforeDestroy = delegate
						{
							GameObjectFind("Splashscreen").Destroy();
						};
					}
				}
				while (TextConsole.bufferExtras.Count > 0)
				{
					ProcessBufferExtra(TextConsole.bufferExtras.Dequeue());
				}
				ImposterManager.Update();
				int num = 0;
				for (int i = 0; i < 80; i++)
				{
					for (int j = 0; j < 25; j++)
					{
						if (!TextConsole.CurrentBuffer.Buffer[i, j].HFlip && !TextConsole.CurrentBuffer.Buffer[i, j].VFlip)
						{
							if (ConsoleCharacter[i, j].transform.localScale != Vector3.one)
							{
								ConsoleCharacter[i, j].transform.localScale = Vector3.one;
								BoxCollider component2 = ConsoleCharacter[i, j].GetComponent<BoxCollider>();
								component2.size = new Vector3(Math.Abs(component2.size.x), Math.Abs(component2.size.y), Math.Abs(component2.size.z));
							}
						}
						else if (TextConsole.CurrentBuffer.Buffer[i, j].HFlip && TextConsole.CurrentBuffer.Buffer[i, j].VFlip)
						{
							if (ConsoleCharacter[i, j].transform.localScale != -Vector3.one)
							{
								ConsoleCharacter[i, j].transform.localScale = -Vector3.one;
								BoxCollider component3 = ConsoleCharacter[i, j].GetComponent<BoxCollider>();
								component3.size = new Vector3(0f - Math.Abs(component3.size.x), 0f - Math.Abs(component3.size.y), 0f - Math.Abs(component3.size.z));
							}
						}
						else if (TextConsole.CurrentBuffer.Buffer[i, j].HFlip)
						{
							if (ConsoleCharacter[i, j].transform.localScale != new Vector3(-1f, 1f, 1f))
							{
								ConsoleCharacter[i, j].transform.localScale = new Vector3(-1f, 1f, 1f);
								BoxCollider component4 = ConsoleCharacter[i, j].GetComponent<BoxCollider>();
								component4.size = new Vector3(0f - Math.Abs(component4.size.x), Math.Abs(component4.size.y), Math.Abs(component4.size.z));
							}
						}
						else if (TextConsole.CurrentBuffer.Buffer[i, j].VFlip && ConsoleCharacter[i, j].transform.localScale != new Vector3(1f, -1f, 1f))
						{
							ConsoleCharacter[i, j].transform.localScale = new Vector3(1f, -1f, 1f);
							BoxCollider component5 = ConsoleCharacter[i, j].GetComponent<BoxCollider>();
							component5.size = new Vector3(Math.Abs(component5.size.x), 0f - Math.Abs(component5.size.y), Math.Abs(component5.size.z));
						}
						if (TextConsole.CurrentBuffer.Buffer[i, j].Char == '\0')
						{
							string tile = TextConsole.CurrentBuffer.Buffer[i, j].Tile;
							Color foreground = TextConsole.CurrentBuffer.Buffer[i, j].Foreground;
							Color background = TextConsole.CurrentBuffer.Buffer[i, j].Background;
							Color detail = TextConsole.CurrentBuffer.Buffer[i, j].Detail;
							if (!(CurrentTile[i, j] != tile) && ColorMatch(ConsoleCharacter[i, j].color, foreground) && ColorMatch(ConsoleCharacter[i, j].backcolor, background) && ColorMatch(ConsoleCharacter[i, j].detailcolor, detail))
							{
								continue;
							}
							if (CurrentTile[i, j] != tile)
							{
								exTextureInfo textureInfo = SpriteManager.GetTextureInfo(tile);
								if (textureInfo == null)
								{
									Debug.LogError("Invalid TextureID: " + tile);
									CurrentTile[i, j] = tile;
									ConsoleCharacter[i, j].textureInfo = null;
								}
								else
								{
									CurrentTile[i, j] = tile;
									ConsoleCharacter[i, j].textureInfo = textureInfo;
									if (textureInfo.ShaderMode != CurrentShadermode[i, j])
									{
										ConsoleCharacter[i, j].shader = SpriteManager.GetShaderMode(textureInfo.ShaderMode);
										CurrentShadermode[i, j] = textureInfo.ShaderMode;
									}
								}
							}
							num++;
							ConsoleCharacter[i, j].color = foreground;
							ConsoleCharacter[i, j].detailcolor = detail;
							ConsoleCharacter[i, j].backcolor = background;
						}
						else
						{
							char c = TextConsole.CurrentBuffer.Buffer[i, j].Char;
							if (c < '\0' || c > 'ÿ')
							{
								c = ' ';
							}
							exTextureInfo exTextureInfo2 = CharInfos[(uint)c];
							Color foreground2 = TextConsole.CurrentBuffer.Buffer[i, j].Foreground;
							Color background2 = TextConsole.CurrentBuffer.Buffer[i, j].Background;
							if (exTextureInfo2 != null && exTextureInfo2.ShaderMode != CurrentShadermode[i, j])
							{
								ConsoleCharacter[i, j].shader = SpriteManager.GetShaderMode(exTextureInfo2.ShaderMode);
								CurrentShadermode[i, j] = exTextureInfo2.ShaderMode;
							}
							if (ConsoleCharacter[i, j].textureInfo != exTextureInfo2 || !ColorMatch(ConsoleCharacter[i, j].backcolor, foreground2) || !ColorMatch(ConsoleCharacter[i, j].color, background2))
							{
								num++;
								CurrentTile[i, j] = null;
								ConsoleCharacter[i, j].textureInfo = exTextureInfo2;
								ConsoleCharacter[i, j].backcolor = foreground2;
								ConsoleCharacter[i, j].detailcolor = foreground2;
								ConsoleCharacter[i, j].color = background2;
							}
						}
					}
				}
				ImposterManager.hideTextCoveredImposters();
				UpdateSelectedAbility();
				TextConsole.BufferUpdated = false;
			}
		}
		if (CurrentGameView == Options.StageViewID || CurrentGameView == "Sifrah")
		{
			if (TargetZoomFactor <= 1f)
			{
				if (OverlayRoot.activeInHierarchy)
				{
					OverlayRoot.SetActive(value: false);
				}
			}
			else if (!OverlayRoot.activeInHierarchy)
			{
				OverlayRoot.SetActive(value: true);
			}
			CombatJuiceManager.update();
		}
		else
		{
			if (OverlayRoot.activeInHierarchy)
			{
				OverlayRoot.SetActive(value: false);
			}
			CombatJuiceManager.pause();
		}
		SteamManager.Update();
		AchievementManager.Update();
		if (MainCamera != null && MainCameraLetterbox.CurrentPosition != lastCameraPosition)
		{
			lastCameraPosition = MainCameraLetterbox.CurrentPosition;
			UpdatePreferredSidebarPosition();
		}
	}

	public bool StringBuilderContentsEquals(StringBuilder sb1, StringBuilder sb2, int maxcheck = 800)
	{
		if (sb1.Length != sb2.Length)
		{
			return false;
		}
		for (int i = 0; i < sb1.Length && i < maxcheck; i++)
		{
			if (sb1[i] != sb2[i])
			{
				return false;
			}
		}
		return true;
	}
}
