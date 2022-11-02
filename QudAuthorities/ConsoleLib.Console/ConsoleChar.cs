using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleLib.Console;

public class ConsoleChar
{
	public class IndexedProperty<TIndex, TValue>
	{
		private readonly Action<TIndex, TValue> SetAction;

		private readonly Func<TIndex, TValue> GetFunc;

		public TValue this[TIndex i]
		{
			get
			{
				return GetFunc(i);
			}
			set
			{
				SetAction(i, value);
			}
		}

		public IndexedProperty(Func<TIndex, TValue> getFunc, Action<TIndex, TValue> setAction)
		{
			GetFunc = getFunc;
			SetAction = setAction;
		}
	}

	[Obsolete("Use TileForeground instead.")]
	public IndexedProperty<int, Color> TileLayerForeground;

	[Obsolete("Use TileBackground instead")]
	public IndexedProperty<int, Color> TileLayerBackground;

	[Obsolete("Use Tile instead")]
	public IndexedProperty<int, string> TileLayer;

	public Color _Foreground = Color.grey;

	public Color _Background = Color.black;

	public Color _TileForeground = Color.grey;

	public Color _TileBackground = Color.black;

	public Color _Detail = Color.magenta;

	public bool _HFlip;

	public bool VFlip;

	public string _Tile;

	public char BackupChar;

	public char _Char;

	[Obsolete("Don't use this. Set foreground and background colors directly. Will be removed ~Q2 2021")]
	public ushort _Attributes;

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

	public Color TileForeground
	{
		get
		{
			return _TileForeground;
		}
		set
		{
			_TileForeground = value;
		}
	}

	public Color TileBackground
	{
		get
		{
			return _TileBackground;
		}
		set
		{
			_TileBackground = value;
		}
	}

	public string Tile
	{
		get
		{
			return _Tile;
		}
		set
		{
			Char = '\0';
			_Tile = value;
		}
	}

	public char Char
	{
		get
		{
			return _Char;
		}
		set
		{
			if (value != 0)
			{
				HFlip = false;
				VFlip = false;
			}
			_Char = value;
		}
	}

	[Obsolete("Don't use this. Set foreground and background colors directly. Will be removed ~Q2 2021")]
	public ushort Attributes
	{
		get
		{
			return _Attributes;
		}
		set
		{
			_Attributes = value;
			foreach (KeyValuePair<ushort, char> item in ColorUtility.ColorAttributeToCharMap)
			{
				if ((_Attributes & item.Key) == item.Key)
				{
					Foreground = ColorUtility.colorFromChar(item.Value);
				}
				if ((_Attributes & (ushort)(item.Key << 5)) == (ushort)(item.Key << 5))
				{
					Background = ColorUtility.colorFromChar(item.Value);
				}
			}
		}
	}

	public Color Foreground
	{
		get
		{
			return _Foreground;
		}
		set
		{
			_Foreground = value;
		}
	}

	public Color Detail
	{
		get
		{
			return _Detail;
		}
		set
		{
			_Detail = value;
		}
	}

	public Color Background
	{
		get
		{
			return _Background;
		}
		set
		{
			SetBackground(value);
		}
	}

	public char ForegroundCode
	{
		get
		{
			if (!ColorUtility.ColorToCharMap.TryGetValue(_Foreground, out var value))
			{
				return 'k';
			}
			return value;
		}
	}

	public char BackgroundCode
	{
		get
		{
			if (!ColorUtility.ColorToCharMap.TryGetValue(_Background, out var value))
			{
				return 'k';
			}
			return value;
		}
	}

	public char DetailCode
	{
		get
		{
			if (!ColorUtility.ColorToCharMap.TryGetValue(_Detail, out var value))
			{
				return 'k';
			}
			return value;
		}
	}

	public ConsoleChar()
	{
		Char = ' ';
		Clear();
		TileLayerForeground = new IndexedProperty<int, Color>((int i) => TileForeground, delegate(int i, Color v)
		{
			TileForeground = v;
		});
		TileLayerBackground = new IndexedProperty<int, Color>((int i) => Detail, delegate(int i, Color v)
		{
			Detail = v;
		});
		TileLayer = new IndexedProperty<int, string>((int i) => Tile, delegate(int i, string v)
		{
			Tile = v;
		});
	}

