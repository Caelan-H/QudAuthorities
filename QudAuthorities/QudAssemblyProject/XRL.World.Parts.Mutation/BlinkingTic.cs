using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BlinkingTic : BaseMutation
{
	public BlinkingTic()
	{
		DisplayName = "Blinking Tic ({{r|D}})";
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You teleport about uncontrollably.\n\nSmall chance each round you're in combat that you randomly teleport to a nearby location.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell == null || cell.ParentZone.IsWorldMap())
			{
				return true;
			}
			if (!ParentObject.FireEvent("CheckRealityDistortionUsability"))
			{
				return true;
			}
			if (1.in1000())
			{
				if (ParentObject.IsPlayer() && !ParentObject.AreHostilesNearby())
				{
					return true;
				}
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You lurch suddenly!", 'r');
				}
				ParentObject.RandomTeleport(Swirl: true);
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
