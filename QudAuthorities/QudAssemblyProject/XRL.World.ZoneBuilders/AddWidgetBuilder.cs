namespace XRL.World.ZoneBuilders;

public class AddWidgetBuilder
{
	public string Blueprint;

	public string Object;

	public bool BuildZone(Zone Z)
	{
		if (!string.IsNullOrEmpty(Blueprint))
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(Blueprint);
			if (gameObject != null)
			{
				Z.GetCell(0, 0)?.AddObject(gameObject);
			}
		}
		if (!string.IsNullOrEmpty(Object))
		{
			GameObject cachedObjects = The.ZoneManager.GetCachedObjects(Object);
			if (cachedObjects != null)
			{
				Z.GetCell(0, 0)?.AddObject(cachedObjects);
			}
		}
		return true;
	}
}
