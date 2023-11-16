using System;

namespace XRL.World.Parts;

[Serializable]
public class AICryptHelpBroadcaster : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		BroadcastHelp();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Opening");
		Object.RegisterPartEvent(this, "TookDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage")
		{
			if (ParentObject.isDamaged(0.9, inclusive: true))
			{
				BroadcastHelp();
			}
		}
		else if (E.ID == "Opening")
		{
			BroadcastHelp();
		}
		return base.FireEvent(E);
	}

	public void BroadcastHelp()
	{
		if (ParentObject.IsNowhere())
		{
			return;
		}
		foreach (GameObject item in ParentObject.CurrentCell.FastFloodVisibility("CryptSitterBehavior", 10))
		{
			item.FireEvent("AICryptHelpBroadcast");
		}
	}
}
