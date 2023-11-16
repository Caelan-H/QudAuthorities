using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Discipline_Lionheart : BaseSkill
{
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
		CheckRemoval(1);
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckRemoval(10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckRemoval(100);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyFear");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyFear" && GetChance().in100())
		{
			ParentObject.ParticleText("*shook off fear*", IComponent<GameObject>.ConsequentialColorChar(ParentObject));
			return false;
		}
		return base.FireEvent(E);
	}

	public void CheckRemoval(int Chances)
	{
		if (!ParentObject.HasEffect("Terrified"))
		{
			return;
		}
		int chance = GetChance();
		for (int i = 0; i < Chances; i++)
		{
			if (chance.in100())
			{
				ParentObject.RemoveEffect("Terrified");
				break;
			}
		}
	}

	public int GetChance()
	{
		return ParentObject.Stat("Willpower") - 10;
	}
}
