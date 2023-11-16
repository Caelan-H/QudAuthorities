using System;

namespace XRL.World;

[Serializable]
public class Point
{
	public int X;

	public int Y;

	public int Direction;

	public char DisplayChar;

	public Point(int x, int y)
	{
		X = x;
		Y = y;
		Direction = 0;
		DisplayChar = ' ';
	}

	public Point(int x, int y, int direction, char displaychar)
	{
		X = x;
		Y = y;
		Direction = direction;
		DisplayChar = displaychar;
	}
}
