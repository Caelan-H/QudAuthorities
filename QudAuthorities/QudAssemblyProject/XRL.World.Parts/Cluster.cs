using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Cluster : IPart
{
	public string Blueprint = "";

	public string Number = "3-6";

	public string Radius = "3-4";

	public bool VisibleOnly = true;

	public bool PassableOnly = true;

	[NonSerialized]
	private static List<Cell> Cells = new List<Cell>();

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cells.Clear();
			ParentObject.pPhysics.CurrentCell.GetAdjacentCells(Stat.Roll(Radius), Cells);
			if (VisibleOnly)
			{
				Cells.RemoveAll((Cell c) => !ParentObject.HasLOSTo(c));
			}
			if (PassableOnly)
			{
				Cells.RemoveAll((Cell c) => !c.IsPassable());
			}
			Cells.ShuffleInPlace();
			int num = Stat.Roll(Number);
			for (int i = 0; i < num && i < Cells.Count; i++)
			{
				GameObject gO = GameObjectFactory.Factory.CreateObject(Blueprint);
				Cells[i].AddObject(gO);
			}
			ParentObject.Destroy();
		}
		return base.FireEvent(E);
	}
}
