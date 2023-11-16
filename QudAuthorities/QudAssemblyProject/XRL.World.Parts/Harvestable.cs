using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Harvestable : IPart
{
	public bool DestroyOnHarvest;

	public string OnSuccess = "Vinewafer";

	public string OnSuccessAmount = "1";

	[FieldSaveVersion(227)]
	public string RipeTiles = "";

	[FieldSaveVersion(227)]
	public string RipeRenderString = "";

	public string RipeColor = "";

	public string RipeTileColor = "";

	public string RipeDetailColor = "";

	[FieldSaveVersion(227)]
	public string UnripeTiles = "";

	[FieldSaveVersion(227)]
	public string UnripeRenderString = "";

	public string UnripeColor = "";

	public string UnripeTileColor = "";

	public string UnripeDetailColor = "";

	[FieldSaveVersion(227)]
	public int TileIndex = -1;

	public bool Ripe;

	public string StartRipeChance = "1:1";

	public int RegenTimer = int.MaxValue;

	public string RegenTime = "";

	public string RipeTimerChance = "1:1";

	[FieldSaveVersion(257)]
	public string HarvestVerb;

	public void UpdateRipeStatus(bool newRipeStatus)
	{
		if (newRipeStatus)
		{
			RegenTimer = int.MaxValue;
		}
		else if (Ripe && RegenTime != "")
		{
			RegenTimer = Stat.Roll(RegenTime);
		}
		Ripe = newRipeStatus;
		if (Ripe)
		{
			if (!string.IsNullOrEmpty(RipeTiles))
			{
				List<string> list = RipeTiles.CachedCommaExpansion();
				if (TileIndex < 0)
				{
					TileIndex = Stat.Rand.Next(0, list.Count);
				}
				if (TileIndex < list.Count)
				{
					ParentObject.pRender.Tile = list[TileIndex];
				}
			}
			if (!string.IsNullOrEmpty(RipeRenderString))
			{
				ParentObject.pRender.RenderString = RipeRenderString;
			}
			if (!string.IsNullOrEmpty(RipeColor))
			{
				ParentObject.pRender.ColorString = RipeColor;
			}
			if (!string.IsNullOrEmpty(RipeTileColor))
			{
				ParentObject.pRender.TileColor = RipeTileColor;
			}
			if (!string.IsNullOrEmpty(RipeDetailColor))
			{
				ParentObject.pRender.DetailColor = RipeDetailColor;
			}
			return;
		}
		if (!string.IsNullOrEmpty(UnripeTiles))
		{
			List<string> list2 = UnripeTiles.CachedCommaExpansion();
			if (TileIndex < 0)
			{
				TileIndex = Stat.Rand.Next(0, list2.Count);
			}
			if (TileIndex < list2.Count)
			{
				ParentObject.pRender.Tile = list2[TileIndex];
			}
		}
		if (!string.IsNullOrEmpty(UnripeRenderString))
		{
			ParentObject.pRender.RenderString = UnripeRenderString;
		}
		if (!string.IsNullOrEmpty(UnripeColor))
		{
			ParentObject.pRender.ColorString = UnripeColor;
		}
		if (!string.IsNullOrEmpty(UnripeTileColor))
		{
			ParentObject.pRender.TileColor = UnripeTileColor;
		}
		if (!string.IsNullOrEmpty(UnripeDetailColor))
		{
			ParentObject.pRender.DetailColor = UnripeDetailColor;
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID && ID != EndTurnEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Ripen();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsHarvestable() && E.Actor.HasPart("CookingAndGathering_Harvestry"))
		{
			E.AddAction("Harvest", "harvest", "Harvest", null, 'h', FireOnActor: false, 15);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Harvest")
		{
			AttemptHarvest(E.Actor, E.Auto, null, E.FromCell, E.Generated);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (AttemptHarvest(E.Object, Automatic: true, null, E.Cell))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (Stat.Chance(StartRipeChance))
		{
			UpdateRipeStatus(newRipeStatus: true);
		}
		else
		{
			UpdateRipeStatus(newRipeStatus: false);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AccelerateRipening");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AccelerateRipening")
		{
			Ripen();
		}
		return base.FireEvent(E);
	}

	public bool AttemptHarvest(GameObject who, bool Automatic, string Verb = null, Cell FromCell = null, List<GameObject> Tracking = null)
	{
		if (!IsHarvestable() || !who.HasPart("CookingAndGathering_Harvestry"))
		{
			return false;
		}
		if (Automatic)
		{
			if (ParentObject.IsImportant())
			{
				return false;
			}
			if (who.IsPlayer())
			{
				if (who.ArePerceptibleHostilesNearby(logSpot: false, popSpot: false, null, null, null, Options.AutoexploreIgnoreEasyEnemies, Options.AutoexploreIgnoreDistantEnemies))
				{
					return false;
				}
			}
			else if (who.Target != null)
			{
				return false;
			}
			CookingAndGathering_Harvestry part = who.GetPart<CookingAndGathering_Harvestry>();
			if (part != null && !part.IsMyActivatedAbilityToggledOn(part.ActivatedAbilityID))
			{
				return false;
			}
		}
		if (!who.CheckFrozen())
		{
			return false;
		}
		ParentObject.SplitFromStack();
		if (!ParentObject.ConfirmUseImportant(who, "harvest"))
		{
			ParentObject.CheckStack();
			return false;
		}
		bool StoredByPlayer = ParentObject.GetIntProperty("StoredByPlayer") > 0;
		Action<GameObject> afterObjectCreated = delegate(GameObject obj)
		{
			if (StoredByPlayer)
			{
				obj.SetIntProperty("FromStoredByPlayer", 1);
			}
			Event @event = Event.New("ObjectExtracted");
			@event.SetParameter("Object", obj);
			@event.SetParameter("Source", ParentObject);
			@event.SetParameter("Actor", who);
			@event.SetParameter("Action", "Harvest");
			obj.FireEvent(@event);
		};
		Cell cell = ParentObject.GetCurrentCell();
		Cell cell2 = FromCell ?? who.GetCurrentCell();
		string text = null;
		if (cell != null && cell != cell2)
		{
			text = ((!who.IsPlayer()) ? (who.its + " " + Directions.GetExpandedDirection(cell2.GetDirectionFromCell(cell))) : ("the " + Directions.GetExpandedDirection(cell2.GetDirectionFromCell(cell))));
		}
		string text2 = Verb ?? HarvestVerb;
		if (((!ParentObject.IsTemporary && OnSuccessAmount.RollCached() != 0) ? 1 : 0) > (false ? 1 : 0))
		{
			int num = OnSuccessAmount.RollCached();
			if (OnSuccess[0] == '@')
			{
				GameObject gameObject = GameObject.create(PopulationManager.RollOneFrom(OnSuccess.Substring(1)).Blueprint, 0, 0, null, null, afterObjectCreated);
				if (!string.IsNullOrEmpty(text2))
				{
					IComponent<GameObject>.WDidXToYWithZ(who, text2, ParentObject, (text == null) ? "into" : ("to " + text + " into"), gameObject, null, null, null, who, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: true, indefiniteIndirectObject: true, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, !Automatic);
				}
				else
				{
					IComponent<GameObject>.XDidYToZ(who, "harvest", gameObject, (text == null) ? null : ("from " + text), null, null, who, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
				}
				who.TakeObject(gameObject, Silent: true, 0, null, Tracking);
			}
			else
			{
				GameObject gameObject2 = GameObject.create(OnSuccess, 0, 0, null, null, afterObjectCreated);
				if (!string.IsNullOrEmpty(text2))
				{
					if (num <= 1)
					{
						IComponent<GameObject>.WDidXToYWithZ(who, text2, ParentObject, (text == null) ? "into" : ("to " + text + " into"), gameObject2, null, null, null, who, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: true, indefiniteIndirectObject: true, indefiniteDirectObjectForOthers: false, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, !Automatic);
					}
					else if (gameObject2.IsPlural)
					{
						IComponent<GameObject>.XDidYToZ(who, text2, ParentObject, ((text == null) ? "" : ("to " + text + " ")) + "into " + Grammar.Cardinal(num) + " " + gameObject2.ShortDisplayName, null, null, who, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Automatic);
					}
					else
					{
						IComponent<GameObject>.XDidYToZ(who, text2, ParentObject, ((text == null) ? "" : ("to " + text + " ")) + "into " + Grammar.Cardinal(num) + " " + Grammar.Pluralize(gameObject2.ShortDisplayName), null, null, who, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, Automatic);
					}
				}
				else if (who.IsPlayer())
				{
					if (num <= 1)
					{
						IComponent<GameObject>.EmitMessage(who, "You harvest " + gameObject2.an() + ((text == null) ? "" : (" from " + text)) + ".", !Automatic);
					}
					else if (gameObject2.IsPlural)
					{
						IComponent<GameObject>.EmitMessage(who, "You harvest " + Grammar.Cardinal(num) + " " + gameObject2.ShortDisplayName + ((text == null) ? "" : (" from " + text)) + ".", !Automatic);
					}
					else
					{
						IComponent<GameObject>.EmitMessage(who, "You harvest " + Grammar.Cardinal(num) + " " + Grammar.Pluralize(gameObject2.ShortDisplayName) + ((text == null) ? "" : (" from " + text)) + ".", !Automatic);
					}
				}
				else if (who.IsVisible())
				{
					if (num <= 1)
					{
						IComponent<GameObject>.EmitMessage(who, who.Does("harvest") + " " + gameObject2.an() + ((text == null) ? "" : (" from " + text)) + ".", !Automatic);
					}
					else if (gameObject2.IsPlural)
					{
						IComponent<GameObject>.EmitMessage(who, who.Does("harvest") + " " + Grammar.Cardinal(num) + " " + gameObject2.ShortDisplayName + ((text == null) ? "" : (" from " + text)) + ".", !Automatic);
					}
					else
					{
						IComponent<GameObject>.EmitMessage(who, who.Does("harvest") + " " + Grammar.Cardinal(num) + " " + Grammar.Pluralize(gameObject2.ShortDisplayName) + ((text == null) ? "" : (" from " + text)) + ".", !Automatic);
					}
				}
				for (int i = 0; i < num; i++)
				{
					who.TakeObject(OnSuccess, Silent: true, 0, 0, 0, null, Tracking, null, afterObjectCreated);
				}
			}
			UpdateRipeStatus(newRipeStatus: false);
			if (DestroyOnHarvest)
			{
				ParentObject.Destroy();
			}
			who.UseEnergy(1000, "Skill");
			ParentObject.CheckStack();
			return true;
		}
		if (who.IsPlayer())
		{
			IComponent<GameObject>.EmitMessage(who, "There is nothing left to harvest.", !Automatic);
		}
		return false;
	}

	public void Ripen()
	{
		if (RegenTimer >= int.MaxValue)
		{
			return;
		}
		RegenTimer--;
		if (RegenTimer <= 0)
		{
			if (Stat.Chance(RipeTimerChance))
			{
				UpdateRipeStatus(newRipeStatus: true);
			}
			else
			{
				RegenTimer = RegenTime.RollCached();
			}
		}
	}

	public bool IsHarvestable()
	{
		if (Ripe && ParentObject.pRender.Visible)
		{
			if (ParentObject.HasPart("HologramMaterial"))
			{
				return ParentObject.pPhysics.CurrentCell.ParentZone.ZoneID.StartsWith("ThinWorld.");
			}
			return true;
		}
		return false;
	}
}
