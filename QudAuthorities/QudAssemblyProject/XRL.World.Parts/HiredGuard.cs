using System;

namespace XRL.World.Parts;

[Serializable]
public class HiredGuard : IBondedCompanion
{
	public HiredGuard()
	{
	}

	public HiredGuard(GameObject HiredBy = null, string Faction = "Merchants", string NameAdjective = null, string NameClause = "and hired guard", string ConversationID = "MerchantGuard", bool StripGear = false)
		: base(HiredBy, Faction, NameAdjective, NameClause, ConversationID, StripGear)
	{
	}
}
