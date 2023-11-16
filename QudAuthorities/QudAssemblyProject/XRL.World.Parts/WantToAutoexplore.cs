using System;

namespace XRL.World.Parts;

[Serializable]
public class WantToAutoexplore : IPart
{
	public string AdjacentAction;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!string.IsNullOrEmpty(AdjacentAction))
		{
			if (!E.Want || E.FromAdjacent != AdjacentAction)
			{
				E.Want = true;
				E.FromAdjacent = AdjacentAction;
			}
		}
		else if (!E.Want)
		{
			E.Want = true;
		}
		return base.HandleEvent(E);
	}
}
