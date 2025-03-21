using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class LightDimmer : IPart
{
	public int ChanceOneIn = 100;

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
		Tick();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		Tick(10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		Tick(100);
	}

	public void Tick(int Increment = 1)
	{
		if (Stat.Random(1, ChanceOneIn) > Increment)
		{
			return;
		}
		LightSource lightSource = ParentObject.GetPart<LightSource>();
		if (lightSource == null)
		{
			lightSource = new LightSource
			{
				Radius = 4
			};
			ParentObject.AddPart(lightSource);
		}
		if (50.in100())
		{
			if (lightSource.Radius > 1)
			{
				lightSource.Radius--;
				DidX("dim");
			}
		}
		else
		{
			lightSource.Radius++;
			DidX("brighten");
		}
	}
}
