using System;

namespace XRL.World.Parts;

[Serializable]
public class HasThralls : IPart
{
	public string numberOfThralls = "2-4";

	public bool stripGear;

	public bool thrallsPlaced;

	public HasThralls()
	{
	}

	public HasThralls(string numberOfThralls, bool stripGear)
		: this()
	{
		this.numberOfThralls = numberOfThralls;
		this.stripGear = stripGear;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return !thrallsPlaced;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!thrallsPlaced)
		{
			thrallsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			string populationName = "DynamicInheritsTable:Creature:Tier" + ParentObject.GetTier();
			int i = 0;
			for (int num = numberOfThralls.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 10)
				{
					GameObjectBlueprint gameObjectBlueprint2 = GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom(populationName).Blueprint];
					if (IsSuitableThrall(gameObjectBlueprint2))
					{
						gameObjectBlueprint = gameObjectBlueprint2;
						break;
					}
				}
				if (gameObjectBlueprint != null)
				{
					Cell firstEmptyAdjacentCell = cell.GetFirstEmptyAdjacentCell(1, 2);
					if (firstEmptyAdjacentCell != null)
					{
						GameObject gameObject = GameObject.create(gameObjectBlueprint.Name);
						gameObject.AddPart(new PsychicThrall(ParentObject, "Seekers", null, "and psychic thrall", "PsychicThrall", stripGear));
						firstEmptyAdjacentCell.AddObject(gameObject).MakeActive();
					}
				}
			}
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool IsSuitableThrall(GameObjectBlueprint BP)
	{
		if (!BP.HasPart("Body"))
		{
			return false;
		}
		if (!BP.HasPart("Brain"))
		{
			return false;
		}
		if (!BP.HasPart("Combat"))
		{
			return false;
		}
		if (BP.HasPart("MentalShield"))
		{
			return false;
		}
		if (BP.GetPartParameter("Brain", "Mobile", "true") == "false")
		{
			return false;
		}
		if (BP.HasPart("AIWallWalker"))
		{
			return false;
		}
		if (BP.GetxTag("Grammar", "Proper", "false") == "true")
		{
			return false;
		}
		return true;
	}
}
