using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class FriendlyFireAmnestyDuringQuest : IPart
{
	public string QuestName;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanBeAngeredByBeingAttacked");
		Object.RegisterPartEvent(this, "CanBeAngeredByDamage");
		Object.RegisterPartEvent(this, "CanBeAngeredByFriendlyFire");
		Object.RegisterPartEvent(this, "CanBeAngeredByPropertyCrime");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanBeAngeredByBeingAttacked" || E.ID == "CanBeAngeredByDamage")
		{
			if (!string.IsNullOrEmpty(QuestName) && XRLCore.Core.Game.HasUnfinishedQuest(QuestName) && ParentObject.IsAlliedTowards(E.GetGameObjectParameter("Attacker")))
			{
				return false;
			}
		}
		else if ((E.ID == "CanBeAngeredByFriendlyFire" || E.ID == "CanBeAngeredByPropertyCrime") && !string.IsNullOrEmpty(QuestName) && XRLCore.Core.Game.HasUnfinishedQuest(QuestName))
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
