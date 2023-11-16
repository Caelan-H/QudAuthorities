using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialExtradimensional : IPart
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
		int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
		if (num < 2)
		{
			pRender.TileColor = "&o";
			pRender.ColorString = "&o";
			pRender.DetailColor = "o";
		}
		else if (num < 4)
		{
			pRender.TileColor = "&O";
			pRender.ColorString = "&O";
			pRender.DetailColor = "o";
		}
		else if (num < 6)
		{
			pRender.TileColor = "&O";
			pRender.ColorString = "&O";
			pRender.DetailColor = "O";
		}
		else if (num < 8)
		{
			pRender.TileColor = "&k";
			pRender.ColorString = "&k";
			pRender.DetailColor = "k";
		}
		else if (num < 10)
		{
			pRender.TileColor = "&o";
			pRender.ColorString = "&o";
			pRender.DetailColor = "O";
		}
		else
		{
			pRender.ColorString = ColorString;
			pRender.DetailColor = DetailColor;
			pRender.TileColor = TileColor;
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(1, 3);
		}
		if (2.in100() && !Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.RandomCosmetic(0, 100);
		}
		return true;
	}
}