	public ConsoleChar(byte c)
	{
		Char = (char)c;
	}

	public ConsoleChar(char c)
	{
		Char = c;
	}

	public ConsoleChar(char c, TextColor a)
	{
		Char = c;
		_Foreground = ColorUtility.colorFromTextColor(a);
	}

	public ConsoleChar(byte c, TextColor a)
	{
		Char = (char)c;
		_Foreground = ColorUtility.colorFromTextColor(a);
	}

	[Obsolete("Obsolete and unused. Will be removed ~Q1 2021")]
	public ConsoleChar(byte c, ushort a)
	{
		throw new NotImplementedException();
	}

	public static Color GetColor(char code)
	{
		if (!ColorUtility.ColorMap.TryGetValue(code, out var value))
		{
			MetricsManager.LogError("unknown color code " + code);
			return Color.magenta;
		}
		return value;
	}

	public static Color GetColor(string code)
	{
		if (!ColorUtility.ColorAliasMap.TryGetValue(code, out var value))
		{
			MetricsManager.LogError("unknown color code " + code);
			return Color.magenta;
		}
		return value;
	}

	public void Clear()
	{
		if (ColorUtility.ColorMap != null)
		{
			Char = ' ';
			_Tile = null;
			_Foreground = GetColor(ColorUtility.DEFAULT_FOREGROUND);
			_Background = GetColor(ColorUtility.DEFAULT_BACKGROUND);
			_TileForeground = GetColor(ColorUtility.DEFAULT_FOREGROUND);
			_TileBackground = GetColor(ColorUtility.DEFAULT_BACKGROUND);
			_Detail = GetColor(ColorUtility.DEFAULT_DETAIL);
			HFlip = false;
			VFlip = false;
		}
	}

	public override bool Equals(object obj)
	{
		ConsoleChar consoleChar = obj as ConsoleChar;
		if (consoleChar == null)
		{
			return false;
		}
		if (consoleChar.Char != Char)
		{
			return false;
		}
		if (consoleChar._Tile != _Tile)
		{
			return false;
		}
		if (consoleChar._Foreground != _Foreground)
		{
			return false;
		}
		if (consoleChar._Background != _Background)
		{
			return false;
		}
		if (consoleChar._Detail != _Detail)
		{
			return false;
		}
		if (consoleChar._TileBackground != _TileBackground)
		{
			return false;
		}
		if (consoleChar._TileForeground != _TileForeground)
		{
			return false;
		}
		if (consoleChar.HFlip != _HFlip)
		{
			return false;
		}
		if (consoleChar.VFlip != VFlip)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return new { Char, Attributes, _Foreground, _Background, _Detail, _TileBackground, _TileForeground }.GetHashCode();
	}

	public static bool operator ==(ConsoleChar a, ConsoleChar b)
	{
		if ((object)a == null)
		{
			if ((object)b == null)
			{
				return true;
			}
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(ConsoleChar a, ConsoleChar b)
	{
		return !(a == b);
	}

	public ConsoleChar GetCopy()
	{
		ConsoleChar consoleChar = new ConsoleChar();
		consoleChar.Copy(this);
		return consoleChar;
	}

	public void Copy(ConsoleChar C)
	{
		Char = C.Char;
		_Tile = C._Tile;
		_Foreground = C._Foreground;
		_Background = C._Background;
		_TileForeground = C._TileForeground;
		_TileBackground = C._TileBackground;
		_Detail = C._Detail;
		_HFlip = C._HFlip;
		VFlip = C.VFlip;
	}

	public void SetDetail(char colorCode)
	{
		_Detail = ColorUtility.ColorMap[colorCode];
	}

	public void SetDetail(Color color)
	{
		_Detail = color;
	}

	public void SetForeground(char colorCode)
	{
		_Foreground = ColorUtility.ColorMap[colorCode];
	}

	public void SetForeground(Color color)
	{
		_Foreground = color;
	}

	public void SetBackground(Color color)
	{
		_Background = color;
	}

	public void SetBackground(char colorCode)
	{
		_Background = ColorUtility.ColorMap[colorCode];
	}
}
