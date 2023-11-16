using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SpacetimeVortex : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public SpacetimeVortex()
	{
		DisplayName = "Space-Time Vortex";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandSpaceTimeVortex");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("chance", 2);
		E.Add("time", 1);
		E.Add("travel", 1);
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You sunder spacetime, sending things nearby careening through a tear in the cosmic fabric.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Summons a vortex that swallows everything in its path.\n";
		int num = 550 - 50 * Level;
		if (num < 5)
		{
			num = 5;
		}
		if (Level > 10)
		{
			text = text + "Bonus duration: {{rules|" + (Level - 10) + "}} rounds\n";
		}
		text = text + "Cooldown: {{rules|" + num + "}} rounds\n";
		text += "You may enter the vortex to teleport to a random location in Qud.\n";
		return text + "+200 reputation with {{w|highly entropic beings}}";
	}

	public void Vortex(Cell C)
	{
		if (C != null)
		{
			List<Cell> adjacentCells = C.GetAdjacentCells();
			if (ParentObject.IsPlayer())
			{
				adjacentCells.Add(C);
			}
			Cell randomElement = adjacentCells.GetRandomElement();
			GameObject gameObject = GameObject.create("Space-Time Vortex");
			Temporary temporary = gameObject.GetPart("Temporary") as Temporary;
			temporary.Duration = Stat.Random(15, 18);
			if (base.Level > 10)
			{
				temporary.Duration += base.Level - 10;
			}
			randomElement.AddObject(gameObject);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 5 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandSpaceTimeVortex");
			}
		}
		else if (E.ID == "CommandSpaceTimeVortex")
		{
			Zone currentZone = ParentObject.CurrentZone;
			if (currentZone == null)
			{
				return false;
			}
			if (currentZone.IsWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You may not use this mutation on the world map.");
				}
				return false;
			}
			if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return false;
			}
			Cell cell = PickDestinationCell(5, AllowVis.OnlyVisible, Locked: false);
			if (cell == null)
			{
				return false;
			}
			if (cell.PathDistanceTo(ParentObject.CurrentCell) > 5)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("That target is out of range! (5 squares)");
				}
				return false;
			}
			Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
			if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
			{
				return false;
			}
			int turns = Math.Max(550 - 50 * base.Level, 5);
			CooldownMyActivatedAbility(ActivatedAbilityID, turns);
			UseEnergy(1000, "Mental Mutation");
			Vortex(cell);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Space-Time Vortex", "CommandSpaceTimeVortex", "Mental Mutation", null, "\u0015", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
