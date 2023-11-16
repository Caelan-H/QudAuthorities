namespace XRL.World.Parts;

public static class CultistTemplate
{
	public static void Apply(GameObject GO, string CultFaction)
	{
		GO.pBrain.FactionMembership.Clear();
		GO.pBrain.FactionMembership.Add(CultFaction, 100);
	}
}
