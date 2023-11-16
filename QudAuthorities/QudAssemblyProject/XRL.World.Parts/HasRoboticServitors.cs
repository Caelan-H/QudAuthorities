using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class HasRoboticServitors : IPart
{
	public string numberOfRoboticServitors = "2-4";

	public bool stripGear;

	public bool servitorsPlaced;

	public HasRoboticServitors()
	{
	}

	public HasRoboticServitors(string numberOfRoboticServitors, bool stripGear)
		: this()
	{
		this.numberOfRoboticServitors = numberOfRoboticServitors;
		this.stripGear = stripGear;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return !servitorsPlaced;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!servitorsPlaced)
		{
			servitorsPlaced = true;
			Cell cell = ParentObject.CurrentCell;
			int tier = ParentObject.GetTier();
			List<string> list = new List<string>();
			list.Add("DynamicInheritsTable:Creature:Tier" + tier);
			list.Add("DynamicInheritsTable:Creature:Tier" + tier);
			list.Add("DynamicInheritsTable:Creature:Tier" + tier);
			if (tier >= 8)
			{
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier - 1));
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier - 2));
			}
			else if (tier <= 1)
			{
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier + 1));
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier + 2));
			}
			else
			{
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier + 1));
				list.Add("DynamicInheritsTable:Creature:Tier" + (tier - 1));
			}
			int i = 0;
			for (int num = numberOfRoboticServitors.RollCached(); i < num; i++)
			{
				GameObjectBlueprint gameObjectBlueprint = null;
				int num2 = 0;
				while (++num2 < 50)
				{
					GameObjectBlueprint gameObjectBlueprint2 = GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom(list.GetRandomElement()).Blueprint];
					if (IsSuitableServitor(gameObjectBlueprint2))
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
						gameObject.AddPart(new RoboticServitor(ParentObject, null, null, null, null, stripGear));
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

	public static bool IsSuitableServitor(GameObjectBlueprint BP)
	{
		if (!BP.HasPart("Body"))
		{
			return false;
		}
		if (!BP.HasPart("Robot"))
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
