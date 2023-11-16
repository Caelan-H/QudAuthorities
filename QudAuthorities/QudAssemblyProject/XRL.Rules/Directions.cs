using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.World;

namespace XRL.Rules;

public static class Directions
{
	public static string[] DirectionList = new string[8] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };

	public static string[] CardinalDirectionList = new string[4] { "N", "E", "S", "W" };

	public static Dictionary<string, string[]> DirectionsAdjacent = new Dictionary<string, string[]>(9)
	{
		{
			"NW",
			new string[2] { "W", "N" }
		},
		{
			"N",
			new string[2] { "NW", "NE" }
		},
		{
			"NE",
			new string[2] { "N", "E" }
		},
		{
			"E",
			new string[2] { "NE", "SE" }
		},
		{
			"SE",
			new string[2] { "E", "S" }
		},
		{
			"S",
			new string[2] { "SE", "SW" }
		},
		{
			"SW",
			new string[2] { "S", "W" }
		},
		{
			"W",
			new string[2] { "SW", "NW" }
		},
		{ ".", DirectionList }
	};

	public static Dictionary<string, string[]> DirectionsOrthogonal = new Dictionary<string, string[]>(9)
	{
		{
			"NW",
			new string[2] { "W", "E" }
		},
		{
			"N",
			new string[2] { "W", "E" }
		},
		{
			"NE",
			new string[2] { "W", "S" }
		},
		{
			"E",
			new string[2] { "N", "S" }
		},
		{
			"SE",
			new string[2] { "N", "W" }
		},
		{
			"S",
			new string[2] { "N", "W" }
		},
		{
			"SW",
			new string[2] { "N", "E" }
		},
		{
			"W",
			new string[2] { "N", "S" }
		},
		{
			".",
			new string[2] { "W", "E" }
		}
	};

	public static bool IsActualDirection(string dir)
	{
		if (string.IsNullOrEmpty(dir))
		{
			return false;
		}
		if (Array.IndexOf(DirectionList, dir) != -1)
		{
			return true;
		}
		if (dir == "U" || dir == "D")
		{
			return true;
		}
		return false;
	}

	public static string GetRandomDirection()
	{
		return DirectionList.GetRandomElement();
	}

	public static string GetRandomCardinalDirection()
	{
		return CardinalDirectionList.GetRandomElement();
	}

	public static string GetArrowForDirection(string D)
	{
		return D switch
		{
			"N" => "\u0018", 
			"S" => "\u0019", 
			"E" => "\u001a", 
			"W" => "\u001b", 
			"n" => "\u0018", 
			"s" => "\u0019", 
			"e" => "\u001a", 
			"w" => "\u001b", 
			"<" => "<", 
			">" => ">", 
			_ => "?", 
		};
	}

	public static string GetExpandedDirection(string D)
	{
		return D switch
		{
			"N" => "north", 
			"S" => "south", 
			"E" => "east", 
			"W" => "west", 
			"NW" => "northwest", 
			"NE" => "northeast", 
			"SW" => "southwest", 
			"SE" => "southeast", 
			"n" => "north", 
			"s" => "south", 
			"e" => "east", 
			"w" => "west", 
			"nw" => "northwest", 
			"ne" => "northeast", 
			"sw" => "southwest", 
			"se" => "southeast", 
			"<" => "up", 
			">" => "down", 
			"." => "here", 
			"?" => "somewhere", 
			_ => D, 
		};
	}

	public static string GetIndicativeDirection(string D)
	{
		return D switch
		{
			"N" => "northward", 
			"S" => "southward", 
			"E" => "eastward", 
			"W" => "westward", 
			"NW" => "northwestward", 
			"NE" => "northeastward", 
			"SW" => "southwestward", 
			"SE" => "southeastward", 
			"n" => "northward", 
			"s" => "southwarwd", 
			"e" => "eastward", 
			"w" => "westward", 
			"nw" => "northwestward", 
			"ne" => "northeastward", 
			"sw" => "southwestward", 
			"se" => "southeastward", 
			"<" => "upward", 
			">" => "downward", 
			"." => "here", 
			"?" => "somewhere", 
			_ => D, 
		};
	}

	public static string GetDirectionDescription(string D)
	{
		return D switch
		{
			">" => "below", 
			"<" => "above", 
			"." => "here", 
			"?" => "somewhere", 
			_ => "to the " + GetExpandedDirection(D), 
		};
	}

	public static string GetDirectionDescription(XRL.World.GameObject who, string D)
	{
		if (who == null || who.IsPlayer())
		{
			return GetDirectionDescription(D);
		}
		return D switch
		{
			">" => "below " + who.them, 
			"<" => "above " + who.them, 
			"." => "near " + who.them, 
			_ => "to " + who.its + " " + GetExpandedDirection(D), 
		};
	}

	public static string GetIncomingDirectionDescription(string D)
	{
		return D switch
		{
			">" => "from below", 
			"<" => "from above", 
			"." => "from nearby", 
			"?" => "from somewhere", 
			_ => "from the " + GetExpandedDirection(D), 
		};
	}

	public static string GetIncomingDirectionDescription(XRL.World.GameObject who, string D)
	{
		if (who == null || who.IsPlayer())
		{
			return GetIncomingDirectionDescription(D);
		}
		return D switch
		{
			">" => "from below " + who.them, 
			"<" => "from above " + who.them, 
			"." => "from near " + who.them, 
			_ => "from " + who.its + " " + GetExpandedDirection(D), 
		};
	}

	public static string GetDirectionShortDescription(string D)
	{
		return D switch
		{
			">" => "D", 
			"<" => "U", 
			"." => "here", 
			"?" => "somewhere", 
			_ => D, 
		};
	}

	public static string GetOppositeDirection(string D)
	{
		if (D == "N")
		{
			return "S";
		}
		if (D == "S")
		{
			return "N";
		}
		if (D == "E")
		{
			return "W";
		}
		if (D == "W")
		{
			return "E";
		}
		if (D == "NW")
		{
			return "SE";
		}
		if (D == "NE")
		{
			return "SW";
		}
		if (D == "SW")
		{
			return "NE";
		}
		if (D == "SE")
		{
			return "NW";
		}
		if (D == "n")
		{
			return "s";
		}
		if (D == "s")
		{
			return "n";
		}
		if (D == "e")
		{
			return "w";
		}
		if (D == "w")
		{
			return "e";
		}
		if (D == "nw")
		{
			return "se";
		}
		if (D == "ne")
		{
			return "sw";
		}
		if (D == "sw")
		{
			return "ne";
		}
		if (D == "se")
		{
			return "nw";
		}
		if (D == "north")
		{
			return "south";
		}
		if (D == "south")
		{
			return "north";
		}
		if (D == "east")
		{
			return "west";
		}
		if (D == "west")
		{
			return "east";
		}
		if (D == "north-east")
		{
			return "south-west";
		}
		if (D == "south-east")
		{
			return "north-west";
		}
		if (D == "north-west")
		{
			return "south-east";
		}
		if (D == "south-east")
		{
			return "south-west";
		}
		if (D == "northeast")
		{
			return "southwest";
		}
		if (D == "southeast")
		{
			return "northwest";
		}
		if (D == "northwest")
		{
			return "southeast";
		}
		if (D == "southeast")
		{
			return "southwest";
		}
		return ".";
	}

	public static void ApplyDirection(string dir, ref int x, ref int y, int d = 1)
	{
		switch (dir)
		{
		case "N":
			y -= d;
			break;
		case "S":
			y += d;
			break;
		case "W":
			x -= d;
			break;
		case "E":
			x += d;
			break;
		case "NW":
			x -= d;
			y -= d;
			break;
		case "NE":
			x += d;
			y -= d;
			break;
		case "SW":
			x -= d;
			y += d;
			break;
		case "SE":
			x += d;
			y -= d;
			break;
		case "U":
		case "D":
			Debug.LogWarning("handling contextually unsupported direction " + dir);
			break;
		case null:
			Debug.LogWarning("handling null direction");
			break;
		default:
			Debug.LogWarning("handling unrecognized direction " + dir);
			break;
		case ".":
			break;
		}
	}

	public static void ApplyDirection(string dir, ref int x, ref int y, ref int z, int d = 1)
	{
		switch (dir)
		{
		case "N":
			y -= d;
			break;
		case "S":
			y += d;
			break;
		case "W":
			x -= d;
			break;
		case "E":
			x += d;
			break;
		case "NW":
			x -= d;
			y -= d;
			break;
		case "NE":
			x += d;
			y -= d;
			break;
		case "SW":
			x -= d;
			y += d;
			break;
		case "SE":
			x += d;
			y += d;
			break;
		case "U":
			z -= d;
			break;
		case "D":
			z += d;
			break;
		case null:
			Debug.LogWarning("handling null direction");
			break;
		default:
			Debug.LogWarning("handling unrecognized direction " + dir);
			break;
		case ".":
			break;
		}
	}

	public static void ApplyDirectionGlobal(string dir, ref int x, ref int y, ref int z, ref int wx, ref int wy, int d = 1)
	{
		ApplyDirection(dir, ref x, ref y, ref z, d);
		while (x < 0)
		{
			x += 3;
			wx--;
		}
		while (x > 2)
		{
			x -= 3;
			wx++;
		}
		while (y < 0)
		{
			y += 3;
			wy--;
		}
		while (y > 2)
		{
			y -= 3;
			wy++;
		}
	}

	public static string CombineDirections(string A, string B, int AD = 1, int BD = 1)
	{
		int x = 0;
		int y = 0;
		ApplyDirection(A, ref x, ref y, AD);
		ApplyDirection(B, ref x, ref y, BD);
		if (x > 0)
		{
			if (y > 0)
			{
				return "SE";
			}
			if (y < 0)
			{
				return "NE";
			}
			return "E";
		}
		if (x < 0)
		{
			if (y > 0)
			{
				return "SW";
			}
			if (y < 0)
			{
				return "NW";
			}
			return "W";
		}
		if (y > 0)
		{
			return "S";
		}
		if (y < 0)
		{
			return "N";
		}
		return ".";
	}

	public static string[] GetOrthogonalDirections(string Middle)
	{
		if (!DirectionsOrthogonal.ContainsKey(Middle))
		{
			return DirectionsOrthogonal["."];
		}
		return DirectionsOrthogonal[Middle];
	}

	public static string[] GetAdjacentDirections(string Dir)
	{
		if (!DirectionsAdjacent.ContainsKey(Dir))
		{
			return null;
		}
		return DirectionsAdjacent[Dir];
	}

	public static List<string> GetAdjacentDirections(string Middle, int Range)
	{
		int num = 0;
		int i = 0;
		for (int num2 = DirectionList.Length; i < num2; i++)
		{
			if (DirectionList[i].EqualsNoCase(Middle))
			{
				num = i;
				break;
			}
		}
		List<string> list = new List<string>();
		for (int j = num - Range; j <= num + Range; j++)
		{
			int num3 = j;
			if (num3 < 0)
			{
				num3 += DirectionList.Length;
			}
			if (num3 >= DirectionList.Length)
			{
				num3 -= DirectionList.Length;
			}
			list.Add(DirectionList[num3]);
		}
		return list;
	}

	public static List<string> DirectionsFromAngle(int degrees, int steps)
	{
		float num = (float)degrees / 58f;
		float num2 = (float)Math.Sin(num);
		float num3 = (float)Math.Cos(num);
		List<string> list = new List<string>(steps);
		float num4 = 0f;
		float num5 = 0f;
		int num6 = 0;
		int num7 = 0;
		int i = 0;
		for (int num8 = steps * 100; i < num8; i++)
		{
			num4 += num2;
			num5 += num3;
			int num9 = (int)Math.Round(num4, MidpointRounding.AwayFromZero);
			int num10 = (int)Math.Round(num5, MidpointRounding.AwayFromZero);
			if (num9 != num6 || num10 != num7)
			{
				string text = "";
				if (num10 > num7)
				{
					text += "N";
				}
				else if (num10 < num7)
				{
					text += "S";
				}
				if (num9 > num6)
				{
					text += "E";
				}
				else if (num9 < num6)
				{
					text += "W";
				}
				list.Add(text);
				if (list.Count >= steps)
				{
					break;
				}
				num6 = num9;
				num7 = num10;
			}
		}
		return list;
	}
}
