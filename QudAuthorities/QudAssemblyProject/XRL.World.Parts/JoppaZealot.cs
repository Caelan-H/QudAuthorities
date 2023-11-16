using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class JoppaZealot : IPart
{
	public int LastTalk;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != CommandTakeActionEvent.ID)
		{
			return ID == GetPointsOfInterestEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.Actor.IsPlayer() && !The.Game.HasQuest("O Glorious Shekhinah!") && E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (ConversationScript.IsPhysicalConversationPossible(ParentObject))
		{
			if (E.Actor.IsPlayer())
			{
				if (The.Game.HasQuest("O Glorious Shekhinah!") && !ParentObject.IsPlayerLed())
				{
					return false;
				}
			}
			else if (ParentObject.InActiveZone())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (ConversationScript.IsPhysicalConversationPossible(ParentObject))
		{
			if (E.Actor.IsPlayer())
			{
				if (The.Game.HasQuest("O Glorious Shekhinah!") && !ParentObject.IsPlayerLed())
				{
					ZealotDeclaim(E.Actor, Dialog: true);
				}
			}
			else if (ParentObject.InActiveZone())
			{
				ZealotDeclaim(Dialog: false);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (ParentObject.InActiveZone() && !ParentObject.IsPlayerLed())
		{
			LastTalk--;
			if (LastTalk < 0 && 55.in100())
			{
				LastTalk = 350;
				ZealotDeclaim(Dialog: false);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void ZealotDeclaim(GameObject who, bool Dialog)
	{
		if (!Dialog && !ParentObject.IsAudible(IComponent<GameObject>.ThePlayer, 80))
		{
			return;
		}
		if (ParentObject.IsFrozen())
		{
			EmitMessage("The Zealot mumbles inaudibly, encased in ice.", Dialog);
			return;
		}
		string text = null;
		switch (Stat.Random(1, 4))
		{
		case 1:
			text = "Look to the north! Journey to the Six Day Stilt and pay homage to your saviors!";
			break;
		case 2:
			text = "Cast down your artifacts! You are not worthy of their make!";
			break;
		case 3:
			text = "Piety compels you to deliver your sacred relics to the priests of the Six Day Stilt! Cleanse them of your filth!";
			break;
		case 4:
			text = "The Machine commands that you exorcise robots and bring their sacred husks to the Six Day Stilt!";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			IComponent<GameObject>.EmitMessage(who ?? ParentObject, "The Zealot yells {{W|'" + text + "'}}", Dialog);
			if (!Dialog)
			{
				ParentObject.ParticleText("{{W|" + text + "}}", (ParentObject.CurrentCell.X < 40) ? 0.4f : (-0.4f), (ParentObject.CurrentCell.Y < 12) ? 0.2f : (-0.2f), ' ', IgnoreVisibility: true);
			}
		}
	}

	public void ZealotDeclaim(bool Dialog)
	{
		ZealotDeclaim(null, Dialog);
	}
}
