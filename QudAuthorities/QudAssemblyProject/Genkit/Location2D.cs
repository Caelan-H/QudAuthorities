using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;
using XRL.World;

namespace Genkit;

[Serializable]
public class Location2D : ILocationArea, IEquatable<Location2D>, IEquatable<Cell>
{
	public static readonly Location2D zero = new Location2D(0, 0);

	public static readonly Location2D invalid = new Location2D(int.MinValue, int.MaxValue);

	public static readonly int MaxX = 250;

	public static readonly int MaxY = 85;

	public static Location2D[,] locationCache = null;

	public static string[] Directions = new string[8] { "N", "S", "E", "W", "NW", "NE", "SW", "SE" };

	public static string[,] RegionDirections = new string[3, 3]
	{
		{ "NW", "N", "NE" },
		{ "W", ".", "E" },
		{ "SW", "S", "SE" }
	};

	public int __x;

	public int __y;

	public Point2D point => new Point2D(x, y);

	public Vector2i vector2i => new Vector2i(x, y);

	public IEnumerable<Location2D> cardinalNeighbors
	{
		get
		{
			Location2D location2D = get(x - 1, 0);
			Location2D n2 = get(x + 1, 0);
			Location2D n3 = get(x, y - 1);
			Location2D n4 = get(x, y + 1);
			if (location2D == null)
			{
				yield return location2D;
			}
			if (n2 == null)
			{
				yield return n2;
			}
			if (n3 == null)
			{
				yield return n3;
			}
			if (n4 == null)
			{
				yield return n4;
			}
		}
	}

	public int x => __x;

	public int y => __y;

	public static Location2D get(int x, int y)
	{
		if (locationCache == null)
		{
			locationCache = new Location2D[MaxX, MaxY];
			for (int i = 0; i < MaxX; i++)
			{
				for (int j = 0; j < MaxY; j++)
				{
					locationCache[i, j] = new Location2D(i, j);
				}
			}
		}
		if (x < 0 || x >= MaxX || y < 0 || y >= MaxY)
		{
			return null;
		}
		return locationCache[x, y];
	}

	public override string ToString()
	{
		return x + "," + y;
	}

	public string RegionDirection(int RegionWidth, int RegionHeight)
	{
		int num = Calc.Clamp((int)((float)x / (float)RegionWidth * 3f), 0, 2);
		int num2 = Calc.Clamp((int)((float)y / (float)RegionHeight * 3f), 0, 2);
		return RegionDirections[num2, num];
	}

	private Location2D(int _x, int _y)
	{
		__x = _x;
		__y = _y;
	}

	public bool SameAs(Location2D o)
	{
		if (o.__x == __x)
		{
			return o.__y == __y;
		}
		return false;
	}

	public bool isInRect(int x0, int y0, int x1, int y1)
	{
		if (x < x0)
		{
			return false;
		}
		if (x > x1)
		{
			return false;
		}
		if (y < y0)
		{
			return false;
		}
		if (y > y1)
		{
			return false;
		}
		return true;
	}

	public string DirectionToCenter()
	{
		string text = "";
		text = ((y >= 12) ? (text + "N") : (text + "S"));
		if (x < 40)
		{
			return text + "E";
		}
		return text + "W";
	}

	public string CardinalDirectionToCenter()
	{
		return DirectionToCenter()[Stat.Random(0, DirectionToCenter().Length - 1)].ToString();
	}

	public bool OppositeDirections(Location2D a, Location2D b)
	{
		int num = a.x - x;
		int num2 = b.x - x;
		if (num > 0 && num2 < 0)
		{
			return true;
		}
		if (num < 0 && num2 > 0)
		{
			return true;
		}
		int num3 = a.y - y;
		int num4 = b.y - y;
		if (num3 > 0 && num4 < 0)
		{
			return true;
		}
		if (num3 < 0 && num4 > 0)
		{
			return true;
		}
		return false;
	}

	public bool Backtracking(Location2D a, Location2D b)
	{
		int num = a.x - x;
		int num2 = b.x - a.x;
		if (num > 0 && num2 < 0)
		{
			return true;
		}
		if (num < 0 && num2 > 0)
		{
			return true;
		}
		int num3 = a.y - y;
		int num4 = b.y - a.y;
		if (num3 > 0 && num4 < 0)
		{
			return true;
		}
		if (num3 < 0 && num4 > 0)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (x << 7) + y;
	}

