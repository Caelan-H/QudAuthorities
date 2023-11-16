using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Miner : IPart
{
	public int MineCooldown;

	public string Cooldown = "5-10";

	public string MineType;

	public string MineName;

	public string MineTimer = "-1";

	public int MaxMinesPerZone = 15;

	public int Mark = 1;

	public override bool SameAs(IPart p)
	{
		Miner miner = p as Miner;
		if (miner.MineCooldown != MineCooldown)
		{
			return false;
		}
		if (miner.Cooldown != Cooldown)
		{
			return false;
		}
		if (miner.MineType != MineType)
		{
			return false;
		}
		if (miner.MineName != MineName)
		{
			return false;
		}
		if (miner.MineTimer != MineTimer)
		{
			return false;
		}
		if (miner.MaxMinesPerZone != MaxMinesPerZone)
		{
			return false;
		}
		if (miner.Mark != Mark)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != BeginTakeActionEvent.ID || ParentObject.IsPlayer()))
		{
			if (ID == AfterObjectCreatedEvent.ID)
			{
				return string.IsNullOrEmpty(MineType);
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		CheckMinerConfiguration();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		int mineCooldown = MineCooldown;
		if (MineCooldown > 0)
		{
			MineCooldown--;
		}
		if (!ParentObject.IsPlayer() && ParentObject.FireEvent("CanAIDoIndependentBehavior") && !ParentObject.HasGoal("LayMineGoal") && !ParentObject.HasGoal("WanderRandomly") && !ParentObject.HasGoal("Flee"))
		{
			if (mineCooldown > 0 || ParentObject.CurrentZone.CountObjects("MineShell") >= MaxMinesPerZone)
			{
				GameObject target = ParentObject.Target;
				if (target != null)
				{
					ParentObject.pBrain.PushGoal(new Flee(target, Stat.Random(10, 20)));
				}
			}
			else
			{
				Cell randomElement = ParentObject.CurrentZone.GetEmptyReachableCells().GetRandomElement();
				if (randomElement == null)
				{
					ParentObject.pBrain.PushGoal(new WanderRandomly(5));
				}
				else
				{
					ParentObject.pBrain.Goals.Clear();
					ParentObject.pBrain.PushGoal(new LayMineGoal(randomElement.location, MineType, MineName, MineTimer, 12 + Mark * 3));
				}
				MineCooldown = Cooldown.RollCached();
			}
		}
		return base.HandleEvent(E);
	}

	public void CheckMinerConfiguration()
	{
		if (!string.IsNullOrEmpty(MineType))
		{
			return;
		}
		string text = Mark.ToString();
		string populationName = "Explosives " + text;
		int num = 0;
		GameObject gameObject = null;
		string tag;
		do
		{
			if (++num > 1000)
			{
				throw new Exception("cannot generate layable grenade");
			}
			MineType = PopulationManager.RollOneFrom(populationName).Blueprint;
			gameObject = GameObject.createSample(MineType);
			tag = gameObject.GetTag("Mark");
		}
		while (!gameObject.HasPart("Tinkering_Layable") || tag != text || gameObject.HasTag("NoMiners") || Scanning.GetScanTypeFor(gameObject) != Scanning.Scan.Tech);
		string displayName = gameObject.pRender.DisplayName;
		int num2 = displayName.IndexOf("grenade");
		if (num2 != -1)
		{
			MineName = displayName.Substring(0, num2);
		}
		else
		{
			MineName = displayName + " ";
		}
		if (!ParentObject.HasProperName)
		{
			if (MineTimer == "-1")
			{
				ParentObject.pRender.DisplayName = MineName + "miner mk " + Grammar.GetRomanNumeral(Mark);
			}
			else
			{
				ParentObject.pRender.DisplayName = MineName + "bomber mk " + Grammar.GetRomanNumeral(Mark);
			}
		}
		if (GameObject.validate(ref gameObject))
		{
			gameObject.Obliterate();
		}
	}
}
