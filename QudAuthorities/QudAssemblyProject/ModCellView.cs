using EnhancedUI.EnhancedScroller;
using QupKit;
using UnityEngine.UI;

public class ModCellView : EnhancedScrollerCellView
{
	public Text modPath;

	public ModScrollerData data;

	public void SetData(ModScrollerData data)
	{
		this.data = data;
		modPath.text = data.info.ID;
	}

	public void OnSelected()
	{
		(LegacyViewManager.Instance.GetView("SteamWorkshopUploader") as SteamWorkshopUploaderView).SetModInfo(data.info);
	}
}
