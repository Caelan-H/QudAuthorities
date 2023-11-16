using System;

namespace XRL.World.Parts;

[Serializable]
public class Companion : IBondedCompanion
{
	public Companion()
	{
	}

	public Companion(GameObject CompanionOf = null, string Faction = null, string NameAdjective = null, string NameClause = null, string ConversationID = null, bool StripGear = false)
		: base(CompanionOf, Faction, NameAdjective, NameClause, ConversationID, StripGear)
	{
	}
}
