using Genkit;

namespace XRL.World.ZoneBuilders;

public class SultanTowerDungeonSegment : SultanCircleDungeonSegment
{
	private int thickness = 2;

	public SultanTowerDungeonSegment(Location2D pos, int radius, int thickness)
		: base(pos, radius)
	{
	}

	public override bool HasCustomColor(int x, int y)
	{
		Location2D location2D = Location2D.get(x, y);
		Point2D point2D = new Point2D(location2D.x - center.x, location2D.y - center.y);
		point2D.x = (int)((float)point2D.x / 1.5f);
		if (point2D.Distance(new Point2D(0, 0)) >= radius - thickness)
		{
			return true;
		}
		return false;
	}
}
