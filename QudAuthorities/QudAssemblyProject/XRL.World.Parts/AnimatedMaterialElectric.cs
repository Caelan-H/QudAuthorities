using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialElectric : IPart
{
	public int nFrameOffset;

	public AnimatedMaterialElectric()
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
			if ((XRLCore.CurrentFrame + nFrameOffset) % 60 % 3 == 0)
			{
				E.ColorString = "&Y";
			}
			else
			{
				E.ColorString = "&W";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
