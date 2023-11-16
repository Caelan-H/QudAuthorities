namespace XRL.Core;

public static class Globals
{
	public static RenderModeType RenderMode = RenderModeType.Tiles;

	public static bool ForceMetricsOff = false;

	public static bool _EnableMetrics = true;

	public static bool EnableSound = false;

	public static bool EnableMusic = false;

	public static bool EnableMetrics
	{
		get
		{
			if (ForceMetricsOff)
			{
				return false;
			}
			return _EnableMetrics;
		}
		set
		{
			_EnableMetrics = value;
		}
	}
}
