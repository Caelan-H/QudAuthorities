using ConsoleLib.Console;
using QupKit;
using UnityEngine.UI;
using XRL.UI;
using XRL.Wish;

[UIView("MarkovCorpusGenerator", false, false, false, null, "MarkovCorpusGenerator", false, 0, false)]
[HasWishCommand]
public class MarkovCorpusGeneratorView : BaseView
{
	private MarkovCorpusGenerator generator;

	public override void OnCreate()
	{
		generator = base.rootObject.AddComponent<MarkovCorpusGenerator>();
		generator.rootObject = base.rootObject;
	}

	public override void OnCommand(string Command)
	{
		if (Command == "Back")
		{
			LegacyViewManager.Instance.SetActiveView("ModToolkit");
		}
		if (Command == "Generate")
		{
			base.rootObject.transform.Find("ProgressPanel").gameObject.SetActive(value: true);
			base.rootObject.transform.Find("ProgressPanel/ProgressLabel").gameObject.GetComponent<UnityEngine.UI.Text>().text = "Hotloading game configuration...";
			generator.Generate();
		}
	}

	[WishCommand("corpusgenerator", null)]
	public static void WishDisplay()
	{
		GameManager.Instance.PushGameView("MarkovCorpusGenerator");
		Keyboard.getvk(MapDirectionToArrows: false);
	}
}
