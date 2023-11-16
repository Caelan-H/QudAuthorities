namespace XRL.World.ZoneBuilders;

public class HindrenClues : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		foreach (string clueItem in HindrenMysteryGamestate.instance.clueItems)
		{
			ZoneBuilderSandbox.PlaceObject(GameObjectFactory.Factory.CreateObject(clueItem), Z);
		}
		foreach (HindrenClueLook lookClue in HindrenMysteryGamestate.instance.lookClues)
		{
			foreach (GameObject @object in Z.GetObjects((GameObject go) => go.Blueprint == lookClue.target))
			{
				lookClue.apply(@object);
			}
		}
		return true;
	}
}
