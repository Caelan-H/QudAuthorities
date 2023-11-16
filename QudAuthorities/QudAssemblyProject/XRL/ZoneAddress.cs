using System;
using System.Text;

namespace XRL;

[Serializable]
public class ZoneAddress
{
	public static StringBuilder sb = new StringBuilder(256);

	public string world;

	public int wx = -1;

	public int wy = -1;

	public int x = -1;

	public int y = -1;

	public int z = -1;

	public ZoneAddress(string zoneID)
	{
		string[] array = zoneID.Split('.');
		world = array[0];
		if (array.Length >= 2)
		{
			wx = Convert.ToInt32(array[1]);
		}
		if (array.Length >= 3)
		{
			wy = Convert.ToInt32(array[2]);
		}
		if (array.Length >= 4)
		{
			x = Convert.ToInt32(array[3]);
		}
		if (array.Length >= 5)
		{
			y = Convert.ToInt32(array[4]);
		}
		if (array.Length >= 6)
		{
			z = Convert.ToInt32(array[5]);
		}
	}

	public string ToID()
	{
		sb.Length = 0;
		sb.Append(world).Append(".").Append(wx)
			.Append(".")
			.Append(wy)
			.Append(".")
			.Append(x)
			.Append(".")
			.Append(y)
			.Append(".")
			.Append(z);
		return sb.ToString();
	}

	public static ZoneAddress FromId(string zoneID)
	{
		return new ZoneAddress(zoneID);
	}
}
