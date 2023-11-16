using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL.Rules;
using XRL.World;

namespace XRL;

public static class LoreGenerator
{
	private static StringBuilder SB = new StringBuilder();

	private static Dictionary<string, int[]> landmarks = new Dictionary<string, int[]>
	{
		{
			"Joppa",
			new int[2] { 11, 22 }
		},
		{
			"Red Rock",
			new int[2] { 11, 20 }
		},
		{
			"Grit Gate",
			new int[2] { 22, 14 }
		},
		{
			"the Six Day Stilt",
			new int[2] { 5, 2 }
		},
		{
			"Golgotha",
			new int[2] { 23, 9 }
		},
		{
			"Bethesda Susa",
			new int[2] { 25, 3 }
		},
		{
			"Kyakukya",
			new int[2] { 27, 20 }
		},
		{
			"Omonporch",
			new int[2] { 53, 4 }
		},
		{
			"the Yd Freehold",
			new int[2] { 67, 17 }
		}
	};

	private static Dictionary<string, int[]> landmarks_altStart = new Dictionary<string, int[]>
	{
		{
			"the ruins of Joppa",
			new int[2] { 11, 22 }
		},
		{
			"Red Rock",
			new int[2] { 11, 20 }
		},
		{
			"Grit Gate",
			new int[2] { 22, 14 }
		},
		{
			"the Six Day Stilt",
			new int[2] { 5, 2 }
		},
		{
			"Golgotha",
			new int[2] { 23, 9 }
		},
		{
			"Bethesda Susa",
			new int[2] { 25, 3 }
		},
		{
			"Kyakukya",
			new int[2] { 27, 20 }
		},
		{
			"Omonporch",
			new int[2] { 53, 4 }
		},
		{
			"the Yd Freehold",
			new int[2] { 67, 17 }
		}
	};

	public static string RuinOfHouseIsnerLore(int x, int y)
	{
		string text;
		int num;
		int num2;
		if (The.Game.AlternateStart)
		{
			text = landmarks_altStart.Keys.ToArray()[Stat.Random(0, landmarks_altStart.Keys.Count - 1)];
			num = x - landmarks_altStart[text][0];
			num2 = y - landmarks_altStart[text][1];
		}
		else
		{
			text = landmarks.Keys.ToArray()[Stat.Random(0, landmarks.Keys.Count - 1)];
			num = x - landmarks[text][0];
			num2 = y - landmarks[text][1];
		}
		string text2 = ((num < 0) ? "west" : "east");
		string text3 = ((num2 < 0) ? "north" : "south");
		num = Math.Abs(num);
		num2 = Math.Abs(num2);
		int num3;
		string text4;
		if (Stat.Random(0, 1) == 0)
		{
			num3 = num;
			text4 = text2;
		}
		else
		{
			num3 = num2;
			text4 = text3;
		}
		int num4 = Stat.Random(0, 40);
		string text5 = ((num4 < 10) ? "of the " : ((num4 < 20) ? "to the " : ((num4 >= 30) ? "in the " : "with the ")));
		num4 = Stat.Random(0, 70);
		string text6 = ((num4 < 10) ? "{Ruin of House Isner, " : ((num4 < 20) ? "{masterwork pistol Ruin of House Isner, " : ((num4 < 30) ? "{chest containing the Ruin of House Isner, " : ((num4 < 40) ? "{chest holding the Ruin of House Isner, " : ((num4 < 50) ? "{famous revolver Ruin of House Isner, " : ((num4 >= 60) ? "{lost masterwork pistol, " : "{hiding place of the Ruin of House Isner, "))))));
		num4 = Stat.Random(0, 60);
		string text7 = ((num4 < 10) ? ("said to be stored somewhere " + num3 + " parasangs " + text4 + " of " + text + ".}") : ((num4 < 20) ? ("rumored to be stored somewhere " + num3 + " parasangs " + text4 + " of " + text + ".}") : ((num4 < 30) ? ("located somewhere " + num3 + " parasangs " + text4 + " of " + text + ", as the story goes.}") : ((num4 < 40) ? ("located somewhere near " + text + " some " + num3 + " parasangs " + text4 + ".}") : ((num4 >= 50) ? ("where I stored it somewhere " + num3 + " parasangs " + text4 + " of " + text + ".}") : ("which I hid somewhere " + text4 + " of " + text + ", about " + num3 + " parasangs.}"))))));
		return text5 + text6 + text7;
	}

	public static string GenerateLandmarkDirectionsTo(Zone Z, bool bAltStarts = false)
	{
		return GenerateLandmarkDirectionsTo(Z.ZoneID, bAltStarts);
	}

	public static string GenerateLandmarkDirectionsTo(string ZoneID)
	{
		return GenerateLandmarkDirectionsTo(ZoneID, The.Game.AlternateStart);
	}

	public static string GenerateLandmarkDirectionsTo(string ZoneID, bool bAltStarts)
	{
		SB.Clear();
		ZoneAddress zoneAddress = ZoneAddress.FromId(ZoneID);
		string text = "";
		string text2 = "";
		int num = 0;
		int num2 = 0;
		num = 0;
		num2 = 0;
		int num3 = int.MaxValue;
		int num4 = int.MaxValue;
		foreach (KeyValuePair<string, int[]> item in bAltStarts ? landmarks_altStart : landmarks)
		{
			if (Math.Abs(zoneAddress.wx - item.Value[0]) + Math.Abs(zoneAddress.wy - item.Value[1]) < num3)
			{
				int num5 = Math.Abs(zoneAddress.wx - item.Value[0]);
				num4 = Math.Abs(zoneAddress.wy - item.Value[1]);
				num3 = num5 + num4;
				num = zoneAddress.wx - item.Value[0];
				num2 = zoneAddress.wy - item.Value[1];
				text = item.Key;
				text2 = item.Key;
			}
		}
		if (num == 0 && num2 == 0)
		{
			return "near " + text;
		}
		if (text == text2)
		{
			if (num < 0)
			{
				if (num == -1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang west");
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs west");
				}
			}
			else if (num > 0)
			{
				if (num == 1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang east");
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs east");
				}
			}
			if (num2 < 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(" and ");
				}
				if (num2 == -1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang north");
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs north");
				}
			}
			else if (num2 > 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(" and ");
				}
				if (num2 == 1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang south");
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs south");
				}
			}
			SB.Append(" of ").Append(text);
		}
		else
		{
			if (num < 0)
			{
				if (num == -1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang west of ").Append(text);
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs west of ").Append(text);
				}
			}
			else if (num > 0)
			{
				if (num == 1)
				{
					SB.Append(Math.Abs(num)).Append(" parasang east of ").Append(text);
				}
				else
				{
					SB.Append(Math.Abs(num)).Append(" parasangs east of ").Append(text);
				}
			}
			if (num2 < 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(", ");
				}
				if (num2 == -1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang north of ").Append(text2);
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs north of ").Append(text2);
				}
			}
			else if (num2 > 0)
			{
				if (SB.Length > 0)
				{
					SB.Append(", ");
				}
				if (num2 == 1)
				{
					SB.Append(Math.Abs(num2)).Append(" parasang south of ").Append(text2);
				}
				else
				{
					SB.Append(Math.Abs(num2)).Append(" parasangs south of ").Append(text2);
				}
			}
		}
		if (zoneAddress.z > 10)
		{
			SB.Append(", " + (zoneAddress.z - 10) + " strata deep");
		}
		return SB.ToString();
	}
}
