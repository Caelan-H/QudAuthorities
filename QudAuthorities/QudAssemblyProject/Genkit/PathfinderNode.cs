using System.Collections.Generic;

namespace Genkit;

public class PathfinderNode
{
	public int weight;

	public Location2D pos;

	public List<PathfinderNode> adjacentNodes = new List<PathfinderNode>();

	public Dictionary<string, PathfinderNode> nodesByDirection = new Dictionary<string, PathfinderNode>();

	public int X => pos.x;

	public int Y => pos.y;

	public override int GetHashCode()
	{
		return pos.GetHashCode();
	}

	public int SquareDistance(PathfinderNode target)
	{
		return pos.SquareDistance(target.pos);
	}

	public PathfinderNode GetNodeFromDirection(string dir)
	{
		if (nodesByDirection.TryGetValue(dir, out var value))
		{
			return value;
		}
		return null;
	}
}
