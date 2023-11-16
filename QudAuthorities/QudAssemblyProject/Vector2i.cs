using System;
using UnityEngine;

[Serializable]
public class Vector2i
{
	public int x;

	public int y;

	public Vector2i(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Vector2i)
		{
			return this == (Vector2i)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode();
	}

	public static bool operator ==(Vector2i x, Vector2i y)
	{
		if ((object)x == null)
		{
			return (object)y == null;
		}
		if ((object)y == null)
		{
			return false;
		}
		if (x.x == y.x)
		{
			return x.y == y.y;
		}
		return false;
	}

	public static bool operator !=(Vector2i x, Vector2i y)
	{
		return !(x == y);
	}

	public int DistanceTo(Vector2i d)
	{
		return (int)Mathf.Sqrt((d.x - x) * (d.x - x) + (d.y - y) * (d.y - y));
	}
}
