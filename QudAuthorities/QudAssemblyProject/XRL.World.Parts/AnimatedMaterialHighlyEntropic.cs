using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialHighlyEntropic : IPart
{
	public string ColorString;

	public string TileColor;

	public string DetailColor;

	public int FrameOffset;

	public override bool Render(RenderEvent E)
	{
		Render pRender = ParentObject.pRender;
		if (ColorString == null)
		{
			ColorString = ParentObject.pRender.ColorString;
		}
		if (TileColor == null)
		{
			TileColor = ParentObject.pRender.TileColor;
		}
		if (DetailColor == null)
		{
			DetailColor = ParentObject.pRender.DetailColor;
		}
		int num = (XRLCore.CurrentFrame + FrameOffset) % 150;
		if (num < 3)
		{
			pRender.TileColor = "&k";
			pRender.ColorString = "&k";
		}
		else if (num < 6)
		{
			pRender.DetailColor = "K";
		}
		else if (num < 9)
		{
			pRender.TileColor = "&m";
			pRender.ColorString = "&m";
		}
		else if (num < 12)
		{
			pRender.DetailColor = "m";
		}
		else if (num < 15)
		{
			pRender.TileColor = "&K";
			pRender.ColorString = "&K";
			pRender.DetailColor = "y";
		}
		else if (num < 18)
		{
			pRender.TileColor = "&m";
			pRender.ColorString = "&m";
			pRender.DetailColor = "K";
		}
		else
		{
			pRender.ColorString = ColorString;
			pRender.DetailColor = DetailColor;
			pRender.TileColor = TileColor;
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += (50.in100() ? 1 : Stat.Random(1, 3));
		}
		if (2.in100() && !Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.RandomCosmetic(0, 100);
		}
		return true;
	}
}
