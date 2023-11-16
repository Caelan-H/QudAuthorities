using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialStasisfield : IPart
{
	public int nFrameOffset;

	public bool Rushing;

	public AnimatedMaterialStasisfield()
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
				E.BackgroundString = "^m";
				E.DetailColor = "m";
			}
			else
			{
				E.BackgroundString = "^C";
				E.DetailColor = "C";
			}
		}
		else if ((XRLCore.CurrentFrame + nFrameOffset) % 60 < 45)
		{
			E.BackgroundString = "^m";
			E.DetailColor = "m";
		}
		else
		{
			E.BackgroundString = "^C";
			E.DetailColor = "C";
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		_ = ParentObject.pPhysics;
		Render pRender = ParentObject.pRender;
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
			if (num < 15)
			{
				E.RenderString = "°";
				if (E.ColorsVisible)
				{
					E.ColorString = "&C";
					E.DetailColor = "k";
				}
			}
			else if (num < 30)
			{
				E.RenderString = "°";
				if (E.ColorsVisible)
				{
					E.ColorString = "&c";
					E.DetailColor = "m";
				}
			}
			else if (num < 45)
			{
				E.RenderString = "°";
				if (E.ColorsVisible)
				{
					E.ColorString = "&M";
					E.DetailColor = "c";
				}
			}
			else
			{
				E.RenderString = pRender.RenderString;
				if (E.ColorsVisible)
				{
					E.ColorString = "&m";
					E.DetailColor = "C";
				}
			}
		}
		else
		{
			int num2 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (num2 < 15)
			{
				E.RenderString = "°";
				if (E.ColorsVisible)
				{
					E.ColorString = "&C";
					E.DetailColor = "k";
				}
			}
			else if (num2 < 30)
			{
				E.RenderString = "°";
				if (E.ColorsVisible)
				{
					E.ColorString = "&c";
					E.DetailColor = "M";
				}
			}
			else if (num2 < 45)
			{
				E.RenderString = "°";
				if (E.ColorsVisible)
				{
					E.ColorString = "&C";
					E.DetailColor = "m";
				}
			}
			else
			{
				E.RenderString = pRender.RenderString;
				if (E.ColorsVisible)
				{
					E.ColorString = "&m";
					E.DetailColor = "C";
				}
			}
		}
		return true;
	}
}
