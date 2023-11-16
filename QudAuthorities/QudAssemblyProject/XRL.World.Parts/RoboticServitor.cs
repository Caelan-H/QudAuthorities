using System;

namespace XRL.World.Parts;

[Serializable]
public class RoboticServitor : IBondedCompanion
{
	public RoboticServitor()
	{
	}

	public RoboticServitor(GameObject ServitorOf = null, string Faction = null, string NameAdjective = null, string NameClause = null, string ConversationID = null, bool StripGear = false)
		: base(ServitorOf, Faction, NameAdjective, NameClause, ConversationID, StripGear)
	{
	}
}
