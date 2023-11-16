using QupKit;
using UnityEngine.UI;
using XRL;
using XRL.Core;
using XRL.UI;

[UIView("ModConfiguration", false, false, false, null, "ModConfiguration", false, 0, false)]
public class ModConfigurationView : BaseView
{
	private bool bDoHotload;

	private ModInfo clickedInfo;

	public override void Enter()
	{
		base.Enter();
		string text = "Provider Status: Not Connected";
		if (SteamManager.Initialized)
		{
			text = "Steam Status: Connected";
		}
		base.rootObject.gameObject.transform.Find("SteamStatus").GetComponent<UnityEngine.UI.Text>().text = text;
		RefreshStatus();
	}

	public override void Leave()
	{
		if (bDoHotload)
		{
			ModManager.BuildScriptMods();
			XRLCore.Core.HotloadConfiguration();
		}
		base.Leave();
	}

	public void RefreshStatus()
	{
		base.rootObject.gameObject.transform.Find("ModScrollerController").GetComponent<InstalledModScrollerController>().Refresh();
	}

	public void OnModClick(ModInfo i)
	{
		if (!i.IsApproved && Options.GetOption("OptionAllowCSMods", "No") == "Yes")
		{
			clickedInfo = i;
			FindChild("ApprovalPanel").SetActive(value: true);
		}
	}

	public override void OnCommand(string Command)
	{
		if (Command == "Approve")
		{
			clickedInfo.Approve();
			bDoHotload = true;
			OnCommand("Reload");
		}
		_ = Command == "Deny";
		if (Command == "Back")
		{
			LegacyViewManager.Instance.SetActiveView("MainMenu");
		}
		if (Command == "Reload")
		{
			RefreshStatus();
		}
	}
}
