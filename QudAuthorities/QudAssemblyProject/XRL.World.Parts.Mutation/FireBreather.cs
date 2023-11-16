using System;
using ConsoleLib.Console;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FireBreather : BreatherBase
{
	[Obsolete("save compat")]
	public int Placeholder;

	public FireBreather()
	{
		DisplayName = "Fire Breath";
	}

	public override string GetFaceObject()
	{
		return "Ghostly Flames";
	}

	public override string GetCommandDisplayName()
	{
		return "Breathe Fire";
	}

	public override string GetDescription()
	{
		return "You breathe fire.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Breathes fire in a cone.\n";
		text = text + "Damage: " + ComputeDamage(Level) + "\n";
		text = text + "Cone length: " + GetConeLength() + " tiles\n";
		text = text + "Cone angle: " + GetConeAngle() + " degrees\n";
		text += "Cooldown: 15 rounds\n";
		if (Level != base.Level)
		{
			text += "{{rules|Increased temperature}}";
		}
		return text;
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

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		base.StatShifter.SetStatShift("HeatResistance", base.Level * 2);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Unmutate(GameObject GO)
	{
		base.StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (!item.PhaseMatches(ParentObject))
				{
					continue;
				}
				item.TemperatureChange(310 + 25 * base.Level, ParentObject);
				if (doEffect)
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
				}
			}
			foreach (GameObject item2 in C.GetObjectsWithPart("Combat"))
			{
				if (item2.PhaseMatches(ParentObject))
				{
					Damage damage = new Damage(Stat.Roll(dice));
					damage.AddAttribute("Fire");
					damage.AddAttribute("Heat");
					Event @event = Event.New("TakeDamage");
					@event.AddParameter("Damage", damage);
					@event.AddParameter("Owner", ParentObject);
					@event.AddParameter("Attacker", ParentObject);
					@event.AddParameter("Message", "from %o flames!");
					item2.FireEvent(@event);
				}
			}
		}
		if (doEffect)
		{
			DrawBreathInCell(C, Buffer, "R", "r", "W");
		}
	}
}
