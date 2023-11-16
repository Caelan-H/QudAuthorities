using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;

public class HotkeySpread
{
	private List<UnityEngine.KeyCode> keys = new List<UnityEngine.KeyCode>();

	private int pos;

	public HotkeySpread(List<UnityEngine.KeyCode> keys)
	{
		this.keys = keys;
	}

	public void restart()
	{
		pos = 0;
	}

	public void next()
	{
		pos++;
	}

	public void prev()
	{
		if (pos > 0)
		{
			pos--;
		}
	}

	public UnityEngine.KeyCode code()
	{
		return codeAt(pos);
	}

	public char ch()
	{
		return charAt(pos);
	}

	public UnityEngine.KeyCode codeAt(int n)
	{
		if (keys.Count <= n)
		{
			return UnityEngine.KeyCode.None;
		}
		return keys[n];
	}

	public char charAt(int n)
	{
		return Keyboard.ConvertKeycodeToLowercaseChar(codeAt(n));
	}

	public static HotkeySpread get(IEnumerable<string> layers)
	{
		return new HotkeySpread(ControlManager.GetHotkeySpread(layers));
	}
}
