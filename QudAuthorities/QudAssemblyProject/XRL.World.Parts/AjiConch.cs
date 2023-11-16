using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class AjiConch : IPart
{
	public const string ABL_NAME = "Blow Aji Conch";

	public const string ABL_CMD = "ActivateAjiConch";

	public const int CONE_LENGTH = 4;

	public const int CONE_ANGLE = 30;

	public const int GAS_DENSITY = 800;

	public const int GAS_LEVEL = 5;

	[Obsolete("save compat")]
	public int Placeholder1;

	[Obsolete("save compat")]
	public int Placeholder2;

	[Obsolete("save compat")]
	public int Placeholder3;

	[Obsolete("save compat")]
	public string Placeholder4;

	public string PID;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (PID == null)
		{
			PID = "ActivateAjiConch " + ParentObject.id;
		}
		E.Actor.RegisterPartEvent(this, PID);
		ActivatedAbilityID = E.Actor.AddActivatedAbility("Blow Aji Conch", PID, "Items", null, "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		E.Actor.UnregisterPartEvent(this, PID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Equipped != null && ParentObject.Equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			E.AddAction("Blow", "blow", "ActivateAjiConch", null, 'b', FireOnActor: false, 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateAjiConch")
		{
			ActivateAjiConch();
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveItemList");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveItemList" && ParentObject.Equipped != null)
		{
			if (ParentObject.Equipped.IsActivatedAbilityUsable(ActivatedAbilityID) && E.GetIntParameter("Distance") <= 4)
			{
				E.AddAICommand("ActivateAjiConch", 1, ParentObject, Inv: true);
			}
		}
		else if (E.ID == PID && ParentObject.Equipped != null)
		{
			ActivateAjiConch();
		}
		return base.FireEvent(E);
	}

	private void ActivateAjiConch()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped != null && equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			string text = PopulationManager.GenerateOne("DynamicObjectsTable:AjiConch")?.Blueprint;
			if (!text.IsNullOrEmpty() && GasGeneration.PickGasCone(equipped, text, 4, 30, 800, 5, "Blow Aji Conch"))
			{
				equipped.CooldownActivatedAbility(ActivatedAbilityID, 150);
			}
		}
	}
}
