using System;
using XRL.Core;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class WorldTeleporter : IPart
{
	public string TargetZone = "";

	public string TargetObject = "";

	public int MaxLevel = -1;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		CheckAgainstPlayer();
		base.TurnTick(TurnNumber);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectEnteredCell");
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	private void CheckAgainstPlayer()
	{
		if (XRLCore.Core.Game.Player.Body.Stat("Level") > MaxLevel && ParentObject.CurrentCell != null)
		{
			ParentObject.CurrentCell.AddObject("Shale");
			ParentObject.Destroy();
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			CheckAgainstPlayer();
		}
		else if (E.ID == "ObjectEnteredCell")
		{
			if (XRLCore.Core.Game.Player.Body.Statistics["Level"].Value > MaxLevel)
			{
				return true;
			}
			Physics physics = ParentObject.GetPart("Physics") as Physics;
			if (physics.CurrentCell == null)
			{
				return true;
			}
			if (physics.CurrentCell == XRLCore.Core.Game.Graveyard)
			{
				return true;
			}
			GameObject gameObject = E.GetParameter("Object") as GameObject;
			if (gameObject == ParentObject)
			{
				return true;
			}
			if (gameObject == null)
			{
				return true;
			}
			if (!(gameObject.GetPart("Render") is Render render))
			{
				return true;
			}
			if (physics == null)
			{
				return true;
			}
			if (TargetZone[0] == '$')
			{
				TargetZone = XRLCore.Core.Game.GetStringGameState(TargetZone);
			}
			if (render.RenderLayer != 0)
			{
				if (gameObject.IsPlayer())
				{
					if (gameObject.HasEffect("Lost") && gameObject.GetEffect("Lost") is Lost lost)
					{
						lost.DisableUnlost = true;
					}
					if (!gameObject.HasEffect("Lost"))
					{
						gameObject.ApplyEffect(new Lost(1));
					}
					string targetZone = TargetZone;
					Zone zone = XRLCore.Core.Game.ZoneManager.GetZone(TargetZone);
					Cell cell = null;
					for (int i = 0; i < zone.Width; i++)
					{
						for (int j = 0; j < zone.Height; j++)
						{
							foreach (GameObject item in zone.GetCell(i, j).GetObjectsInCell())
							{
								if (!(item.Blueprint == TargetObject))
								{
									continue;
								}
								foreach (Cell adjacentCell in zone.GetCell(i, j).GetAdjacentCells())
								{
									if (adjacentCell.IsEmpty())
									{
										cell = adjacentCell;
										goto end_IL_024b;
									}
								}
							}
						}
						continue;
						end_IL_024b:
						break;
					}
					if (cell == null)
					{
						return true;
					}
					The.ZoneManager.SetActiveZone(targetZone);
					The.Player.TeleportTo(cell, 0);
					The.ZoneManager.ProcessGoToPartyLeader();
					IComponent<GameObject>.AddPlayerMessage("You are sucked through the surface of the sphere!", 'C');
					if (gameObject.HasEffect("Lost") && gameObject.GetEffect("Lost") is Lost lost2)
					{
						lost2.DisableUnlost = false;
					}
				}
				else
				{
					gameObject.Destroy();
				}
			}
			return true;
		}
		return true;
	}
}
