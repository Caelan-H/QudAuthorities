using System;
using Genkit;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SpiralBorerCurio : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "activate", "ActivateSpiralBorer", null, 'a');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateSpiralBorer")
		{
			if (E.Actor.OnWorldMap())
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You cannot do that on the world map.");
				}
				return false;
			}
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			Point2D pos2D = E.Actor.pPhysics.CurrentCell.Pos2D;
			Point2D p = ((pos2D.x <= 0) ? E.Actor.CurrentCell.Pos2D.FromDirection("E") : E.Actor.CurrentCell.Pos2D.FromDirection("W"));
			Popup.Show("The metal satchel opens, folds itself inside out, and transforms into a contraption studded with pinions and drills. It starts to burrow into the ground.");
			int i = 0;
			for (int num = 20; i <= num; i++)
			{
				string zoneIDFromDirection = E.Actor.CurrentCell.ParentZone.GetZoneIDFromDirection("D", i);
				if (XRLCore.Core.Game.ZoneManager.IsZoneBuilt(zoneIDFromDirection))
				{
					Zone zone = XRLCore.Core.Game.ZoneManager.GetZone(zoneIDFromDirection);
					if (i == 0)
					{
						zone.GetCell(pos2D).ClearObjectsWithTag("Wall");
						zone.GetCell(pos2D).AddObject("StairsDownUnconnected");
					}
					else if (i == num)
					{
						zone.GetCell(p).ClearObjectsWithTag("Wall");
						zone.GetCell(p).AddObject("StairsUpUnconnected");
					}
					else if (i % 2 == 0)
					{
						zone.GetCell(pos2D).ClearObjectsWithTag("Wall");
						zone.GetCell(p).ClearObjectsWithTag("Wall");
						zone.GetCell(pos2D).AddObject("StairsDownUnconnected");
						zone.GetCell(p).AddObject("StairsUpUnconnected");
					}
					else
					{
						zone.GetCell(pos2D).ClearObjectsWithTag("Wall");
						zone.GetCell(p).ClearObjectsWithTag("Wall");
						zone.GetCell(pos2D).AddObject("StairsUpUnconnected");
						zone.GetCell(p).AddObject("StairsDownUnconnected");
					}
					continue;
				}
				ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
				ZoneBuilderBlueprint zoneBuilderBlueprint = null;
				if (i == 0)
				{
					zoneBuilderBlueprint = new ZoneBuilderBlueprint();
					zoneBuilderBlueprint.Class = "ClearWallAddObject";
					zoneBuilderBlueprint.AddParameter("x", pos2D.x.ToString());
					zoneBuilderBlueprint.AddParameter("y", pos2D.y.ToString());
					zoneBuilderBlueprint.AddParameter("obj", "StairsDownUnconnected");
					zoneManager.AddZonePostBuilder(zoneIDFromDirection, zoneBuilderBlueprint);
				}
				else if (i == num)
				{
					zoneBuilderBlueprint = new ZoneBuilderBlueprint();
					zoneBuilderBlueprint.Class = "ClearWallAddObject";
					zoneBuilderBlueprint.AddParameter("x", p.x.ToString());
					zoneBuilderBlueprint.AddParameter("y", p.y.ToString());
					zoneBuilderBlueprint.AddParameter("obj", "StairsUpUnconnected");
					zoneManager.AddZonePostBuilder(zoneIDFromDirection, zoneBuilderBlueprint);
				}
				else if (i % 2 == 0)
				{
					zoneBuilderBlueprint = new ZoneBuilderBlueprint();
					zoneBuilderBlueprint.Class = "ClearWallAddObject";
					zoneBuilderBlueprint.AddParameter("x", pos2D.x.ToString());
					zoneBuilderBlueprint.AddParameter("y", pos2D.y.ToString());
					zoneBuilderBlueprint.AddParameter("obj", "StairsDownUnconnected");
					zoneManager.AddZonePostBuilder(zoneIDFromDirection, zoneBuilderBlueprint);
					zoneBuilderBlueprint = new ZoneBuilderBlueprint();
					zoneBuilderBlueprint.Class = "ClearWallAddObject";
					zoneBuilderBlueprint.AddParameter("x", p.x.ToString());
					zoneBuilderBlueprint.AddParameter("y", p.y.ToString());
					zoneBuilderBlueprint.AddParameter("obj", "StairsUpUnconnected");
					zoneManager.AddZonePostBuilder(zoneIDFromDirection, zoneBuilderBlueprint);
				}
				else
				{
					zoneBuilderBlueprint = new ZoneBuilderBlueprint();
					zoneBuilderBlueprint.Class = "ClearWallAddObject";
					zoneBuilderBlueprint.AddParameter("x", pos2D.x.ToString());
					zoneBuilderBlueprint.AddParameter("y", pos2D.y.ToString());
					zoneBuilderBlueprint.AddParameter("obj", "StairsUpUnconnected");
					zoneManager.AddZonePostBuilder(zoneIDFromDirection, zoneBuilderBlueprint);
					zoneBuilderBlueprint = new ZoneBuilderBlueprint();
					zoneBuilderBlueprint.Class = "ClearWallAddObject";
					zoneBuilderBlueprint.AddParameter("x", p.x.ToString());
					zoneBuilderBlueprint.AddParameter("y", p.y.ToString());
					zoneBuilderBlueprint.AddParameter("obj", "StairsDownUnconnected");
					zoneManager.AddZonePostBuilder(zoneIDFromDirection, zoneBuilderBlueprint);
				}
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
