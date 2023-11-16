using System;
using System.Diagnostics;
using System.IO;
using QupKit;
using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

[ExecuteAlways]
[UIView("ModToolkit", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "ModToolkit", UICanvasHost = 1)]
public class ModToolkit : SingletonWindowBase<ModToolkit>
{
	public QudTextMenuController menuController;

	public override void Hide()
	{
		base.Hide();
		base.gameObject.SetActive(value: false);
	}

	public override void Show()
	{
		base.gameObject.SetActive(value: true);
		base.Show();
	}

	public override void Init()
	{
		base.Init();
		if (menuController != null)
		{
			menuController.cancelHandlers.AddListener(OnCancel);
			menuController.activateHandlers.AddListener(OnActivate);
			menuController.isCurrentWindow = base.isCurrentWindow;
		}
	}

	public void OnCancel()
	{
		LegacyViewManager.Instance.SetActiveView("MainMenu");
		Hide();
	}

	public void OnActivate(QudMenuItem data)
	{
		if (data.command == "ModManager")
		{
			UIManager.pushWindow("ModManager");
		}
		if (data.command == "MapEditor")
		{
			UIManager.getWindow("MapEditor").Show();
			Hide();
		}
		if (data.command == "Workshop")
		{
			LegacyViewManager.Instance.SetActiveView("SteamWorkshopUploader");
			Hide();
		}
		if (data.command == "Corpus")
		{
			LegacyViewManager.Instance.SetActiveView("MarkovCorpusGenerator");
			Hide();
		}
		if (data.command == "Wiki")
		{
			Application.OpenURL("https://wiki.cavesofqud.com/Modding:Overview");
		}
		if (data.command == "History")
		{
			LegacyViewManager.Instance.SetActiveView("HistoryTest");
			Hide();
		}
		if (data.command == "Waveform")
		{
			LegacyViewManager.Instance.SetActiveView("WaveformTest");
			Hide();
		}
		if (data.command == "Blueprint")
		{
			LegacyViewManager.Instance.SetActiveView("ModToolkit:BrowseBlueprints");
			Hide();
		}
		if (data.command == "OpenSave")
		{
			OpenFolder();
		}
		if (data.command == "csproj")
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
