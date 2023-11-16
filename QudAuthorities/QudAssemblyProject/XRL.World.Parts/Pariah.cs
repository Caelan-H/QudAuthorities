using System;
using XRL.Language;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class Pariah : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		MakePariah(ParentObject);
		return base.HandleEvent(E);
	}

	public static void MakePariah(GameObject go, bool AlterName = true, bool IsUnique = false)
	{
		if (go.pBrain != null)
		{
			if (go.pBrain.FactionMembership.ContainsKey("Pariahs"))
			{
				return;
			}
			go.pBrain.FactionMembership.Clear();
			go.pBrain.FactionMembership.Add("Pariahs", 100);
			go.pBrain.Hostile = false;
		}
		if (IsUnique)
		{
			HeroMaker.MakeHero(go);
			AlterName = false;
		}
		if (AlterName)
		{
			if (go.HasProperName)
			{
				go.RequirePart<SocialRoles>().RequireRole(Grammar.MakeTitleCase(go.GetBlueprint().DisplayName()) + " Pariah");
			}
			else
			{
				go.RequirePart<SocialRoles>().RequireRole("pariah to =pronouns.possessive= people");
			}
		}
	}
}
