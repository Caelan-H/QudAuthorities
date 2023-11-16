using System;
using System.Collections.Generic;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class SpireRoomData
{
	public int[] LeftAt;

	public int[] RightAt;

	public int[] TopAt;

	public int[] BottomAt;

	public List<Point> Doors = new List<Point>();

	public int Left = 999;

	public int Right;

	public int Top = 999;

	public int Bottom;

	public int Width;

	public int Height;

	public int Size;

	public int[,] Room;

	public SpireRoomData(int Width, int Height)
	{
		LeftAt = new int[Height];
		RightAt = new int[Height];
		TopAt = new int[Width];
		BottomAt = new int[Width];
		Room = new int[Width, Height];
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				LeftAt[j] = 999;
				RightAt[j] = 0;
				TopAt[i] = 999;
				BottomAt[i] = 0;
				Room[i, j] = 0;
			}
		}
	}
}
