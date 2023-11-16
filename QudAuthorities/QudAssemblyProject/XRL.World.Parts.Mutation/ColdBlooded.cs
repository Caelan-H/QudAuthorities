using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ColdBlooded : BaseMutation
{
	public GameObject HornsObject;

	public int _SpeedBonus;

	public int SpeedBonus
	{
		get
		{
			return _SpeedBonus;
		}
		set
		{
			if (_SpeedBonus != value)
			{
				if (_SpeedBonus > 0)
				{
					ParentObject.Statistics["Speed"].Bonus -= _SpeedBonus;
				}
				if (_SpeedBonus < 0)
				{
					ParentObject.Statistics["Speed"].Penalty -= -_SpeedBonus;
				}
				_SpeedBonus = value;
				if (_SpeedBonus > 0)
				{
					ParentObject.Statistics["Speed"].Bonus += _SpeedBonus;
				}
				if (_SpeedBonus < 0)
				{
					ParentObject.Statistics["Speed"].Penalty += -_SpeedBonus;
				}
			}
		}
	}

	public ColdBlooded()
	{
		DisplayName = "Cold-Blooded ({{r|D}})";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "Your vitality depends on your temperature; at higher temperatures, you are more lively. At lower temperatures, you are more torpid.\n\nYour base quickness score is reduced by 10.\nYour quickness increases as your temperature increases and decreases as your temperature decreases.\n+100 reputation with {{w|unshelled reptiles}}";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndSegment");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndSegment")
		{
			if (ParentObject.pPhysics.Temperature == 25)
			{
				SpeedBonus = -10;
			}
			else if (ParentObject.pPhysics.Temperature < 25)
			{
				double num = 90.0;
				for (int i = 26; i <= 25 - ParentObject.pPhysics.Temperature + 25; i++)
				{
					num -= 1250.0 / (double)((i + 25) * (i + 25));
				}
				SpeedBonus = (int)(num - 100.0);
			}
			else
			{
				double num2 = 90.0;
				for (int j = 26; j <= ParentObject.pPhysics.Temperature; j++)
				{
					num2 += 1250.0 / (double)((j + 25) * (j + 25));
				}
				SpeedBonus = (int)(num2 - 100.0);
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		SpeedBonus = 0;
		return base.Unmutate(GO);
	}
}
