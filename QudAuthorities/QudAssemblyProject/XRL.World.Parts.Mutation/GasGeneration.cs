using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class GasGeneration : BaseMutation
{
	public string GasObject = "AcidGas";

	public new Guid ActivatedAbilityID = Guid.Empty;

	public int BillowsTimer = 20;

	public int GasObjectDensity = 800;

	[NonSerialized]
	private bool AddSeeping;

	[NonSerialized]
	private bool AlreadySeeping;

	[NonSerialized]
	private string Description;

	[NonSerialized]
	private string GasType;

	[NonSerialized]
	private string ReleaseAbilityCommand;

	public GasGeneration()
	{
		SyncFromBlueprint();
	}

	public GasGeneration(string GasObject)
	{
		this.GasObject = GasObject;
		SyncFromBlueprint();
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		GasGeneration obj = base.DeepCopy(Parent, MapInv) as GasGeneration;
		obj.SyncFromBlueprint();
		return obj;
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		SyncFromBlueprint();
	}

	private GameObjectBlueprint GeneratedGasBlueprint()
	{
		return GameObjectFactory.Factory.GetBlueprint(GasObject);
	}

	protected void SyncFromBlueprint()
	{
		ReleaseAbilityCommand = "GasGenerationCommand" + GasObject;
		GameObjectBlueprint gameObjectBlueprint = GeneratedGasBlueprint();
		DisplayName = gameObjectBlueprint.GetTag("GasGenerationName", null) ?? (GasObject + " Generation");
		GasType = gameObjectBlueprint.GetPartParameter("Gas", "GasType");
		AddSeeping = gameObjectBlueprint.GetTag("GasGenerationAddSeeping").EqualsNoCase("true");
		AlreadySeeping = gameObjectBlueprint.GetPartParameter("Gas", "Seeping", "false").EqualsNoCase("true");
		string partParameter = gameObjectBlueprint.GetPartParameter("Render", "DisplayName");
		if (!string.IsNullOrEmpty(partParameter))
		{
			Description = "You release a burst of " + partParameter + " around yourself.";
		}
		else
		{
			Description = "You release a gaseous burst around yourself.";
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == CheckGasCanAffectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckGasCanAffectEvent E)
	{
		if (E.Gas.GasType == GasType)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, GetReleaseAbilityCommand());
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return Description;
	}

	public virtual int GetReleaseDuration(int Level)
	{
		return 1 + Level / 2;
	}

	public virtual int GetReleaseCooldown(int Level)
	{
		return 40;
	}

	public virtual string GetReleaseAbilityName()
	{
		return "Gas Generation";
	}

	public virtual string GetReleaseAbilityCommand()
	{
		return ReleaseAbilityCommand;
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text = text + "Releases gas for {{rules|" + GetReleaseDuration(Level) + "}} rounds";
		if (Level != base.Level)
		{
			string tag = GeneratedGasBlueprint().GetTag("LevelEffectDescription");
			if (tag != null)
			{
				text = ((Level <= base.Level) ? (text + "\n{{rules|Decreased " + tag + "}}") : (text + "\n{{rules|Increased " + tag + "}}"));
			}
		}
		return text + "\nCooldown: " + GetReleaseCooldown(Level) + " rounds";
	}

	public override bool Render(RenderEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			string tag = GeneratedGasBlueprint().GetTag("ActivationColorString");
			if (tag != null)
			{
				E.ColorString = tag;
			}
		}
		return true;
	}

	public virtual int GetGasDensityForLevel(int Level)
	{
		return GasObjectDensity;
	}

	public virtual void PumpGas()
	{
		List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
		List<Cell> list = new List<Cell>(8);
		foreach (Cell item in adjacentCells)
		{
			if (AddSeeping || AlreadySeeping || !item.IsOccluding())
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			list.Add(ParentObject.CurrentCell);
		}
		Phase.carryOverPrep(ParentObject, out var FX, out var FX2);
		Event @event = Event.New("CreatorModifyGas", "Gas", (object)null);
		foreach (Cell item2 in list)
		{
			GameObject gameObject = GameObject.create(GasObject);
			Gas gas = gameObject.GetPart("Gas") as Gas;
			gas.Creator = ParentObject;
			gas.Density = GetGasDensityForLevel(base.Level) / list.Count;
			if (AddSeeping)
			{
				gas.Seeping = true;
			}
			gas.Level = base.Level;
			Phase.carryOver(ParentObject, gameObject, FX, FX2);
			@event.SetParameter("Gas", gas);
			ParentObject.FireEvent(@event);
			item2.AddObject(gameObject);
		}
	}

	public static bool PickGasCone(GameObject Actor, string Blueprint, int Length, int Angle, int Density, int Level, string Label = null)
	{
		List<Cell> list = Actor.pPhysics.PickCone(Length, Angle, AllowVis.Any, null, Label);
		if (list == null)
		{
			return false;
		}
		GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(Blueprint);
		bool flag = blueprint.GetTag("GasGenerationAddSeeping").EqualsNoCase("true") || blueprint.GetPartParameter("Gas", "Seeping").EqualsNoCase("true");
		if (!flag)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].IsSolid())
				{
					list.RemoveAt(num);
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(Actor.CurrentCell);
		}
		Event @event = Event.New("CreatorModifyGas", "Gas", (object)null);
		foreach (Cell item in list)
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(blueprint);
			Gas part = gameObject.GetPart<Gas>();
			if (flag)
			{
				part.Seeping = true;
			}
			part.Creator = Actor;
			part.Density = Density / list.Count;
			part.Level = Level;
			@event.SetParameter("Gas", part);
			Actor.FireEvent(@event);
			item.AddObject(gameObject);
			The.Core.RenderDelay(25, Interruptible: false);
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				if (ParentObject.OnWorldMap())
				{
					BillowsTimer = -1;
				}
				else
				{
					BillowsTimer--;
				}
				if (BillowsTimer < 0)
				{
					ToggleMyActivatedAbility(ActivatedAbilityID);
					DidX("stop", "releasing " + GeneratedGasBlueprint().DisplayName());
				}
				else
				{
					PumpGas();
				}
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand(GetReleaseAbilityCommand());
			}
		}
		else if (E.ID == GetReleaseAbilityCommand())
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetReleaseCooldown(base.Level));
			ToggleMyActivatedAbility(ActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				BillowsTimer = GetReleaseDuration(base.Level);
				PumpGas();
				DidX("start", "releasing " + GeneratedGasBlueprint().DisplayName());
			}
			UseEnergy(1000, "Physical Mutation Gas Generation");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility(GetReleaseAbilityName(), GetReleaseAbilityCommand(), "Physical Mutation", null, "á", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
