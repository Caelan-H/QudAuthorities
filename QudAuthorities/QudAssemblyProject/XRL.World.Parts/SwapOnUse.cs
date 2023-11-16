using System;

namespace XRL.World.Parts;

[Serializable]
public class SwapOnUse : IPart
{
	public string Blueprint = "Campfire";

	public string Message;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUse");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUse")
		{
			if (!string.IsNullOrEmpty(Message))
			{
				EmitMessage(Message);
			}
			ParentObject.ReplaceWith(GameObject.create(Blueprint));
		}
		return base.FireEvent(E);
	}
}
