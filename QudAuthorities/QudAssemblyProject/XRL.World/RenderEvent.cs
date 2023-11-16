using ConsoleLib.Console;
using UnityEngine;

namespace XRL.World;

public class RenderEvent : IRenderable
{
	public bool WantsToPaint;

	public bool CustomDraw;

	public string _Tile;

	public string Final;

	public string ColorString;

	public string RenderString;

	public string BackgroundString;

	public string DetailColor;

	public bool _HFlip;

	public bool VFlip;

	public int HighestLayer = -1;

	public LightLevel Lit;

	public string Tile
	{
		get
		{
			return _Tile;
		}
		set
		{
			if (value == null)
			{
				HFlip = false;
				VFlip = false;
			}
			_Tile = value;
		}
	}

	public bool HFlip
	{
		get
		{
			return _HFlip;
		}
		set
		{
			_HFlip = value;
		}
	}

	public bool ColorsVisible => Zone.ColorsVisible(Lit);

	string IRenderable.getTile()
	{
		return Tile;
	}

	public string getColorString()
	{
		return ColorString;
	}

	public string getRenderString()
	{
		return RenderString;
	}

	public char getDetailColor()
	{
		if (string.IsNullOrEmpty(DetailColor))
		{
			return '\0';
		}
		return DetailColor[0];
	}

	public bool getHFlip()
	{
		return HFlip;
	}

	public bool getVFlip()
	{
		return VFlip;
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

	public Color GetForegroundColor()
	{
		if (ColorString != null && ColorString.Length > 1)
		{
			return ConsoleLib.Console.ColorUtility.ColorMap[ColorString[1]];
		}
		return ConsoleLib.Console.ColorUtility.ColorMap['y'];
	}

	public char GetForegroundColorChar()
	{
		if (ColorString != null && ColorString.Length > 1)
		{
			return ColorString[1];
		}
		return 'y';
	}

	public Color GetDetailColor()
	{
		if (DetailColor != null && DetailColor.Length > 0)
		{
			if (ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(DetailColor[0], out var value))
			{
				return value;
			}
			return GetForegroundColor();
		}
		return GetForegroundColor();
	}

	public char GetDetailColorChar()
	{
		if (DetailColor != null && DetailColor.Length > 0)
		{
			return DetailColor[0];
		}
		return 'w';
	}

	public Color GetBackgroundColor()
	{
		if (BackgroundString != null && BackgroundString.Length > 1)
		{
			return ConsoleLib.Console.ColorUtility.ColorMap[BackgroundString[BackgroundString.Length - 1]];
		}
		return ConsoleLib.Console.ColorUtility.ColorMap['k'];
	}

	string IRenderable.getTileColor()
	{
		return null;
	}

	public void TileVariantColors(string dual, string foreground, string detail)
	{
		if (ColorsVisible)
		{
			if (!string.IsNullOrEmpty(Tile))
			{
				ColorString = foreground;
				DetailColor = detail;
			}
			else
			{
				ColorString = dual;
			}
		}
	}
}
