using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialForcefield : IPart
{
	public int nFrameOffset;

	public bool Rushing;

	public string Color = "Normal";

	[NonSerialized]
	private StringBuilder tileBuilder = new StringBuilder();

	[NonSerialized]
	private int lastN = 1;

	[NonSerialized]
	private long accumulator;

	public AnimatedMaterialForcefield()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
		Rushing = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (bAlt || !Visible() || !E.ColorsVisible)
		{
			return true;
		}
		if (E.Tile == null)
		{
			string backgroundString = "^c";
			string backgroundString2 = "^C";
			if (Color == "Red")
			{
				backgroundString = "^r";
				backgroundString2 = "^R";
			}
			if (Color == "Blue")
			{
				backgroundString = "^b";
				backgroundString2 = "^B";
			}
			if (Rushing)
			{
				int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
				if (!Options.DisableTextAnimationEffects)
				{
					nFrameOffset += 3;
				}
				if (Stat.RandomCosmetic(1, 120) == 1)
				{
					Rushing = false;
				}
				if (num < 45)
				{
					E.BackgroundString = backgroundString;
					E.DetailColor = "r";
				}
				else
				{
					E.BackgroundString = backgroundString2;
					E.DetailColor = "R";
				}
			}
			else if ((XRLCore.CurrentFrame + nFrameOffset) % 60 < 45)
			{
				E.BackgroundString = backgroundString;
				E.DetailColor = "r";
			}
			else
			{
				E.BackgroundString = backgroundString2;
				E.DetailColor = "R";
			}
		}
		else
		{
			string text = "^k";
			string text2 = "^K";
			string text3 = "^c";
			string text4 = "^C";
			string text5 = "&c";
			string text6 = "&C";
			if (Color == "Red")
			{
				text = "^k";
				text2 = "^r";
				text3 = "^r";
				text4 = "^r";
				text5 = "&r";
				text6 = "&R";
			}
			if (Color == "Blue")
			{
				text = "^k";
				text2 = "^K";
				text3 = "^b";
				text4 = "^B";
				text5 = "&b";
				text6 = "&B";
			}
			if (Rushing)
			{
				int num2 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
				if (!Options.DisableTextAnimationEffects)
				{
					nFrameOffset += 3;
				}
				if (Stat.RandomCosmetic(1, 120) == 1)
				{
					Rushing = false;
				}
				if (num2 < 15)
				{
					E.ColorString = text6 + text;
					E.DetailColor = text[1].ToString();
				}
				else if (num2 < 30)
				{
					E.ColorString = text5 + text;
					E.DetailColor = text2[1].ToString();
				}
				else if (num2 < 45)
				{
					E.ColorString = text6 + text;
					E.DetailColor = text3[1].ToString();
				}
				else
				{
					E.ColorString = text5 + text;
					E.DetailColor = text4[1].ToString();
				}
			}
			else
			{
				int num3 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
				if (num3 < 15)
				{
					E.ColorString = text6 + text;
					E.DetailColor = text[1].ToString();
				}
				else if (num3 < 30)
				{
					E.ColorString = text5 + text;
					E.DetailColor = text[1].ToString();
				}
				else if (num3 < 45)
				{
					E.ColorString = text6 + text;
					E.DetailColor = text3[1].ToString();
				}
				else
				{
					E.ColorString = text5 + text;
					E.DetailColor = text4[1].ToString();
				}
			}
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (E.Tile != null)
		{
			if (XRLCore.FrameTimer.ElapsedMilliseconds - accumulator > 500)
			{
				string value = E.Tile.Substring(E.Tile.LastIndexOf('_') + 1);
				accumulator = XRLCore.FrameTimer.ElapsedMilliseconds;
				if (++lastN > 4)
				{
					lastN = 1;
				}
				tileBuilder.Length = 0;
				tileBuilder.Append("Assets_Content_Textures_Tiles2_force_field_").Append(lastN).Append('_')
					.Append(value);
				ParentObject.pRender.Tile = tileBuilder.ToString();
			}
		}
		else
		{
			int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (!Options.DisableTextAnimationEffects)
			{
				nFrameOffset += 3;
			}
			if (num < 45)
			{
				E.RenderString = "°";
			}
			else
			{
				E.RenderString = ParentObject.pRender.RenderString;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
