using System;

namespace XRL.World.Parts;

[Serializable]
public class BellOfRest : IPart
{
	public long lastDamageTurn = -1L;

	public int turn;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == BeforeDestroyObjectEvent.ID;
		}
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "TookDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage" && Calendar.TotalTimeTicks != lastDamageTurn)
		{
			lastDamageTurn = Calendar.TotalTimeTicks;
			ParentObject.ParticleSpray("&c\r", "&C\u000e", "&B\r", "&b\u000e", 6);
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		turn++;
		if (turn >= 300)
		{
			turn = 0;
			SoundManager.PlaySound("Mark of Death_F_Filtered");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		The.Game.SetIntGameState("BellOfRestDestroyed", 1);
		ParentObject.ParticleSpray("&c\r", "&C\u000e", "&B\r", "&b\u000e", 6);
		SoundManager.PlaySound("Mark of Death_F_Filtered");
		return base.HandleEvent(E);
	}
}
