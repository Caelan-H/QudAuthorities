using System;

namespace XRL.World.Parts;

[Serializable]
public class QudzuProperties : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (ParentObject.CurrentCell.IsOccluding())
			{
				ParentObject.pRender.ColorString = "&r^w";
			}
			else
			{
				ParentObject.pRender.ColorString = "&r";
			}
		}
		return base.FireEvent(E);
	}
}
