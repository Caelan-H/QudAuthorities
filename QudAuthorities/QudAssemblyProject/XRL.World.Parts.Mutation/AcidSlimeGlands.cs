using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class AcidSlimeGlands : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public AcidSlimeGlands()
	{
		DisplayName = "Acid Slime Glands";
		Type = "Physical";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		Object.RegisterPartEvent(this, "CommandSpitAcid");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You spit a puddle of corrosive acid.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("" + "Cooldown: 10 rounds\n", "Range: 8\n"), "Area: 3x3\n"), "Covers the area in acidic slime\n");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (intParameter <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasLOSTo(gameObjectParameter, IncludeSolid: true, UseTargetability: true))
			{
				E.AddAICommand("CommandSpitAcid");
			}
		}
		else if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Acid"))
			{
				damage.Amount = 0;
				return false;
			}
		}
		else if (E.ID == "CommandSpitAcid")
		{
			List<Cell> list = PickBurst(1, 8, bLocked: false, AllowVis.OnlyVisible);
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(ParentObject) > 9)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("That is out of range! (8 squares)");
					}
					return false;
				}
			}
			SlimeGlands.SlimeAnimation("&G", ParentObject.CurrentCell, list[0]);
			CooldownMyActivatedAbility(ActivatedAbilityID, 40);
			int num = 0;
			foreach (Cell item2 in list)
			{
				if (num == 0 || 80.in100())
				{
					item2.AddObject("AcidPool");
				}
				num++;
			}
			UseEnergy(1000, "Physical Mutation Acid Slime Glands");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spit Acid", "CommandSpitAcid", "Physical Mutation", null, "­");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
