using System;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialRealityStabilizationField : IPart
{
	public int nFrameOffset;

	[NonSerialized]
	private static StringBuilder tileBuilder = new StringBuilder();

	[NonSerialized]
	private static string AltRender = "°";

	[NonSerialized]
	private int lastN = 1;

	[NonSerialized]
	private long accumulator;

	public AnimatedMaterialRealityStabilizationField()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 2000);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (bAlt || !Visible() || !E.ColorsVisible)
		{
			return true;
		}
		if (E.Tile != null)
		{
			return true;
		}
		int num = (XRLCore.CurrentFrame10 + nFrameOffset) % 2000;
		if (num >= 250 && num <= 750)
		{
			E.BackgroundString = "^K";
			E.DetailColor = "b";
		}
		else if (num >= 1250 && num <= 1750)
		{
			E.BackgroundString = "^y";
			E.DetailColor = "B";
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (E.Tile != null && XRLCore.FrameTimer.ElapsedMilliseconds - accumulator > 1500 + nFrameOffset)
		{
			accumulator = XRLCore.FrameTimer.ElapsedMilliseconds;
			lastN++;
			if (lastN > 4)
			{
				lastN = 1;
			}
			tileBuilder.Length = 0;
			tileBuilder.Append("Assets_Content_Textures_Tiles2_force_field_").Append(lastN).Append('_')
				.Append(E.Tile.Substring(E.Tile.LastIndexOf('_') + 1));
			ParentObject.pRender.Tile = tileBuilder.ToString();
		}
		int num = (XRLCore.CurrentFrame10 + nFrameOffset) % 2000;
		if (num < 500)
		{
			E.RenderString = AltRender;
			if (E.ColorsVisible)
			{
				E.ColorString = "&y^k";
				E.DetailColor = "k";
			}
		}
		else if (num < 1000)
		{
			E.RenderString = AltRender;
			if (E.ColorsVisible)
			{
				E.ColorString = "&K^k";
				E.DetailColor = "k";
			}
		}
		else if (num < 1500)
		{
			E.RenderString = AltRender;
			if (E.ColorsVisible)
			{
				E.ColorString = "&Y^y";
				E.DetailColor = "y";
			}
		}
		else
		{
			E.RenderString = ParentObject.pRender.RenderString;
			if (E.ColorsVisible)
			{
				E.ColorString = "&Y^K";
				E.DetailColor = "K";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
