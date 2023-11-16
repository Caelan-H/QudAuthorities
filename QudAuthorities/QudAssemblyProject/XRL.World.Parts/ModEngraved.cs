using System;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModEngraved : IModification
{
	public bool bLookedAt;

	public HistoricEvent engravedEvent;

	public string Engraving;

	public string Sultan = "";

	public ModEngraved()
	{
	}

	public ModEngraved(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.ModIntProperty("NoCostMods", 1);
	}

	public override void Remove()
	{
		ParentObject.ModIntProperty("NoCostMods", 1, RemoveIfZero: true);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID && ID != GetUnknownShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if ((!E.Want || E.FromAdjacent != "Look") && HasUnrevealedSecret())
		{
			E.Want = true;
			E.FromAdjacent = "Look";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{engraved|engraved}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		AddEngraving(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetUnknownShortDescriptionEvent E)
	{
		AddEngraving(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		GenerateEngraving();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLookedAt");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !bLookedAt)
		{
			bLookedAt = true;
			if (engravedEvent != null)
			{
				engravedEvent.Reveal();
			}
		}
		return base.FireEvent(E);
	}

	public bool HasUnrevealedSecret()
	{
		if (engravedEvent != null)
		{
			return engravedEvent.HasUnrevealedSecret();
		}
		return false;
	}

	private void GenerateEngraving()
	{
		if (Engraving != null || XRLCore.Core.Game == null)
		{
			return;
		}
		History sultanHistory = XRLCore.Core.Game.sultanHistory;
		if (sultanHistory == null)
		{
			return;
		}
		HistoricEntity randomElement = sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan").GetRandomElement(Stat.Rand);
		Sultan = randomElement.GetCurrentSnapshot().GetProperty("name");
		List<HistoricEvent> list = new List<HistoricEvent>();
		for (int i = 0; i < randomElement.events.Count; i++)
		{
			if (randomElement.events[i].hasEventProperty("gospel"))
			{
				list.Add(randomElement.events[i]);
			}
		}
		if (list.Count > 0)
		{
			engravedEvent = list.GetRandomElement();
			Engraving = engravedEvent.GetEventProperty("gospel");
		}
		else
		{
			Engraving = "<marred and unreadable>";
		}
		string propertyOrTag = ParentObject.GetPropertyOrTag("Mods");
		if (propertyOrTag != null && !propertyOrTag.Contains("PotteryMods"))
		{
			string property = randomElement.GetCurrentSnapshot().GetProperty("period", "0");
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(Faction.getSultanFactionName(property)).Append(":").Append(Stat.Random(8, 12) * 5);
			AddsRep.AddModifier(ParentObject, stringBuilder.ToString());
		}
	}

	private void AddEngraving(IShortDescriptionEvent E)
	{
		if (engravedEvent == null)
		{
			GenerateEngraving();
		}
		E.Postfix.Append("\n{{cyan|Engraved: This item is engraved with a scene from the life of the ancient sultan {{magenta|").Append(Sultan).Append("}}:\n\n")
			.Append(Engraving)
			.Append("}}\n");
	}
}
