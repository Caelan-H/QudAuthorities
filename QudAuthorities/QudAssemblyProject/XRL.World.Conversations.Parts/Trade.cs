using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class Trade : IConversationPart
{
	public static bool Enabled = true;

	public static bool Visible
	{
		get
		{
			if (Enabled && ConversationUI.CurrentNode != null)
			{
				return ConversationUI.CurrentNode.AllowEscape;
			}
			return false;
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnterElementEvent.ID)
		{
			return ID == IsElementVisibleEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{g|[begin trade]}}";
		return false;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		TradeUI.ShowTradeScreen(The.Speaker);
		return false;
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		return Visible;
	}

	public static bool CheckEnabled(bool Base = true, bool Physical = true, bool Mental = false)
	{
		Enabled = true;
		GameObject speaker = The.Speaker;
		if (speaker == null)
		{
			Enabled = false;
		}
		else if (!Base)
		{
			Enabled = false;
		}
		else
		{
			Enabled &= The.Player.PhaseMatches(speaker);
			Enabled &= !speaker.HasTagOrProperty("NoTrade");
			Enabled &= speaker.DistanceTo(The.Player) <= 1;
			Enabled = CanTradeEvent.Check(The.Player, speaker, Conversation.Placeholder, Enabled, Physical, Mental);
		}
		return Enabled;
	}
}
