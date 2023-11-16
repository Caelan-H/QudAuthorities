using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedraRuby : CyberneticsCathedra
{
	public int Duration = -1;

	[NonSerialized]
	public Dictionary<Point2D, GameObject> Field = new Dictionary<Point2D, GameObject>();

	[NonSerialized]
	public new List<Point2D> Remove = new List<Point2D>();

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void OnImplanted(GameObject Object)
	{
		base.OnImplanted(Object);
		Object.RegisterPartEvent(this, "BeginMove");
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "MoveFailed");
		Object.RegisterPartEvent(this, "BeforeTemperatureChange");
		ActivatedAbilityID = Object.AddActivatedAbility("Pyrokinesis Field", "CommandActivateCathedra", "Cybernetics", null, "*");
	}

	public override void OnUnimplanted(GameObject Object)
	{
		base.OnUnimplanted(Object);
		Object.UnregisterPartEvent(this, "BeginMove");
		Object.UnregisterPartEvent(this, "EnteredCell");
		Object.UnregisterPartEvent(this, "MoveFailed");
		Object.UnregisterPartEvent(this, "BeforeTemperatureChange");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			SuspendPyroField();
		}
		else if (E.ID == "EnteredCell" || E.ID == "MoveFailed")
		{
			DesuspendPyroField();
		}
		else if (Duration >= 0 && E.ID == "BeforeTemperatureChange")
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		Writer.Write(Field.Count);
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			Writer.Write(item.Key.x);
			Writer.Write(item.Key.y);
			Writer.WriteGameObject(item.Value);
		}
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		Field.Clear();
		int i = 0;
		for (int num = Reader.ReadInt32(); i < num; i++)
		{
			Point2D key = new Point2D(Reader.ReadInt32(), Reader.ReadInt32());
			Field[key] = Reader.ReadGameObject();
		}
	}

	public void Validate()
	{
		if (Field.Count == 0)
		{
			return;
		}
		Remove.Clear();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			if (item.Value == null || item.Value.IsInvalid() || item.Value.IsInGraveyard())
			{
				Remove.Add(item.Key);
			}
		}
		foreach (Point2D item2 in Remove)
		{
			Field.Remove(item2);
		}
	}

	public override void Activate(GameObject Actor)
	{
		Cell cell = Actor.CurrentCell;
		if (cell?.ParentZone == null || cell.ParentZone.IsWorldMap())
		{
			return;
		}
		IComponent<GameObject>.XDidY(Actor, "ignite", "a vortex of shimmering heat", "!");
		List<Cell> cells = cell.YieldAdjacentCells(2, LocalOnly: false, BuiltOnly: false).ToList();
		Duration = 3;
		foreach (GameObject item in Pyrokinesis.Pyro(Actor, GetLevel(Actor), cells, 9999))
		{
			Point2D key = item.CurrentCell.PathDifferenceTo(cell);
			Field[key] = item;
			item.RequirePart<ExistenceSupport>().SupportedBy = Actor;
		}
		base.Activate(Actor);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Field.Count > 0 && --Duration < 0)
		{
			DestroyPyroField();
		}
		return base.HandleEvent(E);
	}

	public void DestroyPyroField()
	{
		Validate();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			item.Value.Obliterate();
		}
		Field.Clear();
	}

	public void SuspendPyroField()
	{
		Validate();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			item.Value.RemoveFromContext();
		}
	}

	public void DesuspendPyroField(GameObject Actor = null, bool Validated = false)
	{
		if (!Validated)
		{
			Validate();
		}
		Cell cell = (Actor ?? base.User)?.CurrentCell;
		Zone zone = cell?.ParentZone;
		if (zone == null || zone.IsWorldMap())
		{
			DestroyPyroField();
			return;
		}
		Remove.Clear();
		foreach (KeyValuePair<Point2D, GameObject> item in Field)
		{
			Point2D key = item.Key;
			GameObject value = item.Value;
			if (value.CurrentCell != null)
			{
				continue;
			}
			Cell cellGlobal = zone.GetCellGlobal(cell.X + key.x, cell.Y + key.y);
			if (cellGlobal == null)
			{
				value.Obliterate();
				Remove.Add(key);
				continue;
			}
			cellGlobal.AddObject(value);
			if (value.CurrentCell != cellGlobal)
			{
				value.Obliterate();
				Remove.Add(key);
			}
		}
		foreach (Point2D item2 in Remove)
		{
			Field.Remove(item2);
		}
	}
}
