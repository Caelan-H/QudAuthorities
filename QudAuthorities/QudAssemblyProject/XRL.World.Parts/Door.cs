using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class Door : IPart, IHackingSifrahHandler
{
	public bool bOpen;

	public bool bLocked;

	public bool bWasLocked;

	public string ClosedDisplay = "+";

	public string OpenDisplay = "/";

	public string ClosedTile = "Tiles/sw_door_basic.bmp";

	public string OpenTile = "Tiles/sw_door_basic_open.bmp";

	public bool bRender = true;

	public string KeyObject;

	public bool SyncAdjacent;

	public int SecurityClearance
	{
		get
		{
			return XRL.World.Capabilities.SecurityClearance.GetSecurityClearanceByKeySpecification(KeyObject);
		}
		set
		{
			XRL.World.Capabilities.SecurityClearance.HandleSecurityClearanceSpecification(value, ref KeyObject);
		}
	}

	public override bool SameAs(IPart p)
	{
		Door door = p as Door;
		if (door.ClosedDisplay != ClosedDisplay)
		{
			return false;
		}
		if (door.OpenDisplay != OpenDisplay)
		{
			return false;
		}
		if (door.ClosedTile != ClosedTile)
		{
			return false;
		}
		if (door.OpenTile != OpenTile)
		{
			return false;
		}
		if (door.bRender != bRender)
		{
			return false;
		}
		if (door.KeyObject != KeyObject)
		{
			return false;
		}
		if (door.SyncAdjacent != SyncAdjacent)
		{
			return false;
		}
		if (door.bLocked != bLocked)
		{
			return false;
		}
		if (door.bWasLocked != bWasLocked)
		{
			return false;
		}
		if (door.bOpen != bOpen)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AnimateEvent.ID && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != EnteredCellEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AnimateEvent E)
	{
		SyncAdjacent = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		Cell cell = base.currentCell;
		if (cell == null || !cell.HasObjectWithPart("Campfire"))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		Event @event = Event.New("Open");
		@event.SetParameter("Opener", E.Actor);
		@event.SetParameter("UsePopupsForFailures", true);
		ParentObject.FireEvent(@event);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (SyncAdjacent)
		{
			PerformAdjacentSync();
		}
		if (!bOpen)
		{
			int i = 0;
			for (int count = E.Cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = E.Cell.Objects[i];
				if (!BlocksClosing(gameObject))
				{
					continue;
				}
				if (gameObject.HasPart("Combat"))
				{
					AttemptToggleOpen(gameObject, UsePopups: false, UsePopupsForFailures: false, IgnoreFrozen: true, IgnoreSpecialConditions: true, FromMove: false, E);
					if (bOpen)
					{
						break;
					}
				}
				else if (!bLocked)
				{
					Open(null, null, FromMove: true);
					if (bOpen)
					{
						break;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!bOpen && BlocksClosing(E.Object))
		{
			if (E.Object.HasPart("Combat"))
			{
				AttemptToggleOpen(E.Object, UsePopups: false, UsePopupsForFailures: false, IgnoreFrozen: true, IgnoreSpecialConditions: false, FromMove: false, E);
			}
			else if (!bLocked)
			{
				Open(null, null, FromMove: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (bOpen)
		{
			E.AddAction("Close", "close", "Close", null, 'c', FireOnActor: false, 10, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		else
		{
			E.AddAction("Open", "open", "Open", null, 'o', FireOnActor: false, 10, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Open" || E.Command == "Close")
		{
			Event @event = Event.New("Open");
			@event.SetParameter("Opener", E.Actor);
			@event.SetFlag("UsePopups", !E.Auto);
			@event.SetFlag("UsePopupsForFailures", State: true);
			if (!ParentObject.FireEvent(@event))
			{
				return false;
			}
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (bOpen)
		{
			if (bRender)
			{
				ParentObject.pRender.RenderString = OpenDisplay;
				ParentObject.pRender.Tile = OpenTile;
			}
			ParentObject.pRender.Occluding = false;
			ParentObject.pPhysics.Solid = false;
		}
		else
		{
			if (bRender)
			{
				ParentObject.pRender.RenderString = ClosedDisplay;
				ParentObject.pRender.Tile = ClosedTile;
			}
			ParentObject.pRender.Occluding = true;
			ParentObject.pPhysics.Solid = true;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforePhysicsRejectObjectEntringCell");
		Object.RegisterPartEvent(this, "Open");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforePhysicsRejectObjectEntringCell")
		{
			if (!ParentObject.IsCreature)
			{
				if (E.HasFlag("Actual"))
				{
					if (!bOpen)
					{
						GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
						if (gameObjectParameter.Body != null)
						{
							Event @event = Event.New("Open");
							@event.SetParameter("Opener", gameObjectParameter);
							@event.SetFlag("FromMove", State: true);
							ParentObject.FireEvent(@event);
						}
					}
				}
				else
				{
					GameObject gameObjectParameter2 = E.GetGameObjectParameter("Object");
					if (gameObjectParameter2.Body != null && CanOpen(gameObjectParameter2))
					{
						return false;
					}
				}
			}
		}
		else if (E.ID == "Open" && !AttemptToggleOpen(E.GetGameObjectParameter("Opener"), E.HasFlag("UsePopups"), E.HasFlag("UsePopupsForFailures"), IgnoreFrozen: false, IgnoreSpecialConditions: false, E.HasFlag("FromMove"), E))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool CanOpen(GameObject Opener = null, bool IgnoreFrozen = false, bool IgnoreSpecialConditions = false, bool FromMove = false)
	{
		if (bOpen)
		{
			return false;
		}
		if (!IgnoreFrozen && Opener != null && !Opener.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		if (Opener != null && !Opener.PhaseMatches(ParentObject))
		{
			return false;
		}
		if (Opener != null && ParentObject.IsFlying && !Opener.IsFlying)
		{
			return false;
		}
		if (!IgnoreSpecialConditions)
		{
			if (Opener != null && Opener.HasTag("Grazer"))
			{
				return false;
			}
			if (Opener != null && Opener.HasTag("CantOpenDoors"))
			{
				return false;
			}
		}
		if (bLocked && Opener != null)
		{
			if (!ParentObject.FireEvent(Event.New("CanAttemptDoorUnlock", "Opener", Opener, "Door", this)))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(KeyObject))
			{
				List<string> list = KeyObject.CachedCommaExpansion();
				if ((list.Count > 1 || list[0] != "*Psychometry") && Opener.FindContainedObjectByAnyBlueprint(list) != null)
				{
					return true;
				}
				if (Opener.GetIntProperty("DoorUnlocker") > 0)
				{
					return true;
				}
				if (list.Contains("*Psychometry") && UsePsychometry(Opener))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public bool AttemptToggleOpen(GameObject Opener = null, bool UsePopups = false, bool UsePopupsForFailures = false, bool IgnoreFrozen = false, bool IgnoreSpecialConditions = false, bool FromMove = false, IEvent FromEvent = null)
	{
		if (!IgnoreFrozen && Opener != null && !Opener.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		if (Opener != null && !Opener.PhaseMatches(ParentObject))
		{
			if (Opener.IsPlayer())
			{
				IComponent<GameObject>.EmitMessage(Opener, "You are out of phase with " + ParentObject.t() + ".", FromDialog: false, UsePopups || UsePopupsForFailures);
			}
			return false;
		}
		if (Opener != null && ParentObject.IsFlying && !Opener.IsFlying)
		{
			if (Opener.IsPlayer())
			{
				IComponent<GameObject>.EmitMessage(Opener, "You cannot reach " + ParentObject.t() + ".", FromDialog: false, UsePopups || UsePopupsForFailures);
			}
			return false;
		}
		if (!bOpen)
		{
			if (!IgnoreSpecialConditions)
			{
				if (Opener != null && Opener.HasTag("Grazer"))
				{
					return false;
				}
				if (Opener != null && Opener.HasTag("CantOpenDoors"))
				{
					return false;
				}
			}
			if (bLocked)
			{
				if (Opener != null)
				{
					if (!ParentObject.FireEvent(Event.New("AttemptDoorUnlock", "Opener", Opener, "Door", this)))
					{
						return false;
					}
					if (!string.IsNullOrEmpty(KeyObject))
					{
						if (ParentObject.DistanceTo(Opener) > 1)
						{
							if (Opener.IsPlayer())
							{
								IComponent<GameObject>.EmitMessage(Opener, "You can't unlock " + ParentObject.t() + " from a distance.", FromDialog: false, UsePopups || UsePopupsForFailures);
							}
							return false;
						}
						List<string> list = KeyObject.CachedCommaExpansion();
						if (list.Count > 1 || list[0] != "*Psychometry")
						{
							GameObject obj = Opener.FindContainedObjectByAnyBlueprint(list);
							if (obj != null)
							{
								if (Opener.IsPlayer())
								{
									Open(Opener, delegate
									{
										IComponent<GameObject>.EmitMessage(Opener, "Your " + obj.ShortDisplayName + " " + obj.GetVerb("unlock", PrependSpace: false) + " " + ParentObject.t() + ".", FromDialog: false, UsePopups);
									});
								}
								else
								{
									Open(Opener);
								}
							}
						}
						if (bLocked && Opener.GetIntProperty("DoorUnlocker") > 0)
						{
							if (Opener.IsPlayer())
							{
								Open(Opener, delegate
								{
									IComponent<GameObject>.EmitMessage(Opener, "You interface with " + ParentObject.t() + " and unlock " + ParentObject.them + ".", FromDialog: false, UsePopups);
								});
							}
							else
							{
								Open(Opener);
							}
						}
						if (bLocked && list.Contains("*Psychometry") && UsePsychometry(Opener))
						{
							if (Opener.IsPlayer())
							{
								Open(Opener, delegate
								{
									IComponent<GameObject>.EmitMessage(Opener, "You lay your hand upon " + ParentObject.t() + " and draw forth " + ParentObject.its + " passcode. You enter the code and " + ParentObject.t() + ParentObject.GetVerb("unlock") + ".", FromDialog: false, UsePopups);
								});
							}
							else
							{
								Open(Opener);
							}
						}
						if (bLocked && Opener.IsPlayer() && IsHackable() && Options.SifrahHacking && ParentObject.GetIntProperty("SifrahHack") >= 0)
						{
							int num = XRL.World.Capabilities.SecurityClearance.GetSecurityClearanceByKeySpecification(KeyObject);
							if (!KeyObject.CachedCommaExpansion().Contains("*Psychometry"))
							{
								num += 2;
							}
							HackingSifrah hackingSifrah = new HackingSifrah(ParentObject, num, num, Opener.Stat("Intelligence"));
							hackingSifrah.HandlerID = ParentObject.id;
							hackingSifrah.HandlerPartName = GetType().Name;
							hackingSifrah.Play(ParentObject);
							if (hackingSifrah.InterfaceExitRequested)
							{
								FromEvent?.RequestInterfaceExit();
							}
							if (ParentObject.GetIntProperty("SifrahHack") > 0)
							{
								ParentObject.ModIntProperty("SifrahHack", -1, RemoveIfZero: true);
								Open(Opener);
							}
						}
					}
					if (bLocked)
					{
						if (Opener.IsPlayer())
						{
							IComponent<GameObject>.EmitMessage(Opener, "You can't unlock " + ParentObject.t() + ".", FromDialog: false, UsePopups || UsePopupsForFailures);
						}
						return false;
					}
				}
			}
			else
			{
				Open(Opener, null, FromMove);
			}
		}
		else if (!AttemptClose(Opener, UsePopups, UsePopupsForFailures, FromMove))
		{
			return false;
		}
		return true;
	}

	private bool ShouldSync(Door d)
	{
		if (d != null && d.SyncAdjacent)
		{
			return d.KeyObject == KeyObject;
		}
		return false;
	}

	private void SyncAdjacentCell(Cell C)
	{
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			Door door = C.Objects[i].GetPart("Door") as Door;
			if (!ShouldSync(door))
			{
				continue;
			}
			if (door.bLocked != bLocked)
			{
				if (bLocked)
				{
					door.Lock();
				}
				else
				{
					door.Unlock();
				}
			}
			if (door.bOpen != bOpen)
			{
				if (bOpen)
				{
					door.Open();
				}
				else
				{
					door.Close();
				}
			}
		}
	}

	public void PerformAdjacentSync()
	{
		if (ParentObject.CurrentCell != null)
		{
			ParentObject.CurrentCell.ForeachCardinalAdjacentCell((Action<Cell>)SyncAdjacentCell);
		}
	}

	public void Open(GameObject Opener = null, Action Message = null, bool FromMove = false)
	{
		if (bLocked)
		{
			bWasLocked = true;
			bLocked = false;
		}
		bOpen = true;
		if (bRender)
		{
			ParentObject.pRender.RenderString = OpenDisplay;
			ParentObject.pRender.Tile = OpenTile;
		}
		ParentObject.pRender.Occluding = false;
		ParentObject.pPhysics.Solid = false;
		Message?.Invoke();
		PlayWorldSound(ParentObject.GetPropertyOrTag("OpenSound"));
		if (ParentObject.CurrentCell != null)
		{
			ParentObject.CurrentCell.ClearOccludeCache();
			ParentObject.CurrentCell.ClearNavigationCache();
		}
		if (SyncAdjacent)
		{
			PerformAdjacentSync();
		}
		if (Opener != null)
		{
			if (Opener.HasRegisteredEvent("Opened"))
			{
				Opener.FireEvent(Event.New("Opened", "Object", ParentObject));
			}
			if (!FromMove)
			{
				Opener.UseEnergy(1000, "Door Open");
			}
		}
	}

	public bool AttemptClose(GameObject Closer = null, bool UsePopups = false, bool UsePopupsForFailures = false, bool FromMove = false)
	{
		if (ParentObject.HasTagOrProperty("NoClose"))
		{
			if (Closer != null && Closer.IsPlayer())
			{
				IComponent<GameObject>.EmitMessage(Closer, ParentObject.T() + " cannot be closed.", FromDialog: false, UsePopups || UsePopupsForFailures);
			}
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = cell.Objects[i];
				if (BlocksClosing(gameObject))
				{
					IComponent<GameObject>.EmitMessage(Closer, ParentObject.T() + " cannot be closed with " + (gameObject.IsPlayer() ? "you" : gameObject.t()) + " in the way.", FromDialog: false, UsePopups || UsePopupsForFailures);
					return false;
				}
			}
			if (SyncAdjacent)
			{
				List<Cell> cardinalAdjacentCells = cell.GetCardinalAdjacentCells();
				int j = 0;
				for (int count2 = cardinalAdjacentCells.Count; j < count2; j++)
				{
					Cell cell2 = cardinalAdjacentCells[j];
					Door door = null;
					int k = 0;
					for (int count3 = cell2.Objects.Count; k < count3; k++)
					{
						Door door2 = cell2.Objects[k].GetPart("Door") as Door;
						if (ShouldSync(door2))
						{
							door = door2;
							break;
						}
					}
					if (door == null)
					{
						continue;
					}
					int l = 0;
					for (int count4 = cell2.Objects.Count; l < count4; l++)
					{
						GameObject gameObject2 = cell2.Objects[l];
						if (door.BlocksClosing(gameObject2))
						{
							IComponent<GameObject>.EmitMessage(Closer, ParentObject.T() + " cannot be closed with " + (gameObject2.IsPlayer() ? "you" : gameObject2.t()) + " in the way to the " + Directions.GetExpandedDirection(cell.GetDirectionFromCell(cell2)) + ".", FromDialog: false, UsePopups || UsePopupsForFailures);
							return false;
						}
					}
				}
			}
		}
		Close(Closer, FromMove);
		return !bOpen;
	}

	public void Close(GameObject Closer = null, bool FromMove = false)
	{
		if (ParentObject.HasTagOrProperty("NoClose"))
		{
			return;
		}
		bLocked = bWasLocked;
		bOpen = false;
		if (bRender)
		{
			ParentObject.pRender.RenderString = ClosedDisplay;
			ParentObject.pRender.Tile = ClosedTile;
		}
		ParentObject.pRender.Occluding = true;
		ParentObject.pPhysics.Solid = true;
		PlayWorldSound(ParentObject.GetPropertyOrTag("CloseSound"));
		if (ParentObject.CurrentCell != null)
		{
			ParentObject.CurrentCell.ClearOccludeCache();
			ParentObject.CurrentCell.ClearNavigationCache();
		}
		if (SyncAdjacent)
		{
			PerformAdjacentSync();
		}
		if (Closer != null)
		{
			if (Closer.HasRegisteredEvent("Closed"))
			{
				Closer.FireEvent(Event.New("Closed", "Object", ParentObject));
			}
			if (!FromMove)
			{
				Closer.UseEnergy(1000, "Door Close");
			}
		}
	}

	public void Unlock()
	{
		if (bLocked)
		{
			bWasLocked = true;
			bLocked = false;
			PlayWorldSound(ParentObject.GetPropertyOrTag("UnlockSound"));
			if (SyncAdjacent)
			{
				PerformAdjacentSync();
			}
		}
	}

	public void Lock()
	{
		if (!bLocked)
		{
			if (bOpen)
			{
				Close();
			}
			bLocked = true;
			PlayWorldSound(ParentObject.GetPropertyOrTag("LockSound"));
			if (SyncAdjacent)
			{
				PerformAdjacentSync();
			}
		}
	}

	public bool CanPathThrough(GameObject who)
	{
		if (bOpen)
		{
			return true;
		}
		if (!bLocked)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(KeyObject) && who != null)
		{
			List<string> list = KeyObject.CachedCommaExpansion();
			if ((list.Count > 1 || list[0] != "*Psychometry") && who.ContainsAnyBlueprint(list))
			{
				return true;
			}
			if (who.GetIntProperty("DoorUnlocker") > 0)
			{
				return true;
			}
			if (list.Contains("*Psychometry") && ShouldUsePsychometry(who))
			{
				return true;
			}
		}
		return false;
	}

	public bool BlocksClosing(GameObject obj)
	{
		if (obj == ParentObject)
		{
			return false;
		}
		if (obj.GetMatterPhase() != 1)
		{
			return false;
		}
		if (obj.HasPart("FungalVision") != ParentObject.HasPart("FungalVision"))
		{
			return false;
		}
		if (!obj.HasTag("Creature") && !obj.HasPropertyOrTag("BlocksDoors") && !obj.ConsiderSolidFor(ParentObject))
		{
			return false;
		}
		if (!obj.PhaseAndFlightMatches(ParentObject))
		{
			return false;
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return false;
		}
		return true;
	}

	public bool IsHackable()
	{
		if (!string.IsNullOrEmpty(KeyObject))
		{
			return XRL.World.Capabilities.SecurityClearance.GetSecurityClearanceByKeySpecification(KeyObject) > 0;
		}
		return false;
	}

	public void HackingResultSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.ModIntProperty("SifrahHack", 1);
			if (who.IsPlayer())
			{
				Popup.Show("You hack " + obj.t() + ".");
			}
		}
	}

	public void HackingResultExceptionalSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahHack", 1);
		if (string.IsNullOrEmpty(KeyObject))
		{
			return;
		}
		List<string> list = KeyObject.CachedCommaExpansion();
		int num = 0;
		while (++num < 10)
		{
			try
			{
				if (70.in100())
				{
					string randomBits = BitType.GetRandomBits("2d4".Roll(), obj.GetTechTier());
					if (!string.IsNullOrEmpty(randomBits))
					{
						who.RequirePart<BitLocker>().AddBits(randomBits);
						if (who.IsPlayer())
						{
							Popup.Show("You hack " + ParentObject.t() + " and find tinkering bits <{{|" + BitType.GetDisplayString(randomBits) + "}}> in " + ParentObject.them + "!");
						}
						break;
					}
					continue;
				}
				GameObject gameObject = GameObject.create(list.GetRandomElement());
				if (gameObject != null)
				{
					if (who.IsPlayer())
					{
						Popup.Show("You hack " + ParentObject.t() + " and find " + gameObject.an() + " stuck in " + ParentObject.them + "!");
					}
					who.ReceiveObject(gameObject);
					break;
				}
			}
			catch (Exception)
			{
			}
		}
	}

	public void HackingResultPartialSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject && who.IsPlayer())
		{
			Popup.Show("You feel like you're making progress on hacking " + obj.t() + " open.");
		}
	}

	public void HackingResultFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.ModIntProperty("SifrahHack", -1);
			if (who.IsPlayer())
			{
				Popup.Show("You cannot seem to work out how to hack " + obj.t() + ".");
			}
		}
	}

	public void HackingResultCriticalFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahHack", -1);
		if (who.HasPart("Dystechnia"))
		{
			Dystechnia.CauseExplosion(ParentObject, who);
			game.RequestInterfaceExit();
			return;
		}
		if (who.IsPlayer())
		{
			Popup.Show("Your attempt to hack " + obj.t() + " has gone very wrong.");
		}
		List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
		Cell cell = who.CurrentCell;
		ParentObject.Discharge((cell != null && localAdjacentCells.Contains(cell)) ? cell : localAdjacentCells.GetRandomElement(), "3d8".Roll(), "2d4");
	}
}
