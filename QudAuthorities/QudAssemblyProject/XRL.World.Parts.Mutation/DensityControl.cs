using System;
using XRL.Messages;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class DensityControl : BaseMutation
{
	public Guid GrowActivatedAbilityID = Guid.Empty;

	public Guid ShrinkActivatedAbilityID = Guid.Empty;

	public DensityState State = DensityState.Normal;

	public int DVBonus;

	public int DVPenalty;

	public DensityControl()
	{
		DisplayName = "Density Control";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetEnergyCostEvent.ID)
		{
			return ID == GetMaxCarriedWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (State == DensityState.Grow)
		{
			if (E.Type != null && Type.Contains("Movement"))
			{
				E.LinearReduction += 50 * base.Level;
			}
		}
		else if (State == DensityState.Shrink && E.Type != null && E.Type.Contains("Movement"))
		{
			E.LinearReduction += 50 * base.Level;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		if (State == DensityState.Grow)
		{
			E.AdjustWeight(1.0 + 0.05 * (double)base.Level);
		}
		else if (State == DensityState.Shrink)
		{
			E.AdjustWeight(1.0 - 0.025 * (double)base.Level);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandDensityGrow");
		Object.RegisterPartEvent(this, "CommandDensityShrink");
		base.Register(Object);
	}

	public override string GetLevelText(int Level)
	{
		float num = 0.05f * (float)Level;
		int num2 = 1010 - 50 * Level;
		string text = "Cooldown: " + num2.Things("turn") + ".\n";
		text = text + "Grow: -" + Level + "DV +" + 15 * Level + "% movespeed +" + num + "% carry capacity\n";
		return text + "Shrink: +" + Level + "DV -" + 15 * Level + "% movespeed -" + num + "% carry capacity\n";
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Level > 5)
		{
			if (State == DensityState.Shrink)
			{
				E.RenderString = "ù";
			}
			if (State == DensityState.Grow)
			{
				E.RenderString = "\u0002";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandDensityGrow")
		{
			if (State == DensityState.Shrink)
			{
				State = DensityState.Normal;
				EnableMyActivatedAbility(GrowActivatedAbilityID);
				EnableMyActivatedAbility(ShrinkActivatedAbilityID);
				ParentObject.Statistics["DV"].Bonus -= DVBonus;
				ParentObject.Statistics["DV"].Penalty -= DVPenalty;
				DVBonus = 0;
				DVPenalty = 0;
				ParentObject.RemoveEffect("Shrunken");
				if (ParentObject.IsPlayer())
				{
					MessageQueue.AddPlayerMessage("You return to normal size.");
				}
			}
			else if (State == DensityState.Normal)
			{
				State = DensityState.Grow;
				DisableMyActivatedAbility(GrowActivatedAbilityID);
				EnableMyActivatedAbility(ShrinkActivatedAbilityID);
				DVPenalty = base.Level;
				ParentObject.Statistics["DV"].Penalty += DVPenalty;
				if (!ParentObject.HasEffect("Enlarged"))
				{
					ParentObject.ApplyEffect(new Enlarged(1));
				}
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You grow larger!");
				}
			}
			CooldownMyActivatedAbility(GrowActivatedAbilityID, 100 - 5 * base.Level);
			CooldownMyActivatedAbility(ShrinkActivatedAbilityID, 100 - 5 * base.Level);
		}
		else if (E.ID == "CommandDensityShrink")
		{
			if (State == DensityState.Grow)
			{
				State = DensityState.Normal;
				EnableMyActivatedAbility(GrowActivatedAbilityID);
				EnableMyActivatedAbility(ShrinkActivatedAbilityID);
				ParentObject.Statistics["DV"].Bonus -= DVBonus;
				ParentObject.Statistics["DV"].Penalty -= DVPenalty;
				DVBonus = 0;
				DVPenalty = 0;
				ParentObject.RemoveEffect("Enlarged");
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You return to normal size.");
				}
			}
			else if (State == DensityState.Normal)
			{
				State = DensityState.Shrink;
				EnableMyActivatedAbility(GrowActivatedAbilityID);
				DisableMyActivatedAbility(ShrinkActivatedAbilityID);
				DVBonus = base.Level;
				ParentObject.Statistics["DV"].Bonus += DVBonus;
				ParentObject.ApplyEffect(new Shrunken(1));
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You shrink!");
				}
			}
			CooldownMyActivatedAbility(GrowActivatedAbilityID, 100 - 5 * base.Level);
			CooldownMyActivatedAbility(ShrinkActivatedAbilityID, 100 - 5 * base.Level);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		State = DensityState.Normal;
		ParentObject.Statistics["DV"].Bonus -= DVBonus;
		ParentObject.Statistics["DV"].Penalty -= DVPenalty;
		DVBonus = 0;
		DVPenalty = 0;
		CarryingCapacityChangedEvent.Send(ParentObject);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		GrowActivatedAbilityID = AddMyActivatedAbility("Grow", "CommandDensityGrow", "Physical Mutation");
		ShrinkActivatedAbilityID = AddMyActivatedAbility("Shrink", "CommandDensityShrink", "Physical Mutation");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		State = DensityState.Normal;
		GO.Statistics["DV"].Bonus -= DVBonus;
		GO.Statistics["DV"].Penalty -= DVPenalty;
		DVBonus = 0;
		DVPenalty = 0;
		ParentObject.RemoveEffect("Shrunken");
		ParentObject.RemoveEffect("Enlarged");
		RemoveMyActivatedAbility(ref GrowActivatedAbilityID);
		RemoveMyActivatedAbility(ref ShrinkActivatedAbilityID);
		CarryingCapacityChangedEvent.Send(ParentObject);
		return base.Unmutate(GO);
	}
}
