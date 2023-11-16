using System;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class RoomData
{
	public int Left = 999;

	public int Right;

	public int Top = 999;

	public int Bottom;

	public int Width;

	public int Height;

	public int Size;

	public int[,] Room;
}
