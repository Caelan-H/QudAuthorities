using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class FeralLah : IPart
{
	public int FearChance = 100;

	public int FearCooldown;

	public int podCooldown;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIAttackRange");
		Object.RegisterPartEvent(this, "BeforeTakeAction");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public void Fear()
	{
		FearCooldown--;
		if (FearCooldown < 0 && FearChance.in100())
		{
			FearCooldown = 20 + Stat.Roll("1d6");
			FearAura.PulseAura(ParentObject);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && !ParentObject.IsFrozen() && podCooldown > 0)
		{
			podCooldown--;
		}
		if (E.ID == "BeforeTakeAction")
		{
			if (!ParentObject.IsFrozen() && ParentObject.Target != null)
			{
				Cell randomElement = ParentObject.CurrentCell.GetEmptyAdjacentCells().GetRandomElement();
				if (randomElement != null && podCooldown <= 0)
				{
					podCooldown = Stat.Random(5, 7);
					GameObject gameObject = ParentObject.PartyLeader ?? ParentObject;
					GameObject gameObject2 = GameObject.create("Feral Lah Pod");
					if (gameObject.IsPlayer())
					{
						gameObject2.SetPartyLeader(ParentObject, takeOnAttitudesOfLeader: true, trifling: true);
					}
					else
					{
						gameObject2.SetPartyLeader(gameObject, takeOnAttitudesOfLeader: true, trifling: true, copyTargetWithAttitudes: true);
					}
					randomElement.AddObject(gameObject2);
					gameObject2.MakeActive();
					ParentObject.Splatter("&g.");
					gameObject2.ParticleBlip("&go");
					ParentObject.UseEnergy(1000);
					return false;
				}
				if (ParentObject.Target.DistanceTo(ParentObject) <= 1)
				{
					Fear();
				}
			}
		}
		else if (E.ID == "AIAttackRange")
		{
			ParentObject.UseEnergy(1000);
			return false;
		}
		return base.FireEvent(E);
	}
}
