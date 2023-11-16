using System;
using System.Diagnostics;
using System.IO;
using Qud.UI;
using QupKit;
using UnityEngine;
using XRL;

public class ModToolkitView : BaseView
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
		if (Command == "MapEditor")
		{
			UIManager.getWindow("MapEditor").Show();
		}
		if (Command == "WorkshopUploader")
		{
			LegacyViewManager.Instance.SetActiveView("SteamWorkshopUploader");
		}
		if (Command == "ModWiki")
		{
			Application.OpenURL("https://wiki.cavesofqud.com/Modding:Overview");
		}
		if (Command == "HistoryTest")
		{
			LegacyViewManager.Instance.SetActiveView("HistoryTest");
		}
		if (Command == "WaveformTest")
		{
			LegacyViewManager.Instance.SetActiveView("WaveformTest");
		}
		if (Command == "BrowseBlueprints")
		{
			LegacyViewManager.Instance.SetActiveView("ModToolkit:BrowseBlueprints");
		}
		if (Command == "OpenFolder")
		{
			OpenFolder();
		}
		if (Command == "WriteCSProj")
		{
			WriteCSProj();
		}
	}

	public void WriteCSProj()
	{
		string path = DataManager.SavePath("Mods.csproj");
		string text = File.ReadAllText(DataManager.FilePath("Mods.csproj.template.txt"));
		File.WriteAllText(path, text.Replace("$MANAGED_PATH$", Path.Combine(Application.dataPath, "Managed") + "/"));
	}

	public void OpenFolder()
	{
		string text = DataManager.SavePath("").TrimEnd('\\', '/');
		try
		{
			Process.Start("open", "\"" + text + "\"");
		}
		catch (Exception)
		{
		}
		try
		{
			Process.Start("xdg-open", "\"" + text + "\"");
		}
		catch (Exception)
		{
		}
		try
		{
			Process.Start("explorer.exe", "\"" + text.Replace("/", "\\") + "\"");
		}
		catch (Exception)
		{
		}
	}
}
