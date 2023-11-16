using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialFire : IPart
{
	public int nFrameOffset;

	public AnimatedMaterialFire()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (E.ColorsVisible)
		{
			int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				nFrameOffset += Stat.RandomCosmetic(1, 5);
			}
			if (num < 15)
			{
				E.ColorString = "&R";
			}
			else if (num < 30)
			{
				E.ColorString = "&W";
			}
			else if (num < 45)
			{
				E.ColorString = "&r";
			}
			else
			{
				E.ColorString = "&W";
			}
		}
		return true;
	}
}
