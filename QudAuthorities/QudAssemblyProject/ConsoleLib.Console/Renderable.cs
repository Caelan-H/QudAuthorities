using System;
using XRL.World;

namespace ConsoleLib.Console;

[Serializable]
public class Renderable : IRenderable
{
	public string Tile;

	public string RenderString = " ";

	public string ColorString = "";

	public string TileColor;

	public char DetailColor;

	public Renderable()
	{
	}

	public Renderable(Renderable Source)
	{
		Tile = Source.Tile;
		RenderString = Source.RenderString;
		ColorString = Source.ColorString;
		TileColor = Source.TileColor;
		DetailColor = Source.DetailColor;
	}

	public Renderable(Renderable Source, string Tile = null, string RenderString = null, string ColorString = null, string TileColor = null, char? DetailColor = null)
	{
		this.Tile = Tile ?? Source.Tile;
		this.RenderString = RenderString ?? Source.RenderString;
		this.ColorString = ColorString ?? Source.ColorString;
		this.TileColor = TileColor ?? Source.TileColor;
		this.DetailColor = DetailColor ?? Source.DetailColor;
	}

	public Renderable(IRenderable Source)
	{
		Tile = Source.getTile();
		RenderString = Source.getRenderString();
		ColorString = Source.getColorString();
		TileColor = Source.getTileColor();
		DetailColor = Source.getDetailColor();
	}

	public Renderable(IRenderable Source, string Tile = null, string RenderString = null, string ColorString = null, string TileColor = null, char? DetailColor = null)
	{
		this.Tile = Tile ?? Source.getTile();
		this.RenderString = RenderString ?? Source.getRenderString();
		this.ColorString = ColorString ?? Source.getColorString();
		this.TileColor = TileColor ?? Source.getTileColor();
		this.DetailColor = DetailColor ?? Source.getDetailColor();
	}

	public Renderable(string Tile, string RenderString = " ", string ColorString = "", string TileColor = null, char DetailColor = '\0')
		: this()
	{
		setTile(Tile);
		setRenderString(RenderString);
		setColorString(ColorString);
		setTileColor(TileColor);
		setDetailColor(DetailColor);
	}

	public Renderable(GameObjectBlueprint Blueprint)
	{
		GamePartBlueprint part = Blueprint.GetPart("Render");
		Tile = part.GetParameter("Tile");
		RenderString = part.GetParameter("RenderString", " ");
		TileColor = part.GetParameter("TileColor");
		ColorString = part.GetParameter("ColorString", "");
		DetailColor = part.GetParameter("DetailColor", "\0")[0];
	}

	public Renderable setTile(string val)
	{
		Tile = val;
		return this;
	}

	public string getTile()
	{
		return Tile;
	}

	public Renderable setRenderString(string val)
	{
		RenderString = val;
		return this;
	}

	public string getRenderString()
	{
		return RenderString;
	}

	public Renderable setColorString(string val)
	{
		ColorString = val;
		return this;
	}

	public string getColorString()
	{
		return ColorString;
	}

	public Renderable setTileColor(string val)
	{
		TileColor = val;
		return this;
	}

	public string getTileColor()
	{
		return TileColor;
	}

	public Renderable setDetailColor(char val)
	{
		DetailColor = val;
		return this;
	}

	public Renderable setDetailColor(string val)
	{
		DetailColor = ((!string.IsNullOrEmpty(val)) ? val[0] : '\0');
		return this;
	}

	public char getDetailColor()
	{
		return DetailColor;
	}

	public ColorChars getColorChars()
	{
		char foreground = 'y';
		char background = 'k';
		string colorString = getColorString();
		if (!string.IsNullOrEmpty(colorString))
		{
			int num = colorString.LastIndexOf(ColorChars.FOREGROUND_INDICATOR);
			int num2 = colorString.LastIndexOf(ColorChars.BACKGROUND_INDICATOR);
			if (num >= 0 && num < colorString.Length - 1)
			{
				foreground = colorString[num + 1];
			}
			if (num2 >= 0 && num2 < colorString.Length - 1)
			{
				background = colorString[num2 + 1];
			}
		}
		ColorChars result = default(ColorChars);
		result.detail = getDetailColor();
		result.foreground = foreground;
		result.background = background;
		return result;
	}

	public bool getHFlip()
	{
		return false;
	}

	public bool getVFlip()
	{
		return false;
	}
}
