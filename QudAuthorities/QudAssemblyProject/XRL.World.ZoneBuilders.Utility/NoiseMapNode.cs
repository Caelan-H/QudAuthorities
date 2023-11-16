namespace XRL.World.ZoneBuilders.Utility;

public class NoiseMapNode
{
	public int x;

	public int y;

	public int depth = -1;

	public NoiseMapNode(int _x, int _y)
	{
		x = _x;
		y = _y;
		depth = -1;
	}

	public NoiseMapNode(int _x, int _y, int _depth)
	{
		x = _x;
		y = _y;
		depth = _depth;
	}
}
