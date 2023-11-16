using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Narcolepsy : BaseMutation
{
	public Narcolepsy()
	{
		DisplayName = "Narcolepsy ({{r|D}})";
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
		return "You fall asleep involuntarily from time to time.\n\nSmall chance each round you're in combat that you fall asleep for 20-29 rounds.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && !ParentObject.OnWorldMap() && !ParentObject.HasEffect("Asleep") && 2.in1000() && ((!ParentObject.IsPlayer() && !ParentObject.WasPlayer()) || ParentObject.AreHostilesNearby()) && ParentObject.ForceApplyEffect(new Asleep(Stat.Random(20, 29), forced: true)))
		{
			ParentObject.StopMoving();
			DidX("fall", "asleep", "!", null, null, ParentObject);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}
}
