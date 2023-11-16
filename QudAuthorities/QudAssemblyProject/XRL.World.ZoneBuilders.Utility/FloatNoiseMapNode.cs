namespace XRL.World.ZoneBuilders.Utility;

public class FloatNoiseMapNode
{
	public int x;

	public int y;

	public float depth = -1f;

	public FloatNoiseMapNode(int _x, int _y)
	{
		x = _x;
		y = _y;
		depth = -1f;
	}

	public FloatNoiseMapNode(int _x, int _y, float _depth)
	{
		x = _x;
		y = _y;
		depth = _depth;
	}
}
