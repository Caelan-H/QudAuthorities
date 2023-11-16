using System.Collections.Generic;
using XRL.Core;
using XRL.Names;
using XRL.World.Parts;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class GenericHero : IPart
{
	public int HitpointMultiplier = 2;

	public string ExtraEquipment = "Junk 2R,Junk 3";

	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public virtual void AfterNameGeneration()
	{
	}

	public virtual List<ObjectRetinueEntry> GenerateRetinue()
	{
		return new List<ObjectRetinueEntry>();
	}

	public virtual void EquipFollower(GameObject Follower)
	{
	}

	private void _GenerateRetinue(Cell Ce)
	{
		Physics obj = ParentObject.GetPart("Physics") as Physics;
		List<Cell> list = new List<Cell>();
		obj.CurrentCell.GetAdjacentCells(4, list);
		List<Cell> list2 = new List<Cell>();
		foreach (Cell item in list)
		{
			if (item.IsEmpty())
			{
				list2.Add(item);
			}
		}
		foreach (ObjectRetinueEntry item2 in GenerateRetinue())
		{
			foreach (GameObject item3 in item2.Generate())
			{
				EquipFollower(item3);
				item3.GetPart<Brain>().PartyLeader = ParentObject;
				Cell randomElement = list2.GetRandomElement();
				randomElement.AddObject(item3);
				XRLCore.Core.Game.ActionManager.AddActiveObject(item3);
				list2.Remove(randomElement);
			}
		}
	}

	public virtual string MakeName()
	{
		return "{{M|" + NameMaker.MakeName(ParentObject, null, null, null, null, null, null, null, null, "Hero", null, FailureOkay: false, SpecialFaildown: true) + "}}";
	}

	public virtual void GenerateEquipment()
	{
		string[] array = ExtraEquipment.Split(',');
		foreach (string tableName in array)
		{
			ParentObject.TakeObject(EncounterFactory.Factory.RollOneFromTable(tableName), Silent: false, 0);
		}
	}

	public virtual void ModifyStats()
	{
		ParentObject.GetStat("Hitpoints").BaseValue *= HitpointMultiplier;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			try
			{
				if (bCreated)
				{
					return true;
				}
				bCreated = true;
				ParentObject.DisplayName = MakeName();
				ParentObject.HasProperName = true;
				AfterNameGeneration();
				_GenerateRetinue(E.GetParameter("Cell") as Cell);
				GenerateEquipment();
				ModifyStats();
			}
			catch
			{
			}
		}
		return true;
	}
}
