using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FearAura : BaseMutation
{
	public int Chance = 25;

	public int EnergyCost = 1000;

	public new Guid ActivatedAbilityID;

	public int SpinTimer;

	public int nCooldown;

	public FearAura()
	{
		DisplayName = "Fear Aura";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override string GetLevelText(int Level)
	{
		return "You're really scary.\n";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			nCooldown--;
			if (nCooldown < 0 && Chance.in100())
			{
				nCooldown = 20 + Stat.Random(1, 6);
				PulseAura(ParentObject);
			}
			if (!ParentObject.IsPlayer())
			{
				UseEnergy(EnergyCost);
				if (EnergyCost != 0)
				{
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}

	public static void PulseAura(GameObject Actor)
	{
		List<Cell> list = Actor?.CurrentCell?.GetLocalAdjacentCells();
		if (list == null)
		{
			return;
		}
		Actor.ParticleBlip("&W!");
		foreach (Cell item in list)
		{
			foreach (GameObject @object in item.Objects)
			{
				if (@object.pBrain != null)
				{
					Mental.PerformAttack(ApplyFear, Actor, @object, null, "Terrify Aura", "1d8+4", 8388609, "1d3".RollCached());
				}
			}
		}
	}

	public static bool ApplyFear(MentalAttackEvent E)
	{
		if (!E.Defender.FireEvent("CanApplyFear"))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check(E.Defender, "Terrified"))
		{
			return false;
		}
		if (E.Penetrations > 0)
		{
			Terrified e = new Terrified(E.Magnitude, E.Attacker, Panicked: true, Psionic: true);
			if (E.Defender.ApplyEffect(e))
			{
				return true;
			}
		}
		if (E.Defender.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You feel uneasy.", 'K');
		}
		return false;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("FearAura", "CommandFearAura", "Physical Mutation", null, "®");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
