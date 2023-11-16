using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.UI;
using UnityEngine;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.Core;

[Serializable]
public class ActionManager
{
	[NonSerialized]
	public static bool SkipPlayerTurn = false;

	[NonSerialized]
	public CleanQueue<XRL.World.GameObject> ActionQueue = new CleanQueue<XRL.World.GameObject>();

	[NonSerialized]
	public List<XRL.World.GameObject> RemovedObjects = new List<XRL.World.GameObject>(8);

	[NonSerialized]
	public List<XRL.World.GameObject> TurnTickObjects = new List<XRL.World.GameObject>(32);

	[NonSerialized]
	public List<XRL.World.GameObject> TenTurnTickObjects = new List<XRL.World.GameObject>(32);

	[NonSerialized]
	public List<XRL.World.GameObject> HundredTurnTickObjects = new List<XRL.World.GameObject>(32);

	[NonSerialized]
	public bool ProcessingTurnTick;

	[NonSerialized]
	public Dictionary<string, int> ZoneSuspendFailures = new Dictionary<string, int>();

	public List<CommandQueueEntry> Commands = new List<CommandQueueEntry>(3);

	public bool RestingUntilHealed;

	public int RestingUntilHealedCount;

	[NonSerialized]
	private List<XRL.World.GameObject> EndTurnList = new List<XRL.World.GameObject>(32);

	[NonSerialized]
	private InfluenceMap AutoexploreMap = new InfluenceMap(80, 25);

	[NonSerialized]
	private Color32[] minimapScratch;

	[NonSerialized]
	private static int SegCount = 0;

	[NonSerialized]
	private static Stopwatch AutomoveTimer = new Stopwatch();

	public ActionManager()
	{
		ActionQueue.Enqueue(null);
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.WriteObject(this);
		Writer.Write(ActionQueue.Count);
		foreach (XRL.World.GameObject item in ActionQueue.Items)
		{
			Writer.WriteGameObject(item);
		}
		Writer.Write(RemovedObjects.Count);
		foreach (XRL.World.GameObject removedObject in RemovedObjects)
		{
			Writer.WriteGameObject(removedObject);
		}
	}

