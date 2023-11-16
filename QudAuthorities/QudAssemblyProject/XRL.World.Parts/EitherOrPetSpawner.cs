using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class EitherOrPetSpawner : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public void spawn(Cell c, bool either)
	{
		GameObject gameObject = null;
		if (either)
		{
			gameObject = GameObject.create("EitherPet");
		}
		if (!either)
		{
			gameObject = GameObject.create("OrPet");
		}
		gameObject.pRender.Tile = IComponent<GameObject>.ThePlayer.pRender.Tile;
		gameObject.TakeOnAttitudesOf(IComponent<GameObject>.ThePlayer);
		gameObject.PartyLeader = IComponent<GameObject>.ThePlayer;
		gameObject.SetActive();
		c.AddObject(gameObject);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.CurrentCell != null)
		{
			List<Cell> list = new List<Cell>();
			IComponent<GameObject>.ThePlayer.CurrentCell.GetConnectedSpawnLocations(2, list);
			if (list.Count >= 2)
			{
				spawn(list[0], either: true);
				spawn(list[1], either: false);
				ParentObject.Destroy();
			}
		}
		return base.FireEvent(E);
	}
}
