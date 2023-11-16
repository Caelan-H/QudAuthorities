using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialTechlight : IPart
{
	public int nFrameOffset;

	public int FrameOffset;

	public int FlickerFrame;

	public string baseColor = "c";

	public AnimatedMaterialTechlight()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		Render pRender = ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + FrameOffset) % 500;
		pRender.TileColor = baseColor;
		if (Stat.Random(1, 200) == 1 || FlickerFrame > 0)
		{
			pRender.ColorString = "&c";
			pRender.DetailColor = "c";
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
		}
		if (num < 4)
		{
			pRender.ColorString = "&C";
			pRender.DetailColor = "C";
		}
		else if (num < 8)
		{
			pRender.ColorString = "&B";
			pRender.DetailColor = "B";
		}
		else if (num < 12)
		{
			pRender.ColorString = "&b";
			pRender.DetailColor = "b";
		}
		else
		{
			pRender.ColorString = "&Y";
			pRender.DetailColor = "Y";
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(0, 20);
		}
		if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
		{
			pRender.ColorString = "&b";
			pRender.DetailColor = "b";
		}
		return true;
	}
}
