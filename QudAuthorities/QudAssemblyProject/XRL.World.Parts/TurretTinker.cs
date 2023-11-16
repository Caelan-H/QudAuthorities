using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class TurretTinker : IPart
{
	public int TurretCooldown;

	public string Cooldown = "5-10";

	public string TurretType;

	public int MaxTurretsPlaced = 10;

	public override bool SameAs(IPart p)
	{
		TurretTinker turretTinker = p as TurretTinker;
		if (turretTinker.TurretCooldown != TurretCooldown)
		{
			return false;
		}
		if (turretTinker.Cooldown != Cooldown)
		{
			return false;
		}
		if (turretTinker.TurretType != TurretType)
		{
			return false;
		}
		if (turretTinker.MaxTurretsPlaced != MaxTurretsPlaced)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		if (!TurretType.Contains("Turret"))
		{
			return;
		}
		TurretType = null;
		GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(TurretType);
		if (blueprint == null)
		{
			return;
		}
		foreach (InventoryObject item in blueprint.Inventory)
		{
			GameObjectBlueprint blueprint2 = GameObjectFactory.Factory.GetBlueprint(item.Blueprint);
			if (blueprint2 != null && blueprint2.DescendsFrom("MissileWeapon"))
			{
				TurretType = item.Blueprint;
				break;
			}
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public string GetRandomTurretWeaponBlueprint()
	{
		string populationName = "DynamicInheritsTable:MissileWeapon:Tier" + ParentObject.GetTier();
		int num = 0;
		PopulationResult populationResult;
		while (true)
		{
			populationResult = PopulationManager.RollOneFrom(populationName);
			if (populationResult == null)
			{
				return null;
			}
			if (++num > 10)
			{
				return null;
			}
			GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(populationResult.Blueprint);
			if (blueprint != null)
			{
				string partParameter = blueprint.GetPartParameter("MissileWeapon", "FiresManually");
				if (string.IsNullOrEmpty(partParameter) || partParameter.EqualsNoCase("true"))
				{
					break;
				}
			}
		}
		return populationResult.Blueprint;
	}

	public string GetTurretWeaponBlueprintInstance()
	{
		string text = TurretType;
		if (text == "*")
		{
			text = GetRandomTurretWeaponBlueprint();
		}
		else if (text[0] == '@')
		{
			string text2 = text.Substring(1);
			if (!text2.Contains(":Tier"))
			{
				text2 = text2 + ":Tier" + ParentObject.GetTier();
			}
			if (text2.Contains("{zonetier}"))
			{
				text2 = text2.Replace("{zonetier}", ZoneManager.zoneGenerationContextTier.ToString());
			}
			text = PopulationManager.RollOneFrom(text2)?.Blueprint;
		}
		return text;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (MaxTurretsPlaced > 0 && !ParentObject.IsPlayer())
			{
				if (TurretCooldown > 0 && ParentObject.pBrain != null && !ParentObject.pBrain.HasGoal("PlaceTurretGoal"))
				{
					TurretCooldown--;
				}
				else if (TurretCooldown <= 0 && !ParentObject.pBrain.HasGoal("PlaceTurretGoal"))
				{
					Cell cell = null;
					int num = 0;
					do
					{
						cell = ParentObject.CurrentCell.ParentZone.GetEmptyReachableCells().GetRandomElement();
						if (cell.HasObjectWithTag("ExcavatoryTerrainFeature"))
						{
							cell = null;
						}
					}
					while (cell == null && ++num < 10);
					if (cell == null)
					{
						ParentObject.pBrain.PushGoal(new WanderRandomly(5));
					}
					else
					{
						ParentObject.pBrain.Goals.Clear();
						string turretWeaponBlueprintInstance = GetTurretWeaponBlueprintInstance();
						if (!string.IsNullOrEmpty(turretWeaponBlueprintInstance))
						{
							ParentObject.pBrain.PushGoal(new PlaceTurretGoal(cell.location, turretWeaponBlueprintInstance));
						}
					}
					TurretCooldown = Cooldown.RollCached();
				}
			}
		}
		else if (E.ID == "EnteredCell" && string.IsNullOrEmpty(TurretType))
		{
			TurretType = GetRandomTurretWeaponBlueprint();
		}
		return base.FireEvent(E);
	}
}
