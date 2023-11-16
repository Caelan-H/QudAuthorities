using System;

namespace XRL.World.Parts;

[Serializable]
public class ColorShift : IPart
{
	public string ColorString;

	public string TileColor;

	public string DetailColor;

	public override bool SameAs(IPart p)
	{
		ColorShift colorShift = p as ColorShift;
		if (colorShift.ColorString != ColorString)
		{
			return false;
		}
		if (colorShift.TileColor != TileColor)
		{
			return false;
		}
		if (colorShift.DetailColor != DetailColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void Apply(string ColorString = null, string TileColor = null, string DetailColor = null)
	{
		if (ColorString != null && ParentObject.pRender != null)
		{
			if (this.ColorString == null)
			{
				this.ColorString = ParentObject.pRender.ColorString;
			}
			ParentObject.pRender.ColorString = ColorString;
		}
		if (TileColor != null && ParentObject.pRender != null)
		{
			if (this.TileColor == null)
			{
				this.TileColor = ParentObject.pRender.TileColor;
			}
			ParentObject.pRender.TileColor = TileColor;
		}
		if (DetailColor != null && ParentObject.pRender != null)
		{
			if (this.DetailColor == null)
			{
				this.DetailColor = ParentObject.pRender.DetailColor;
			}
			ParentObject.pRender.DetailColor = DetailColor;
		}
	}

	public void Unapply()
	{
		if (ColorString != null && ParentObject.pRender != null)
		{
			ParentObject.pRender.ColorString = ColorString;
		}
		if (TileColor != null && ParentObject.pRender != null)
		{
			ParentObject.pRender.TileColor = TileColor;
		}
		if (DetailColor != null && ParentObject.pRender != null)
		{
			ParentObject.pRender.DetailColor = DetailColor;
		}
		ParentObject.RemovePart(this);
	}
}