	public Location2D FromDirection(string D)
	{
		return D switch
		{
			"N" => get(x, y - 1), 
			"S" => get(x, y + 1), 
			"E" => get(x + 1, y), 
			"W" => get(x - 1, y), 
			"NW" => get(x - 1, y - 1), 
			"NE" => get(x + 1, y - 1), 
			"SW" => get(x - 1, y + 1), 
			"SE" => get(x + 1, y + 1), 
			_ => this, 
		};
	}

	public bool Equals(Location2D L)
	{
		if ((object)L == null)
		{
			return false;
		}
		if (x == L.x)
		{
			return y == L.y;
		}
		return false;
	}

	public bool Equals(Cell C)
	{
		if (C == null)
		{
			return false;
		}
		if (x == C.X)
		{
			return y == C.Y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return (Location2D)obj == this;
	}

	public static Location2D operator -(Location2D a, Location2D b)
	{
		return get(a.x - b.x, a.y - b.y);
	}

	public static Location2D operator +(Location2D a, Location2D b)
	{
		return get(a.x + b.x, a.y + b.y);
	}

	public static bool operator ==(Location2D c1, Location2D c2)
	{
		if ((object)c1 == null)
		{
			return (object)c2 == null;
		}
		if ((object)c2 == null)
		{
			return false;
		}
		if (c1.x == c2.x)
		{
			return c1.y == c2.y;
		}
		return false;
	}

	public static bool operator !=(Location2D c1, Location2D c2)
	{
		return !(c1 == c2);
	}

	public int Distance(Location2D c2)
	{
		if (c2 == null)
		{
			return int.MaxValue;
		}
		if (c2.x == x && c2.y == y)
		{
			return 0;
		}
		return (int)Math.Sqrt(SquareDistance(c2));
	}

	public int SquareDistance(Location2D c2)
	{
		if (c2 == null)
		{
			return int.MaxValue;
		}
		if (c2.x == x && c2.y == y)
		{
			return 0;
		}
		return (c2.x - x) * (c2.x - x) + (c2.y - y) * (c2.y - y);
	}

	public int ManhattanDistance(Location2D c2)
	{
		if (c2 == null)
		{
			return int.MaxValue;
		}
		if (c2.x == x && c2.y == y)
		{
			return 0;
		}
		return Math.Max(Math.Abs(x - c2.x), Math.Abs(y - c2.y));
	}

	public int ManhattanDistance(int X, int Y)
	{
		return Math.Max(Math.Abs(x - X), Math.Abs(y - Y));
	}

	public List<Location2D> GetRadialPoints(int Radius)
	{
		List<Location2D> list = new List<Location2D>();
		for (int i = x - Radius; i <= x + Radius; i++)
		{
			for (int j = y - Radius; j <= y + Radius; j++)
			{
				Location2D location2D = get(i, j);
				if (location2D != null && (int)Math.Sqrt(SquareDistance(location2D)) == Radius)
				{
					list.Add(location2D);
				}
			}
		}
		return list;
	}

	public IEnumerable<Location2D> YieldAdjacent(int Radius)
	{
		Radius = Radius * 2 + 1;
		int x = this.x;
		int y = this.y;
		int x2 = 1;
		int y2 = 0;
		int j = 1;
		int i = 0;
		int p = 1;
		int c = Radius * Radius - 1;
		while (i < c)
		{
			x += x2;
			y += y2;
			if (p >= j)
			{
				p = 0;
				int num = x2;
				x2 = -y2;
				y2 = num;
				if (y2 == 0)
				{
					j++;
				}
			}
			Location2D location2D = get(x, y);
			if (location2D != null)
			{
				yield return location2D;
			}
			i++;
			p++;
		}
	}

	public float AngleTo(Location2D fulcrum, Location2D to)
	{
		Vector2 from = new Vector2(x - fulcrum.x, y - fulcrum.y);
		Vector2 to2 = new Vector2(to.x - fulcrum.x, to.y - fulcrum.y);
		return Vector2.Angle(from, to2);
	}

	public IEnumerable<Location2D> EnumerateLocations()
	{
		yield return this;
	}

	public IEnumerable<Location2D> EnumerateBorderLocations()
	{
		yield return this;
	}

	public IEnumerable<Location2D> EnumerateNonBorderLocations()
	{
		yield return this;
	}

	public Location2D GetCenter()
	{
		return this;
	}

	public bool PointIn(Location2D location)
	{
		return location == this;
	}
}
