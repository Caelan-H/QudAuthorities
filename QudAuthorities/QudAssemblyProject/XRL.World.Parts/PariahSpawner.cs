using Qud.API;

namespace XRL.World.Parts;

public class PariahSpawner : IPart
{
	public bool DoesWander = true;

	[FieldSaveVersion(240)]
	public bool IsUnique;

	public override bool SameAs(IPart p)
	{
		if ((p as PariahSpawner).DoesWander != DoesWander)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		GameObject gameObject = GeneratePariah(-1, AlterName: true, IsUnique);
		if (gameObject.pBrain != null)
		{
			if (DoesWander)
			{
				gameObject.pBrain.Wanders = true;
				gameObject.pBrain.WandersRandomly = true;
				gameObject.RequirePart<AIShopper>();
			}
			else
			{
				gameObject.pBrain.Wanders = false;
				gameObject.pBrain.WandersRandomly = false;
				gameObject.RequirePart<Sitting>();
			}
			gameObject.MakeActive();
		}
		gameObject.SetStringProperty("SpawnedFrom", ParentObject.Blueprint);
		cell.AddObject(gameObject);
		ParentObject.Destroy();
		return base.HandleEvent(E);
	}

	public static GameObject GeneratePariah(int level = -1, bool AlterName = true, bool IsUnique = false)
	{
		return GeneratePariah((level == -1) ? EncountersAPI.GetACreature() : EncountersAPI.GetCreatureAroundLevel(level), AlterName, IsUnique);
	}

	public static GameObject GeneratePariah(GameObject pariah, bool AlterName = true, bool IsUnique = false)
	{
		Pariah.MakePariah(pariah, AlterName, IsUnique);
		return pariah;
	}
}
