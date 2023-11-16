using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsStasisProjector : IPart
{
	public int BaseCoverage = 6;

	public int BaseCooldown = 100;

	public string BaseDuration = "6-8";

	public string commandId = "";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		commandId = Guid.NewGuid().ToString();
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Project Stasis Field", commandId, "Cybernetics", null, "é");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == commandId && E.Actor == ParentObject.Implantee)
		{
			if (!E.Actor.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", E.Actor, "Device", this), E))
			{
				return false;
			}
			int coverage = GetCoverage(E.Actor);
			List<Cell> list = null;
			if (E.Actor.IsPlayer())
			{
				list = PickFieldAdjacent(coverage, E.Actor);
			}
			else
			{
				Cell cell = E.Actor.CurrentCell;
				List<GameObject> list2 = cell.ParentZone.FastCombatSquareVisibility(cell.X, cell.Y, 25, E.Actor, E.Actor.IsHostileTowards);
				if (list2.Count > 0)
				{
					int num = 0;
					List<GameObject> list3 = Event.NewGameObjectList();
					foreach (GameObject item in list2)
					{
						if (item.InAdjacentCellTo(E.Actor))
						{
							list3.Add(item);
						}
					}
					if (list3.Count > 0)
					{
						list = new List<Cell>(list2.Count);
						GameObject randomElement = list3.GetRandomElement();
						list2.Remove(randomElement);
						list.Add(randomElement.CurrentCell);
						List<Cell> list4 = new List<Cell>();
						while (list2.Count > 1 && list.Count < coverage && ++num < 100)
						{
							Cell cell2 = list[list.Count - 1];
							Cell cell3 = null;
							list4.Clear();
							foreach (Cell localCardinalAdjacentCell in cell2.GetLocalCardinalAdjacentCells())
							{
								if (!list.Contains(localCardinalAdjacentCell) && !localCardinalAdjacentCell.Objects.Contains(E.Actor))
								{
									list4.Add(localCardinalAdjacentCell);
								}
							}
							list4.ShuffleInPlace();
							foreach (Cell item2 in list4)
							{
								foreach (GameObject item3 in list2)
								{
									if (item2.Objects.Contains(item3))
									{
										cell3 = item2;
										break;
									}
								}
								if (cell3 != null)
								{
									break;
								}
							}
							if (cell3 == null)
							{
								int num2 = 0;
								foreach (Cell item4 in list4)
								{
									int num3 = 0;
									foreach (GameObject item5 in list2)
									{
										num3 += 100 - item5.DistanceTo(item4);
									}
									if (num3 > num2)
									{
										cell3 = item4;
										num2 = num3;
									}
								}
							}
							if (cell3 == null)
							{
								break;
							}
							list.Add(cell3);
							foreach (GameObject @object in cell3.Objects)
							{
								list2.Remove(@object);
							}
						}
					}
				}
			}
			if (list == null || list.Count == 0)
			{
				return false;
			}
			E.Actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown(E.Actor));
			E.Actor.UseEnergy(1000, "Cybernetics Stasis Projector");
			Event e = Event.New("CheckRealityDistortionAccessibility");
			foreach (Cell item6 in list)
			{
				if (item6.FireEvent(e))
				{
					GameObject gameObject = GameObject.create("Stasisfield");
					(gameObject.GetPart("Forcefield") as Forcefield).Creator = E.Actor;
					gameObject.AddPart(new Temporary(GetDuration() + 1));
					Phase.carryOver(E.Actor, gameObject);
					item6.AddObject(gameObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public int GetCoverage(GameObject who = null)
	{
		return GetAvailableComputePowerEvent.AdjustUp(who ?? ParentObject.Implantee, BaseCoverage);
	}

	public int GetDuration(GameObject who = null)
	{
		return GetAvailableComputePowerEvent.AdjustUp(who ?? ParentObject.Implantee, BaseDuration.RollCached());
	}

	public int GetCooldown(GameObject who = null)
	{
		return GetAvailableComputePowerEvent.AdjustDown(who ?? ParentObject.Implantee, BaseCooldown);
	}
}
