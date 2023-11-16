using System;
using System.Text;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Render : IPart, IRenderable
{
	public const int RENDER_OCCLUDING = 1;

	public const int RENDER_IF_DARK = 2;

	public const int RENDER_VISIBLE = 4;

	public const int RENDER_CUSTOM = 8;

	public const int RENDER_HFLIP = 16;

	public const int RENDER_VFLIP = 32;

	public const int RENDER_IGNORE_COLOR_FOR_STACK = 64;

	public string DisplayName;

	public string RenderString;

	public string ColorString;

	public string DetailColor = "";

	public string TileColor = "";

	public int RenderLayer;

	public int Flags = 4;

	public string Tile;

	public bool Occluding
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	public bool RenderIfDark
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	public new bool Visible
	{
		get
		{
			return (Flags & 4) == 4;
		}
		set
		{
			Flags = (value ? (Flags | 4) : (Flags & -5));
		}
	}

	public bool CustomRender
	{
		get
		{
			return (Flags & 8) == 8;
		}
		set
		{
			Flags = (value ? (Flags | 8) : (Flags & -9));
		}
	}

	public bool HFlip
	{
		get
		{
			return (Flags & 0x10) == 16;
		}
		set
		{
			Flags = (value ? (Flags | 0x10) : (Flags & -17));
		}
	}

	public bool VFlip
	{
		get
		{
			return (Flags & 0x20) == 32;
		}
		set
		{
			Flags = (value ? (Flags | 0x20) : (Flags & -33));
		}
	}

	public bool IgnoreColorForStack
	{
		get
		{
			return (Flags & 0x40) == 64;
		}
		set
		{
			Flags = (value ? (Flags | 0x40) : (Flags & -65));
		}
	}

	public Render()
	{
		PoolReset();
	}

	public override bool IsPoolabe()
	{
		return true;
	}

	public override bool PoolReset()
	{
		base.PoolReset();
		DisplayName = null;
		RenderString = "?";
		ColorString = "&y";
		DetailColor = "";
		TileColor = "";
		RenderLayer = 0;
		Flags = 4;
		Tile = null;
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetDebugInternalsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "DisplayName", DisplayName);
		E.AddEntry(this, "RenderString", RenderString);
		E.AddEntry(this, "ColorString", ColorString);
		E.AddEntry(this, "TileColor", TileColor);
		E.AddEntry(this, "DetailColor", DetailColor);
		E.AddEntry(this, "RenderLayer", RenderLayer);
		E.AddEntry(this, "Visible", Visible);
		E.AddEntry(this, "Occluding", Occluding);
		E.AddEntry(this, "RenderIfDark", RenderIfDark);
		E.AddEntry(this, "CustomRender", CustomRender);
		E.AddEntry(this, "Tile", Tile);
		return base.HandleEvent(E);
	}

	public string GetForegroundColor()
	{
		if (ColorString != null)
		{
			int num = ColorString.LastIndexOf('&');
			if (num >= 0)
			{
				return ColorString[num + 1].ToString();
			}
		}
		return "y";
	}

	public string GetBackgroundColor()
	{
		if (ColorString != null)
		{
			int num = ColorString.LastIndexOf('^');
			if (num >= 0)
			{
				return ColorString[num + 1].ToString();
			}
		}
		return "k";
	}

	public void SetForegroundColor(char color)
	{
		if (string.IsNullOrEmpty(ColorString))
		{
			ColorString = "&" + color;
		}
		else if (ColorString.Contains("&"))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(ColorString);
			stringBuilder[ColorString.LastIndexOf('&') + 1] = color;
			ColorString = stringBuilder.ToString();
		}
		else
		{
			ColorString = "&" + color + ColorString;
		}
		if (!string.IsNullOrEmpty(TileColor))
		{
			if (TileColor.Contains("&"))
			{
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				stringBuilder2.Append(TileColor);
				stringBuilder2[TileColor.LastIndexOf('&') + 1] = color;
				TileColor = stringBuilder2.ToString();
			}
			else
			{
				TileColor = "&" + color + TileColor;
			}
		}
	}

	public void SetForegroundColor(string color)
	{
		if (!string.IsNullOrEmpty(color))
		{
			if (color[0] == '&' && color.Length > 1)
			{
				SetForegroundColor(color[1]);
			}
			else
			{
				SetForegroundColor(color[0]);
			}
		}
	}

	public void SetBackgroundColor(char color)
	{
		if (string.IsNullOrEmpty(ColorString))
		{
			ColorString = "^" + color;
		}
		else if (ColorString.Contains("^"))
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(ColorString);
			stringBuilder[ColorString.LastIndexOf('^') + 1] = color;
			ColorString = stringBuilder.ToString();
		}
		else
		{
			ColorString = ColorString + "^" + color;
		}
		if (!string.IsNullOrEmpty(TileColor))
		{
			if (TileColor.Contains("^"))
			{
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				stringBuilder2.Append(TileColor);
				stringBuilder2[TileColor.LastIndexOf('^') + 1] = color;
				TileColor = stringBuilder2.ToString();
			}
			else
			{
				TileColor = TileColor + "^" + color;
			}
		}
	}

	public void SetBackgroundColor(string color)
	{
		if (!string.IsNullOrEmpty(color))
		{
			if (color[0] == '^' && color.Length > 1)
			{
				SetBackgroundColor(color[1]);
			}
			else
			{
				SetBackgroundColor(color[0]);
			}
		}
	}

	public bool SetRenderString(string s)
	{
		string text = ProcessRenderString(s);
		if (text != RenderString)
		{
			RenderString = text;
			return true;
		}
		return false;
	}

	public override bool SameAs(IPart p)
	{
		Render render = p as Render;
		if (render.DisplayName != DisplayName)
		{
			return false;
		}
		if (render.RenderString != RenderString)
		{
			return false;
		}
		if (render.ColorString != ColorString && !IgnoreColorForStack)
		{
			return false;
		}
		if (render.DetailColor != DetailColor && !IgnoreColorForStack)
		{
			return false;
		}
		if (render.TileColor != TileColor && !IgnoreColorForStack)
		{
			return false;
		}
		if (render.RenderLayer != RenderLayer)
		{
			return false;
		}
		if (render.Visible != Visible)
		{
			return false;
		}
		if (render.Occluding != Occluding)
		{
			return false;
		}
		if (render.RenderIfDark != RenderIfDark)
		{
			return false;
		}
		if (render.CustomRender != CustomRender)
		{
			return false;
		}
		if (render.Tile != Tile)
		{
			return false;
		}
		if (render.Flags != Flags)
		{
			return false;
		}
		return base.SameAs(p);
	}

	[Obsolete("save compat")]
	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		if (Reader.FileVersion <= 262 && ParentObject.HasIntProperty("Wall") && (DetailColor.IsNullOrEmpty() || DetailColor == "k"))
		{
			char? foreground = null;
			char? background = null;
			ColorUtility.FindLastForegroundAndBackground(TileColor.IsNullOrEmpty() ? ColorString : TileColor, ref foreground, ref background);
			if (foreground.HasValue && background.HasValue && background != 'k')
			{
				char? c = foreground;
				TileColor = "&" + c;
				DetailColor = background.ToString();
			}
		}
	}

	public void FinalizeBuild()
	{
		RenderString = ProcessRenderString(RenderString);
	}

	public static string ProcessRenderString(string what)
	{
		if (what.Length > 1)
		{
			what = ((char)Convert.ToInt32(what)).ToString();
		}
		else if (what == "&")
		{
			what = "&&";
		}
		else if (what == "^")
		{
			what = "^^";
		}
		return what;
	}

	public bool getHFlip()
	{
		bool result = HFlip;
		if (Tile != null && ParentObject != null && ParentObject.pBrain != null)
		{
			GameObject partyLeader = ParentObject.pBrain.PartyLeader;
			if (partyLeader != null && partyLeader.IsPlayer() && Options.UseTiles)
			{
				result = !HFlip;
			}
		}
		return result;
	}

	public bool getVFlip()
	{
		return VFlip;
	}

	public string getTile()
	{
		return Tile;
	}

	public string getRenderString()
	{
		return RenderString;
	}

	public string getColorString()
	{
		return ColorString;
	}

	public string getTileColor()
	{
		if (!string.IsNullOrEmpty(TileColor))
		{
			return TileColor;
		}
		return null;
	}

	public char getDetailColor()
	{
		if (!string.IsNullOrEmpty(DetailColor))
		{
			return DetailColor[0];
		}
		return '\0';
	}

	public string GetRenderColor()
	{
		if (Globals.RenderMode != RenderModeType.Tiles)
		{
			return ColorString;
		}
		if (!string.IsNullOrEmpty(TileColor))
		{
			return TileColor;
		}
		return ColorString;
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
}
