using XRL.UI;

namespace Qud.UI;

[UIView("Stage", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "Stage", UICanvasHost = 1)]
public class StageWindow : SingletonWindowBase<StageWindow>
{
	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public override void Show()
	{
		base.Show();
		UIManager.getWindow("PlayerStatusBar").Show();
		UIManager.getWindow("AbilityBar").Show();
		UIManager.getWindow("MessageLog").Show();
		UIManager.getWindow<NearbyItemsWindow>("NearbyItems").ShowIfEnabled();
		UIManager.getWindow<MinimapWindow>("Minimap").ShowIfEnabled();
	}

	public override void Hide()
	{
		base.Hide();
		UIManager.getWindow("PlayerStatusBar").Hide();
		UIManager.getWindow("AbilityBar").Hide();
		UIManager.getWindow("MessageLog").Hide();
		UIManager.getWindow("NearbyItems").Hide();
		UIManager.getWindow("Minimap").Hide();
	}
}
