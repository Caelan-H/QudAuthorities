using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Butcherable : IPart
{
	public string OnSuccessAmount = "1";

	public string OnSuccess = "";

	public override bool SameAs(IPart p)
	{
		Butcherable butcherable = p as Butcherable;
		if (butcherable.OnSuccessAmount != OnSuccessAmount)
		{
			return false;
		}
		if (butcherable.OnSuccess != OnSuccess)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsButcherable() && E.Actor.HasSkill("CookingAndGathering_Butchery"))
		{
			E.AddAction("Butcher", "butcher", "Butcher", null, 'b', FireOnActor: false, 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Butcher" && AttemptButcher(E.Actor, E.Auto, SkipSkill: false, IntoInventory: false, null, E.FromCell, E.Generated))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		AttemptButcher(E.Object, Automatic: true, SkipSkill: false, IntoInventory: false, null, E.Cell);
		return base.HandleEvent(E);
	}

	public bool AttemptButcher(GameObject who, bool Automatic = false, bool SkipSkill = false, bool IntoInventory = false, string Verb = null, Cell FromCell = null, List<GameObject> Tracking = null)
	{
		if (!IsButcherable())
		{
			return false;
		}
		if (!SkipSkill && !who.HasSkill("CookingAndGathering_Butchery"))
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
			CookingAndGathering_Butchery part = who.GetPart<CookingAndGathering_Butchery>();
			if (part != null && !part.IsMyActivatedAbilityToggledOn(part.ActivatedAbilityID))
			{
				return false;
			}
			if (ParentObject.HasTagOrProperty("QuestItem"))
			{
				return false;
			}
		}
		if (!who.CheckFrozen())
		{
			return false;
		}
		if (!ParentObject.ConfirmUseImportant(who, "butcher"))
		{
			return false;
		}
		Cell cell = ParentObject.GetCurrentCell();
		Cell cell2 = FromCell ?? who.GetCurrentCell();
		string text = null;
		if (cell != null && cell != cell2)
		{
			text = Directions.GetDirectionDescription(who, cell2.GetDirectionFromCell(cell));
		}
		bool StoredByPlayer = ParentObject.GetIntProperty("StoredByPlayer") > 0;
		Action<GameObject> afterObjectCreated = delegate(GameObject o)
		{
			if (StoredByPlayer)
			{
				o.SetIntProperty("FromStoredByPlayer", 1);
			}
			Event @event = Event.New("ObjectExtracted");
			@event.SetParameter("Object", o);
			@event.SetParameter("Source", ParentObject);
			@event.SetParameter("Actor", who);
			@event.SetParameter("Action", "Butcher");
			o.FireEvent(@event);
		};
		bool result = false;
		GameObject gameObject = ParentObject.RemoveOne();
		int num = ((!ParentObject.IsTemporary) ? OnSuccessAmount.RollCached() : 0);
		int num2 = 1000;
		if (num > 0)
		{
			num2 /= Math.Max(who.GetIntProperty("ButcheryToolEquipped") + 1, 1);
			GameObject gameObject2 = ((OnSuccess[0] != '@') ? GameObject.create(OnSuccess, 0, 0, null, null, afterObjectCreated) : GameObject.create(PopulationManager.RollOneFrom(OnSuccess.Substring(1)).Blueprint, 0, 0, null, null, afterObjectCreated));
			if (!IntoInventory && who.IsPlayer() && gameObject2.ShouldAutoget())
			{
				IntoInventory = true;
			}
			if (OnSuccess[0] != '@' && num > 1)
			{
				IComponent<GameObject>.XDidYToZ(who, Verb ?? "butcher", gameObject, ((text == null) ? "" : (text + " ")) + "into " + Grammar.Cardinal(num) + " " + Grammar.Pluralize(gameObject2.ShortDisplayName));
				if (IntoInventory)
				{
					who.TakeObject(gameObject2, Silent: true, 0, null, Tracking);
					who.TakeObject(OnSuccess, num - 1, Silent: true, 0, null, 0, 0, Tracking, null, afterObjectCreated);
				}
				else
				{
					cell.AddObject(gameObject2);
					cell.AddObject(OnSuccess, num - 1, null, null, afterObjectCreated);
				}
			}
			else
			{
				IComponent<GameObject>.WDidXToYWithZ(who, Verb ?? "butcher", gameObject, ((text == null) ? "" : (text + " ")) + "into", gameObject2, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: true, indefiniteIndirectObject: true);
				if (IntoInventory)
				{
					who.TakeObject(gameObject2, Silent: true, 0, null, Tracking);
				}
				else
				{
					cell.AddObject(gameObject2);
				}
			}
			result = true;
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(who, "fail", "to " + (Verb ?? "butcher") + " anything useful from", gameObject, text, null, null, null, who);
		}
		gameObject.Bloodsplatter(bSelfsplatter: false);
		gameObject.Destroy();
		who.UseEnergy(num2, "Skill");
		return result;
	}

	public bool IsButcherable()
	{
		if (!string.IsNullOrEmpty(OnSuccess) && ParentObject.pRender.Visible)
		{
			return !ParentObject.HasPart("HologramMaterial");
		}
		return false;
	}
}
