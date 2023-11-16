using System;
using ConsoleLib.Console;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class IceBreather : BreatherBase
{
	[Obsolete("save compat")]
	public int Placeholder;

	public IceBreather()
	{
		DisplayName = "Ice Breath";
	}

	public override string GetFaceObject()
	{
		return "Icy Vapor";
	}

	public override string GetCommandDisplayName()
	{
		return "Breathe Ice";
	}

	public override string GetDescription()
	{
		return "You breathe ice.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Breathes ice in a cone.\n";
		text = text + "Damage: " + ComputeDamage(Level) + "\n";
		text = text + "Cone length: " + GetConeLength() + " tiles\n";
		text = text + "Cone angle: " + GetConeAngle() + " degrees\n";
		text += "Cooldown: 15 rounds\n";
		if (Level != base.Level)
		{
			text += "Decreased temperature.";
		}
		return text;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		base.StatShifter.SetStatShift("ColdResistance", base.Level * 2);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Unmutate(GameObject GO)
	{
		base.StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
	}

	public string ComputeDamage(int UseLevel)
	{
		string text = UseLevel + "d4";
		if (ParentObject != null)
		{
			int partCount = ParentObject.Body.GetPartCount(BodyPartType);
			if (partCount > 0)
			{
				text = text + "+" + partCount;
			}
		}
		else
		{
			text += "+1";
		}
		return text;
	}

	public string ComputeDamage()
	{
		return ComputeDamage(base.Level);
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (item.PhaseMatches(ParentObject))
				{
					item.TemperatureChange(-120 - 7 * base.Level, ParentObject);
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
				}
			}
			foreach (GameObject item2 in C.GetObjectsWithPart("Combat"))
			{
				if (item2.PhaseMatches(ParentObject))
				{
					Damage damage = new Damage(Stat.Roll(dice));
					damage.AddAttribute("Cold");
					Event @event = Event.New("TakeDamage");
					@event.AddParameter("Damage", damage);
					@event.AddParameter("Owner", ParentObject);
					@event.AddParameter("Attacker", ParentObject);
					@event.AddParameter("Message", "from %o freezing effect!");
					item2.FireEvent(@event);
				}
			}
		}
		DrawBreathInCell(C, Buffer, "C", "B", "Y");
	}
}
