namespace XRL.World;

public class ExternalEventBind
{
	public string Event;

	public GameObject GO;

	public string Part;

	public ExternalEventBind(string _Event, GameObject _GO, string _Part)
	{
		Event = _Event;
		GO = _GO;
		Part = _Part;
	}
}
