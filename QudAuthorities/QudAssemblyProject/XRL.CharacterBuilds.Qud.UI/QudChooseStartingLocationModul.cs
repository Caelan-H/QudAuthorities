using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:ChooseStartingLocation", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Chargen/ChooseStartingLocation", UICanvasHost = 1)]
public class QudChooseStartingLocationModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudChooseStartingLocationModule, HorizontalScroller>
{
	public IEnumerable<StartingLocationData> GetSelections()
	{
		foreach (StartingLocationData value in base.module.startingLocations.Values)
		{
			yield return value;
		}
	}

	public void onNext()
	{
		base.module.onNext();
	}

	public void onBack()
	{
		base.module.onBack();
	}

	public void onHighlightStartingLocation(FrameworkDataElement element)
	{
		base.module.data.StartingLocation = element.Id;
		base.module.setData(base.module.data);
	}

	public void onSelectStartingLocation(FrameworkDataElement element)
	{
		base.module.data.StartingLocation = element.Id;
		base.module.setData(base.module.data);
		onNext();
	}

	public override void ResetSelection()
	{
		base.module.setData(new QudChooseStartingLocationModuleData("Joppa"));
	}

	public override void DebugQuickstart(string type)
	{
		base.module.data.StartingLocation = "Joppa";
		base.module.setData(base.module.data);
	}

	public override void RandomSelection()
	{
		int index = Stat.Random(0, base.prefabComponent.choices.Count - 1);
		base.prefabComponent.scrollContext.GetContextAt(index).Activate();
		base.module.data.StartingLocation = base.prefabComponent.choices[index].Id;
		base.module.setData(base.module.data);
	}

	public override void Init()
	{
		base.prefabComponent.selectionPrefab = ((GameObject)Resources.Load("Prefabs/StartingLocationButton")).GetComponent<FrameworkUnityScrollChild>();
		base.prefabComponent.gridLayout.cellSize = new Vector2(200f, 225f);
		base.prefabComponent.gridLayout.spacing = new Vector2(20f, 20f);
		base.Init();
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		base.module.data = new QudChooseStartingLocationModuleData();
		base.module.data.StartingLocation = "Joppa";
		base.prefabComponent.autoHotkey = true;
		base.prefabComponent.scrollContext.wraps = false;
		base.prefabComponent.onSelected.RemoveAllListeners();
		base.prefabComponent.onSelected.AddListener(onSelectStartingLocation);
		base.prefabComponent.onHighlight.AddListener(onHighlightStartingLocation);
		base.prefabComponent.BeforeShow(descriptor, GetSelections());
		base.BeforeShow(descriptor);
	}
}
