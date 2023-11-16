using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialWater : IPart
{
	public int nFrameOffset;

	public bool Rushing;

	public bool Fresh;

	public bool Acid;

	public bool Bloody;

	public AnimatedMaterialWater()
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

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.pPhysics.IsFreezing())
		{
			E.RenderString = "~";
			if (E.ColorsVisible)
			{
				E.ColorString = "&c";
				E.DetailColor = "C";
			}
		}
		else if (Acid)
		{
			Render pRender = ParentObject.pRender;
			int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.pPhysics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "÷";
				E.TileVariantColors("&Y^g", "&Y", "g");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num < 15)
				{
					pRender.RenderString = "÷";
					if (E.ColorsVisible)
					{
						pRender.ColorString = "&g^G";
						pRender.TileColor = "&g";
						pRender.DetailColor = "G";
					}
				}
				else if (num < 30)
				{
					pRender.RenderString = " ";
					if (E.ColorsVisible)
					{
						pRender.ColorString = "&Y^g";
						pRender.TileColor = "&Y";
						pRender.DetailColor = "g";
					}
				}
				else if (num < 45)
				{
					pRender.RenderString = "÷";
					if (E.ColorsVisible)
					{
						pRender.ColorString = "&g^G";
						pRender.TileColor = "&g";
						pRender.DetailColor = "G";
					}
				}
				else
				{
					pRender.RenderString = "~";
					if (E.ColorsVisible)
					{
						pRender.ColorString = "&y^G";
						pRender.TileColor = "&y";
						pRender.DetailColor = "G";
					}
				}
			}
		}
		else if (Bloody)
		{
			Render pRender2 = ParentObject.pRender;
			int num2 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.pPhysics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "÷";
				E.TileVariantColors("&Y^r", "&Y", "r");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num2 < 15)
				{
					pRender2.RenderString = "÷";
					pRender2.ColorString = "&b^R";
					pRender2.TileColor = "&b";
					pRender2.DetailColor = "R";
				}
				else if (num2 < 30)
				{
					pRender2.RenderString = " ";
					pRender2.ColorString = "&Y^R";
					pRender2.TileColor = "&Y";
					pRender2.DetailColor = "R";
				}
				else if (num2 < 45)
				{
					pRender2.RenderString = "÷";
					pRender2.ColorString = "&b^R";
					pRender2.TileColor = "&b";
					pRender2.DetailColor = "R";
				}
				else
				{
					pRender2.RenderString = "~";
					pRender2.ColorString = "&y^r";
					pRender2.TileColor = "&y";
					pRender2.DetailColor = "r";
				}
			}
		}
		else if (Fresh)
		{
			Render pRender3 = ParentObject.pRender;
			int num3 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.pPhysics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "÷";
				E.TileVariantColors("&Y^B", "&Y", "B");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num3 < 15)
				{
					pRender3.RenderString = "÷";
					pRender3.ColorString = "&b^B";
					pRender3.TileColor = "&b";
					pRender3.DetailColor = "B";
				}
				else if (num3 < 30)
				{
					pRender3.RenderString = " ";
					pRender3.ColorString = "&Y^B";
					pRender3.TileColor = "&Y";
					pRender3.DetailColor = "B";
				}
				else if (num3 < 45)
				{
					pRender3.RenderString = "÷";
					pRender3.ColorString = "&b^B";
					pRender3.TileColor = "&b";
					pRender3.DetailColor = "B";
				}
				else
				{
					pRender3.RenderString = "~";
					pRender3.ColorString = "&y^B";
					pRender3.TileColor = "&y";
					pRender3.DetailColor = "B";
				}
			}
		}
		else if (Rushing)
		{
			Render pRender4 = ParentObject.pRender;
			int num4 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (num4 < 15)
			{
				E.RenderString = "~";
				E.TileVariantColors("&B^b", "&B", "b");
			}
			else if (num4 < 30)
			{
				E.RenderString = pRender4.RenderString;
				E.TileVariantColors("&Y^b", "&Y", "b");
			}
			else if (num4 < 45)
			{
				E.RenderString = "~";
				E.TileVariantColors("&B^b", "&B", "b");
			}
			else
			{
				E.RenderString = pRender4.RenderString;
				E.TileVariantColors("&B^b", "&B", "b");
			}
		}
		else
		{
			Render pRender5 = ParentObject.pRender;
			int num5 = (XRLCore.CurrentFrame + nFrameOffset) % 60;
			if (ParentObject.pPhysics.CurrentCell.ParentZone.Z == 10 && Stat.RandomCosmetic(1, 600) == 1)
			{
				E.RenderString = "~";
				E.TileVariantColors("&Y^b", "&Y", "b");
			}
			if (Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num5 < 15)
				{
					pRender5.RenderString = "÷";
					pRender5.ColorString = "&B^b";
					pRender5.TileColor = "&B";
					pRender5.DetailColor = "b";
				}
				else if (num5 < 30)
				{
					pRender5.RenderString = "~";
					pRender5.ColorString = "&B^b";
					pRender5.TileColor = "&B";
					pRender5.DetailColor = "b";
				}
				else if (num5 < 45)
				{
					pRender5.RenderString = " ";
					pRender5.ColorString = "&B^b";
					pRender5.TileColor = "&B";
					pRender5.DetailColor = "b";
				}
				else
				{
					pRender5.RenderString = "~";
					pRender5.ColorString = "&B^b";
					pRender5.TileColor = "&B";
					pRender5.DetailColor = "b";
				}
			}
		}
		return base.Render(E);
	}
}
