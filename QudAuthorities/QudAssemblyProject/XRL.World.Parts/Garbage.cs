using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Garbage : IPart
{
	public int? Depth;

	public int Level;

	public override bool SameAs(IPart p)
	{
		Garbage garbage = p as Garbage;
		if (garbage.Depth != Depth)
		{
			return false;
		}
		if (garbage.Level != Level)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && (ID != EnteredCellEvent.ID || Depth.HasValue) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IComponent<GameObject>.ThePlayer.HasPart("TrashRifling"))
		{
			E.AddAction("Rifle", "rifle", "RifleThroughGarbage", null, 'r', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RifleThroughGarbage" && AttemptRifle(E.Actor, E.Auto, E.FromCell, E.Generated))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Depth.HasValue)
		{
			Depth = ParentObject.CurrentCell.ParentZone.GetZoneZ();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		AttemptRifle(E.Object, Automatic: true, E.Cell);
		return base.HandleEvent(E);
	}

	public bool AttemptRifle(GameObject who, bool Automatic, Cell FromCell = null, List<GameObject> Tracking = null)
	{
		if (!(who.GetPart("TrashRifling") is TrashRifling trashRifling) || ((Automatic || !who.IsPlayer()) && !trashRifling.IsMyActivatedAbilityVoluntarilyUsable(trashRifling.ActivatedAbilityID)))
		{
			return false;
		}
		bool flag = who.HasSkill("Customs_TrashDivining");
		bool flag2 = who.HasSkill("Tinkering_Scavenger");
		Cell cell = ParentObject.GetCurrentCell();
		GameObject gameObject = null;
		bool result = false;
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
		}
		if (!who.CheckFrozen(Telepathic: false, Telekinetic: true, Silent: true))
		{
			if (!Automatic && who.IsPlayer())
			{
				who.CheckFrozen();
			}
			return false;
		}
		Cell cell2 = FromCell ?? who.GetCurrentCell();
		string text = null;
		if (cell != null && cell != cell2)
		{
			text = ((!who.IsPlayer() && !IComponent<GameObject>.Visible(who)) ? ("to their " + Directions.GetExpandedDirection(cell2.GetDirectionFromCell(cell))) : Directions.GetDirectionDescription(who, cell2.GetDirectionFromCell(cell)));
		}
		bool flag3 = true;
		if (flag)
		{
			if (gameObject == null)
			{
				gameObject = ParentObject.RemoveOne();
			}
			if ((who.IsPlayer() || (who.IsPlayerLed() && The.Player.HasSkill("Customs_TrashDivining"))) && 5.in100())
			{
				string text2 = null;
				IBaseJournalEntry randomUnrevealedNote = JournalAPI.GetRandomUnrevealedNote();
				if (randomUnrevealedNote is JournalMapNote)
				{
					text2 = "Rifling through " + gameObject.the + gameObject.ShortDisplayName + ((text == null) ? "" : (" " + text)) + ", " + (who.IsPlayer() ? "you" : (who.the + who.ShortDisplayName)) + who.GetVerb("piece") + " together clues and" + who.GetVerb("determine") + " the location of:\n\n";
					flag3 = false;
				}
				else
				{
					text2 = "Rifling through " + gameObject.the + gameObject.ShortDisplayName + ((text == null) ? "" : (" " + text)) + ", " + (who.IsPlayer() ? "you" : (who.the + who.ShortDisplayName)) + who.GetVerb("piece") + " together clues and" + who.GetVerb("arrive") + " at the following conclusion:\n\n";
					flag3 = false;
				}
				text2 += randomUnrevealedNote.text;
				Popup.Show(text2);
				randomUnrevealedNote.Reveal();
			}
		}
		if (flag2)
		{
			if (gameObject == null)
			{
				gameObject = ParentObject.RemoveOne();
			}
			int num = Stat.Random(1, 100);
			int num2 = Tier.Constrain((Depth.Value - 10) / 4 + Level);
			if (num > 75)
			{
				if (num <= 99)
				{
					GameObject gameObject2 = GameObjectFactory.create(PopulationManager.RollOneFrom("Scrap " + num2).Blueprint);
					cell.AddObject(gameObject2);
					if (who.IsPlayer())
					{
						IComponent<GameObject>.EmitMessage(who, "You rifle through " + gameObject.the + gameObject.ShortDisplayName + ((text == null) ? "" : (" " + text)) + ", and find " + gameObject2.a + gameObject2.ShortDisplayName + ".", !Automatic, !Options.ShowScavengeItemAsMessage);
					}
					result = true;
					flag3 = false;
				}
				else
				{
					GameObject gameObject3 = GameObjectFactory.create(PopulationManager.RollOneFrom("Junk " + num2).Blueprint);
					cell.AddObject(gameObject3);
					if (who.IsPlayer())
					{
						EmitMessage("You rifle through " + gameObject.the + gameObject.ShortDisplayName + ((text == null) ? "" : (" " + text)) + ", and find " + gameObject3.a + gameObject3.ShortDisplayName + ".", !Automatic, !Options.ShowScavengeItemAsMessage);
					}
					result = true;
					flag3 = false;
				}
			}
		}
		if (gameObject != null)
		{
			if (who.IsPlayer())
			{
				if (flag3)
				{
					IComponent<GameObject>.EmitMessage(who, "{{K|You rifle through " + gameObject.the + gameObject.ShortDisplayName + ((text == null) ? "" : (" " + text)) + ", but you find nothing.}}", !Automatic);
				}
			}
			else if (IComponent<GameObject>.Visible(who))
			{
				IComponent<GameObject>.AddPlayerMessage(who.The + who.ShortDisplayName + who.GetVerb("rifle") + " through " + gameObject.the + gameObject.ShortDisplayName + ((text == null) ? "" : (" " + text)) + ".");
			}
			else if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage("Somebody rifles through " + gameObject.the + gameObject.ShortDisplayName + ((text == null) ? "" : (" " + text)) + ".");
			}
			if (IComponent<GameObject>.Visible(gameObject))
			{
				gameObject.DustPuff();
			}
			gameObject.Destroy();
			who.UseEnergy(1000, "Skill");
		}
		return result;
	}
}
