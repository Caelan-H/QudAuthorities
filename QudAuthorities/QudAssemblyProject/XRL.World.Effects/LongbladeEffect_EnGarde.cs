using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class LongbladeEffect_EnGarde : Effect
{
	public LongbladeEffect_EnGarde()
	{
		base.DisplayName = "{{G|En garde!}}";
		base.Duration = 1;
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override string GetDetails()
	{
		return "Lunge and Swipe have no cooldown.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RemoveEffect("En garde!");
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "!";
			E.ColorString = "&G";
			E.DetailColor = "W";
		}
		return true;
	}
}
