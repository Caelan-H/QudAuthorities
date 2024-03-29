using System;

namespace XRL.World.Parts;

[Serializable]
public class EnableSwitch : IPart
{
	public int x = 10;

	public int y = 15;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "SwitchActivated");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SwitchActivated")
		{
			foreach (GameObject item in ParentObject.CurrentZone.GetCell(x, y).GetObjectsWithPart("Switch"))
			{
				item.FireEvent("Enable");
			}
		}
		return base.FireEvent(E);
	}
}
