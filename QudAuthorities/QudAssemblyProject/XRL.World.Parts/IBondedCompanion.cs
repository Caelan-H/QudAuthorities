using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IBondedCompanion : IPart
{
	public GameObject CompanionOf;

	public string Faction;

	public string NameAdjective;

	public string NameClause;

	public string ConversationID;

	public bool StripGear;

	public IBondedCompanion(GameObject CompanionOf = null, string Faction = null, string NameAdjective = null, string NameClause = null, string ConversationID = null, bool StripGear = false)
	{
		this.CompanionOf = CompanionOf;
		this.Faction = Faction;
		this.NameAdjective = NameAdjective;
		this.NameClause = NameClause;
		this.ConversationID = ConversationID;
		this.StripGear = StripGear;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (GameObject.validate(ref CompanionOf) && E.Understood())
		{
			if (!string.IsNullOrEmpty(NameAdjective))
			{
				E.AddAdjective(NameAdjective);
			}
			if (!string.IsNullOrEmpty(NameClause))
			{
				E.AddClause(NameClause);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (ParentObject.pBrain != null)
		{
			if (CompanionOf != null)
			{
				ParentObject.pBrain.BecomeCompanionOf(CompanionOf);
			}
			else if (!string.IsNullOrEmpty(Faction))
			{
				ParentObject.pBrain.Factions = Faction + "-100";
				ParentObject.pBrain.InitFromFactions();
				ParentObject.FireEvent("FactionsAdded");
			}
		}
		if (!string.IsNullOrEmpty(ConversationID))
		{
			ParentObject.RequirePart<ConversationScript>().ConversationID = ConversationID;
		}
		if (StripGear)
		{
			ParentObject.StripOffGear();
		}
		ParentObject.FireEvent("VillageInit");
		InitializeBondedCompanion();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanAIDoIndependentBehavior");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanAIDoIndependentBehavior" && GameObject.validate(ref CompanionOf))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public virtual void InitializeBondedCompanion()
	{
	}
}
