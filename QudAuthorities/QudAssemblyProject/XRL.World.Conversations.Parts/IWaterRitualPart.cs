using System;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public abstract class IWaterRitualPart : IConversationPart
{
	public bool Visible;

	public int Reputation;

	public virtual bool Affordable => The.Game.PlayerReputation.get(WaterRitual.RecordFaction.Name) >= Reputation;

	public virtual bool Available => true;

	public virtual string Highlight
	{
		get
		{
			if (!Affordable || !Available)
			{
				return "K";
			}
			return "G";
		}
	}

	public virtual string Lowlight
	{
		get
		{
			if (!Affordable || !Available)
			{
				return "K";
			}
			return "g";
		}
	}

	public virtual string Numeric
	{
		get
		{
			if (!Affordable)
			{
				return "r";
			}
			if (!Available)
			{
				return "";
			}
			return "C";
		}
	}

	public bool UseReputation()
	{
		if (The.Game.PlayerReputation.get(WaterRitual.RecordFaction) < Reputation)
		{
			Popup.Show("You don't have a high enough reputation with " + WaterRitual.RecordFaction.getFormattedName() + ".");
			return false;
		}
		The.Game.PlayerReputation.modify(WaterRitual.RecordFaction, -Reputation);
		return true;
	}

	public void AwardReputation(int Bonus = 0)
	{
		int num = Math.Min(Reputation, WaterRitual.Record.totalFactionAvailable);
		WaterRitual.Record.totalFactionAvailable -= num;
		The.Game.PlayerReputation.modify(WaterRitual.RecordFaction, num + Bonus);
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != IsElementVisibleEvent.ID && ID != ColorTextEvent.ID && ID != HideElementEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsElementVisibleEvent E)
	{
		if (base.HandleEvent(E))
		{
			return Visible;
		}
		return false;
	}

	public override bool HandleEvent(ColorTextEvent E)
	{
		E.Color = Highlight;
		return false;
	}

	public override bool HandleEvent(HideElementEvent E)
	{
		return false;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		Visible = false;
		Awake();
		return base.HandleEvent(E);
	}
}
