using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PickRandomTile : IPart
{
	public string Tile = "";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == RefreshTileEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RefreshTileEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public void RandomizeTile()
	{
		string text = "";
		string[] array = Tile.Split('~');
		if (array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (!string.IsNullOrEmpty(array[i]))
				{
					text = ((array[i][0] != '#') ? (text + array[i]) : (text + Stat.Roll(array[i].Substring(1))));
				}
			}
		}
		ParentObject.pRender.Tile = text;
	}
}
