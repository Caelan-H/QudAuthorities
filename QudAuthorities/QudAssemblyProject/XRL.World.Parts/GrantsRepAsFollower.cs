using System;
using System.Linq;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class GrantsRepAsFollower : IPart
{
	public string Faction = "";

	public int Value;

	public bool AppliedBonus;

	public GrantsRepAsFollower()
	{
	}

	public GrantsRepAsFollower(string _Faction, int Value)
		: this()
	{
		Faction = _Faction;
		this.Value = Value;
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		GrantsRepAsFollower obj = base.DeepCopy(Parent, MapInv) as GrantsRepAsFollower;
		obj.AppliedBonus = false;
		return obj;
	}

	public override bool SameAs(IPart p)
	{
		GrantsRepAsFollower grantsRepAsFollower = p as GrantsRepAsFollower;
		if (grantsRepAsFollower.Faction != Faction)
		{
			return false;
		}
		if (grantsRepAsFollower.Value != Value)
		{
			return false;
		}
		if (grantsRepAsFollower.AppliedBonus != AppliedBonus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	private void ApplyBonus(GameObject who)
	{
		if (AppliedBonus || who == null || !who.IsPlayer())
		{
			return;
		}
		AppliedBonus = true;
		if (Faction.StartsWith("*allvisiblefactions:"))
		{
			int delta = Convert.ToInt32(Faction.Split(':')[1]);
			{
				foreach (string visibleFactionName in Factions.getVisibleFactionNames())
				{
					XRLCore.Core.Game.PlayerReputation.modify(visibleFactionName, delta, null, null, silent: true, transient: true);
				}
				return;
			}
		}
		string[] array = Faction.Split(',');
		foreach (string text in array)
		{
			string faction = text;
			int delta2 = Value;
			if (text.Contains(':'))
			{
				string[] array2 = text.Split(':');
				faction = array2[0];
				delta2 = Convert.ToInt32(array2[1]);
			}
			XRLCore.Core.Game.PlayerReputation.modify(faction, delta2, null, null, silent: true, transient: true);
		}
	}

	private void UnapplyBonus()
	{
		if (!AppliedBonus)
		{
			return;
		}
		AppliedBonus = false;
		if (Faction.StartsWith("*allvisiblefactions:"))
		{
			int num = Convert.ToInt32(Faction.Split(':')[1]);
			{
				foreach (string visibleFactionName in Factions.getVisibleFactionNames())
				{
					XRLCore.Core.Game.PlayerReputation.modify(visibleFactionName, -num, null, null, silent: true, transient: true);
				}
				return;
			}
		}
		string[] array = Faction.Split(',');
		foreach (string text in array)
		{
			string faction = text;
			int num2 = Value;
			if (text.Contains(':'))
			{
				string[] array2 = text.Split(':');
				faction = array2[0];
				num2 = Convert.ToInt32(array2[1]);
			}
			XRLCore.Core.Game.PlayerReputation.modify(faction, -num2, null, null, silent: true, transient: true);
		}
	}

	public void CheckApplyBonus(GameObject who)
	{
		if (AppliedBonus)
		{
			if (who == null || !who.IsValid() || !who.IsPlayer() || who.CurrentZone != ParentObject.CurrentZone)
			{
				UnapplyBonus();
			}
		}
		else if (who != null && who.IsValid() && who.IsPlayer() && who.CurrentZone == ParentObject.CurrentZone)
		{
			ApplyBonus(who);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (ID != OnDestroyObjectEvent.ID && ID != SuspendingEvent.ID)
		{
			return base.WantEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		UnapplyBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		UnapplyBonus();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			CheckApplyBonus(ParentObject.PartyLeader);
		}
		return base.FireEvent(E);
	}
}
