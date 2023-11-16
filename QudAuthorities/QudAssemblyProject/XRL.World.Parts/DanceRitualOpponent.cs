using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DanceRitualOpponent : IPart
{
	private int n;

	private PlayerDanceRitual Ritual => IComponent<GameObject>.ThePlayer.GetPart("PlayerDanceRitual") as PlayerDanceRitual;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeAITakingAction");
		Object.RegisterPartEvent(this, "AfterMoved");
		Object.RegisterPartEvent(this, "EndAction");
		Object.RegisterPartEvent(this, "BeforeDie");
		Object.RegisterPartEvent(this, "PreventSmartUse");
		Object.RegisterPartEvent(this, "EndTurn");
		IComponent<GameObject>.AddPlayerMessage("Debug: Angor Began The Dance", 'K');
		ParentObject.Energy.BaseValue = 1001;
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndAction")
		{
			ParentObject.Energy.BaseValue = 999;
			IComponent<GameObject>.ThePlayer.Energy.BaseValue = 1001;
			return true;
		}
		if (E.ID == "BeforeAITakingAction")
		{
			MessageQueue.AddPlayerMessage("&KDebug: Angor taking a turn...");
			Ritual.TurnsLeft--;
			if (Ritual.CurrentState != "#MakeChoice" && Ritual.TurnsLeft <= 0)
			{
				if (Ritual.CurrentState == "FreeTurn")
				{
					Ritual.CurrentState = "#MakeChoice";
					Ritual.CurrentLeader = "Angor";
				}
				else
				{
					MessageQueue.AddPlayerMessage("&KDebug: Dance Phase Ends Positive:" + Ritual.StepsPassed + " Negative:" + Ritual.StepsFailed);
					ParentObject.pRender.ColorString = "&G";
					ParentObject.pRender.TileColor = "&G";
					Ritual.PlayerMovementLog.Clear();
					Ritual.OpponentMovementLog.Clear();
					Ritual.TurnsLeft = 2;
					Ritual.StepsPassed = 0;
					Ritual.StepsFailed = 0;
					Ritual.CurrentState = "FreeTurn";
				}
			}
			if (Ritual.CurrentState == "#MakeChoice" && Ritual.CurrentLeader == "Angor")
			{
				int index = Stat.Random(0, Ritual.Rituals.Count - 1);
				RitualTypeEntry ritualTypeEntry = Ritual.Rituals[index];
				ParentObject.pRender.ColorString = "&" + ritualTypeEntry.Color;
				ParentObject.pRender.TileColor = "&" + ritualTypeEntry.Color;
				Ritual.CurrentState = ritualTypeEntry.Name;
				MessageQueue.AddPlayerMessage("&KDebug: Angor chooses " + ritualTypeEntry.Name);
				Ritual.PlayerMovementLog.Clear();
				Ritual.OpponentMovementLog.Clear();
				Ritual.TurnsLeft = 5;
			}
			if (Ritual.CurrentState == "Mimic" || Ritual.CurrentState == "Mirror")
			{
				if (Ritual.CurrentState == "Mirror")
				{
					bool flag = true;
					if (Ritual.PlayerMovementLog.Count != Ritual.OpponentMovementLog.Count)
					{
						flag = false;
						Ritual.PlayerMovementLog.Clear();
						Ritual.OpponentMovementLog.Clear();
					}
					else
					{
						for (int i = 0; i < Ritual.OpponentMovementLog.Count && i < Ritual.PlayerMovementLog.Count; i++)
						{
							if (Ritual.OpponentMovementLog[i] != Directions.GetOppositeDirection(Ritual.PlayerMovementLog[i]))
							{
								flag = false;
								break;
							}
						}
					}
					if (!flag)
					{
						Ritual.FailStep("Movement log didn't match");
					}
					else
					{
						Ritual.PassStep("Movement log matches");
					}
				}
				if (Ritual.CurrentState == "Mimic")
				{
					bool flag2 = true;
					if (Ritual.PlayerMovementLog.Count != Ritual.OpponentMovementLog.Count)
					{
						flag2 = false;
						Ritual.PlayerMovementLog.Clear();
						Ritual.OpponentMovementLog.Clear();
					}
					else
					{
						for (int j = 0; j < Ritual.OpponentMovementLog.Count && j < Ritual.PlayerMovementLog.Count; j++)
						{
							if (Ritual.OpponentMovementLog[j] != Ritual.PlayerMovementLog[j])
							{
								flag2 = false;
								break;
							}
						}
					}
					if (!flag2)
					{
						Ritual.FailStep("Movement log didn't match");
					}
					else
					{
						Ritual.PassStep("Movement log matches");
					}
				}
				List<Cell> localEmptyAdjacentCells = ParentObject.pPhysics.CurrentCell.GetLocalEmptyAdjacentCells();
				if (localEmptyAdjacentCells.Count == 0)
				{
					return true;
				}
				ParentObject.Move(localEmptyAdjacentCells.GetRandomElement().GetDirectionFromCell(ParentObject.CurrentCell));
				return true;
			}
			return true;
		}
		if (E.ID == "AfterMoved")
		{
			Ritual.ExecuteMove("Opponent", E.GetStringParameter("Direction"));
			return true;
		}
		if (E.ID == "BeforeDie")
		{
			XRLCore.Core.Game.Player.Body.GetPart<PlayerDanceRitual>().FireEvent(Event.New("DanceOpponentDied"));
			return true;
		}
		if (E.ID == "PreventSmartUse")
		{
			return false;
		}
		if (E.ID == "CanHaveConversation")
		{
			if (!E.IsSilent())
			{
				Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " busy dancing!");
			}
			return false;
		}
		if (E.ID == "EndTurn")
		{
			Ritual.EndTurn("Actor");
			MessageQueue.AddPlayerMessage("&KDebug: Dance party opponent turn tick " + n);
			n++;
			return true;
		}
		return true;
	}
}
