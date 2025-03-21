using System;

namespace XRL.World.Effects;

[Serializable]
public class BlinkingTicSickness : Effect
{
	public BlinkingTicSickness()
	{
		base.DisplayName = "{{B|acute blinking tic}}";
	}

	public BlinkingTicSickness(int duration)
		: this()
	{
		base.Duration = duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 117440768;
	}

	public override string GetDetails()
	{
		return "There is a small chance each round you're in combat that you randomly teleport to a nearby location.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EndTurn");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Remove(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			base.Duration--;
			Cell cell = base.Object.CurrentCell;
			if (cell == null || cell.ParentZone.IsWorldMap())
			{
				return true;
			}
			if (base.Object.IsPlayer() && !base.Object.AreHostilesNearby())
			{
				return true;
			}
			if (!base.Object.FireEvent("CheckRealityDistortionUsability"))
			{
				return true;
			}
			if (2.in1000())
			{
				if (base.Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You lurch suddenly!", 'r');
				}
				base.Object.RandomTeleport(Swirl: true);
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
