using System.IO;
using HistoryKit;
using QupKit;
using UnityEngine.UI;
using XRL.Annals;
using XRL.UI;

[UIView("HistoryTest", false, false, false, null, "HistoryTest", false, 0, false)]
public class HistoryTestView : BaseView
{
	public override void Enter()
	{
		base.Enter();
	}

	public override void OnCommand(string Command)
	{
		if (Command == "Back")
		{
			LegacyViewManager.Instance.SetActiveView("MainMenu");
		}
		if (Command == "Generate")
		{
			History history = QudHistoryFactory.GenerateNewSultanHistory();
			GetChildComponent<UnityEngine.UI.Text>("Controls/Scroll View/Viewport/Content/Output").text = history.Dump(bVerbose: false);
			GetChildComponent<ScrollRect>("Controls/Scroll View").verticalNormalizedPosition = 1f;
			File.WriteAllText("history_log.txt", GetChildComponent<UnityEngine.UI.Text>("Controls/Scroll View/Viewport/Content/Output").text);
		}
		_ = Command == "FailureRedirect";
		if (Command == "TestVar")
		{
			History history2 = QudHistoryFactory.GenerateNewSultanHistory();
			HistoricEntity newEntity = history2.GetNewEntity(0L);
			newEntity.ApplyEvent(new SetEntityProperty("organizingPrincipleType", "glassblower"));
			GetChildComponent<UnityEngine.UI.Text>("Controls/Scroll View/Viewport/Content/Output").text = HistoricStringExpander.ExpandString("<$prof=entity.organizingPrincipleType><spice.professions.$prof.plural>", newEntity.GetCurrentSnapshot(), history2);
		}
	}
}
