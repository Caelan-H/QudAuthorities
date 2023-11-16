using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class LoveTonic : Effect
{
	public LoveTonic()
	{
		base.DisplayName = "{{amorous|amorous}}";
	}

	public LoveTonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override string GetDescription()
	{
		return "{{amorous|love}} tonic";
	}

	public override string GetDetails()
	{
		return "Will fall in love with the first thing examined.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your heart swells with a burning sensation.");
		}
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your heart rate slows again.");
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "LookedAt");
		Object.RegisterEffectEvent(this, "Overdose");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "LookedAt");
		Object.UnregisterEffectEvent(this, "Overdose");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LookedAt")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && (gameObjectParameter.HasPart("Combat") || gameObjectParameter.GetBlueprint().InheritsFrom("Furniture")))
			{
				base.Object.ApplyEffect(new Lovesick(Stat.Random(3000, 3600), gameObjectParameter));
				base.Duration = 0;
				base.Object.CleanEffects();
			}
		}
		if (E.ID == "Overdose" && base.Duration > 0)
		{
			base.Duration = 0;
			if (base.Object.IsPlayer())
			{
				if (base.Object.GetLongProperty("Overdosing") == 1)
				{
					Popup.Show("Your mutant physiology reacts adversely to the tonic. You erupt into flames!");
				}
				else
				{
					Popup.Show("The tonics you ingested react adversely to each other. You erupt into flames!");
				}
			}
			base.Object.pPhysics.Temperature = base.Object.pPhysics.FlameTemperature + 200;
			base.Object.CleanEffects();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (base.Duration > 0 && num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "\u0003";
			switch (Stat.RandomCosmetic(1, 4))
			{
			case 1:
				E.ColorString = "&r";
				break;
			case 2:
				E.ColorString = "&R";
				break;
			case 3:
				E.ColorString = "&M";
				break;
			case 4:
				E.ColorString = "&m";
				break;
			}
		}
		return true;
	}
}
