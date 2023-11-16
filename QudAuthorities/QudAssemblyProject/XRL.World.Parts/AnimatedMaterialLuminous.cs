using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialLuminous : IPart
{
	public int nFrameOffset;

	public AnimatedMaterialLuminous()
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
				E.ColorString = "&C";
			}
			else
			{
				E.ColorString = "&Y";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
