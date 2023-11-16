using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
[HasGameBasedStaticCache]
[UIView("NearbyItems", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "NearbyItems", UICanvasHost = 1)]
public class MinimapWindow : MovableSceneFrameWindowBase<MinimapWindow>
{
	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void TogglePreferredState()
	{
		Toggle();
		SaveOptions();
	}

	public void SaveOptions()
	{
		Options.OverlayMinimap = base.Visible;
	}

	public void ShowIfEnabled()
	{
		if (Options.OverlayMinimap)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public override void Update()
	{
		base.Update();
	}
}
