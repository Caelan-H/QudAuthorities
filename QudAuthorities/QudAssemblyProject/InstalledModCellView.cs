using EnhancedUI.EnhancedScroller;
using ModelShark;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL;

public class InstalledModCellView : EnhancedScrollerCellView
{
	public Text modPath;

	public InstalledModScrollerData data;

	public void SetData(InstalledModScrollerData data)
	{
		this.data = data;
		modPath.text = data.info.ID;
		if (data.info.Source == ModSource.Local)
		{
			modPath.text += " from Mods Folder";
		}
		else if (data.info.Source == ModSource.Pet)
		{
			modPath.text += " from Pets Folder";
		}
		else if (data.info.Source == ModSource.Steam)
		{
			modPath.text += " from Steam Workshop";
		}
		if (!data.info.IsApproved)
		{
			GetComponent<Image>().color = new Color(1f, 0f, 0f);
			GetComponent<TooltipTrigger>().parameterizedTextFields[0].value = "This mod contains a script and has changed since it was last approved. Click for approval options.\n\nWARNING: Scripting mods execute with the same privileges as Caves of Qud and may contain malicious code.";
		}
		else
		{
			GetComponent<Image>().color = new Color(0.05f, 0.2f, 0.05f);
			GetComponent<TooltipTrigger>().parameterizedTextFields[0].value = "This mod is approved and enabled.";
		}
	}

	public void OnSelected()
	{
		(LegacyViewManager.Instance.GetView("ModConfiguration") as ModConfigurationView).OnModClick(data.info);
	}
}
