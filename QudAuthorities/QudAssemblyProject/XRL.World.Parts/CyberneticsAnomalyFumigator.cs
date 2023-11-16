using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsAnomalyFumigator : IPart
{
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

	public override bool HandleEvent(ImplantedEvent E)
	{
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Anomaly Fumigator", "CommandToggleAnomalyFumigator", "Cybernetics", null, "\a", null, Toggleable: true, DefaultToggleState: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleAnomalyFumigator" && E.Actor == ParentObject.Implantee)
		{
			ParentObject.Implantee.ToggleActivatedAbility(ActivatedAbilityID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee == null || implantee.OnWorldMap() || !implantee.IsActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			return;
		}
		Cell cell = implantee.CurrentCell;
		if (cell != null)
		{
			List<Cell> localEmptyAdjacentCells = cell.GetLocalEmptyAdjacentCells();
			int num = Stat.Random(0, 2);
			while (num > 0 && localEmptyAdjacentCells.Count > 0)
			{
				GameObject gameObject = GameObject.create("NormalityGas");
				int num3 = ((gameObject.GetPart("Gas") as Gas).Density = GetAvailableComputePowerEvent.AdjustUp(implantee, Stat.Random(10, 120)));
				Cell randomElement = localEmptyAdjacentCells.GetRandomElement();
				randomElement.AddObject(gameObject);
				localEmptyAdjacentCells.Remove(randomElement);
				num--;
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
