using System;

namespace XRL.World.Parts;

[Serializable]
public class HasCompanions : IPart
{
	public string numberOfCompanions = "2-4";

	public bool stripGear;

	public bool companionsPlaced;

	public HasCompanions()
	{
	}

	public HasCompanions(string numberOfCompanions, bool stripGear)
		: this()
	{
		this.numberOfCompanions = numberOfCompanions;
		this.stripGear = stripGear;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return !companionsPlaced;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!companionsPlaced)
		{
			companionsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			string populationName = "DynamicInheritsTable:Creature:Tier" + ParentObject.GetTier();
			int i = 0;
			for (int num = numberOfCompanions.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 10)
				{
					GameObjectBlueprint gameObjectBlueprint2 = GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom(populationName).Blueprint];
					if (IsSuitableCompanion(gameObjectBlueprint2))
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
						gameObject.AddPart(new Companion(ParentObject, null, null, null, null, stripGear));
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

	public static bool IsSuitableCompanion(GameObjectBlueprint BP)
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
		if (!BP.HasStat("Level"))
		{
			return false;
		}
		if (BP.GetPartParameter("Brain", "Mobile", "true") == "false")
		{
			return false;
		}
		if (BP.GetPartParameter("Brain", "Aquatic", "false") == "true")
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
