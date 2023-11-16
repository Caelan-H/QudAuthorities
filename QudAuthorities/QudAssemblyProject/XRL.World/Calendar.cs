using System;
using XRL.Core;

namespace XRL.World;

[Serializable]
public static class Calendar
{
	public const int TurnsPerYear = 438000;

	public const int turnsPerDay = 1200;

	public const int turnsPerHour = 50;

	public const int startOfDay = 3250;

	public const int startOfNight = 10000;

	public static long TotalTimeTicks => XRLCore.Core.Game.TimeTicks;

	public static int CurrentDaySegment => (int)TotalTimeTicks % 1200 * 10;

	public static int getYear()
	{
		return (int)(TotalTimeTicks / 438000) + 1001;
	}

	public static string GetMonth(long Time)
	{
		return getMonth((int)(Time % 438000));
	}

	public static string getMonth(int _dayOfYear = -1)
	{
		int num = ((_dayOfYear <= -1) ? ((int)TotalTimeTicks % 438000) : _dayOfYear);
		if (num < 36001)
		{
			return "Nivvun Ut";
		}
		if (num < 72001)
		{
			return "Iyur Ut";
		}
		if (num < 108001)
		{
			return "Simmun Ut";
		}
		if (num < 144001)
		{
			return "Tuum Ut";
		}
		if (num < 180001)
		{
			return "Ubu Ut";
		}
		if (num < 216001)
		{
			return "Uulu Ut";
		}
		if (num < 222001)
		{
			return "Ut yara Ux";
		}
		if (num < 258001)
		{
			return "Tishru i Ux";
		}
		if (num < 294001)
		{
			return "Tishru ii Ux";
		}
		if (num < 330001)
		{
			return "Kisu Ux";
		}
		if (num < 366001)
		{
			return "Tebet Ux";
		}
		if (num < 402001)
		{
			return "Shwut Ux";
		}
		_ = 438001;
		return "Uru Ux";
	}

	public static string GetDay(long Time)
	{
		return getDay((int)(Time % 438000));
	}

	public static string getDay(int _dayOfYear = -1)
	{
		int num = ((_dayOfYear <= -1) ? ((int)TotalTimeTicks % 438000) : _dayOfYear);
		if (num > 216000 && num < 222001)
		{
			if (num < 217201)
			{
				return "1st";
			}
			if (num < 218401)
			{
				return "2nd";
			}
			if (num < 219601)
			{
				return "3rd";
			}
			if (num < 220801)
			{
				return "4th";
			}
			if (num < 222001)
			{
				return "5th";
			}
			return "0th";
		}
		if (num > 222000)
		{
			num -= 6000;
		}
		int num2 = num % 36000;
		if (num2 < 1200)
		{
			return "1st";
		}
		if (num2 < 2400)
		{
			return "2nd";
		}
		if (num2 < 3600)
		{
			return "3rd";
		}
		if (num2 < 4800)
		{
			return "4th";
		}
		if (num2 < 6000)
		{
			return "5th";
		}
		if (num2 < 7200)
		{
			return "6th";
		}
		if (num2 < 8400)
		{
			return "7th";
		}
		if (num2 < 9600)
		{
			return "8th";
		}
		if (num2 < 10800)
		{
			return "9th";
		}
		if (num2 < 12000)
		{
			return "10th";
		}
		if (num2 < 13200)
		{
			return "11th";
		}
		if (num2 < 14400)
		{
			return "12th";
		}
		if (num2 < 15600)
		{
			return "13th";
		}
		if (num2 < 16800)
		{
			return "14th";
		}
		if (num2 < 18000)
		{
			return "Ides";
		}
		if (num2 < 19200)
		{
			return "16th";
		}
		if (num2 < 20400)
		{
			return "17th";
		}
		if (num2 < 21600)
		{
			return "18th";
		}
		if (num2 < 22800)
		{
			return "19th";
		}
		if (num2 < 24000)
		{
			return "20th";
		}
		if (num2 < 25200)
		{
			return "21st";
		}
		if (num2 < 26400)
		{
			return "22nd";
		}
		if (num2 < 27600)
		{
			return "23rd";
		}
		if (num2 < 28800)
		{
			return "24th";
		}
		if (num2 < 30000)
		{
			return "25th";
		}
		if (num2 < 31200)
		{
			return "26th";
		}
		if (num2 < 32400)
		{
			return "27th";
		}
		if (num2 < 33600)
		{
			return "28th";
		}
		if (num2 < 34800)
		{
			return "29th";
		}
		if (num2 < 36000)
		{
			return "30th";
		}
		return "0th";
	}

	public static string getTime(string zoneID = null)
	{
		if (zoneID == null || zoneID == "JoppaWorld")
		{
			return getTime((int)TotalTimeTicks % 1200);
		}
		return "";
	}

	public static string getTime(int minute)
	{
		if (minute >= 0 && minute < 26)
		{
			return "Beetle Moon Zenith";
		}
		if (minute < 151)
		{
			return "Waning Beetle Moon";
		}
		if (minute < 301)
		{
			return "The Shallows";
		}
		if (minute < 451)
		{
			return "Harvest Dawn";
		}
		if (minute < 576)
		{
			return "Waxing Salt Sun";
		}
		if (minute < 626)
		{
			return "High Salt Sun";
		}
		if (minute < 751)
		{
			return "Waning Salt Sun";
		}
		if (minute < 901)
		{
			return "Hindsun";
		}
		if (minute < 1051)
		{
			return "Jeweled Dusk";
		}
		if (minute < 1176)
		{
			return "Waxing Beetle Moon";
		}
		if (minute < 1201)
		{
			return "Beetle Moon Zenith";
		}
		return "Zero Hour";
	}

	public static bool IsDay()
	{
		if (CurrentDaySegment >= 2500)
		{
			return CurrentDaySegment < 9124;
		}
		return false;
	}
}
