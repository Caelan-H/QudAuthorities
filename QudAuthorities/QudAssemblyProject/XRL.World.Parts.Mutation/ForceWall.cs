using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ForceWall : BaseMutation
{
	public new Guid ActivatedAbilityID;

	public ForceWall()
	{
		DisplayName = "Force Wall";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == CommandEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandForceWall")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return false;
			}
			List<Cell> list = null;
			if (ParentObject.IsPlayer())
			{
				list = PickField(9);
			}
			else
			{
				Cell cell = ParentObject.CurrentCell;
				List<GameObject> list2 = cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 25, "ForceWallTarget", ParentObject);
				if (list2.Count > 0)
				{
					list = new List<Cell>(list2.Count);
					foreach (GameObject item in list2)
					{
						list.Add(item.CurrentCell);
					}
				}
				else
				{
					if (ParentObject.Target == null)
					{
						return false;
					}
					if (DoIHaveAMissileWeapon() || ParentObject.HasPart("Cryokinesis") || ParentObject.HasPart("Pyrokinesis") || ParentObject.HasPart("FlamingHands") || ParentObject.HasPart("FreezingHands"))
					{
						list = ParentObject.Target.CurrentCell.GetLocalAdjacentCells();
					}
					else
					{
						string directionFromCell = cell.GetDirectionFromCell(ParentObject.Target.CurrentCell);
						Cell cellFromDirection = ParentObject.Target.CurrentCell.GetCellFromDirection(directionFromCell);
						if (cellFromDirection != null)
						{
							list = new List<Cell>(9) { cellFromDirection };
							string[] orthogonalDirections = Directions.GetOrthogonalDirections(directionFromCell);
							Cell cell2 = cellFromDirection;
							Cell cell3 = cellFromDirection;
							for (int i = 0; i < 4; i++)
							{
								cell2 = cell2?.GetCellFromDirection(orthogonalDirections[0]);
								cell3 = cell3?.GetCellFromDirection(orthogonalDirections[1]);
								if (cell2 != null)
								{
									list.Add(cell2);
								}
								if (cell3 != null)
								{
									list.Add(cell3);
								}
							}
						}
					}
				}
			}
			if (list == null || list.Count == 0)
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown());
			UseEnergy(1000, "Mental Mutation Force Wall");
			int duration = GetDuration(base.Level) + 1;
			Event e = Event.New("CheckRealityDistortionAccessibility");
			foreach (Cell item2 in list)
			{
				if (item2.FireEvent(e))
				{
					GameObject gameObject = GameObject.create("Forcefield");
					Forcefield obj = gameObject.GetPart("Forcefield") as Forcefield;
					obj.Creator = ParentObject;
					obj.RejectOwner = false;
					gameObject.AddPart(new Temporary(duration));
					Phase.carryOver(ParentObject, gameObject);
					item2.AddObject(gameObject);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You generate a wall of force that protects you from your enemies.";
	}

	public int GetCooldown()
	{
		return 100;
	}

	public int GetDuration(int Level)
	{
		return 14 + Level * 2;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(str2: GetCooldown().ToString(), str0: "Creates 9 contiguous squares of immobile force field\n" + "Duration: {{rules|" + GetDuration(Level) + "}} rounds\n", str1: "Cooldown: ", str3: " rounds\n"), "You may fire missile weapons through the force field.");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (!ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
			{
				return true;
			}
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				Cell cell = ParentObject.CurrentCell;
				if (cell != null && cell.ParentZone != null)
				{
					GameObject gameObject = cell.ParentZone.FastSquareVisibilityFirst(cell.X, cell.Y, 25, "ForceWallTarget", ParentObject);
					int intParameter = E.GetIntParameter("Distance");
					if (gameObject != null && intParameter < ParentObject.DistanceTo(gameObject))
					{
						E.AddAICommand("CommandForceWall");
					}
					else
					{
						gameObject = E.GetGameObjectParameter("Target");
						if (gameObject != null)
						{
							Cell cell2 = gameObject.CurrentCell;
							if (cell2 != null && cell2.ParentZone != null && !cell2.ParentZone.FastSquareVisibilityAny(cell2.X, cell2.Y, 5, "Forcefield", ParentObject))
							{
								E.AddAICommand("CommandForceWall");
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Force Wall", "CommandForceWall", "Mental Mutation", null, "°", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
