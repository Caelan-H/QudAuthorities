using System;
using System.Text;

namespace XRL.World;

public static class ZoneID
{
	private static StringBuilder SB = new StringBuilder();

	public static string Assemble(string World, int ParasangX, int ParasangY, int ZoneX, int ZoneY, int ZoneZ)
	{
		return SB.Clear().Append(World).Append('.')
			.Append(ParasangX)
			.Append('.')
			.Append(ParasangY)
			.Append('.')
			.Append(ZoneX)
			.Append('.')
			.Append(ZoneY)
			.Append('.')
			.Append(ZoneZ)
			.ToString();
	}

	public static string Assemble(string World, int ParasangX, int ParasangY, string ZoneX, string ZoneY, int ZoneZ)
	{
		return SB.Clear().Append(World).Append('.')
			.Append(ParasangX)
			.Append('.')
			.Append(ParasangY)
			.Append('.')
			.Append(ZoneX)
			.Append('.')
			.Append(ZoneY)
			.Append('.')
			.Append(ZoneZ)
			.ToString();
	}

	public static bool Parse(string ID, out string World, out int ParasangX, out int ParasangY, out int ZoneX, out int ZoneY, out int ZoneZ)
	{
		ParasangX = 0;
		ParasangY = 0;
		ZoneX = 0;
		ZoneY = 0;
		ZoneZ = 10;
		int num = ID.IndexOf('.');
		if (num == -1)
		{
			World = ID;
			return false;
		}
		World = ID.Substring(0, num);
		int num2 = ID.IndexOf('.', num + 1);
		if (num2 == -1)
		{
			return false;
		}
		int num3 = ID.IndexOf('.', num2 + 1);
		if (num3 == -1)
		{
			return false;
		}
		int num4 = ID.IndexOf('.', num3 + 1);
		if (num4 == -1)
		{
			return false;
		}
		int num5 = ID.IndexOf('.', num4 + 1);
		if (num5 == -1)
		{
			return false;
		}
		try
		{
			ParasangX = int.Parse(ID.Substring(num + 1, num2 - num - 1));
			ParasangY = int.Parse(ID.Substring(num2 + 1, num3 - num2 - 1));
			ZoneX = int.Parse(ID.Substring(num3 + 1, num4 - num3 - 1));
			ZoneY = int.Parse(ID.Substring(num4 + 1, num5 - num4 - 1));
			ZoneZ = int.Parse(ID.Substring(num5 + 1, ID.Length - num5 - 1));
		}
		catch (Exception x)
		{
			MetricsManager.LogError("error parsing \"" + ID + "\"", x);
		}
		return true;
	}

	public static bool Parse(string ID, out string World, out int ParasangX, out int ParasangY)
	{
		int ZoneX;
		int ZoneY;
		int ZoneZ;
		return Parse(ID, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	public static bool Parse(string ID, out int ParasangX, out int ParasangY, out int ZoneX, out int ZoneY, out int ZoneZ)
	{
		string World;
		return Parse(ID, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	public static bool Parse(string ID, out int ParasangX, out int ParasangY, out int ZoneX, out int ZoneY)
	{
		string World;
		int ZoneZ;
		return Parse(ID, out World, out ParasangX, out ParasangY, out ZoneX, out ZoneY, out ZoneZ);
	}

	public static string GetWorldID(string ID)
	{
		Parse(ID, out var World, out var _, out var _, out var _, out var _, out var _);
		return World;
	}
}
