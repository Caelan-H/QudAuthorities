using System;
using XRL.Messages;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Discipline_IronMind : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyConfusion");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyConfusion")
		{
			if (GetChance().in100())
			{
				ParentObject.ParticleText("*shook off confusion*", IComponent<GameObject>.ConsequentialColorChar(ParentObject));
				return false;
			}
		}
		else if (E.ID == "EndTurn" && ParentObject.HasEffect("Confused") && GetChance().in100())
		{
			ParentObject.RemoveEffect("Confused");
			if (ParentObject.IsPlayer())
			{
				if (IComponent<GameObject>.ThePlayer.GetConfusion() > 0)
				{
					MessageQueue.AddPlayerMessage("You muster your will and shake off some of your confusion.");
				}
				else
				{
					MessageQueue.AddPlayerMessage("You muster your will and shake off your confusion.");
				}
			}
			ParentObject.ParticleText("*shook off confusion*", IComponent<GameObject>.ConsequentialColorChar(ParentObject));
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return base.RemoveSkill(GO);
	}

	public int GetChance()
	{
		return ParentObject.Stat("Willpower") - 10;
	}
}
