using System;
using ConsoleLib.Console;
using UnityEngine;

namespace Qud.UI;

[Serializable]
public struct QudMenuItem
{
	public string text;

	public string command;

	public string hotkey;

	public IRenderable icon;

	public override string ToString()
	{
		return JsonUtility.ToJson(this);
	}

	public override int GetHashCode()
	{
		return text.GetHashCode() & command.GetHashCode() & hotkey.GetHashCode();
	}
}
