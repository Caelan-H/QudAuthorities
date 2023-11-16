using XRL.Core;

namespace XRL.UI;

internal static class Overlay
{
	public static string CurrentScreen = Options.StageViewID;

	public static Vector2i CurrentCell
	{
		get
		{
			if (XRLCore.Core.Game.Player != null && XRLCore.Core.Game.Player.Body != null && XRLCore.Core.Game.Player.Body.pPhysics != null && XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell != null)
			{
				return new Vector2i(XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.X, XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.Y);
			}
			return new Vector2i(39, 12);
		}
	}
}
