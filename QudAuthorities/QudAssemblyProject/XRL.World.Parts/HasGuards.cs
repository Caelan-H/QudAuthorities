using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class HasGuards : IPart
{
	public string numberOfGuards = "2-3";

	public bool guardsPlaced;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return !guardsPlaced;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!guardsPlaced)
		{
			guardsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			string populationName = "DynamicInheritsTable:Creature:Tier" + Tier.Constrain(cell.ParentZone.NewTier + 1);
			int i = 0;
			for (int num = numberOfGuards.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 10)
				{
					GameObjectBlueprint gameObjectBlueprint2 = GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom(populationName).Blueprint];
					if (IsSuitableGuard(gameObjectBlueprint2))
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
						gameObject.AddPart(new HiredGuard(ParentObject));
						firstEmptyAdjacentCell.AddObject(gameObject).MakeActive();
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool IsSuitableGuard(GameObjectBlueprint BP)
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
