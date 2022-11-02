using Overlay.MapEditor;
using XRL.UI;

namespace Qud.UI;

[UIView("MapEditor", true, false, false, "Menu", "MapEditor", false, 0, false, UICanvasHost = 1)]
public class MapEditorWindow : LegacyViewWindow<MapEditorView>
{
	public override void Show()
	{
		base.Show();
		base.gameObject.SetActive(value: true);
	}

	public override void Hide()
	{
		base.Hide();
		base.gameObject.SetActive(value: false);
	}

	public void OnCommand(string command)
	{
		baseView.OnCommand(command);
	}
}