	public static ActionManager Load(SerializationReader Reader)
	{
		ActionManager actionManager = (ActionManager)Reader.ReadObject();
		actionManager.ActionQueue = new CleanQueue<XRL.World.GameObject>();
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			actionManager.ActionQueue.Enqueue(Reader.ReadGameObject("action queue"));
		}
		if (actionManager.ActionQueue.Items.Any((XRL.World.GameObject o) => o?.IsPlayer() ?? false))
		{
			while (actionManager.ActionQueue.Peek() == null || !actionManager.ActionQueue.Peek().IsPlayer())
			{
				XRL.World.GameObject gameObject = actionManager.ActionQueue.Dequeue();
				actionManager.ActionQueue.Enqueue(gameObject);
				if (gameObject != null && gameObject.IsPlayer() && gameObject.Energy != null && gameObject.Energy.Value < 1000)
				{
					gameObject.Energy.BaseValue = 1000;
				}
			}
		}
		int num2 = Reader.ReadInt32();
		actionManager.RemovedObjects = new List<XRL.World.GameObject>(num2);
		for (int j = 0; j < num2; j++)
		{
			actionManager.RemovedObjects.Add(Reader.ReadGameObject("removed queue"));
		}
		actionManager.EndTurnList = new List<XRL.World.GameObject>();
		actionManager.TurnTickObjects = new List<XRL.World.GameObject>(32);
		actionManager.TenTurnTickObjects = new List<XRL.World.GameObject>(32);
		actionManager.HundredTurnTickObjects = new List<XRL.World.GameObject>(32);
		actionManager.ZoneSuspendFailures = new Dictionary<string, int>();
		actionManager.AutoexploreMap = new InfluenceMap(80, 25);
		return actionManager;
	}

	public bool HasAction(string Action, object Target)
	{
		foreach (CommandQueueEntry command in Commands)
		{
			if (command.Action == Action && command.Target == Target)
			{
				return true;
			}
		}
		return false;
	}

	public void EnqueAction(string Action, object Target, int Delay)
	{
		CommandQueueEntry commandQueueEntry = new CommandQueueEntry();
		commandQueueEntry.Action = Action;
		commandQueueEntry.Target = Target;
		commandQueueEntry.SegmentDelay = Delay;
		Commands.Add(commandQueueEntry);
	}

	public int DequeAction(string Action, object Target)
	{
		int num = 0;
		for (int i = 0; i < Commands.Count; i++)
		{
			if (Commands[i].Action == Action && Commands[i].Target == Target)
			{
				Commands.Remove(Commands[i]);
				num++;
				i--;
			}
		}
		return num;
	}

	public void FlushRemovedObjects()
	{
		XRL.World.GameObject gameObject = ActionQueue.Dequeue();
		XRL.World.GameObject gameObject2 = gameObject;
		while (gameObject != null)
		{
			if (!RemovedObjects.CleanContains(gameObject))
			{
				ActionQueue.Enqueue(gameObject);
			}
			gameObject = ActionQueue.Dequeue();
			if (gameObject == gameObject2)
			{
				break;
			}
		}
		ActionQueue.Enqueue(null);
		RemovedObjects.Clear();
	}

	public void AddActiveObject(XRL.World.GameObject GO)
	{
		if (!XRL.World.GameObject.validate(GO))
		{
			MetricsManager.LogError("trying to add invalid object to action queue: " + GO.DebugName);
			return;
		}
		if (GO.Energy == null)
		{
			MetricsManager.LogError("trying to add object with no Energy stat to action queue: " + GO.DebugName);
			return;
		}
		if (GO.IsInGraveyard())
		{
			MetricsManager.LogError("trying to add object in graveyard to action queue: " + GO.DebugName);
			return;
		}
		Zone currentZone = GO.CurrentZone;
		if (currentZone == null || currentZone == The.ZoneManager.ActiveZone)
		{
			if (!ActionQueue.Contains(GO))
			{
				ActionQueue.Enqueue(GO);
			}
			if (RemovedObjects.CleanContains(GO))
			{
				RemovedObjects.Remove(GO);
			}
		}
	}

	public void RemoveActiveObject(XRL.World.GameObject GO)
	{
		if (ActionQueue.Contains(GO) && !RemovedObjects.CleanContains(GO))
		{
			RemovedObjects.Add(GO);
		}
	}

	public void UpdateMinimap()
	{
		if (minimapScratch == null)
		{
			minimapScratch = new Color32[4000];
		}
		if (!Options.OverlayMinimap)
		{
			return;
		}
		try
		{
			Zone zone = The.Player?.CurrentZone;
			if (zone == null)
			{
				return;
			}
			for (int i = 0; i < zone.Height; i++)
			{
				for (int j = 0; j < zone.Width; j++)
				{
					Color32 minimapColor = zone.GetCell(j, i).GetMinimapColor();
					int num = j + (24 - i) * 2 * 80;
					int num2 = j + ((24 - i) * 2 + 1) * 80;
					minimapScratch[num] = minimapColor;
					minimapScratch[num2] = minimapColor;
				}
			}
			lock (GameManager.minimapColors)
			{
				for (int k = 0; k < 4000; k++)
				{
					GameManager.minimapColors[k] = minimapScratch[k];
				}
			}
			GameManager.Instance.uiQueue.queueSingletonTask("updateMinimap", delegate
			{
				GameManager.Instance.UpdateMinimap();
			});
		}
		catch (Exception x)
		{
			MetricsManager.LogError("minimap update", x);
		}
	}

	private void SyncSingleTurnRecipients()
	{
		FlushSingleTurnRecipients();
		foreach (KeyValuePair<string, Zone> cachedZone in The.ZoneManager.CachedZones)
		{
			Zone value = cachedZone.Value;
			if (value.IsActive())
			{
				for (int i = 0; i < value.Height; i++)
				{
					for (int j = 0; j < value.Width; j++)
					{
						Cell cell = value.Map[j][i];
						int k = 0;
						for (int count = cell.Objects.Count; k < count; k++)
						{
							XRL.World.GameObject gameObject = cell.Objects[k];
							if (gameObject.HasRegisteredEventDirect("EndTurn") || gameObject.WantEvent(EndTurnEvent.ID, EndTurnEvent.CascadeLevel))
							{
								EndTurnList.Add(gameObject);
							}
							if (gameObject.WantTurnTick())
							{
								TurnTickObjects.Add(gameObject);
							}
						}
					}
				}
				continue;
			}
			for (int l = 0; l < value.Height; l++)
			{
				for (int m = 0; m < value.Width; m++)
				{
					Cell cell2 = value.Map[m][l];
					if (cell2.IsReallyExplored())
					{
						int n = 0;
						for (int count2 = cell2.Objects.Count; n < count2; n++)
						{
							XRL.World.GameObject gameObject2 = cell2.Objects[n];
							if (gameObject2.HasRegisteredEventDirect("EndTurn") || gameObject2.WantEvent(EndTurnEvent.ID, EndTurnEvent.CascadeLevel))
							{
								EndTurnList.Add(gameObject2);
							}
							if (gameObject2.WantTurnTick())
							{
								TurnTickObjects.Add(gameObject2);
							}
						}
						continue;
					}
					int num = 0;
					for (int count3 = cell2.Objects.Count; num < count3; num++)
					{
						XRL.World.GameObject gameObject3 = cell2.Objects[num];
						if (gameObject3.WantTurnTick())
						{
							TurnTickObjects.Add(gameObject3);
						}
					}
				}
			}
		}
	}

	public void FlushSingleTurnRecipients()
	{
		EndTurnList.Clear();
		TurnTickObjects.Clear();
	}

	public void SyncTurnTickRecipients()
	{
		FlushTurnTickRecipients();
		foreach (KeyValuePair<string, Zone> cachedZone in The.ZoneManager.CachedZones)
		{
			foreach (Cell cell in cachedZone.Value.GetCells())
			{
				int i = 0;
				for (int count = cell.Objects.Count; i < count; i++)
				{
					XRL.World.GameObject gameObject = cell.Objects[i];
					if (gameObject.WantTurnTick())
					{
						TurnTickObjects.Add(gameObject);
					}
					if (gameObject.WantTenTurnTick())
					{
						TenTurnTickObjects.Add(gameObject);
					}
					if (gameObject.WantHundredTurnTick())
					{
						HundredTurnTickObjects.Add(gameObject);
					}
				}
			}
		}
	}

	public void FlushTurnTickRecipients()
	{
		TurnTickObjects.Clear();
		TenTurnTickObjects.Clear();
		HundredTurnTickObjects.Clear();
	}

	public void ProcessTurnTick()
	{
		ProcessingTurnTick = true;
		long currentTurn = XRLCore.CurrentTurn;
		for (int i = 0; i < TurnTickObjects.Count; i++)
		{
			try
			{
				XRL.World.GameObject gameObject = TurnTickObjects[i];
				gameObject.TurnTick(currentTurn);
				gameObject.CleanEffects();
			}
			catch (Exception ex)
			{
				XRLCore.LogError("Exception during TurnTick it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
			}
		}
		ProcessingTurnTick = false;
	}

	public void ProcessTurnTickHierarchical()
	{
		ProcessingTurnTick = true;
		long currentTurn = XRLCore.CurrentTurn;
		for (int i = 0; i < TurnTickObjects.Count; i++)
		{
			try
			{
				XRL.World.GameObject gameObject = TurnTickObjects[i];
				if (!TenTurnTickObjects.Contains(gameObject) && !HundredTurnTickObjects.Contains(gameObject))
				{
					gameObject.TurnTick(currentTurn);
					gameObject.CleanEffects();
				}
			}
			catch (Exception ex)
			{
				XRLCore.LogError("Exception during TurnTick it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
			}
		}
		ProcessingTurnTick = false;
	}

	public void ProcessTenTurnTick()
	{
		ProcessingTurnTick = true;
		long currentTurn = XRLCore.CurrentTurn;
		for (int i = 0; i < TenTurnTickObjects.Count; i++)
		{
			try
			{
				XRL.World.GameObject gameObject = TenTurnTickObjects[i];
				gameObject.TenTurnTick(currentTurn);
				gameObject.CleanEffects();
			}
			catch (Exception ex)
			{
				XRLCore.LogError("Exception during TurnTick it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
			}
		}
		ProcessingTurnTick = false;
	}

	public void ProcessTenTurnTickHierarchical()
	{
		ProcessingTurnTick = true;
		long currentTurn = XRLCore.CurrentTurn;
		for (int i = 0; i < TenTurnTickObjects.Count; i++)
		{
			try
			{
				XRL.World.GameObject gameObject = TenTurnTickObjects[i];
				if (!HundredTurnTickObjects.Contains(gameObject))
				{
					gameObject.TenTurnTick(currentTurn);
					gameObject.CleanEffects();
				}
			}
			catch (Exception ex)
			{
				XRLCore.LogError("Exception during TurnTick it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
			}
		}
		ProcessingTurnTick = false;
	}

	public void ProcessTenTurnTickCatchup(int Turns)
	{
		int num = Turns % 10;
		for (int i = 0; i < num; i++)
		{
			ProcessingTurnTick = true;
			long currentTurn = XRLCore.CurrentTurn;
			for (int j = 0; j < TenTurnTickObjects.Count; j++)
			{
				try
				{
					XRL.World.GameObject gameObject = TenTurnTickObjects[j];
					if (TurnTickObjects.Contains(gameObject) && !HundredTurnTickObjects.Contains(gameObject))
					{
						gameObject.TurnTick(currentTurn);
						gameObject.CleanEffects();
					}
				}
				catch (Exception ex)
				{
					XRLCore.LogError("Exception during TurnTick it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
				}
			}
			ProcessingTurnTick = false;
		}
	}

	public void ProcessHundredTurnTick()
	{
		ProcessingTurnTick = true;
		long currentTurn = XRLCore.CurrentTurn;
		for (int i = 0; i < HundredTurnTickObjects.Count; i++)
		{
			try
			{
				XRL.World.GameObject gameObject = HundredTurnTickObjects[i];
				gameObject.HundredTurnTick(currentTurn);
				gameObject.CleanEffects();
			}
			catch (Exception ex)
			{
				XRLCore.LogError("Exception during TurnTick it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
			}
		}
		ProcessingTurnTick = false;
	}

	public void ProcessHundredTurnTickHierarchical()
	{
		ProcessHundredTurnTick();
	}

	public void ProcessHundredTurnTickCatchup(int Turns)
	{
		int num = Turns % 100;
		for (int i = 0; i < num; i++)
		{
			ProcessingTurnTick = true;
			long currentTurn = XRLCore.CurrentTurn;
			for (int j = 0; j < HundredTurnTickObjects.Count; j++)
			{
				try
				{
					XRL.World.GameObject gameObject = HundredTurnTickObjects[j];
					if (TurnTickObjects.Contains(gameObject))
					{
						gameObject.TurnTick(currentTurn);
						gameObject.CleanEffects();
					}
				}
				catch (Exception ex)
				{
					XRLCore.LogError("Exception during TurnTick it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
				}
			}
			ProcessingTurnTick = false;
		}
	}

	private XRL.World.GameObject FindAutoexploreObjectToOpen(XRL.World.GameObject Player, Cell C)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		if (C.Objects.Count > 0)
		{
			if (!flag2)
			{
				flag3 = Options.AutoexploreChests;
				flag2 = true;
				if (flag2 && flag4 && !flag3 && !flag5)
				{
					return null;
				}
			}
			if (flag3)
			{
				int i = 0;
				for (int count = C.Objects.Count; i < count; i++)
				{
					if (!C.Objects[i].ShouldAutoexploreAsChest())
					{
						continue;
					}
					if (!flag)
					{
						if (AutoAct.ShouldHostilesInterrupt())
						{
							return null;
						}
						flag = true;
					}
					return C.Objects[i];
				}
			}
			if (!flag4)
			{
				flag5 = Options.AutoexploreBookshelves;
				flag4 = true;
				if (flag2 && flag4 && !flag3 && !flag5)
				{
					return null;
				}
			}
			if (flag5)
			{
				int j = 0;
				for (int count2 = C.Objects.Count; j < count2; j++)
				{
					if (!C.Objects[j].ShouldAutoexploreAsShelf())
					{
						continue;
					}
					if (!flag)
					{
						if (AutoAct.ShouldHostilesInterrupt())
						{
							return null;
						}
						flag = true;
					}
					return C.Objects[j];
				}
			}
		}
		List<Cell> localAdjacentCells = C.GetLocalAdjacentCells();
		int k = 0;
		for (int count3 = localAdjacentCells.Count; k < count3; k++)
		{
			Cell cell = localAdjacentCells[k];
			List<XRL.World.GameObject> list;
			if (cell.IsSolid())
			{
				list = XRL.World.Event.NewGameObjectList();
				int l = 0;
				for (int count4 = cell.Objects.Count; l < count4; l++)
				{
					XRL.World.GameObject gameObject = cell.Objects[l];
					if (gameObject.pPhysics != null && gameObject.pPhysics.Solid)
					{
						list.Add(gameObject);
					}
				}
			}
			else
			{
				list = cell.Objects;
			}
			if (list.Count <= 0)
			{
				continue;
			}
			if (!flag2)
			{
				flag3 = Options.AutoexploreChests;
				flag2 = true;
				if (flag2 && flag4 && !flag3 && !flag5)
				{
					return null;
				}
			}
			if (flag3)
			{
				int m = 0;
				for (int count5 = list.Count; m < count5; m++)
				{
					if (!list[m].ShouldAutoexploreAsChest())
					{
						continue;
					}
					if (!flag)
					{
						if (AutoAct.ShouldHostilesInterrupt())
						{
							return null;
						}
						flag = true;
					}
					return list[m];
				}
			}
			if (!flag4)
			{
				flag5 = Options.AutoexploreBookshelves;
				flag4 = true;
				if (flag2 && flag4 && !flag3 && !flag5)
				{
					return null;
				}
			}
			if (!flag5)
			{
				continue;
			}
			int n = 0;
			for (int count6 = list.Count; n < count6; n++)
			{
				if (!list[n].ShouldAutoexploreAsShelf())
				{
					continue;
				}
				if (!flag)
				{
					if (AutoAct.ShouldHostilesInterrupt())
					{
						return null;
					}
					flag = true;
				}
				return list[n];
			}
		}
		return null;
	}

	private XRL.World.GameObject FindAutoexploreObjectToGet(XRL.World.GameObject Player, Cell C)
	{
		List<Cell> localAdjacentCells = C.GetLocalAdjacentCells();
		int i = 0;
		for (int count = localAdjacentCells.Count; i < count; i++)
		{
			Cell cell = localAdjacentCells[i];
			bool flag = cell.IsSolid();
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				XRL.World.GameObject gameObject = cell.Objects[j];
				if ((!flag || (gameObject.pPhysics != null && gameObject.pPhysics.Solid)) && gameObject.ShouldAutoget() && !gameObject.IsAutogetLiquid())
				{
					if (!AutoAct.ShouldHostilesInterrupt())
					{
						return gameObject;
					}
					return null;
				}
			}
		}
		return null;
	}

	private XRL.World.GameObject FindAutoexploreObjectToProcess(XRL.World.GameObject Player, Cell C)
	{
		bool flag = false;
		List<Cell> localAdjacentCells = C.GetLocalAdjacentCells();
		bool flag2 = C.IsSolid();
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			XRL.World.GameObject gameObject = C.Objects[i];
			if ((flag2 && (gameObject.pPhysics == null || !gameObject.pPhysics.Solid)) || !AutoexploreObjectEvent.CheckForAdjacent(Player, gameObject))
			{
				continue;
			}
			if (!flag)
			{
				if (AutoAct.ShouldHostilesInterrupt())
				{
					return null;
				}
				flag = true;
			}
			return gameObject;
		}
		int j = 0;
		for (int count2 = localAdjacentCells.Count; j < count2; j++)
		{
			Cell cell = localAdjacentCells[j];
			flag2 = cell.IsSolid();
			int k = 0;
			for (int count3 = cell.Objects.Count; k < count3; k++)
			{
				XRL.World.GameObject gameObject2 = cell.Objects[k];
				if ((flag2 && (gameObject2.pPhysics == null || !gameObject2.pPhysics.Solid)) || !AutoexploreObjectEvent.CheckForAdjacent(Player, gameObject2))
				{
					continue;
				}
				if (!flag)
				{
					if (AutoAct.ShouldHostilesInterrupt())
					{
						return null;
					}
					flag = true;
				}
				return gameObject2;
			}
		}
		return null;
	}

	public void RunSegment(bool bUnityDebug = false)
	{
		try
		{
			XRLGame game = The.Game;
			for (int i = 0; i < game.Systems.Count; i++)
			{
				game.Systems[i].BeginSegment();
			}
			GameManager.Instance.CurrentGameView = Options.StageViewID;
			XRL.World.Event.ResetPool();
			The.Core.AllowWorldMapParticles = false;
			XRL.World.GameObject obj = null;
			XRL.World.GameObject LastDoor = null;
			if (game.Player.Body != null && !ActionQueue.Contains(game.Player.Body))
			{
				ActionQueue.Enqueue(game.Player.Body);
			}
			XRL.World.GameObject gameObject;
			bool turnWaitFlag;
			while ((gameObject = ActionQueue.Dequeue()) != null)
			{
				if (!game.Running)
				{
					return;
				}
				Cell currentCell = gameObject.CurrentCell;
				if (RemovedObjects.Count > 0 && RemovedObjects.CleanContains(gameObject))
				{
					if (!ActionQueue.Contains(gameObject))
					{
						RemovedObjects.Remove(gameObject);
					}
					continue;
				}
				if (!gameObject.IsValid())
				{
					if (gameObject.IsPlayer())
					{
						MessageQueue.AddPlayerMessage("Action queue inconsistency: Removing invalid player object!?", 'K');
					}
					else
					{
						MessageQueue.AddPlayerMessage("Action queue inconsistency: Removing invalid object", 'K');
					}
					continue;
				}
				if (currentCell == null)
				{
					if (gameObject.IsPlayer())
					{
						MessageQueue.AddPlayerMessage("Action queue inconsistency: Well, the player managed to end a segment with no current cell and that's not ideal so I'm going to return the player to a cell on the current map.", 'K');
						The.ZoneManager?.ActiveZone?.GetSpawnCell()?.AddObject(gameObject);
					}
					else
					{
						MessageQueue.AddPlayerMessage("Action queue inconsistency: Removing object with no current cell " + gameObject.DebugName, 'K');
					}
					continue;
				}
				if (currentCell.IsGraveyard())
				{
					MessageQueue.AddPlayerMessage("Action queue inconsistency: Removing object in graveyard " + gameObject.DebugName, 'K');
					continue;
				}
				if (currentCell.ParentZone == null)
				{
					MessageQueue.AddPlayerMessage("Action queue inconsistency: Removing zoneless cell object " + gameObject.DebugName, 'K');
					continue;
				}
				if (!The.ZoneManager.CachedZonesContains(currentCell.ParentZone.ZoneID))
				{
					MessageQueue.AddPlayerMessage("Action queue inconsistency: Removing noncached zone object " + gameObject.DebugName, 'K');
					continue;
				}
				if (gameObject.Energy == null)
				{
					MessageQueue.AddPlayerMessage("Action queue inconsistency: Removing energyless object " + gameObject.DebugName, 'K');
					continue;
				}
				game.Segments++;
				ActionQueue.Enqueue(gameObject);
				gameObject.Energy.BaseValue += gameObject.Speed;
				if (gameObject.Energy.Value >= 1000)
				{
					if (!EarlyBeforeBeginTakeActionEvent.Check(gameObject) || !BeforeBeginTakeActionEvent.Check(gameObject) || !BeginTakeActionEvent.Check(gameObject))
					{
						gameObject.Energy.BaseValue = 0;
					}
					if (gameObject.IsPlayer() && Options.LogTurnSeparator && gameObject.ArePerceptibleHostilesNearby())
					{
						game.Player.Messages.Add("[--turn start--]");
					}
				}
				bool flag = false;
				int num = 0;
				while (gameObject.Energy.Value >= 1000 && game.Running)
				{
					game.ActionTicks++;
					if (GameManager.runWholeTurnOnUIThread && gameObject != null && gameObject.IsPlayer())
					{
						GameManager.runWholeTurnOnUIThread = false;
						return;
					}
					XRL.World.Event.ResetPool();
					flag = true;
					num++;
					try
					{
						if (gameObject.IsPlayer())
						{
							UpdateMinimap();
						}
						gameObject.CleanEffects();
						_ = gameObject.Energy.Value;
						if (!BeforeTakeActionEvent.Check(gameObject))
						{
							flag = true;
							if (gameObject.Energy != null)
							{
								gameObject.Energy.BaseValue = 0;
							}
						}
						else if (CommandTakeActionEvent.Check(gameObject))
						{
							gameObject.CleanEffects();
							if (gameObject.IsPlayer() && AutoAct.IsInterruptable() && AutoAct.Setting[0] != 'M')
							{
								AutoAct.CheckHostileInterrupt();
							}
							if (gameObject.Energy.Value >= 1000 && gameObject.IsPlayer())
							{
								currentCell = gameObject.CurrentCell;
								if (currentCell != null && currentCell.ParentZone != null && !currentCell.ParentZone.IsActive())
								{
									currentCell.ParentZone.SetActive();
								}
								if (AutoAct.IsActive())
								{
									Sidebar.UpdateState();
									XRLCore.CallBeginPlayerTurnCallbacks();
									The.Core.RenderBase(UpdateSidebar: false);
									if (Keyboard.kbhit() && !bUnityDebug)
									{
										AutoAct.Interrupt();
										Keyboard.getch();
									}
									else if (currentCell.X == 0 && (AutoAct.Setting == "W" || AutoAct.Setting == "NW" || AutoAct.Setting == "SW"))
									{
										AutoAct.Interrupt();
									}
									else if (currentCell.X == 79 && (AutoAct.Setting == "E" || AutoAct.Setting == "NE" || AutoAct.Setting == "SE"))
									{
										AutoAct.Interrupt();
									}
									else if (currentCell.Y == 0 && (AutoAct.Setting == "N" || AutoAct.Setting == "NW" || AutoAct.Setting == "NE"))
									{
										AutoAct.Interrupt();
									}
									else if (currentCell.Y == 24 && (AutoAct.Setting == "S" || AutoAct.Setting == "SW" || AutoAct.Setting == "SE"))
									{
										AutoAct.Interrupt();
									}
								}
								if (RestingUntilHealed && !AutoAct.IsActive())
								{
									SingletonWindowBase<LoadingStatusWindow>.instance.SetLoadingStatus(null);
									RestingUntilHealed = false;
								}
								if (AutoAct.IsActive())
								{
									if (AutoAct.IsRateLimited())
									{
										AutomoveTimer.Reset();
										AutomoveTimer.Start();
									}
									string setting = AutoAct.Setting;
									char c = setting[0];
									XRL.World.GameObject obj2;
									bool flag2;
									XRL.World.GameObject gameObject2;
									switch (c)
									{
									case 'M':
									case 'P':
									case 'U':
									case 'X':
									{
										string text = setting.Substring(1);
										int num17 = 0;
										if (c == 'P')
										{
											int num18 = text.IndexOf(':');
											if (num18 != -1)
											{
												num17 = Convert.ToInt32(text.Substring(0, num18));
												text = text.Substring(num18 + 1);
											}
										}
										XRL.World.GameObject gameObject5 = null;
										int num19;
										int num20;
										if (text.Contains(","))
										{
											string[] array = text.Split(',');
											num19 = Convert.ToInt32(array[0]);
											num20 = Convert.ToInt32(array[1]);
										}
										else
										{
											gameObject5 = XRL.World.GameObject.findById(text);
											Cell cell3 = gameObject5?.CurrentCell;
											if (cell3 == null || cell3.ParentZone != currentCell.ParentZone)
											{
												AutoAct.Interrupt();
												break;
											}
											num19 = cell3.X;
											num20 = cell3.Y;
										}
										if (c == 'U' && currentCell.ParentZone.GetCell(num19, num20).DistanceTo(gameObject) <= 1)
										{
											Cell cell4 = currentCell.ParentZone.GetCell(num19, num20);
											AutoAct.Interrupt();
											The.Core.AttemptSmartUse(cell4);
											break;
										}
										if (currentCell.X == num19 && currentCell.Y == num20)
										{
											AutoAct.Interrupt();
											break;
										}
										if (c == 'P' && currentCell.ParentZone.GetCell(num19, num20).DistanceTo(gameObject) <= num17)
										{
											AutoAct.Interrupt();
											break;
										}
										FindPath findPath3 = new FindPath(currentCell.ParentZone, currentCell.X, currentCell.Y, currentCell.ParentZone, num19, num20, PathGlobal: false, PathUnlimited: true, gameObject, The.Core.PlayerAvoid, ExploredOnly: true);
										if (!findPath3.bFound || findPath3.Steps.Count == 0)
										{
											if (gameObject5 != null)
											{
												Popup.Show("You cannot find a path to " + gameObject5.t() + ".");
												AutoAct.Interrupt(null, null, gameObject5);
											}
											else
											{
												Popup.Show("You cannot find a path to your destination.");
												AutoAct.Interrupt(null, currentCell.ParentZone.GetCell(num19, num20));
											}
										}
										else
										{
											The.Core.PlayerAvoid.Enqueue(new XRLCore.SortPoint(currentCell.X, currentCell.Y));
											AutoAct.TryToMove(gameObject, currentCell, ref LastDoor, findPath3.Steps[1], findPath3.Directions[0], AllowDigging: true, OpenDoors: true, Peaceful: true, c == 'M' || c == 'P' || c == 'S');
										}
										break;
									}
									case 'G':
									{
										char c2 = setting[1];
										int num4 = -1;
										int num5 = -1;
										int num6 = 0;
										int num7 = 0;
										bool flag4 = false;
										switch (c2)
										{
										case 'N':
											num5 = 0;
											num7 = 1;
											flag4 = true;
											break;
										case 'S':
											num5 = 24;
											num7 = -1;
											flag4 = true;
											break;
										case 'E':
											num4 = 79;
											num6 = -1;
											flag4 = false;
											break;
										case 'W':
											num4 = 0;
											num6 = 1;
											flag4 = false;
											break;
										default:
											throw new Exception("invalid screen edge " + c2);
										}
										Cell cell = null;
										FindPath findPath = null;
										int num8 = 0;
										double num9 = 0.0;
										int num10 = -1;
										while (true)
										{
											if (cell == null)
											{
												int num11 = (flag4 ? currentCell.X : num4);
												int num12 = (flag4 ? num5 : currentCell.Y);
												for (int l = 0; l < 160; l++)
												{
													int num13 = l / 2;
													if (flag4)
													{
														if (num11 + num13 >= 80 && num11 - num13 < 0)
														{
															break;
														}
													}
													else if (num12 + num13 >= 25 && num12 - num13 < 0)
													{
														break;
													}
													if (l % 2 == 1)
													{
														num13 = -num13;
													}
													int num14 = (flag4 ? (num11 + num13) : num11);
													int num15 = (flag4 ? num12 : (num12 + num13));
													Cell cell2 = currentCell.ParentZone.GetCell(num14, num15);
													if (cell2 == null)
													{
														continue;
													}
													if (cell2 == currentCell)
													{
														goto end_IL_05af;
													}
													FindPath findPath2 = new FindPath(currentCell.ParentZone, currentCell.X, currentCell.Y, currentCell.ParentZone, num14, num15, PathGlobal: false, PathUnlimited: true, gameObject, The.Core.PlayerAvoid, ExploredOnly: true);
													if (findPath2.bFound && findPath2.Steps.Count > 0)
													{
														if (cell == null || findPath2.Steps.Count < num8)
														{
															cell = cell2;
															findPath = findPath2;
															num8 = findPath2.Steps.Count;
															num9 = currentCell.RealDistanceTo(cell);
															num10 = l;
														}
														else if (findPath2.Steps.Count == num8)
														{
															double num16 = currentCell.RealDistanceTo(cell2);
															if (num16 < num9)
															{
																cell = cell2;
																findPath = findPath2;
																num8 = findPath2.Steps.Count;
																num9 = num16;
																num10 = l;
															}
														}
													}
													if (cell != null && l > num10 + 1)
													{
														break;
													}
												}
												if (cell != null)
												{
													continue;
												}
												if (num4 != -1)
												{
													num4 += num6;
													if ((num6 < 0 && num4 <= currentCell.X) || (num6 > 0 && num4 >= currentCell.X))
													{
														goto IL_0ad5;
													}
												}
												if (num5 == -1)
												{
													continue;
												}
												num5 += num7;
												if ((num7 >= 0 || num5 > currentCell.Y) && (num7 <= 0 || num5 < currentCell.Y))
												{
													continue;
												}
											}
											goto IL_0ad5;
											IL_0ad5:
											if (cell == null || findPath == null)
											{
												Popup.Show("You cannot find a path toward the " + Directions.GetExpandedDirection(c2.ToString() ?? "") + ".");
												AutoAct.Interrupt();
											}
											else
											{
												The.Core.PlayerAvoid.Enqueue(new XRLCore.SortPoint(currentCell.X, currentCell.Y));
												AutoAct.TryToMove(gameObject, currentCell, ref LastDoor, findPath.Steps[1], findPath.Directions[0]);
											}
											break;
										}
										break;
									}
									case 'd':
									{
										string[] array2 = setting.Substring(1).Split(',');
										int x = Convert.ToInt32(array2[0]);
										int y = Convert.ToInt32(array2[1]);
										int num22 = Convert.ToInt32(array2[2]);
										int num23 = Convert.ToInt32(array2[3]);
										if (currentCell.X == num22 && currentCell.Y == num23)
										{
											AutoAct.Interrupt();
											break;
										}
										List<Point> list = Zone.Line(x, y, num22, num23);
										Point point = null;
										for (int num24 = list.Count - 2; num24 >= 0; num24--)
										{
											if (list[num24].X == currentCell.X && list[num24].Y == currentCell.Y)
											{
												point = list[num24 + 1];
												break;
											}
										}
										if (point == null)
										{
											AutoAct.Interrupt();
										}
										else
										{
											AutoAct.TryToMove(gameObject, currentCell, ref LastDoor, currentCell.ParentZone.GetCell(point), null, AllowDigging: true, OpenDoors: false);
										}
										break;
									}
									case '?':
									{
										AutoexploreMap.ClearSeeds();
										XRL.World.GameObject gameObject7 = FindAutoexploreObjectToProcess(gameObject, currentCell);
										if (gameObject7 != null)
										{
											string adjacentAction = AutoexploreObjectEvent.GetAdjacentAction(gameObject, gameObject7);
											if (adjacentAction != null)
											{
												string sProperty = "AutoexploreAction" + adjacentAction;
												if (gameObject7.GetIntProperty(sProperty) <= 0)
												{
													gameObject7.SetIntProperty(sProperty, 1);
													InventoryActionEvent.Check(out InventoryActionEvent E, gameObject7, gameObject, gameObject7, adjacentAction, Auto: true, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, (XRL.World.GameObject)null, (Cell)null, (Cell)null);
													if (E != null)
													{
														foreach (XRL.World.GameObject item in E.Generated)
														{
															if (item.IsValid())
															{
																Sidebar.AddAutogotItem(item);
																Sidebar.Update();
															}
														}
														break;
													}
												}
											}
										}
										XRL.World.GameObject gameObject8 = FindAutoexploreObjectToOpen(gameObject, currentCell);
										if (gameObject8 != null)
										{
											gameObject8.SetIntProperty("Autoexplored", 1);
											gameObject8.FireEvent(XRL.World.Event.New("Open", "Opener", gameObject));
											break;
										}
										XRL.World.GameObject obj3 = FindAutoexploreObjectToGet(gameObject, currentCell);
										if (obj3 != null)
										{
											if (gameObject.TakeObject(obj3, Silent: false, 0) && XRL.World.GameObject.validate(ref obj3))
											{
												Sidebar.AddAutogotItem(obj3);
												Sidebar.Update();
											}
											break;
										}
										if (currentCell.ParentZone.SetInfluenceAutoexploreSeeds(AutoexploreMap) == 0)
										{
											Popup.Show("There doesn't seem to be anywhere else to explore.");
											AutoAct.Interrupt();
											break;
										}
										AutoexploreMap.UsingWeights();
										currentCell.ParentZone.SetInfluenceMapAutoexploreWeightsAndWalls(AutoexploreMap.Weights, AutoexploreMap.Walls);
										AutoexploreMap.RecalculateCostOnly();
										string lowestWeightedCostDirectionFrom = AutoexploreMap.GetLowestWeightedCostDirectionFrom(currentCell.Pos2D);
										if (lowestWeightedCostDirectionFrom == ".")
										{
											Popup.Show("There doesn't seem to be anywhere else to explore from here.");
											AutoAct.Interrupt();
										}
										else if (!AutoAct.CheckHostileInterrupt())
										{
											if (Keyboard.kbhit())
											{
												AutoAct.Interrupt();
											}
											else
											{
												AutoAct.TryToMove(gameObject, currentCell, ref LastDoor, null, lowestWeightedCostDirectionFrom);
											}
										}
										break;
									}
									case '<':
									case '>':
									{
										AutoexploreMap.ClearSeeds();
										int num21 = 0;
										string text2 = "stairways";
										if (c == '<')
										{
											if (currentCell.HasObjectWithPart("StairsUp"))
											{
												AutoAct.Interrupt();
												break;
											}
											num21 = currentCell.ParentZone.SetInfluenceMapStairsUp(AutoexploreMap);
											text2 = "stairways leading upward";
										}
										else if (c == '>')
										{
											if (currentCell.HasObjectWithPart("StairsDown") && !currentCell.HasObject("Pit"))
											{
												AutoAct.Interrupt();
												break;
											}
											num21 = currentCell.ParentZone.SetInfluenceMapStairsDown(AutoexploreMap);
											text2 = "stairways leading downward";
										}
										if (num21 == 0)
										{
											Popup.ShowFail("There are no " + text2 + " nearby.");
											AutoAct.Interrupt();
											break;
										}
										AutoexploreMap.UsingWeights();
										currentCell.ParentZone.SetInfluenceMapAutoexploreWeightsAndWalls(AutoexploreMap.Weights, AutoexploreMap.Walls);
										AutoexploreMap.RecalculateCostOnly();
										string lowestWeightedCostDirectionFrom2 = AutoexploreMap.GetLowestWeightedCostDirectionFrom(currentCell.Pos2D);
										if (lowestWeightedCostDirectionFrom2 == ".")
										{
											MessageQueue.AddPlayerMessage("You can't figure out how to safely reach the stairs from here.");
											AutoAct.Interrupt();
										}
										else if (!AutoAct.CheckHostileInterrupt())
										{
											if (Keyboard.kbhit())
											{
												AutoAct.Interrupt();
											}
											else
											{
												AutoAct.TryToMove(gameObject, currentCell, ref LastDoor, null, lowestWeightedCostDirectionFrom2);
											}
										}
										break;
									}
									case 'g':
										obj2 = null;
										flag2 = false;
										if (setting.Length == 1)
										{
											flag2 = true;
											int num3 = 0;
											int count = currentCell.Objects.Count;
											while (num3 < count)
											{
												gameObject2 = currentCell.Objects[num3];
												if (gameObject2 == gameObject || !gameObject2.ShouldAutoget())
												{
													num3++;
													continue;
												}
												goto IL_105c;
											}
											goto IL_1079;
										}
										if (setting[1] == 'o')
										{
											XRL.World.GameObject gameObject3 = XRL.World.GameObject.findById(setting.Substring(2));
											if (gameObject3 != null && gameObject3.InSameOrAdjacentCellTo(gameObject))
											{
												Inventory inventory = gameObject3.Inventory;
												Cell currentCell2 = gameObject3.GetCurrentCell();
												if (inventory != null && currentCell2 != null && (!currentCell2.IsSolidFor(gameObject) || gameObject3.ConsiderSolidFor(gameObject)))
												{
													List<XRL.World.GameObject> objects = inventory.GetObjects();
													int j = 0;
													for (int count2 = objects.Count; j < count2; j++)
													{
														if (objects[j].ShouldTakeAll())
														{
															if (AutoAct.CheckHostileInterrupt(logSpot: true))
															{
																goto end_IL_05af;
															}
															obj2 = objects[j];
														}
													}
												}
											}
										}
										else if (setting[1] == 'd')
										{
											Cell cellFromDirection2 = currentCell.GetCellFromDirection(setting.Substring(2));
											if (cellFromDirection2 != null)
											{
												bool flag3 = cellFromDirection2.IsSolidFor(gameObject);
												int k = 0;
												for (int count3 = cellFromDirection2.Objects.Count; k < count3; k++)
												{
													XRL.World.GameObject gameObject4 = cellFromDirection2.Objects[k];
													if ((!flag3 || gameObject4.ConsiderSolidFor(gameObject)) && gameObject4.ShouldTakeAll())
													{
														if (AutoAct.CheckHostileInterrupt(logSpot: true))
														{
															goto end_IL_05af;
														}
														obj2 = gameObject4;
													}
												}
											}
										}
										goto IL_1260;
									case 'o':
										if (AutoAct.Action == null)
										{
											MetricsManager.LogError("had autoact o but no ongoing action");
											AutoAct.Interrupt();
										}
										else if (AutoAct.Action.CanComplete())
										{
											AutoAct.Action.Complete();
											AutoAct.Action.End();
											AutoAct.Resume();
										}
										else if (AutoAct.Action.Continue())
										{
											if (AutoAct.Action.CanComplete())
											{
												AutoAct.Action.Complete();
												AutoAct.Action.End();
												AutoAct.Resume();
											}
										}
										else
										{
											AutoAct.Interrupt();
										}
										break;
									case 'z':
										if (Calendar.IsDay())
										{
											AutoAct.Interrupt();
											break;
										}
										gameObject.UseEnergy(1000, "Pass");
										if (++The.ActionManager.RestingUntilHealedCount % 10 == 0)
										{
											XRLCore.TenPlayerTurnsPassed();
											The.Core.RenderBase(UpdateSidebar: false, DuringRestOkay: true);
										}
										break;
									case 'r':
										The.ActionManager.RestingUntilHealedCount++;
										Loading.SetLoadingStatus("Resting until healed... Turn: " + The.ActionManager.RestingUntilHealedCount);
										gameObject.UseEnergy(1000, "Pass");
										if (gameObject.GetStat("Hitpoints").Penalty <= 0)
										{
											AutoAct.Interrupt();
										}
										else if (The.ActionManager.RestingUntilHealedCount % 10 == 0)
										{
											XRLCore.TenPlayerTurnsPassed();
											The.Core.RenderBase(UpdateSidebar: false, DuringRestOkay: true);
										}
										break;
									case 'a':
									{
										XRL.World.GameObject target = gameObject.Target;
										if (target == null || (The.Core.ConfusionLevel > 0 && The.Core.FuriousConfusion <= 0))
										{
											AutoAct.Interrupt();
											break;
										}
										if (target.pBrain != null && !target.IsHostileTowards(gameObject))
										{
											MessageQueue.AddPlayerMessage("You will not auto-attack " + target.t() + " because " + target.itis + " not hostile to you.");
											AutoAct.Interrupt();
											break;
										}
										if (The.Core.ConfusionLevel > 0 && The.Core.FuriousConfusion > 0)
										{
											gameObject.FireEvent(XRL.World.Event.New("CmdMove" + Directions.GetRandomDirection()));
											break;
										}
										if (!target.IsVisible())
										{
											MessageQueue.AddPlayerMessage("You cannot see your target.");
											AutoAct.Interrupt();
											break;
										}
										Cell currentCell3 = target.CurrentCell;
										switch (gameObject.DistanceTo(target))
										{
										case 0:
										{
											Cell randomElement = currentCell.GetLocalNavigableAdjacentCells(gameObject).GetRandomElement();
											if (randomElement == null)
											{
												MessageQueue.AddPlayerMessage("You can't find a way to navigate to " + target.t() + ".");
												AutoAct.Interrupt(null, null, target);
												break;
											}
											string directionFromCell2 = currentCell.GetDirectionFromCell(randomElement);
											if (string.IsNullOrEmpty(directionFromCell2) || directionFromCell2 == ".")
											{
												AutoAct.Interrupt();
											}
											else if (!gameObject.Move(directionFromCell2, Forced: false, System: false, IgnoreGravity: false, NoStack: false, null, NearestAvailable: false, null, null, null, Peaceful: true) || gameObject.CurrentCell != randomElement)
											{
												AutoAct.Interrupt(null, randomElement);
											}
											break;
										}
										case 1:
										{
											if (currentCell3.GetCombatTarget(gameObject) != target)
											{
												MessageQueue.AddPlayerMessage("You are unable to attack " + target.t() + ".");
												AutoAct.Interrupt(null, null, target);
												break;
											}
											string directionFromCell = currentCell.GetDirectionFromCell(currentCell3);
											if (string.IsNullOrEmpty(directionFromCell) || directionFromCell == ".")
											{
												AutoAct.Interrupt();
											}
											else if (!gameObject.AttackDirection(directionFromCell))
											{
												AutoAct.Interrupt(null, currentCell3);
											}
											else if (target.IsInvalid() || target.IsInGraveyard() || target != gameObject.Target)
											{
												AutoAct.Interrupt();
											}
											break;
										}
										default:
										{
											FindPath findPath4 = new FindPath(currentCell, currentCell3, PathGlobal: false, PathUnlimited: true, gameObject, 20);
											if (!findPath4.bFound || findPath4.Steps.Count == 0)
											{
												MessageQueue.AddPlayerMessage("You can't seem to find a way to reach " + target.t() + ".");
												AutoAct.Interrupt(null, null, target);
											}
											else
											{
												AutoAct.TryToMove(gameObject, currentCell, findPath4.Steps[1], findPath4.Directions[0]);
											}
											break;
										}
										}
										break;
									}
									default:
										{
											if (setting.Contains("."))
											{
												int num2 = Convert.ToInt32(setting.Split('.')[1]) - 1;
												if (num2 > 0)
												{
													gameObject.UseEnergy(1000, "Pass");
													Loading.SetLoadingStatus(XRL.World.Event.NewStringBuilder().Clear().Append("Waiting for ")
														.Append(num2.Things("turn"))
														.Append("...")
														.ToString());
													AutoAct.Setting = "." + num2;
													if (num2 % 10 == 0)
													{
														XRLCore.TenPlayerTurnsPassed();
														The.Core.RenderBase(UpdateSidebar: false, DuringRestOkay: true);
													}
												}
												else
												{
													Loading.SetLoadingStatus("Done waiting");
													AutoAct.Interrupt();
												}
											}
											else if (!AutoAct.CheckHostileInterrupt())
											{
												Cell cellFromDirection = currentCell.GetCellFromDirection(setting);
												if (InterruptAutowalkEvent.Check(gameObject, cellFromDirection, out var IndicateObject, out var IndicateCell))
												{
													XRL.World.GameObject indicateObject = IndicateObject;
													AutoAct.Interrupt(null, IndicateCell, indicateObject);
												}
												else
												{
													AutoAct.TryToMove(gameObject, currentCell, ref LastDoor, null, setting, AllowDigging: false);
												}
											}
											break;
										}
										IL_1260:
										if (obj2 == null)
										{
											AutoAct.Resume();
											break;
										}
										if (XRL.World.GameObject.validate(ref obj) && obj2 == obj)
										{
											AutoAct.Interrupt();
											break;
										}
										obj = obj2;
										if (obj2.IsTakeable())
										{
											if (gameObject.TakeObject(obj2, Silent: false, null, "Autoget"))
											{
												if (flag2 && XRL.World.GameObject.validate(ref obj2))
												{
													Sidebar.AddAutogotItem(obj2);
													Sidebar.Update();
												}
											}
											else
											{
												AutoAct.Interrupt();
											}
										}
										else if (obj2.IsAutogetLiquid())
										{
											if (!InventoryActionEvent.Check(obj2, gameObject, obj2, "CollectLiquid", Auto: true, OwnershipHandled: false, OverrideEnergyCost: false, 0, 0, null, null, currentCell))
											{
												AutoAct.Interrupt();
											}
										}
										else
										{
											MetricsManager.LogError("invalid object for autoget, " + obj2.DebugName);
											AutoAct.Interrupt();
										}
										break;
										IL_105c:
										if (AutoAct.CheckHostileInterrupt(logSpot: true))
										{
											break;
										}
										obj2 = gameObject2;
										goto IL_1079;
										IL_1079:
										if (obj2 == null)
										{
											List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells();
											int m = 0;
											for (int count4 = localAdjacentCells.Count; m < count4; m++)
											{
												Cell cell5 = localAdjacentCells[m];
												bool flag5 = cell5.IsSolidFor(gameObject);
												int n = 0;
												for (int count5 = cell5.Objects.Count; n < count5; n++)
												{
													XRL.World.GameObject gameObject6 = cell5.Objects[n];
													if ((!flag5 || gameObject6.ConsiderSolidFor(gameObject)) && gameObject6.ShouldAutoget() && (Options.AutogetFromNearby || gameObject6.IsAutogetLiquid()))
													{
														if (AutoAct.CheckHostileInterrupt(logSpot: true))
														{
															goto end_IL_05af;
														}
														obj2 = gameObject6;
													}
												}
											}
										}
										goto IL_1260;
										end_IL_05af:
										break;
									}
									if (AutomoveTimer.IsRunning)
									{
										AutomoveTimer.Stop();
										if (AutoAct.IsRateLimited())
										{
											int autoexploreRate = Options.AutoexploreRate;
											if (autoexploreRate != 0)
											{
												int num25 = 1000 / autoexploreRate;
												long elapsedMilliseconds = AutomoveTimer.ElapsedMilliseconds;
												if (elapsedMilliseconds < num25)
												{
													Thread.Sleep((int)(num25 - elapsedMilliseconds));
												}
											}
										}
									}
								}
								else
								{
									if (gameObject.pBrain.Goals.Count > 0)
									{
										gameObject.pBrain.FireEvent(XRL.World.Event.New("CommandTakeAction"));
										The.Core.RenderBase();
									}
									if (SkipPlayerTurn)
									{
										SkipPlayerTurn = false;
									}
									else
									{
										if (GameManager.runWholeTurnOnUIThread && Thread.CurrentThread != XRLCore.CoreThread)
										{
											The.Player.Energy.BaseValue = 0;
											return;
										}
										if (GameManager.runPlayerTurnOnUIThread && Thread.CurrentThread == XRLCore.CoreThread)
										{
											while (!Keyboard.kbhit())
											{
											}
											turnWaitFlag = true;
											MetricsManager.LogEditorWarning("--- queing player turn");
											GameManager.Instance.uiQueue.queueTask(delegate
											{
												MetricsManager.LogEditorWarning(" -- running player turn on game thread");
												The.Core.PlayerTurn();
												turnWaitFlag = false;
												MetricsManager.LogEditorWarning(" -- player turn on game thread done");
											}, 1);
											while (turnWaitFlag)
											{
											}
											MetricsManager.LogEditorWarning("--- exiting player turn wait");
										}
										else if (Thread.CurrentThread == XRLCore.CoreThread)
										{
											The.Core.PlayerTurn();
										}
									}
								}
							}
							else if (gameObject.IsPlayer())
							{
								The.Core.RenderBase();
							}
							else if (num > 5)
							{
								gameObject.Energy.BaseValue -= 1000;
							}
						}
					}
					catch (Exception ex)
					{
						XRLCore.LogError("Exception during turn it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
						if (gameObject != null && !gameObject.IsPlayer())
						{
							gameObject.Energy.BaseValue = 0;
						}
						if (bUnityDebug)
						{
							return;
						}
					}
					if (bUnityDebug)
					{
						break;
					}
				}
				if (flag)
				{
					EndActionEvent.Send(gameObject);
				}
				if (bUnityDebug)
				{
					break;
				}
			}
			if (!ActionQueue.Contains(null))
			{
				ActionQueue.Enqueue(null);
			}
			SegCount++;
			foreach (XRL.World.GameObject item2 in ActionQueue.Items)
			{
				try
				{
					if (item2 != null)
					{
						EndSegmentEvent.Send(item2);
					}
				}
				catch (Exception x2)
				{
					MetricsManager.LogError("EndSegment", x2);
				}
			}
			The.ZoneManager.CheckEventQueue();
			if (SegCount >= 10)
			{
				for (int num26 = 0; num26 < game.Systems.Count; num26++)
				{
					game.Systems[num26].EndTurn();
				}
				SyncSingleTurnRecipients();
				ProcessTurnTick();
				EndTurnList.ShuffleInPlace();
				int num27 = 0;
				for (int count6 = EndTurnList.Count; num27 < count6; num27++)
				{
					try
					{
						EndTurnEvent.Send(EndTurnList[num27]);
					}
					catch (Exception ex2)
					{
						XRLCore.LogError("Exception during EndTurn it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex2.ToString());
					}
				}
				SegCount = 0;
				if (The.Graveyard.Objects.Count > 0)
				{
					int num28 = 0;
					for (int count7 = The.Graveyard.Objects.Count; num28 < count7; num28++)
					{
						XRL.World.GameObject gameObject9 = The.Graveyard.Objects[num28];
						GameObjectFactory.Factory.Pool(gameObject9);
						if (count7 != The.Graveyard.Objects.Count)
						{
							count7 = The.Graveyard.Objects.Count;
							if (num28 < count7 && gameObject9 != The.Graveyard.Objects[num28])
							{
								num28--;
							}
						}
					}
					The.Graveyard.Objects.Clear();
				}
				FlushSingleTurnRecipients();
				foreach (KeyValuePair<string, Zone> cachedZone in The.ZoneManager.CachedZones)
				{
					if (cachedZone.Value.IsActive())
					{
						cachedZone.Value.CheckWeather(The.Game.TimeTicks);
					}
				}
				The.ZoneManager.CheckEventQueue();
				The.ZoneManager.Tick(bAllowFreeze: false);
				The.Game.Turns++;
				The.Game.TimeTicks++;
			}
			int num29 = 0;
			for (int count8 = Commands.Count; num29 < count8; num29++)
			{
				Commands[num29].SegmentDelay--;
				if (Commands[num29].SegmentDelay <= 0)
				{
					IssueCommand(Commands[num29]);
				}
			}
			for (int num30 = 0; num30 < Commands.Count; num30++)
			{
				if (Commands[num30].SegmentDelay <= 0)
				{
					Commands.Remove(Commands[num30]);
					num30--;
				}
			}
		}
		catch (Exception ex3)
		{
			XRLCore.LogError("Exception during RunSegment it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex3.ToString());
		}
	}

	public void IssueCommand(CommandQueueEntry Command)
	{
		if (!(Command.Action == "SuspendZone"))
		{
			return;
		}
		UnityEngine.Debug.Log("Attempting to suspend zone");
		string text = Command.Target as string;
		Zone zone = The.ZoneManager.GetZone(text);
		if (zone != null && (zone.GetSuspendability(Zone.GetSuspendabilityTurns()) != Suspendability.Suspendable || !The.ZoneManager.SuspendZone(zone)))
		{
			int num;
			if (!ZoneSuspendFailures.ContainsKey(text))
			{
				num = 1;
				ZoneSuspendFailures.Add(text, 1);
			}
			else
			{
				num = ++ZoneSuspendFailures[text];
			}
			int num2 = 100 * num * num;
			if (num2 <= 40000)
			{
				Command.SegmentDelay += num2;
			}
		}
	}

	public void ProcessIndependentEndSegment(XRL.World.GameObject obj)
	{
		EndSegmentEvent.Send(obj);
	}

	public void ProcessIndependentEndTurn(XRL.World.GameObject obj)
	{
		EndTurnEvent.Send(obj);
		obj.CleanEffects();
	}
}
