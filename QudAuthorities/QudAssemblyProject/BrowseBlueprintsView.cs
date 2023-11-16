using QupKit;
using XRL.UI;

[UIView("ModToolkit:BrowseBlueprints", false, false, false, null, "ModToolkit:BrowseBlueprints", false, 0, false)]
public class BrowseBlueprintsView : BaseView
{
	public string mode = "old";

	public override void OnEnter()
	{
		if (GameManager.Instance.PrereleaseInput)
		{
			mode = "new";
		}
		else
		{
			mode = "old";
		}
	}

	public override void OnLeave()
	{
		_ = mode == "new";
	}

	public override void OnCommand(string Command)
	{
		if (Command == "Back")
		{
			LegacyViewManager.Instance.SetActiveView("ModToolkit");
		}
	}
}
