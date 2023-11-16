using System;

namespace XRL.World.Parts;

[Serializable]
public class PsychicThrall : IBondedCompanion
{
	public PsychicThrall()
	{
	}

	public PsychicThrall(GameObject EnthralledBy = null, string Faction = "Seekers", string NameAdjective = null, string NameClause = "and psychic thrall", string ConversationID = "PsychicThrall", bool StripGear = false)
		: base(EnthralledBy, Faction, NameAdjective, NameClause, ConversationID, StripGear)
	{
	}
}
